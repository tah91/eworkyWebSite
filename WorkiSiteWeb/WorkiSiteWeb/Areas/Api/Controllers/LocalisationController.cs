using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Rest;
using Worki.Infrastructure.Logging;
using Worki.Services;
using Worki.Web.Helpers;

namespace Worki.Web.Areas.Api.Controllers
{
    public partial class LocalisationController : Controller
    {
        ILocalisationRepository _LocalisationRepository;
        IMemberRepository _MemberRepository;
        ILogger _Logger;
        ISearchService _SearchService;

        public LocalisationController(ILocalisationRepository localisationRepository, IMemberRepository memberRepository, ILogger logger, ISearchService searchService)
        {
            _LocalisationRepository = localisationRepository;
            _MemberRepository = memberRepository;
            _Logger = logger;
            _SearchService = searchService;
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
                                            int offerType = -1, 
                                            string types = null,
                                            string features = null,
                                            int maxCount = 30)
        {
            //validate
            if (string.IsNullOrEmpty(place))
                return new ObjectResult<List<LocalisationJson>>(null, 400, "The \"place\" parameter must be filled");

            //fill from parameter
            var criteria = new SearchCriteria();

            //types
            if (!string.IsNullOrEmpty(types))
            {
                try
                {
                    string[] typesArray = types.Split(',');
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
                    return new ObjectResult<List<LocalisationJson>>(null, 400, "The \"types\" parameter is not correctly filled");
                }
            }

            //features
            if (!string.IsNullOrEmpty(features))
            {
                try
                {
                    var offerId = Localisation.GetFeatureTypeFromOfferType(criteria.LocalisationOffer);
                    string[] featuresArray = features.Split(',');
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
                    criteria.Everything = false;
                }
                catch (Exception)
                {
                    return new ObjectResult<List<LocalisationJson>>(null, 400, "The \"types\" parameter is not correctly filled");
                }
            }

            //place
            criteria.Place = place;
            float lat = 0, lng = 0;
            _SearchService.GeoCode(place, out lat, out lng);
            if (lat == 0 || lng==0)
                return new ObjectResult<List<LocalisationJson>>(null, 404, "The \"place\" can not be geocoded");
            criteria.LocalisationData.Latitude = lat;
            criteria.LocalisationData.Longitude = lng;
            criteria.LocalisationOffer = offerType;

            //search for matching localisations
            var results = _SearchService.FillSearchResults(criteria);

            //take the json
            var neededLocs = (from item in results.Results.Take(maxCount) select item.GetJson(this)).ToList();
            return new ObjectResult<List<LocalisationJson>>(neededLocs);
        }
    }
}
