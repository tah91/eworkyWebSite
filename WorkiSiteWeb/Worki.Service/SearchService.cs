using System;
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
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var results = lRepo.FindByCriteria(criteriaViewModel.Criteria);

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
        }

		#endregion

		public SearchService(ILogger logger,IGeocodeService geocodeService)
		{
            _Logger = logger;
			_GeocodeService = geocodeService;
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
            var value = string.Empty;

            if (MiscHelpers.GetRequestValue(parameters, "lieu", ref value))
                criteria.Place = value;

			if (criteria.LocalisationData.Latitude == 0 && criteria.LocalisationData.Longitude == 0)
			{
				float lat = 0, lng = 0;
				_GeocodeService.GeoCode(criteria.Place, out lat, out lng);
				criteria.LocalisationData.Latitude = lat;
				criteria.LocalisationData.Longitude = lng;
			}

            if (MiscHelpers.GetRequestValue(parameters, "offer-type", ref value))
                criteria.LocalisationOffer = int.Parse(value, CultureInfo.InvariantCulture);

            if (MiscHelpers.GetRequestValue(parameters, "tout", ref value) && string.Compare(value, Boolean.TrueString, true) == 0)
                criteria.Everything = true;
            else
            {
                criteria.Everything = false;
                if (MiscHelpers.GetRequestValue(parameters, "spot-wifi", ref value))
                    criteria.SpotWifi = true;
                if (MiscHelpers.GetRequestValue(parameters, "cafe", ref value))
                    criteria.CoffeeResto = true;
                if (MiscHelpers.GetRequestValue(parameters, "biblio", ref value))
                    criteria.Biblio = true;
                if (MiscHelpers.GetRequestValue(parameters, "public", ref value))
                    criteria.PublicSpace = true;
                if (MiscHelpers.GetRequestValue(parameters, "voyageur", ref value))
                    criteria.TravelerSpace = true;
                if (MiscHelpers.GetRequestValue(parameters, "hotel", ref value))
                    criteria.Hotel = true;
                if (MiscHelpers.GetRequestValue(parameters, "telecentre", ref value))
                    criteria.Telecentre = true;
                if (MiscHelpers.GetRequestValue(parameters, "centre-affaire", ref value))
                    criteria.BuisnessCenter = true;
                if (MiscHelpers.GetRequestValue(parameters, "coworking", ref value))
                    criteria.CoworkingSpace = true;
                if (MiscHelpers.GetRequestValue(parameters, "entreprise", ref value))
                    criteria.WorkingHotel = true;
                if (MiscHelpers.GetRequestValue(parameters, "prive", ref value))
                    criteria.PrivateArea = true;
            }

            var keys = MiscHelpers.GetFeatureIds(parameters.Params.AllKeys.ToList());
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
				var display = MiscHelpers.FeatureToString(neededFeature.FeatureID);
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