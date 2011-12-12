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
        SearchCriteria GetCriteria(HttpRequestBase parameters);
		RouteValueDictionary GetRVD(SearchCriteria criteria, int page = 1);
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
        public SearchCriteria GetCriteria(HttpRequestBase parameters)
        {
            var criteria = new SearchCriteria();
            var value = string.Empty;

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Place, ref value))
                criteria.Place = value;

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.PlaceName, ref value))
                criteria.LocalisationData.Name = value;

            int intVal;
            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Order, ref value) && int.TryParse(value, out intVal))
                criteria.OrderBy = (eOrderBy)intVal;

            float floatVal;
            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Latitude, ref value) && float.TryParse(value, out floatVal))
                criteria.LocalisationData.Latitude = floatVal;
            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Longitude, ref value) && float.TryParse(value, out floatVal))
                criteria.LocalisationData.Longitude = floatVal;

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.OfferType, ref value) && int.TryParse(value, out intVal))
                criteria.OfferData.Type = intVal;

            if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.All, ref value) && string.Compare(value, Boolean.TrueString, true) == 0)
                criteria.Everything = true;
            else
            {
                criteria.Everything = false;
                if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.SpotWifi, ref value))
                    criteria.SpotWifi = true;
                if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.CoffeeResto, ref value))
                    criteria.CoffeeResto = true;
                if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Biblio, ref value))
                    criteria.Biblio = true;
                if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.PublicSpace, ref value))
                    criteria.PublicSpace = true;
                if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.TravelerSpace, ref value))
                    criteria.TravelerSpace = true;
                if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Hotel, ref value))
                    criteria.Hotel = true;
                if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.Telecentre, ref value))
                    criteria.Telecentre = true;
                if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.BuisnessCenter, ref value))
                    criteria.BuisnessCenter = true;
                if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.CoworkingSpace, ref value))
                    criteria.CoworkingSpace = true;
                if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.WorkingHotel, ref value))
                    criteria.WorkingHotel = true;
                if (MiscHelpers.GetRequestValue(parameters, MiscHelpers.SeoConstants.PrivateArea, ref value))
                    criteria.PrivateArea = true;
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
            return criteria;
        }

		/// <summary>
		/// private method to create route data from a SearchCriteria object
		/// used to pass search criteria in url
		/// </summary>
		/// <returns>the created RouteValueDictionary</returns>
		public RouteValueDictionary GetRVD(SearchCriteria criteria, int page = 1)
		{
			var rvd = new RouteValueDictionary();
			rvd[MiscHelpers.SeoConstants.Page] = page;
			rvd[MiscHelpers.SeoConstants.Place] = criteria.Place;
			rvd[MiscHelpers.SeoConstants.OfferType] = criteria.OfferData.Type;
            rvd[MiscHelpers.SeoConstants.Latitude] = criteria.LocalisationData.Latitude;
            rvd[MiscHelpers.SeoConstants.Longitude] = criteria.LocalisationData.Longitude;
            rvd[MiscHelpers.SeoConstants.PlaceName] = criteria.LocalisationData.Name;

            rvd[MiscHelpers.SeoConstants.Order] = (int)criteria.OrderBy;

			if (!criteria.Everything)
			{
                rvd[MiscHelpers.SeoConstants.All] = false;
				if (criteria.SpotWifi)
					rvd[MiscHelpers.SeoConstants.SpotWifi] = true;
				if (criteria.CoffeeResto)
					rvd[MiscHelpers.SeoConstants.CoffeeResto] = true;
				if (criteria.Biblio)
					rvd[MiscHelpers.SeoConstants.Biblio] = true;
				if (criteria.PublicSpace)
					rvd[MiscHelpers.SeoConstants.PublicSpace] = true;
				if (criteria.TravelerSpace)
					rvd[MiscHelpers.SeoConstants.TravelerSpace] = true;
				if (criteria.Hotel)
					rvd[MiscHelpers.SeoConstants.Hotel] = true;
				if (criteria.Telecentre)
					rvd[MiscHelpers.SeoConstants.Telecentre] = true;
				if (criteria.BuisnessCenter)
					rvd[MiscHelpers.SeoConstants.BuisnessCenter] = true;
				if (criteria.CoworkingSpace)
					rvd[MiscHelpers.SeoConstants.CoworkingSpace] = true;
				if (criteria.WorkingHotel)
					rvd[MiscHelpers.SeoConstants.WorkingHotel] = true;
				if (criteria.PrivateArea)
					rvd[MiscHelpers.SeoConstants.PrivateArea] = true;
			}
			else
				rvd[MiscHelpers.SeoConstants.All] = true;

			foreach (var neededFeature in criteria.LocalisationData.LocalisationFeatures)
			{
				var display = FeatureHelper.FeatureToString(neededFeature.FeatureID, FeatureHelper.LocalisationPrefix);
				rvd[display] = true;
			}

			foreach (var offerFeature in criteria.OfferData.OfferFeatures)
			{
				var display = FeatureHelper.FeatureToString(offerFeature.FeatureId, FeatureHelper.OfferPrefix);
				rvd[display] = true;
			}
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