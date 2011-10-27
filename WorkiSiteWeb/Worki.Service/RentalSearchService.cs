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
	public interface IRentalSearchService
	{
        void FillSearchResults(ref RentalSearchCriteria criteria);
        RentalSearchCriteria GetCriteria(HttpRequestBase parameters);
        RouteValueDictionary GetRVD(RentalSearchCriteria criteria, int page = 1);
        RentalSearchSingleResultViewModel GetSingleResult(HttpRequestBase parameters, int index);
	}

    public class RentalSearchService : IRentalSearchService
	{
		#region private

        ILogger _Logger;
		IGeocodeService _GeocodeService;

		#endregion

        public RentalSearchService(ILogger logger, IGeocodeService geocodeService)
		{
            _Logger = logger;
			_GeocodeService = geocodeService;
		}

        const string _DateFormat = "dd/MM/yy";

        /// <summary>
        /// private method to create a SearchCriteria object from route data
        /// used to create search criteria from an url
        /// </summary>
        /// <returns>the created SearchCriteria</returns>
        public RentalSearchCriteria GetCriteria(HttpRequestBase parameters)
        {
            var criteria = new RentalSearchCriteria();
            var value = string.Empty;

            if (MiscHelpers.GetRequestValue(parameters, "prix-min", ref value))
                criteria.MinRate = int.Parse(value);
            if (MiscHelpers.GetRequestValue(parameters, "prix-max", ref value))
                criteria.MaxRate = int.Parse(value);
            if (MiscHelpers.GetRequestValue(parameters, "surf-min", ref value))
                criteria.MinSurface = int.Parse(value);
            if (MiscHelpers.GetRequestValue(parameters, "surf-max", ref value))
                criteria.MaxSurface = int.Parse(value);
            if (MiscHelpers.GetRequestValue(parameters, "type", ref value))
                criteria.RentalData.Type = int.Parse(value);
            if (MiscHelpers.GetRequestValue(parameters, "lease-type", ref value))
                criteria.RentalData.LeaseType = int.Parse(value);

            if (MiscHelpers.GetRequestValue(parameters, "places", ref value))
                criteria.Place = value;

            if (MiscHelpers.GetRequestValue(parameters, "avail", ref value))
            {
                criteria.RentalData.AvailableDate = DateTime.ParseExact(value, _DateFormat, CultureInfo.InvariantCulture);
            }

            if (MiscHelpers.GetRequestValue(parameters, "avail-now", ref value))
                criteria.RentalData.AvailableNow = true;
            else
                criteria.RentalData.AvailableNow = false;

            var keys = FeatureHelper.GetFeatureIds(parameters.Params.AllKeys.ToList(), FeatureHelper.LocalisationPrefix);
            criteria.RentalData.RentalFeatures.Clear();
            foreach (var key in keys)
            {
                criteria.RentalData.RentalFeatures.Add(new RentalFeature { FeatureId = key });
            }
            return criteria;
        }

        /// <summary>
        /// private method to create route data from a SearchCriteria object
        /// used to pass search criteria in url
        /// </summary>
        /// <returns>the created RouteValueDictionary</returns>
        public RouteValueDictionary GetRVD(RentalSearchCriteria criteria, int page = 1)
        {
            var rvd = new RouteValueDictionary();
            rvd["page"] = page;

            if (criteria.MinRate.HasValue)
                rvd["prix-min"] = criteria.MinRate;
            if (criteria.MaxRate.HasValue)
                rvd["prix-max"] = criteria.MaxRate;
            if (criteria.MinSurface.HasValue)
                rvd["surf-min"] = criteria.MinSurface;
            if (criteria.MaxSurface.HasValue)
                rvd["surf-max"] = criteria.MaxSurface;

            rvd["type"] = criteria.RentalData.Type;
            rvd["lease-type"] = criteria.RentalData.LeaseType;

            if (!string.IsNullOrEmpty(criteria.Place))
                rvd["places"] = criteria.Place;

            if (criteria.RentalData.AvailableDate.HasValue)
                rvd["avail"] = criteria.RentalData.AvailableDate.Value.ToString(_DateFormat);

            if (criteria.RentalData.AvailableNow)
                rvd["avail-now"] = true;

            foreach (var neededFeature in criteria.RentalData.RentalFeatures)
            {
				var display = FeatureHelper.FeatureToString(neededFeature.FeatureId, FeatureHelper.LocalisationPrefix);
                rvd[display] = true;
            }
            return rvd;
        }

        /// <summary>
        /// Fill search results from criteria
        /// search matching rental in repository
        /// </summary>
        public void FillSearchResults(ref RentalSearchCriteria criteria)
        {
            var context = ModelFactory.GetUnitOfWork();
            var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
            var results = rRepo.FindByCriteria(criteria);

            criteria.Results = results;
        }

		/// <summary>
		/// Get single result for a given index, within search results
		/// </summary>
		/// <param name="parameters">parameters from which to build result</param>
		/// <param name="index">index of the result item</param>
		/// <returns>result item</returns>
        public RentalSearchSingleResultViewModel GetSingleResult(HttpRequestBase parameters, int index)
		{
            var criteria = GetCriteria(parameters);
            FillSearchResults(ref criteria);

            if (index < 0 || index >= criteria.Results.Count)
				return null;

            var detailModel = criteria.GetSingleResult(index);
			return detailModel;
		}
	}
}