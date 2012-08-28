using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Rest;
using Worki.Service;
using Worki.Web.Helpers;

namespace Worki.Web.Areas.Api.Controllers
{
    public partial class LocalisationController : Controller
    {
        ILogger _Logger;
        ISearchService _SearchService;
        IGeocodeService _GeocodeService;

        public LocalisationController(ILogger logger,
                                      ISearchService searchService,
                                      IGeocodeService geocodeService)
        {
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
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var localisation = lRepo.Get(id);
            if (localisation == null)
                return new ObjectResult<LocalisationJson>(null, 400, "The id is not present in database");

            var json = localisation.GetJson(this);
            return new ObjectResult<LocalisationJson>(json);
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
        public virtual ActionResult Search(SearchCriteriaApi critariaApi)
        {
            //validate
            if (critariaApi.IsEmpty())
                return new ObjectResult<List<LocalisationJson>>(null, 400, "The \"place or name or latitude/longitude\" parameters must be filled");

            //fill from parameter
            eSearchType searchType = string.IsNullOrEmpty(critariaApi.name) ? eSearchType.ePerOffer : eSearchType.ePerName;
            eOrderBy order = string.IsNullOrEmpty(critariaApi.name) ? (eOrderBy)critariaApi.orderBy : eOrderBy.Rating;
            var criteria = new SearchCriteria { SearchType = searchType, OrderBy = order };

            criteria.Boundary = critariaApi.boundary;

            try
            {
                critariaApi.FillCriteria(ref  criteria);
            }
            catch (Exception ex)
            {
                return new ObjectResult<List<LocalisationJson>>(null, 400, ex.Message);
            }

            //place
            if (!string.IsNullOrEmpty(critariaApi.place))
            {
                criteria.Place = critariaApi.place;
                float lat, lng;
                _GeocodeService.GeoCode(critariaApi.place, out lat, out lng);
                if (lat != 0)
                    critariaApi.latitude = lat;
                if (lng != 0)
                    critariaApi.longitude = lng;
                if (critariaApi.latitude == 0 || critariaApi.longitude == 0)
                    return new ObjectResult<List<LocalisationJson>>(null, 404, "The \"place\" can not be geocoded");
            }

            criteria.LocalisationData.Latitude = critariaApi.latitude;
            criteria.LocalisationData.Longitude = critariaApi.longitude;
            criteria.NorthEastLat = critariaApi.neLat;
            criteria.NorthEastLng = critariaApi.neLng;
            criteria.SouthWestLat = critariaApi.swLat;
            criteria.SouthWestLng = critariaApi.swLng;

            if (!string.IsNullOrEmpty(critariaApi.name))
                criteria.LocalisationData.Name = critariaApi.name;

            //search for matching localisations
            var results = _SearchService.FillSearchResults(criteria);

            //take the json
            var list = results.List;

            var neededLocs = (from item in list.Skip((critariaApi.page - 1) * MiscHelpers.Constants.PageSize).Take(MiscHelpers.ApiConstants.TakeCount) select item.GetJson(this, criteria));
            var neededLocList = neededLocs.ToList();

            double centerLat, centerLng;
            if (criteria.HasBounds())
            {
                centerLat = (criteria.NorthEastLat + criteria.SouthWestLat) / 2;
                centerLng = (criteria.NorthEastLng + criteria.SouthWestLng) / 2;
            }
            else
            {
                centerLat = criteria.LocalisationData.Latitude;
                centerLng = criteria.LocalisationData.Longitude;
            }

            var toRet = new LocalisationsContainer
            {
                list = neededLocList,
                maxCount = list.Count,
                latitude = centerLat,
                longitude = centerLng
            };

            return new ObjectResult<LocalisationsContainer>(toRet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Comment(int id,string token, Comment com)
        {
            if (ModelState.IsValid)
            {
                var context = ModelFactory.GetUnitOfWork();
                var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
                var member = mRepo.GetMemberFromToken(token);
                var localisation = lRepo.Get(id);
                try
                {
                    var error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
                    com.Localisation = localisation;
                    com.PostUserID = member.MemberId;
                    com.Date = System.DateTime.UtcNow;
                    com.RatingDispo = com.Rating;
                    com.RatingPrice = com.Rating;
                    com.RatingWifi = com.Rating;
                    com.RatingWelcome = com.Rating;
                    com.Validate(ref  error);

                    localisation.Comments.Add(com);

                    context.Commit();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    return new ObjectResult<AuthJson>(null, 400, dbEx.GetErrors());
                }
                catch (Exception ex)
                {
                    _Logger.Error("Comment", ex);
                    context.Complete();
                    return new ObjectResult<LocalisationJson>(null, 400, ex.Message);
                }

                var newContext = ModelFactory.GetUnitOfWork();
                lRepo = ModelFactory.GetRepository<ILocalisationRepository>(newContext);
                localisation = lRepo.Get(id);
                return new ObjectResult<LocalisationJson>(localisation.GetJson(this), 200, "ok");
            }
            else
            {
                return new ObjectResult<LocalisationJson>(null, 400, ModelState.GetErrors());
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult AddToFavorites(int id, string token)
        {
            if (ModelState.IsValid)
            {
                var context = ModelFactory.GetUnitOfWork();
                var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
                var member = mRepo.GetMemberFromToken(token);
                var localisation = lRepo.Get(id);
                try
                {
                    if (member.FavoriteLocalisations.Count(fl => fl.LocalisationId == id) == 0)
                    {
                        member.FavoriteLocalisations.Add(new FavoriteLocalisation { LocalisationId = id });
                        context.Commit();
                    }
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    return new ObjectResult<AuthJson>(null, 400, dbEx.GetErrors());
                }
                catch (Exception ex)
                {
                    _Logger.Error("AddToFavorites", ex);
                    context.Complete();
                    return new ObjectResult<LocalisationJson>(null, 400, ex.Message);
                }
                return new ObjectResult<LocalisationJson>(null, 200, "Ok");
            }
            else
            {
                return new ObjectResult<LocalisationJson>(null, 400, ModelState.GetErrors());
            }
        }

        /// <summary>
        /// Get action method to get localisation details
        /// </summary>
        /// <param name="id">localisation id</param>
        /// <returns>an object containing localisatiojn details, comments, and fans</returns>
        public virtual ActionResult GetFavorites(string token)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            try
            {
                var member = mRepo.GetMemberFromToken(token);

                var toRet = new LocalisationsContainer
                {
                    list = member.FavoriteLocalisations.Select(l => l.Localisation.GetJson(this)).ToList(),
                    maxCount = member.FavoriteLocalisations.Count,
                    latitude = 0,
                    longitude = 0
                };

                return new ObjectResult<LocalisationsContainer>(toRet);
            }
            catch (Exception ex)
            {
                _Logger.Error("GetFavorites", ex);
                context.Complete();
                return new ObjectResult<LocalisationsContainer>(null, 400, ex.Message);
            }           
        }
    }
}
