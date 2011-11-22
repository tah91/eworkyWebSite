﻿using System.Collections.Generic;
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

namespace Worki.Data.Models
{
	public interface ILocalisationRepository : IRepository<Localisation>
	{
		IList<Localisation> FindByLocation(float latitude, float longitude);
		IList<Localisation> FindSimilarLocalisation(float latitude, float longitude);
		IList<Localisation> FindByCriteria(SearchCriteria criteria);
		float DistanceBetween(float latitude, float longitude, int localisationId);
		Comment GetComment(int comId);
		IList<MemberEdition> GetLatestModifications(int count, EditionType type);
        IList<Localisation> GetSpace(LocalisationType type, string country);
	}

	public class LocalisationRepository : RepositoryBase<Localisation>, ILocalisationRepository
	{
		public LocalisationRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

		#region ILocalisationRepository

		//the radius of circle within which the results of the seach must be (in km)
		public const float BoundDistance = 50;
		//the radius of circle within which two localisation are considered same (in km)
		public const float SeparationDistance = 0.01F;

		public IList<Localisation> FindByLocation(float latitude, float longitude)
		{
			return (from localisation in _Context.Localisations
					where EdmMethods.DistanceBetween(latitude, longitude, (float)localisation.Latitude, (float)localisation.Longitude) < BoundDistance
					select localisation).ToList();
		}

		public IList<Localisation> FindSimilarLocalisation(float latitude, float longitude)
		{
			return (from localisation in _Context.Localisations
					where EdmMethods.DistanceBetween(latitude, longitude, (float)localisation.Latitude, (float)localisation.Longitude) < SeparationDistance
					select localisation).ToList();
		}

		public IList<Localisation> FindByCriteria(SearchCriteria criteria)
		{
			var idsToLoad = new List<int>();
			//all
			var localisations = _Context.Localisations.AsQueryable();
            //exclude offline ones
			localisations = localisations.Where(loc => loc.MainLocalisation != null && !loc.MainLocalisation.IsOffline);
			//matching address
			var critLat = (float)criteria.LocalisationData.Latitude;
			var critLng = (float)criteria.LocalisationData.Longitude;
			if (critLat != 0 && critLng != 0)
				localisations = from loc
									 in localisations
								where EdmMethods.DistanceBetween(critLat, critLng, (float)loc.Latitude, (float)loc.Longitude) < BoundDistance
								select loc;

			//matching type
			if (!criteria.Everything)
			{
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
				localisations = localisations.Where(loc => allowedTypes.Contains(loc.TypeValue));
			}

			//retrieve list from db, then filter it
			var locProjectionList = (from item in localisations
									 select new
									 {
										 ID = item.ID,
										 LocalisationType = item.TypeValue,
										 Features = (from f in item.LocalisationFeatures select f.FeatureID),
										 OfferTypes = (from o in item.Offers select o.Type),
										 Ratings = (from c in item.Comments select new { Price = c.RatingPrice, Wifi = c.RatingWifi, Dispo = c.RatingDispo, Welcome = c.RatingWelcome, Rating = c.Rating })
									 }).ToList();

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

			var neededLocalisationFeatures = (from item in criteria.LocalisationData.LocalisationFeatures select item.FeatureID).ToList();

			idsToLoad = locProjectionList.Where(loc =>
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
				}).Select(loc => loc.ID).ToList();

			var ratingDict = new Dictionary<int, double>();
			foreach (var item in locProjectionList)
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

            //build an offerlist which contains correct ids
            if (criteria.OfferData.OfferFeatures.Count != 0)
            {
                var offers = _Context.Offers.AsQueryable();
                //all offers from the localisations
                offers = offers.Where(o => idsToLoad.Contains(o.LocalisationId));

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
                idsToLoad = offerProjectionList.Where(offer =>
                {
                    foreach (var item in neededOfferFeatures)
                    {
                        if (!offer.Features.Contains(item))
                            return false;
                    }
                    return true;
                }).Select(offer => offer.LocID).ToList();
            }

			return _Context.Localisations.Where(loc => idsToLoad.Contains(loc.ID)).ToList().OrderByDescending(loc => ratingDict[loc.ID]).ToList();
		}

		public Comment GetComment(int comId)
		{
			return _Context.Comments.SingleOrDefault(d => d.ID == comId);
		}

		public float DistanceBetween(float latitude, float longitude, int localisationId)
		{
			var loc = _Context.Localisations.SingleOrDefault(d => d.ID == localisationId);
			if (loc == null)
				return 0;
			return EdmMethods.DistanceBetween((float)loc.Latitude, (float)loc.Longitude, latitude, longitude) ?? 0;
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

        /// <summary>
        /// Get back all localisation of an asked type and location
        /// </summary>
        /// <param name="type">Type of localisation</param>
        /// <param name="country">Location desired</param>
        /// <returns></returns>
        public IList<Localisation> GetSpace(LocalisationType type, string country)
        {
            var locs = from item in _Context.Localisations
                          where item.Country == country && item.TypeValue == (int)type
                          orderby item.ID descending
                          select item;

            return locs.ToList();
        }

		#endregion
	}
}
