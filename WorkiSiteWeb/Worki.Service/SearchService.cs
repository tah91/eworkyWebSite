﻿using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Worki.Data.Models;
using Worki.Data.Repository;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Repository;
using System.Web.Routing;
using System.Web;
using System.Net;
using Worki.Infrastructure.Logging;
using System.Collections.Generic;

namespace Worki.Service
{
	public interface ISearchService
	{
        SearchCriteriaFormViewModel FillSearchResults(SearchCriteria parameters);
        SearchCriteria GetCriteria(HttpRequestBase parameters, int page = 1);
		RouteValueDictionary GetRVD(SearchCriteria criteria);
        SearchSingleResultViewModel GetSingleResult(HttpRequestBase parameters, int index);
		void ValidateLocalisation(Localisation toValidate, ref string error);
	}

    public class SearchService : ISearchService
	{
		#region private

        ILogger _Logger;
		IGeocodeService _GeocodeService;

        /// <summary>
        /// Fill search results from criteria
        /// 1 search matching localisations in repository
        /// 2 compute distance between place to search and search result for each result
        /// 3 push result in session
        /// </summary>
        void FillResults(SearchCriteriaFormViewModel criteriaViewModel)
        {
			float lat = 0, lng = 0;
			_GeocodeService.GeoCode(criteriaViewModel.Criteria.Place, out lat, out lng);

			if (criteriaViewModel.Criteria.LocalisationData.Latitude == 0 && criteriaViewModel.Criteria.LocalisationData.Longitude == 0)
			{
				criteriaViewModel.Criteria.LocalisationData.Latitude = lat;
				criteriaViewModel.Criteria.LocalisationData.Longitude = lng;
			}

			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var results = lRepo.FindByCriteria(criteriaViewModel.Criteria);

			var nameSimiltude = new Dictionary<int, int>();

            foreach (var item in results)
            {
                var distance = MiscHelpers.GetDistanceBetween(criteriaViewModel.Criteria.LocalisationData.Latitude, criteriaViewModel.Criteria.LocalisationData.Longitude, item.Latitude, item.Longitude);
                criteriaViewModel.DistanceFromLocalisation.Add(item.ID, distance);

				nameSimiltude[item.ID] = MiscHelpers.GetSimilitude(criteriaViewModel.Criteria.LocalisationData.Name, item.Name);
            }

            switch (criteriaViewModel.Criteria.OrderBy)
            {
				//already ordered by rating
				case eOrderBy.Rating:
					criteriaViewModel.List = results.OrderByDescending(loc => nameSimiltude[loc.ID]).ToList();
					break;
                case eOrderBy.Distance:
					criteriaViewModel.List = results.OrderBy(loc=>criteriaViewModel.DistanceFromLocalisation[loc.ID])
													.OrderByDescending(loc => nameSimiltude[loc.ID]).ToList();
                    break;
                default:
                    break;
            }

            criteriaViewModel.FillPageInfo();
        }

		#endregion

		public SearchService(ILogger logger,IGeocodeService geocodeService)
		{
            _Logger = logger;
			_GeocodeService = geocodeService;
		}

        public const string DefaultSearchPlace = "Paris";

		/// <summary>
		/// get a SearchCriteriaFormViewModel containing the criteria and the results of a search
		/// if results in cache
		///     get it
		/// else
		///     build searchcriteria from url
		///     with this, search matchings localisations in repository
		///     fill the session store with computed results
		/// and returns the results
		/// </summary>
		/// <param name="session">session to look up for results</param>
		/// <param name="parameters">parameters from which to build result of not in session</param>
		/// <returns>a object containing the criteria and the results of a search</returns>
		public SearchCriteriaFormViewModel FillSearchResults(SearchCriteria criteria)
		{
            var place = string.IsNullOrEmpty(criteria.Place) ? "" : criteria.Place.ToLower();
            if (MiscHelpers.SeoConstants.Places.ContainsKey(place))
            {
                var coor = MiscHelpers.SeoConstants.Places[place];
                criteria.LocalisationData.Latitude = coor.Latitude;
                criteria.LocalisationData.Longitude = coor.Longitude;
            }
            var criteriaViewModel = new SearchCriteriaFormViewModel(criteria, true);
			FillResults(criteriaViewModel);
			return criteriaViewModel;
		}

		/// <summary>
		/// Get single result for a given index, within search results
		/// </summary>
		/// <param name="session">session to look up for results</param>
		/// <param name="parameters">parameters from which to build result of not in session</param>
		/// <param name="index">index of the result item</param>
		/// <returns>result item</returns>
        public SearchSingleResultViewModel GetSingleResult(HttpRequestBase parameters, int index)
		{
            var criteria = GetCriteria(parameters);
            var criteriaViewModel = FillSearchResults(criteria);

			if (index < 0 || index >= criteriaViewModel.List.Count)
				return null;

			//refresh item before returning it, don't take the one from session
			//var fromDb = _LocalisationRepository.Get(criteriaViewModel.Results[index].ID);
			//criteriaViewModel.Results[index] = fromDb;

			var detailModel = criteriaViewModel.GetSingleResult(index);
			return detailModel;
		}

		/// <summary>
		/// private method to create a SearchCriteria object from route data
		/// used to create search criteria from an url
		/// </summary>
		/// <returns>the created SearchCriteria</returns>
        public SearchCriteria GetCriteria(HttpRequestBase parameters, int page = 1)
        {
            var criteria = new SearchCriteria();
            var value = string.Empty;

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Page, ref value))
                criteria.Page = int.Parse(value);
            else
                criteria.Page = page;

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Place, ref value))
                criteria.Place = value;

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.PlaceName, ref value))
                criteria.LocalisationData.Name = value;

            int intVal;
            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Order, ref value) && int.TryParse(value, out intVal))
                criteria.OrderBy = (eOrderBy)intVal;

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Search, ref value) && int.TryParse(value, out intVal))
                criteria.SearchType = (eSearchType)intVal;

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.View, ref value) && int.TryParse(value, out intVal))
				criteria.ResultView = (eResultView)intVal;

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Boundary, ref value))
                criteria.Boundary = float.Parse(value);
            else
                criteria.Boundary = 50;

            float floatVal;
            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Latitude, ref value) && float.TryParse(value, out floatVal))
                criteria.LocalisationData.Latitude = floatVal;
            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Longitude, ref value) && float.TryParse(value, out floatVal))
                criteria.LocalisationData.Longitude = floatVal;

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.SearchOfferType, ref value))
                criteria.OfferData.Type = Localisation.GetOfferTypeFromSeoString(value);

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.GlobalType, ref value) && int.TryParse(value, out intVal))
                criteria.GlobalType = (eGlobalType)intVal;

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.LocalisationType, ref value))
            {
                var localisationTypes = value.Split(',');
                foreach (var item in localisationTypes)
                {
                    if (item == MiscHelpers.SeoConstants.Telecentre)
                        criteria.Telecentre = true;
                    if (item == MiscHelpers.SeoConstants.BuisnessCenter)
                        criteria.BuisnessCenter = true;
                    if (item == MiscHelpers.SeoConstants.CoworkingSpace)
                        criteria.CoworkingSpace = true;
					if (item == MiscHelpers.SeoConstants.SharedOffice)
						criteria.SharedOffice = true;
                    if (item == MiscHelpers.SeoConstants.SpotWifi)
                        criteria.SpotWifi = true;
                    if (item == MiscHelpers.SeoConstants.CoffeeResto)
                        criteria.CoffeeResto = true;
                    if (item == MiscHelpers.SeoConstants.Biblio)
                        criteria.Biblio = true;
                    if (item == MiscHelpers.SeoConstants.TravelerSpace)
                        criteria.TravelerSpace = true;
                    if (item == MiscHelpers.SeoConstants.WorkingHotel)
                        criteria.WorkingHotel = true;
                    if (item == MiscHelpers.SeoConstants.PrivateArea)
                        criteria.PrivateArea = true;
                    if (item == MiscHelpers.SeoConstants.PublicSpace)
                        criteria.PublicSpace = true;
                    if (item == MiscHelpers.SeoConstants.Hotel)
                        criteria.Hotel = true;
                }
            }

            var locKeys = FeatureHelper.GetFeatureIds(parameters.Params.AllKeys.ToList(), FeatureHelper.LocalisationPrefix);
            criteria.LocalisationData.LocalisationFeatures.Clear();
            foreach (var key in locKeys)
            {
                criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = key });
            }

            var offerKeys = FeatureHelper.GetFeatureIds(parameters.Params.AllKeys.ToList(), FeatureHelper.OfferPrefix);
            criteria.OfferData.OfferFeatures.Clear();
            foreach (var key in offerKeys)
            {
                criteria.OfferData.OfferFeatures.Add(new OfferFeature { FeatureId = key });
            }

            var countries = "";
            var types = "";
            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.WidgetConstants.Country, ref value))
                countries = value;
            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.WidgetConstants.Type, ref value))
                types = value;

            if (!string.IsNullOrEmpty(countries) || !string.IsNullOrEmpty(types))
            {
                criteria.PreFilter = new SearchCriteria.Filter { Countries = countries, Types = types };
            }

            return criteria;
        }

		/// <summary>
		/// private method to create route data from a SearchCriteria object
		/// used to pass search criteria in url
		/// </summary>
		/// <returns>the created RouteValueDictionary</returns>
		public RouteValueDictionary GetRVD(SearchCriteria criteria)
		{
            var dict = criteria.GetDictionnary();
            var rvd = new RouteValueDictionary(dict);

			return rvd;
		}

		/// <summary>
		/// Validate a localisation against database state, throws exception if not valide
		/// </summary>
		/// <param name="toValidate">localisation to validate</param>
		/// <param name="error">error to fill</param>
		public void ValidateLocalisation(Localisation toValidate, ref string error)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			if (toValidate == null)
			{
				error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
				throw new Exception(error);
			}

			var similarLoc = (from loc
								  in lRepo.FindSimilarLocalisation((float)toValidate.Latitude, (float)toValidate.Longitude)
							  where string.Compare(loc.Name, toValidate.Name, StringComparison.InvariantCultureIgnoreCase) == 0
							  select loc).Count();
			if (similarLoc > 0)
			{
				error = Worki.Resources.Validation.ValidationString.DuplicateName;
				throw new Exception(error);
			}
		}        
	}
}