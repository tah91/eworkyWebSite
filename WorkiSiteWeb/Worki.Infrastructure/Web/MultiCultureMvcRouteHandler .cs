using System.Web.Mvc;
using System.Web;
using System.Web.Routing;
using System.Globalization;
using System.Threading;
using System.Linq;
using System;
using System.Web.SessionState;
using System.Collections.Generic;

namespace Worki.Infrastructure 
{
    public class MultiCultureMvcRouteHandler : MvcRouteHandler
    {
		static List<string> _Languages = new List<string>() { Culture.fr.ToString(), Culture.en.ToString() };

		public const Culture DefaultCulture = Culture.fr;

		/// <summary>
		/// Get culture type from url
		/// </summary>
		/// <param name="url">the url</param>
		/// <returns>the culture type</returns>
		public static Culture GetCulture(Uri url)
		{
			var suffix = ExtractDomainSuffix(url);

			switch(suffix)
			{
				case ".fr":
					return Culture.fr;
				case ".es":
					return Culture.es;
				case ".com":
					return Culture.en;
				default:
					return DefaultCulture;
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
		/// Get suffix from culture
		/// </summary>
		/// <param name="lang">the lang</param>
		/// <returns>the suffix</returns>
		public static string GetSuffix(string lang)
		{
			Culture culture = DefaultCulture;
			Enum.TryParse<Culture>(lang, out culture);

			switch (culture)
			{
				case Culture.fr:
					return ".fr";
				//case Culture.es:
					//return ".es";
				case Culture.en:
				default:
					return ".com";
			}
		}

		/// <summary>
		/// Extract culture from an url
		/// </summary>
		/// <param name="url">the url</param>
		/// <returns>the domain suffix, null if no culture found</returns>
		public static string ExtractDomainSuffix(Uri url)
		{
			if (string.IsNullOrEmpty(url.Host))
				return null;

			var lastDot = url.Host.LastIndexOf(".");
			if (lastDot == -1)
				return null;

			return url.Host.Substring(lastDot);
		}

		/// <summary>
		/// Replace the suffix of an url
		/// </summary>
		/// <param name="url">the url</param>
		/// <param name="lang">the lang of the suffix</param>
		/// <returns>the new url</returns>
		public static string SetDomainSuffix(Uri url, string lang, bool addLocal = true)
		{
			if (string.IsNullOrEmpty(url.Host))
				return url.PathAndQuery;

			var suffix = GetSuffix(lang);

			var lastDot = url.Host.LastIndexOf(".");
			if (lastDot == -1)
				return null;

			var newHost = url.Host.Substring(0, lastDot) + suffix;

			var newUrl = url.Scheme + System.Uri.SchemeDelimiter + newHost + (url.IsDefaultPort ? "" : ":" + url.Port) + url.PathAndQuery;
			if (addLocal)
			{
				newUrl += "?" + _LocalQuery + "=" + lang;
			}

			return newUrl;
		}

		const string _CultureChanged = "CultureChanged";
		const string _LocalQuery = "local";

		bool ShouldSetCulture()
		{
			if (HttpContext.Current.Request.Cookies.AllKeys.Contains(_CultureChanged))
				return false;

			var query = HttpUtility.ParseQueryString(HttpContext.Current.Request.Url.Query);
			if (query != null && query.AllKeys.Contains(_LocalQuery))
				return false;

			if (ExtractDomainSuffix(HttpContext.Current.Request.Url) != ".com")
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
			var urlCulture = GetCulture(HttpContext.Current.Request.Url);

			//check has cookie to tell no redirect
			if (ShouldSetCulture())
			{
				var userCulture = GetCulture(HttpContext.Current.Request.UserLanguages);
				if (userCulture != urlCulture)
				{
					var correctUrl = SetDomainSuffix(HttpContext.Current.Request.Url, userCulture.ToString(), false);
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
        private string[] _values;
        public CultureConstraint(params string[] values)
        {
            this._values = values;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName,
                            RouteValueDictionary values, RouteDirection routeDirection)
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
		es
    }

    public class SingleCultureMvcRouteHandler
    {
        public SingleCultureMvcRouteHandler()
        {

        }
    }
}