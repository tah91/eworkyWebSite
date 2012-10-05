using System.Collections.Generic;
using Worki.Infrastructure.Repository;
using System.Net;
using System.Globalization;
using Worki.Infrastructure.Logging;
using System;
using System.Text;
using System.Data.Objects;
using System.Linq;
using System.Data.Entity;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;
using Worki.Infrastructure.Helpers;

namespace Worki.Data.Models
{
	public interface ILocalisationRepository : IRepository<Localisation>
	{
		IList<Localisation> FindSimilarLocalisation(float latitude, float longitude);
        IList<Localisation> FindByCriteria(SearchCriteria criteria);
        void FillCriteriaProjection(SearchCriteria criteria);
		float DistanceBetween(float latitude, float longitude, int localisationId);
		Comment GetComment(int comId);
		IList<MemberEdition> GetLatestModifications(int count, EditionType type);
		IList<Localisation> GetMostBooked(int memberId, int count);
	}

	public class LocalisationRepository : RepositoryBase<Localisation>, ILocalisationRepository
	{
		public LocalisationRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

		#region ILocalisationRepository

		//the radius of circle within which two localisation are considered same (in km)
		public const float SeparationDistance = 0.01F;

		public const float EarthRadius = 6376.5F;

		public IList<Localisation> FindSimilarLocalisation(float latitude, float longitude)
		{
			var factor = CultureHelpers.GetDistanceFactor();
			return (from localisation in _Context.Localisations
					where EdmMethods.DistanceBetween(latitude, longitude, (float)localisation.Latitude, (float)localisation.Longitude, EarthRadius * factor) < SeparationDistance * factor
					select localisation).ToList();
		}

        void FilterList(SearchCriteria.Filter filter, ref IQueryable<Localisation> localisations)
        {
            if (filter == null)
                return;

            var countries = filter.GetCountries();
            if (countries.Count() > 0)
                localisations = localisations.Where(loc => countries.Contains(loc.CountryId));

            var types = filter.GetTypes();
            if (types.Count() > 0)
                localisations = localisations.Where(loc => types.Contains(loc.TypeValue));
        }

        public void FillCriteriaProjection(SearchCriteria criteria)
        {
            //all
            var localisations = _Context.Localisations.AsQueryable();
            //exclude offline ones
            localisations = localisations.Where(loc => loc.MainLocalisation != null && !loc.MainLocalisation.IsOffline);

            FilterList(criteria.PreFilter, ref localisations);

            var factor = CultureHelpers.GetDistanceFactor();

            //matching address
            //if NorthEast and SouthWest given
            if (criteria.HasBounds())
            {
                localisations = from loc
                                        in localisations
                                where criteria.SouthWestLat < (float)loc.Latitude && (float)loc.Latitude < criteria.NorthEastLat
                                        && criteria.SouthWestLng < (float)loc.Longitude && (float)loc.Longitude < criteria.NorthEastLng
                                select loc;
            }
            else
            {
                var critLat = (float)criteria.LocalisationData.Latitude;
                var critLng = (float)criteria.LocalisationData.Longitude;
                var boundary = criteria.Boundary != 0 ? criteria.Boundary : 50;
                if (critLat != 0 && critLng != 0)
                    localisations = from loc
                                         in localisations
                                    where EdmMethods.DistanceBetween(critLat, critLng, (float)loc.Latitude, (float)loc.Longitude, EarthRadius * factor) < boundary * factor
                                    select loc;
            }

            if (criteria.FreeAreas)
            {
                criteria.SpotWifi = true;
                criteria.CoffeeResto = true;
                criteria.Biblio = true;
                criteria.TravelerSpace = true;
            }

            if (criteria.OtherTypes)
            {
                criteria.PrivateArea = true;
                criteria.WorkingHotel = true;
                criteria.PublicSpace = true;
                criteria.Hotel = true;
            }

            switch (criteria.GlobalType)
            {
                case eGlobalType.BuisnessCenter_Smartworkcenter:
                    criteria.BuisnessCenter = true;
                    criteria.Telecentre = true;
                    break;
                case eGlobalType.Coworking_SharedOffice:
                    criteria.CoworkingSpace = true;
                    criteria.SharedOffice = true;
                    break;
                case eGlobalType.MeetingRoom:
                    criteria.OfferData.Type = (int)LocalisationOffer.MeetingRoom;
                    break;
                default:
                    break;
            }

            //matching type
            var allowedTypes = new List<int>();
            if (criteria.SpotWifi)
                allowedTypes.Add((int)LocalisationType.SpotWifi);
            if (criteria.CoffeeResto)
                allowedTypes.Add((int)LocalisationType.CoffeeResto);
            if (criteria.Biblio)
                allowedTypes.Add((int)LocalisationType.Biblio);
            if (criteria.PublicSpace)
                allowedTypes.Add((int)LocalisationType.PublicSpace);
            if (criteria.TravelerSpace)
                allowedTypes.Add((int)LocalisationType.TravelerSpace);
            if (criteria.Hotel)
                allowedTypes.Add((int)LocalisationType.Hotel);
            if (criteria.Telecentre)
                allowedTypes.Add((int)LocalisationType.Telecentre);
            if (criteria.BuisnessCenter)
                allowedTypes.Add((int)LocalisationType.BuisnessCenter);
            if (criteria.CoworkingSpace)
                allowedTypes.Add((int)LocalisationType.CoworkingSpace);
            if (criteria.WorkingHotel)
                allowedTypes.Add((int)LocalisationType.WorkingHotel);
            if (criteria.PrivateArea)
                allowedTypes.Add((int)LocalisationType.PrivateArea);
            if (criteria.SharedOffice)
                allowedTypes.Add((int)LocalisationType.SharedOffice);
            if (allowedTypes.Count > 0)
                localisations = localisations.Where(loc => allowedTypes.Contains(loc.TypeValue));

            //retrieve list from db, then filter it
            var locProjectionList = (from item in localisations
                                     select new LocalisationProjection
                                     {
                                         ID = item.ID,
                                         Latitude = (float)item.Latitude,
                                         Longitude = (float)item.Longitude,
                                         LocalisationType = item.TypeValue,
                                         LocalisationName = item.Name,
                                         Features = (from f in item.LocalisationFeatures select f.FeatureID),
                                         OfferTypes = (from o in item.Offers where o.IsOnline select o.Type),
                                         Ratings = (from c in item.Comments select new CommentProjection  { Price = c.RatingPrice, Wifi = c.RatingWifi, Dispo = c.RatingDispo, Welcome = c.RatingWelcome, Rating = c.Rating })
                                     }).ToList();

            //match name if needed
            if (!string.IsNullOrEmpty(criteria.LocalisationData.Name))
            {
                var nameToSearch = criteria.LocalisationData.Name.ToLower().Split(' ');
                var nameIds = new List<int>();
                foreach (var item in nameToSearch)
                {
                    var containItem = locProjectionList.Where(p => p.LocalisationName.ToLower().Contains(item)).Select(p => p.ID);
                    nameIds = nameIds.Concat(containItem).ToList();
                }

                locProjectionList = locProjectionList.Where(p => nameIds.Contains(p.ID)).ToList();
            }

            //if list of offer types (in api)
            if (!string.IsNullOrEmpty(criteria.OfferTypes))
            {
                var offerTypeArray = criteria.OfferTypes.SplitAndParse();

                var needFreeArea = offerTypeArray.Contains((int)LocalisationOffer.FreeArea);
                if (offerTypeArray.Count() > 0)
                    locProjectionList = locProjectionList.Where(p => (p.OfferTypes.Intersect(offerTypeArray).Count() > 0) || (needFreeArea && Localisation.FreeLocalisationTypes.Contains(p.LocalisationType))).ToList();
            }
            else
            {
                //match offer type
                var offerType = (LocalisationOffer)criteria.OfferData.Type;
                switch (offerType)
                {
                    case LocalisationOffer.FreeArea:
                        {
                            locProjectionList = locProjectionList.Where(p => Localisation.FreeLocalisationTypes.Contains(p.LocalisationType)).ToList();
                            break;
                        }
                    case LocalisationOffer.BuisnessLounge:
                    case LocalisationOffer.Desktop:
                    case LocalisationOffer.Workstation:
                    case LocalisationOffer.MeetingRoom:
                    case LocalisationOffer.SeminarRoom:
                    case LocalisationOffer.VisioRoom:
                        {
                            locProjectionList = locProjectionList.Where(p => p.OfferTypes.Contains((int)offerType)).ToList();
                            break;
                        }
                    case LocalisationOffer.AllOffers:
                    default:
                        break;
                }
            }

            //match localisation features
            var neededLocalisationFeatures = (from item in criteria.LocalisationData.LocalisationFeatures select item.FeatureID).ToList();
            locProjectionList = locProjectionList.Where(loc =>
            {
                foreach (var item in neededLocalisationFeatures)
                {
                    //if we search wifi, it is for not free as well
                    if (item == (int)Feature.Wifi_Free)
                    {
                        if (!loc.Features.Contains(item) && !loc.Features.Contains((int)Feature.Wifi_Not_Free))
                            return false;
                        continue;
                    }

                    if (!loc.Features.Contains(item))
                        return false;
                }
                return true;
            }).ToList();

            //build an offerlist which contains correct ids
            if (criteria.OfferData.OfferFeatures.Count != 0)
            {
                var offers = _Context.Offers.AsQueryable();
                //all offers from the localisations that are online
                var correctIds = locProjectionList.Select(loc => loc.ID).ToList();
                offers = offers.Where(o => o.IsOnline && correctIds.Contains(o.LocalisationId));

                var offerProjectionList = (from item in offers
                                           select new
                                           {
                                               ID = item.Id,
                                               LocID = item.LocalisationId,
                                               OfferType = item.Type,
                                               Features = (from f in item.OfferFeatures select f.FeatureId)
                                           }).ToList();

                var neededOfferFeatures = (from item in criteria.OfferData.OfferFeatures select item.FeatureId).ToList();

                //all localisation which offer match needed features
                var idsToLoad = offerProjectionList.Where(offer =>
                {
                    foreach (var item in neededOfferFeatures)
                    {
                        if (!offer.Features.Contains(item))
                            return false;
                    }
                    return true;
                }).Select(offer => offer.LocID).ToList();

                locProjectionList = locProjectionList.Where(loc => idsToLoad.Contains(loc.ID)).ToList();
            }

            criteria.Projection = locProjectionList;
        }

        public IList<Localisation> FindByCriteria(SearchCriteria criteria)
		{
            FillCriteriaProjection(criteria);

            //to order by ratings
			var ratingDict = new Dictionary<int, double>();
            foreach (var item in criteria.Projection)
			{
				if (item.Ratings.Count() == 0)
				{
					ratingDict[item.ID] = -1;
					continue;
				}
				try
				{
					if (Localisation.FreeLocalisationTypes.Contains(item.LocalisationType))
					{
						var ratings = new List<double>
                    { 
                        item.Ratings.Where(c => c.Price >= 0).Average(c => c.Price),
						item.Ratings.Where(c => c.Wifi >= 0).Average(c => c.Wifi),
						item.Ratings.Where(c => c.Dispo >= 0).Average(c => c.Dispo),
						item.Ratings.Where(c => c.Welcome >= 0).Average(c => c.Welcome)
                    };
						ratingDict[item.ID] = ratings.Where(d => d >= 0).Average();
					}
					else
					{
						ratingDict[item.ID] = item.Ratings.Where(c => c.Rating >= 0).Average(c => c.Rating);
					}
				}
				catch (Exception)
				{
					ratingDict[item.ID] = -1;
				}
			}

            var ids = criteria.Projection.Select(p => p.ID);
            return _Context.Localisations.Where(loc => ids.Contains(loc.ID)).ToList().OrderByDescending(loc => ratingDict[loc.ID]).ToList();
		}

		public Comment GetComment(int comId)
		{
			return _Context.Comments.SingleOrDefault(d => d.ID == comId);
		}

		public float DistanceBetween(float latitude, float longitude, int localisationId)
		{
			var factor = CultureHelpers.GetDistanceFactor();
			var loc = _Context.Localisations.SingleOrDefault(d => d.ID == localisationId);
			if (loc == null)
				return 0;
			return EdmMethods.DistanceBetween((float)loc.Latitude, (float)loc.Longitude, latitude, longitude, EarthRadius * factor) ?? 0;
		}

        /// <summary>
        /// Get back the 100 last modifications (creation/edition)
        /// </summary>
        /// <param name="count">Amount of modifications you want to see</param>
        /// <param name="type">Type of modification (Creation/Edition)</param>
        /// <returns></returns>
		public IList<MemberEdition> GetLatestModifications(int count, EditionType type)
		{
			var lastest = from item in _Context.MemberEditions
						  where item.ModificationType == (int)type
						  orderby item.ModificationDate descending
						  select item;

			return lastest.Take(count).ToList();
		}

		public IList<Localisation> GetMostBooked(int memberId, int count)
		{
			var locBookings = (from item
						  in _Context.MemberBookings
					   where item.Offer.Localisation.OwnerID == memberId
					   group item by item.Offer.LocalisationId into bookings
					   where bookings.Count() > 0
					   orderby bookings.Count() descending
					   select new { LocalisationId = bookings.Key, BookingCount = bookings.Count() }).Take(count);

            var ids = locBookings.Select(r => r.LocalisationId).ToList();
			var fetchedCount = ids.Count;
			if (fetchedCount < count)
			{
				var extra = (from loc in _Context.Localisations
							 where loc.OwnerID == memberId && !ids.Contains(loc.ID) && !Localisation.FreeLocalisationTypes.Contains(loc.TypeValue)
							 select loc.ID).Take(count - fetchedCount).ToList();
				ids = ids.Concat(extra).ToList();
			}

			return GetMany(loc => ids.Contains(loc.ID));
		}

		#endregion
	}
}
