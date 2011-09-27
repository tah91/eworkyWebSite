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

            if (MiscHelpers.GetRequestValue(parameters, "places", ref value))
            {
                var places = value.Split('|');
                foreach (var p in places)
                {
                    criteria.Places.Add(new RentalPlace { Place = p });
                }
            }

            if (MiscHelpers.GetRequestValue(parameters, "avail", ref value))
            {
                criteria.RentalData.AvailableDate = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            var keys = MiscHelpers.GetFeatureIds(parameters.Params.AllKeys.ToList());
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
            rvd["prix-min"] = criteria.MinRate;
            rvd["prix-max"] = criteria.MaxRate;
            rvd["surf-min"] = criteria.MinSurface;
            rvd["surf-max"] = criteria.MaxSurface;

            var places = string.Empty;
            foreach (var item in criteria.Places)
            {
                places += item.Place + "|";
            }
            places = places.TrimEnd('|');

            if (!string.IsNullOrEmpty(places))
                rvd["places"] = places;

            if (criteria.RentalData.AvailableDate.HasValue)
                rvd["avail"] = criteria.RentalData.AvailableDate.Value.ToString("dd/MM/yy");

            foreach (var neededFeature in criteria.RentalData.RentalFeatures)
            {
                var display = MiscHelpers.FeatureToString(neededFeature.FeatureId);
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
            var results = rRepo.FindByCriteria(criteria);//.ToList();

            criteria.Results = results.ToList();
            criteria.FillPageInfo();
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