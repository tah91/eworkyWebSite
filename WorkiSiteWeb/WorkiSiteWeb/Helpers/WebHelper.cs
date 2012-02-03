using System;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Worki.Data.Models;
using Worki.Infrastructure.Email;
using Worki.Infrastructure.Helpers;
using Worki.Service;
using Worki.Web.Singletons;
using System.Web.Routing;

namespace Worki.Web.Helpers
{
    /// <summary>
    /// static classe containings utility methods for web
    /// </summary>
    public static class WebHelper
    {
        public static string ResolveServerUrl(string serverUrl, bool forceHttps)
        {
            if (serverUrl.IndexOf("://") > -1)
                return serverUrl;

            string newUrl = serverUrl;
            Uri originalUri = HttpContext.Current.Request.Url;
            newUrl = (forceHttps ? "https" : originalUri.Scheme) +
                     "://" + originalUri.Authority + newUrl;
            return newUrl;
        }

        /// <summary>
        /// Get member display name from authentication data
        /// </summary>
        /// <param name="ident">the identity containing authentication data</param>
        /// <returns>display name</returns>
        public static string GetIdentityDisplayName(IIdentity ident)
        {
            FormsIdentity formIdent = ident as FormsIdentity;
            if (formIdent == null)
                return string.Empty;
            var ticket = formIdent.Ticket;
            return Member.GetNameFromUserData(ticket.UserData);
        }

        /// <summary>
        /// Get member id from authentication data
        /// </summary>
        /// <param name="ident">the identity containing authentication data</param>
        /// <returns>member id</returns>
        public static int GetIdentityId(IIdentity ident)
        {
            FormsIdentity formIdent = ident as FormsIdentity;
            if (formIdent == null)
                return 0;
            var ticket = formIdent.Ticket;
            return Member.GetIdFromUserData(ticket.UserData);
        }

        /// <summary>
        /// tell if the identiry match the member id
        /// </summary>
        /// <param name="ident">the identity containing authentication data</param>
        /// <param name="id">id of the member to match</param>
        /// <returns>true if match</returns>
        public static bool MatchIdentity(IIdentity ident, int id)
        {
            FormsIdentity formIdent = ident as FormsIdentity;
            if (formIdent == null)
                return false;
            var ticket = formIdent.Ticket;
            return Member.GetIdFromUserData(ticket.UserData) == id;
        }

		static bool _IsAzureDebug = bool.Parse(ConfigurationManager.AppSettings["IsAzureDebug"]);

		/// <summary>
		/// Tell if it is in debug or prod
		/// </summary>
		/// <returns>true if in debug</returns>
		public static bool IsDebug()
		{
			return _IsAzureDebug;
		}

		static string _FacebookId = null;

		/// <summary>
		/// Get facebook appid
		/// </summary>
		/// <returns>facebook appid</returns>
		public static string GetFacebookId()
		{
			if (!string.IsNullOrEmpty(_FacebookId))
				return _FacebookId;
			var fbSettings = ConfigurationManager.GetSection("facebookSettings") as Facebook.FacebookConfigurationSection;
			_FacebookId= fbSettings != null ? fbSettings.AppId : string.Empty;
			return _FacebookId;
		}

		public static RouteValueDictionary GetRVD(WebViewPage page)
		{
			RouteValueDictionary rvd = new RouteValueDictionary(page.ViewContext.RouteData.Values);
			foreach (string key in page.Request.QueryString.Keys)
			{
				rvd[key] = page.Request.QueryString[key].ToString();
			}
			return rvd;
		}

        public static string GetLoginTwitter(string twitter)
        {
            var toRet = "";
            var link = "twitter.com/";

            if (!string.IsNullOrEmpty(twitter))
            {
                if (twitter.Contains(link))
                {
                    var tab = twitter.Split('/');
                    toRet += tab[tab.Length - 1];
                }
                else
                {
                    toRet += twitter;
                }
                return (!toRet.Contains("@") ? ("@" + toRet) : toRet);
            }

            return (toRet);
        }

        public static string GetTwitter(string twitter)
        {
            var toRet = "";
            var link = "twitter.com/";
            var http = "http://";

            if (twitter.Contains(link))
            {
                var tab = twitter.Split('/');
                toRet += tab[tab.Length - 1];
                toRet = http + link + toRet;
            }

            return (!string.IsNullOrEmpty(toRet) ? toRet : twitter.Contains(http) ? twitter : twitter.Contains(link) ? (http + twitter) : (http + link + twitter));
        }

        public static string GetWebsite(string website)
        {
            var http = "http://";

            return (website.Contains(http) ? website : (http + website));
        }

        public static string DisplayCurrency(int price, bool b = true)
        {
            return (price + (b == true ? " €" : ""));
        }

        public static string DisplayCurrency(decimal price, bool b = true)
        {
            return ((price - decimal.Truncate(price) == 0 ? decimal.Truncate(price) : price) + (b == true ? " €" : ""));
        }

        public static string DisplaySurface(int surface, bool b = true)
        {
            return (surface + (b == true ? " m²" : ""));
        }

        public static string DisplaySurface(decimal surface, bool b = true)
        {
            return ((surface - decimal.Truncate(surface) == 0 ? decimal.Truncate(surface) : surface) + (b == true ? " m²" : ""));
		}

		#region Routes

		public static Route CultureMapRoute(this RouteCollection routes, string name, string url, object defaults, string[] namespaces)
		{
			return routes.CultureMapRoute(
				name,
				url,
				defaults,
				null,
				namespaces
			);
		}
		//
		// Summary:
		//     Maps the specified URL route and sets default route values, constraints,
		//     and namespaces.
		//
		// Parameters:
		//   routes:
		//     A collection of routes for the application.
		//
		//   name:
		//     The name of the route to map.
		//
		//   url:
		//     The URL pattern for the route.
		//
		//   defaults:
		//     An object that contains default route values.
		//
		//   constraints:
		//     A set of expressions that specify values for the url parameter.
		//
		//   namespaces:
		//     A set of namespaces for the application.
		//
		// Returns:
		//     A reference to the mapped route.
		//
		// Exceptions:
		//   System.ArgumentNullException:
		//     The routes or url parameter is null.
		public static Route CultureMapRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces)
		{
			//other languages
			var enUrl = "{culture}/" + url;
			var enDefault = defaults != null ? new RouteValueDictionary(defaults) : new RouteValueDictionary();
			enDefault.Add("culture", Worki.Infrastructure.Culture.en.ToString());
			var enRoute = routes.MapRoute(
				name,
				enUrl,
				defaults,
				constraints,
				namespaces
			);
			enRoute.RouteHandler = new Worki.Infrastructure.MultiCultureMvcRouteHandler();
			//enRoute.Defaults = enDefault;

			//default language
			var frDefault = defaults != null ? new RouteValueDictionary(defaults) : new RouteValueDictionary();
			frDefault.Add("culture", Worki.Infrastructure.Culture.fr.ToString());
			var frRoute = routes.MapRoute(
				"",
				url,
				defaults,
				constraints,
				namespaces
			);
			frRoute.Defaults = frDefault;
			frRoute.RouteHandler = new Worki.Infrastructure.MultiCultureMvcRouteHandler();
			return frRoute;
		}

		#endregion
	}
}