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
	}
}