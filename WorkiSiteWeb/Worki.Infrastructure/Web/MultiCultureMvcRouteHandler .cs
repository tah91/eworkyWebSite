using System.Web.Mvc;
using System.Web;
using System.Web.Routing;
using System.Globalization;
using System.Threading;
using System.Linq;
using System;
using System.Web.SessionState;
using System.Collections.Generic;
using Worki.Infrastructure.Helpers;
using System.IO;

namespace Worki.Infrastructure 
{
    public class MultiCultureMvcRouteHandler : MvcRouteHandler
    {
		public static Culture DefaultCulture = Culture.en;

		/// <summary>
		/// Get culture type from url
		/// </summary>
		/// <param name="url">the url</param>
        /// <param name="culture">culture to be set</param>
		/// <returns>true if from query string</returns>
		public static bool GetCulture(Uri url, out Culture culture)
		{
            var parameters = HttpUtility.ParseQueryString(url.Query);
            var fromQuery = parameters[MiscHelpers.WidgetConstants.LocaleCulture];
            if (!string.IsNullOrEmpty(fromQuery))
            {
                if (!Enum.TryParse<Culture>(fromQuery, out culture))
                    culture = DefaultCulture;

                return true;
            }

			var prefix = ExtractDomainPrefix(url);

            switch (prefix)
			{
				case "fr":
					culture =  Culture.fr;
                    break;
				case "es":
					culture =  Culture.es;
                    break;
                case "de":
                    culture =  Culture.de;
                    break;
                case "nl":
                    culture = Culture.nl;
                    break;
				case "en":
					culture =  Culture.en;
                    break;
				default:
					culture =  DefaultCulture;
                    break;
			}

            return false;
		}

        /// <summary>
        /// Get suffix from culture
        /// </summary>
        /// <param name="lang">the lang</param>
        /// <returns>the suffix</returns>
        public static string GetPrefix(string lang)
        {
            Culture culture = DefaultCulture;
            Enum.TryParse<Culture>(lang, out culture);

            switch (culture)
            {
                case Culture.fr:
                    return "fr";
                case Culture.es:
                    return "es";
                case Culture.de:
                    return "de";
                case Culture.nl:
                    return "nl";
                case Culture.en:
                    return "en";
                default:
                    return "";
            }
        }

		/// <summary>
		/// Get culture type from user language
		/// </summary>
		/// <param name="url">the url</param>
		/// <returns>the culture type</returns>
		public static Culture GetCulture(string[] userLanguages)
		{
			if (userLanguages == null || userLanguages.Length == 0)
				return DefaultCulture;

			var langName = HttpContext.Current.Request.UserLanguages[0].Substring(0, 2);

			Culture culture = DefaultCulture;
			Enum.TryParse<Culture>(langName, out culture);

			return culture;
		}

		/// <summary>
		/// Extract culture from an url
		/// </summary>
		/// <param name="url">the url</param>
		/// <returns>the domain suffix, null if no culture found</returns>
		public static string ExtractDomainPrefix(Uri url)
		{
			if (string.IsNullOrEmpty(url.PathAndQuery))
				return null;

			var paths = url.PathAndQuery.Split('/');
			if (paths.Length == 0)
				return null;

            return paths[1];
		}

		/// <summary>
		/// Replace the suffix of an url
		/// </summary>
		/// <param name="url">the url</param>
		/// <param name="lang">the lang of the suffix</param>
		/// <returns>the new url</returns>
		public static string SetDomainPrefix(Uri url, string lang)
		{
            var suffix = GetPrefix(lang);

            var paths = url.PathAndQuery.Split('/').Where(s => !string.IsNullOrEmpty(s)).Skip(1);

            var newPathAndQuery = "/" + suffix + "/" + string.Join("/", paths);

            var newUrl = url.Scheme + System.Uri.SchemeDelimiter + url.Host + (url.IsDefaultPort ? "" : ":" + url.Port) + newPathAndQuery;

			return newUrl;
		}

		const string _CultureChanged = "CultureChanged";

		bool ShouldGuessCulture(bool fromQuery)
		{
            //in query, no need to force any other culture
            if (fromQuery)
                return false;

			//already have switched
			if (HttpContext.Current.Request.Cookies.AllKeys.Contains(_CultureChanged))
				return false;

			//comming from a link of the site
			if (HttpContext.Current.Request.UrlReferrer != null && HttpContext.Current.Request.UrlReferrer.IsFromThisSite())
				return false;

			//comming from a .fr or so, mean that the language is already defined
            if (!string.IsNullOrEmpty(ExtractDomainPrefix(HttpContext.Current.Request.Url)))
                return false;

			return true;
		}

		void TryAddCultureChangedCookie()
		{
			if (HttpContext.Current.Request.Cookies.AllKeys.Contains(_CultureChanged))
				return;

			HttpCookie cookie = new HttpCookie(_CultureChanged);
			cookie.Value = "Hello Cookie! CreatedOn: " + DateTime.Now.ToShortTimeString();

			HttpContext.Current.Response.Cookies.Add(cookie);
		}

        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            if (!string.IsNullOrEmpty(Path.GetExtension(requestContext.HttpContext.Request.CurrentExecutionFilePath)))
                return base.GetHttpHandler(requestContext);

            Culture urlCulture;
            var fromQuery = GetCulture(HttpContext.Current.Request.Url, out urlCulture);

			//check has cookie to tell no redirect
            if (ShouldGuessCulture(fromQuery))
			{
				var userCulture = GetCulture(HttpContext.Current.Request.UserLanguages);
				if (userCulture != urlCulture)
				{
					var correctUrl = SetDomainPrefix(HttpContext.Current.Request.Url, userCulture.ToString());
                    //avoid loop...
                    if (correctUrl == HttpContext.Current.Request.Url.AbsoluteUri)
                    {
                        TryAddCultureChangedCookie();
                    }
					if (!string.IsNullOrEmpty(correctUrl))
					{
						HttpContext.Current.Response.Redirect(correctUrl);
					}
					else
					{
						TryAddCultureChangedCookie();
					}
				}
			}
			else
			{
				TryAddCultureChangedCookie();
			}

			string langName = urlCulture.ToString();

            //set it
			try
			{
				var ci = new CultureInfo(langName);
				Thread.CurrentThread.CurrentUICulture = ci;
				Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
				Thread.CurrentThread.CurrentCulture.NumberFormat = CultureInfo.InvariantCulture.NumberFormat;
			}
			catch (Exception)
			{

			}
            return base.GetHttpHandler(requestContext);
        }
    }

    public class CultureConstraint : IRouteConstraint
    {
        static List<string> _Cultures = new List<string> { "fr", "es", "de", "nl", "en" };
        private string[] _values;

        public CultureConstraint(params string[] values)
        {
            this._values = values;
        }

        public CultureConstraint()
        {
            this._values = _Cultures.ToArray();
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {

            // Get the value called "parameterName" from the 
            // RouteValueDictionary called "value"
            string value = values[parameterName].ToString();
            // Return true is the list of allowed values contains 
            // this value.
            return _values.Contains(value);
        }

    }

    public enum Culture
    {
        fr,
        en,
		es,
        de,
        nl
    }

    public class SingleCultureMvcRouteHandler
    {
        public SingleCultureMvcRouteHandler()
        {

        }
    }
}