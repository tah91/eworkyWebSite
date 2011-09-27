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
			using (var client = new WebClient())
			{
				try
				{
					string textString = client.DownloadString(sPath);
                    JObject gmapJson = JObject.Parse(textString);
                    var location = gmapJson["results"][0]["geometry"]["location"];
					_Logger.Info("geocoded");
                    lat = (float)location["lat"];
                    lg = (float)location["lng"]; 
				}
				catch (WebException ex)
				{
					_Logger.Error(ex.Message);
				}
			}
		}
	}
}