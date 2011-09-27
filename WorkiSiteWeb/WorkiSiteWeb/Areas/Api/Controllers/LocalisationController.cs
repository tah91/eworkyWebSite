using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure.Logging;
using Worki.Rest;
using Worki.Service;
using Worki.Web.Helpers;

namespace Worki.Web.Areas.Api.Controllers
{
    public partial class LocalisationController : Controller
    {
        ILocalisationRepository _LocalisationRepository;
        IMemberRepository _MemberRepository;
        ILogger _Logger;
        ISearchService _SearchService;
        IGeocodeService _GeocodeService;

        public LocalisationController(ILocalisationRepository localisationRepository, IMemberRepository memberRepository, ILogger logger, ISearchService searchService, IGeocodeService geocodeService)
        {
            _LocalisationRepository = localisationRepository;
            _MemberRepository = memberRepository;
            _Logger = logger;
            _SearchService = searchService;
            _GeocodeService = geocodeService;
        }

        /// <summary>
        /// Get action method to get localisation details
        /// </summary>
        /// <param name="id">localisation id</param>
        /// <returns>an object containing localisatiojn details, comments, and fans</returns>
        public virtual ActionResult Details(int id)
        {
            var localisation = _LocalisationRepository.Get(id);
            if (localisation == null)
                return new ObjectResult<LocalisationJson>(null, 400, "The id is not present in database");

            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
            var json = localisation.GetJson(this);
            return new ObjectResult<LocalisationJson>(json);
        }

        enum ApiFeatures
        {
            Wifi,
            Cofee,
            Resto,
            Parking,
            Handicap
        }

        static char[] _arrayTrim = { '[', ']' };

		void FillCriteria(ref SearchCriteria criteria, string types, string features, int offerType)
		{
			//types
			if (!string.IsNullOrEmpty(types))
			{
				try
				{
					string[] typesArray = types.Trim(_arrayTrim).Split(',');
					foreach (var item in typesArray)
					{
						var intType = (LocalisationType)Int32.Parse(item);
						switch (intType)
						{
							case LocalisationType.SpotWifi:
								criteria.SpotWifi = true;
								break;
							case LocalisationType.CoffeeResto:
								criteria.CoffeeResto = true;
								break;
							case LocalisationType.Biblio:
								criteria.Biblio = true;
								break;
							case LocalisationType.PublicSpace:
								criteria.PublicSpace = true;
								break;
							case LocalisationType.TravelerSpace:
								criteria.TravelerSpace = true;
								break;
							case LocalisationType.Hotel:
								criteria.Hotel = true;
								break;
							case LocalisationType.Telecentre:
								criteria.Telecentre = true;
								break;
							case LocalisationType.BuisnessCenter:
								criteria.BuisnessCenter = true;
								break;
							case LocalisationType.CoworkingSpace:
								criteria.CoworkingSpace = true;
								break;
							case LocalisationType.WorkingHotel:
								criteria.WorkingHotel = true;
								break;
							case LocalisationType.PrivateArea:
								criteria.PrivateArea = true;
								break;
							default:
								break;
						}
					}
					criteria.Everything = false;
				}
				catch (Exception)
				{
					throw new Exception("The \"types\" parameter is not correctly filled");
				}
			}

			//features
			if (!string.IsNullOrEmpty(features))
			{
				try
				{
					var offerId = Localisation.GetFeatureTypeFromOfferType(criteria.LocalisationOffer);
					string[] featuresArray = features.Trim(_arrayTrim).Split(',');
					foreach (var item in featuresArray)
					{
						var intType = (ApiFeatures)Int32.Parse(item);
						switch (intType)
						{
							case ApiFeatures.Wifi:
								criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Wifi_Free, OfferID = offerId });
								break;
							case ApiFeatures.Cofee:
								criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Coffee, OfferID = offerId });
								break;
							case ApiFeatures.Resto:
								criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Restauration, OfferID = offerId });
								break;
							case ApiFeatures.Parking:
								criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Parking, OfferID = offerId });
								break;
							case ApiFeatures.Handicap:
								criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Handicap, OfferID = offerId });
								break;
							default:
								break;
						}
					}
				}
				catch (Exception)
				{
					throw new Exception("The \"features\" parameter is not correctly filled");
				}
			}

			criteria.LocalisationOffer = offerType;
		}

        /// <summary>
        /// get action result to search for localisation, for given criteria
        /// </summary>
        /// <param name="place">an address near where to find localisations</param>
        /// <param name="offerType"></param>
        /// <param name="types"></param>
        /// <param name="features"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public virtual ActionResult Search( string place,
											float latitude = 0,
											float longitude = 0,
                                            int offerType = -1, 
                                            string types = null,
                                            string features = null,
                                            int maxCount = 30)
        {
            //validate
            if (string.IsNullOrEmpty(place) && (latitude == 0 || longitude == 0))
				return new ObjectResult<List<LocalisationJson>>(null, 400, "The \"place or latitude/longitude\" parameters must be filled");

            //fill from parameter
            var criteria = new SearchCriteria();

			try
			{
				FillCriteria(ref  criteria, types, features, offerType);
			}
			catch (Exception ex)
			{
				return new ObjectResult<List<LocalisationJson>>(null, 400, ex.Message);
			}

			//place
			if (latitude == 0 || longitude == 0)
			{
				criteria.Place = place;
                _GeocodeService.GeoCode(place, out latitude, out longitude);
				if (latitude == 0 || longitude == 0)
					return new ObjectResult<List<LocalisationJson>>(null, 404, "The \"place\" can not be geocoded");
			}
			criteria.LocalisationData.Latitude = latitude;
			criteria.LocalisationData.Longitude = longitude;

            //search for matching localisations
            var results = _SearchService.FillSearchResults(criteria);

            //take the json
            var neededLocs = (from item in results.Results.Take(maxCount) select item.GetJson(this)).ToList();
            return new ObjectResult<List<LocalisationJson>>(neededLocs);
        }
    }
}
