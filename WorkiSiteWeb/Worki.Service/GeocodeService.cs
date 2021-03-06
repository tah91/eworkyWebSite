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
using Newtonsoft.Json.Linq;

namespace Worki.Service
{
	public interface IGeocodeService
	{
		void GeoCode(string address, out float lat, out float lg);
	}

	public class GeocodeService : IGeocodeService
	{
		#region private

        ILogger _Logger;

		#endregion

		public GeocodeService(ILogger logger)
		{
            _Logger = logger;
		}

		/// <summary>
		/// Geocode address via google api
		/// </summary>
		/// <param name="address">address to geocode</param>
		/// <param name="lat">place latitude</param>
		/// <param name="lg">place longitude</param>
		public void GeoCode(string address, out float lat, out float lg)
		{
			lat = 0;
			lg = 0;
			if (string.IsNullOrEmpty(address))
				return;
            string sPath = "http://maps.googleapis.com/maps/api/geocode/json?address=" + address + "&sensor=true&region=fr";
			var textString = string.Empty;
			using (var client = new WebClient())
			{
				try
				{
					textString = client.DownloadString(sPath);
                    JObject gmapJson = JObject.Parse(textString);
                    var results = gmapJson["results"];
                    foreach (var item in results)
                    {
                        if (item == null)
                            continue;
                        var geometry = item["geometry"];
                        if (geometry == null)
                            continue;
                        var location = geometry["location"];
                        if (location == null)
                            continue;
                        lat = (float)location["lat"];
                        lg = (float)location["lng"];
                        break;
                    }
					_Logger.Info("geocoded"); 
				}
				catch (Exception ex)
				{
                    _Logger.Error("GeoCode", ex);
					_Logger.Error(textString);
                    lat = (float)48.8566140;
                    lg = (float)2.35222190;
				}
			}
		}
	}
}