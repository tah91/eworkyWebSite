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

namespace Worki.Data.Models
{
	public interface ILocalisationRepository : IRepository<Localisation>
	{
		IList<Localisation> FindByLocation(float latitude, float longitude);
		IList<Localisation> FindSimilarLocalisation(float latitude, float longitude);
		IList<Localisation> FindByCriteria(SearchCriteria criteria);
		float DistanceBetween(float latitude, float longitude, int localisationId);
		Comment GetComment(int comId);
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

		#region FeatureProjection

		public class FeatureProjection
		{
			public int Feature { get; set; }
			public int Offer { get; set; }
		}

		public class FeatureProjectionEqualityComparer : IEqualityComparer<FeatureProjection>
		{
			#region IEqualityComparer<FeatureProjection> Members

			public bool Equals(FeatureProjection x, FeatureProjection y)
			{
				//case of general feature
				if (x.Offer == 0 || y.Offer == 0)
					return x.Feature == y.Feature;
				else
					return x.Feature == y.Feature && x.Offer == y.Offer;
			}

			public int GetHashCode(FeatureProjection obj)
			{
				return base.GetHashCode();
			}

			#endregion
		}

		#endregion

		public IList<Localisation> FindByCriteria(SearchCriteria criteria)
		{
			var idsToLoad = new List<int>();
			//all
			var localisations = _Context.Localisations.AsQueryable();
			//matching address
			var critLat = (float)criteria.LocalisationData.Latitude;
			var critLng = (float)criteria.LocalisationData.Longitude;
			if (critLat != 0 && critLng != 0)
				localisations = from loc
									 in localisations
								where EdmMethods.DistanceBetween(critLat, critLng, (float)loc.Latitude, (float)loc.Longitude) < BoundDistance
								select loc;// as DbSet<Localisation>;

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
				localisations = localisations.Where(loc => allowedTypes.Contains(loc.TypeValue));// as DbSet<Localisation>;
			}

			//retrieve list from db, then filter it
			var tempList = (from item in localisations
							select new
							{
								ID = item.ID,
								Features = (from f in item.LocalisationFeatures
											select new FeatureProjection
											{
												Feature = f.FeatureID,
												Offer = f.OfferID
											})
							}).ToList();
			var neededFeatures = new List<FeatureProjection>();

			//matching the offer type
			var locOffer = (LocalisationOffer)criteria.LocalisationOffer;
			switch (locOffer)
			{
				case LocalisationOffer.FreeArea:
					{
						neededFeatures.Add(new FeatureProjection { Feature = (int)Feature.FreeArea });
					}
					break;
				case LocalisationOffer.BuisnessRoom:
					{
						neededFeatures.Add(new FeatureProjection { Feature = (int)Feature.BuisnessRoom });
					}
					break;
				case LocalisationOffer.Workstation:
					{
						neededFeatures.Add(new FeatureProjection { Feature = (int)Feature.Workstation });
					}
					break;
				case LocalisationOffer.SingleDesk:
					{
						neededFeatures.Add(new FeatureProjection { Feature = (int)Feature.SingleDesk });
					}
					break;
				case LocalisationOffer.MeetingRoom:
					{
						neededFeatures.Add(new FeatureProjection { Feature = (int)Feature.MeetingRoom });
					}
					break;
				case LocalisationOffer.SeminarRoom:
					{
						neededFeatures.Add(new FeatureProjection { Feature = (int)Feature.SeminarRoom });
					}
					break;
				case LocalisationOffer.VisioRoom:
					{
						neededFeatures.Add(new FeatureProjection { Feature = (int)Feature.VisioRoom });
					}
					break;
				default:
					break;
			}

			//matching the needed features
			foreach (var criteriaFeatures in criteria.LocalisationData.LocalisationFeatures)
			{
				//offerid match an offer from search, or a general criteria not corresponding to any offer
				neededFeatures.Add(new FeatureProjection { Feature = criteriaFeatures.FeatureID, Offer = criteriaFeatures.OfferID });
			}

			var wifi_Not_Free = new FeatureProjection { Feature = (int)Feature.Wifi_Not_Free, Offer = (int)FeatureType.General };
			var equalityComparer = new FeatureProjectionEqualityComparer();
			idsToLoad = tempList.Where(loc =>
				{
					foreach (var item in neededFeatures)
					{
						//if we search wifi, it is for not free as well
						if (item.Feature == (int)Feature.Wifi_Free)
						{
							if (!loc.Features.Contains(item, equalityComparer) && !loc.Features.Contains(wifi_Not_Free, equalityComparer))
								return false;
							continue;
						}

						if (!loc.Features.Contains(item, equalityComparer))
							return false;
					}
					return true;
				}).Select(loc => loc.ID).ToList();

			//var db2 = new WorkiDBEntities();
			{
				return _Context.Localisations.Where(loc => idsToLoad.Contains(loc.ID)).ToList();
			}
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

		#endregion
	}
}
