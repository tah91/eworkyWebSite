using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Worki.Data.Models;
using Worki.Data.Repository;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Repository;
using System.Web.Routing;
using Worki.Service;
using System.Web;
using System.Net;
using Worki.Infrastructure.Logging;

namespace Worki.Services
{
	public interface ISearchService
	{
        SearchCriteriaFormViewModel GetCurrentSearchCriteria(HttpRequestBase parameters);
        SearchCriteria GetCriteria(HttpRequestBase parameters);
		RouteValueDictionary GetRVD(SearchCriteria criteria, int page = 1);
        SearchSingleResultViewModel GetSingleResult(HttpRequestBase parameters, int index);
		void ValidateLocalisation(Localisation toValidate, ref string error);
	}

    public class SearchService : ISearchService
	{
		#region private

		ILocalisationRepository _LocalisationRepository;
        ILogger _Logger;

        /// <summary>
        /// Get value corresponding to an url param key
        /// either in path or query string
        /// </summary>
        /// <param name="request">request to parse</param>
        /// <param name="key">paameter key</param>
        /// <returns>corresponding value</returns>
        string GetRequestValue(HttpRequestBase request, string key)
        {
            if (request.Params[key] != null)
                return request.Params[key];
            else if (request.RequestContext.RouteData.Values[key] != null)
                return request.RequestContext.RouteData.Values[key] as string;
            else return null;
        }

        /// <summary>
        /// Geocode address via google api
        /// </summary>
        /// <param name="address">address to geocode</param>
        /// <param name="lat">place latitude</param>
        /// <param name="lg">place longitude</param>
        void GeoCode(string address, out float lat, out float lg)
        {
            lat = 0;
            lg = 0;
            if (string.IsNullOrEmpty(address))
                return;
            string strKey = "ABQIAAAAdG7nmLSCLLMyUXmPZDmWpBRUyfMLYGuEEhDrWo4mEQ8GYiYo8BTxOAimWDrLvSiruY1GasDiBDuCWg";
            string sPath = "http://maps.google.com/maps/geo?q=" + address + "&output=csv&key=" + strKey;
            string latStr = null, lgStr = null;
            using (var client = new WebClient())
            {
                try
                {
                    string textString = client.DownloadString(sPath);
                    string[] eResult = textString.Split(',');
                    _Logger.Info("geocoded");
                    latStr = eResult.GetValue(2).ToString();
                    lgStr = eResult.GetValue(3).ToString();
                    lat = float.Parse(latStr, CultureInfo.InvariantCulture.NumberFormat);
                    lg = float.Parse(lgStr, CultureInfo.InvariantCulture.NumberFormat);
                }
                catch (WebException ex)
                {
                    _Logger.Error(ex.Message);
                }
            }
        }

        /// <summary>
        /// Fill search results from criteria
        /// 1 search matching localisations in repository
        /// 2 compute distance between place to search and search result for each result
        /// 3 push result in session
        /// </summary>
        void FillResults(SearchCriteriaFormViewModel criteriaViewModel)
        {
            var results = _LocalisationRepository.FindByCriteria(criteriaViewModel.Criteria);//.ToList();

            foreach (var item in results)
            {
                var distance = MiscHelpers.GetDistanceBetween(criteriaViewModel.Criteria.LocalisationData.Latitude, criteriaViewModel.Criteria.LocalisationData.Longitude, item.Latitude, item.Longitude);
                criteriaViewModel.DistanceFromLocalisation.Add(item.ID, distance);
            }

            criteriaViewModel.Results = (from item
                                            in results
                                         orderby criteriaViewModel.DistanceFromLocalisation[item.ID]
                                         select item).ToList();

            criteriaViewModel.FillPageInfo();
            //session[CriteriaViewModelKey] = criteriaViewModel;
        }

		#endregion

		public SearchService(ILocalisationRepository localisationRepository, ILogger logger)
		{
			_LocalisationRepository = localisationRepository;
            _Logger = logger;
		}

		public const string CriteriaViewModelKey = "CriteriaViewModelKey";

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
        public SearchCriteriaFormViewModel GetCurrentSearchCriteria(HttpRequestBase parameters)
		{
			SearchCriteriaFormViewModel criteriaViewModel = null;// session[CriteriaViewModelKey] as SearchCriteriaFormViewModel;
			if (criteriaViewModel == null)
			{
				var criteria = GetCriteria(parameters);
				criteriaViewModel = new SearchCriteriaFormViewModel(criteria, true);
				FillResults(criteriaViewModel);
			}
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
			var criteriaViewModel = GetCurrentSearchCriteria(parameters);

			if (index < 0 || index >= criteriaViewModel.Results.Count)
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

            if (GetRequestValue(parameters, "lieu") != null)
                criteria.Place = GetRequestValue(parameters, "lieu");

            float lat = 0, lng = 0;
            GeoCode(criteria.Place, out lat, out lng);
            criteria.LocalisationData.Latitude = lat;
            criteria.LocalisationData.Longitude = lng;

            if (GetRequestValue(parameters, "offer-type") != null)
                criteria.LocalisationOffer = int.Parse(GetRequestValue(parameters, "offer-type"), CultureInfo.InvariantCulture);

            var alltypes = GetRequestValue(parameters, "tout");
            if (!string.IsNullOrEmpty(alltypes) && string.Compare(alltypes, Boolean.TrueString, true) == 0)
                criteria.Everything = true;
            else
            {
                criteria.Everything = false;
                if (GetRequestValue(parameters, "spot-wifi") != null)
                    criteria.SpotWifi = true;
                if (GetRequestValue(parameters, "cafe") != null)
                    criteria.CoffeeResto = true;
                if (GetRequestValue(parameters, "biblio") != null)
                    criteria.Biblio = true;
                if (GetRequestValue(parameters, "public") != null)
                    criteria.PublicSpace = true;
                if (GetRequestValue(parameters, "voyageur") != null)
                    criteria.TravelerSpace = true;
                if (GetRequestValue(parameters, "hotel") != null)
                    criteria.Hotel = true;
                if (GetRequestValue(parameters, "telecentre") != null)
                    criteria.Telecentre = true;
                if (GetRequestValue(parameters, "centre-affaire") != null)
                    criteria.BuisnessCenter = true;
                if (GetRequestValue(parameters, "coworking") != null)
                    criteria.CoworkingSpace = true;
                if (GetRequestValue(parameters, "entreprise") != null)
                    criteria.WorkingHotel = true;
                if (GetRequestValue(parameters, "prive") != null)
                    criteria.PrivateArea = true;
            }

            var keys = Localisation.GetFeatureIds(parameters.Params.AllKeys.ToList());
            criteria.LocalisationData.LocalisationFeatures.Clear();
            var offerId = Localisation.GetFeatureTypeFromOfferType(criteria.LocalisationOffer);
            foreach (var key in keys)
            {
                criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = key, OfferID = offerId });
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
			rvd["page"] = page;
			rvd["lieu"] = criteria.Place;
			rvd["offer-type"] = criteria.LocalisationOffer;

			if (!criteria.Everything)
			{
                rvd["tout"] = false;
				if (criteria.SpotWifi)
					rvd["spot-wifi"] = true;
				if (criteria.CoffeeResto)
					rvd["cafe"] = true;
				if (criteria.Biblio)
					rvd["biblio"] = true;
				if (criteria.PublicSpace)
					rvd["public"] = true;
				if (criteria.TravelerSpace)
					rvd["voyageur"] = true;
				if (criteria.Hotel)
					rvd["hotel"] = true;
				if (criteria.Telecentre)
					rvd["telecentre"] = true;
				if (criteria.BuisnessCenter)
					rvd["centre-affaire"] = true;
				if (criteria.CoworkingSpace)
					rvd["coworking"] = true;
				if (criteria.WorkingHotel)
					rvd["entreprise"] = true;
				if (criteria.PrivateArea)
					rvd["prive"] = true;
			}
			else
				rvd["tout"] = true;

			foreach (var neededFeature in criteria.LocalisationData.LocalisationFeatures)
			{
				var display = Localisation.FeatureToString(neededFeature.FeatureID);
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
			if (toValidate == null)
			{
				error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
				throw new Exception(error);
			}

			var similarLoc = (from loc
								  in _LocalisationRepository.FindSimilarLocalisation((float)toValidate.Latitude, (float)toValidate.Longitude)
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