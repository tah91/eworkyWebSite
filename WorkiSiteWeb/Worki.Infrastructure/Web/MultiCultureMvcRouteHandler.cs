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
    public class RouteInfo
    {
        #region Ctor
        
        public RouteInfo(RouteData data)
        {
            RouteData = data;
        }

        public RouteInfo(Uri uri, string applicationPath)
        {
            RouteData = RouteTable.Routes.GetRouteData(new InternalHttpContext(uri, applicationPath));
        }

        #endregion

        #region private

        private class InternalHttpContext : HttpContextBase
        {
            private readonly HttpRequestBase _request;

            public InternalHttpContext(Uri uri, string applicationPath)
            {
                _request = new InternalRequestContext(uri, applicationPath);
            }

            public override HttpRequestBase Request { get { return _request; } }
        }

        private class InternalRequestContext : HttpRequestBase
        {
            private readonly string _appRelativePath;
            private readonly string _pathInfo;

            public InternalRequestContext(Uri uri, string applicationPath)
            {
                _pathInfo = uri.Query;

                if (String.IsNullOrEmpty(applicationPath) || !uri.AbsolutePath.StartsWith(applicationPath, StringComparison.OrdinalIgnoreCase))
                    _appRelativePath = uri.AbsolutePath.Substring(applicationPath.Length);
                else
                    _appRelativePath = uri.AbsolutePath;
            }

            public override string AppRelativeCurrentExecutionFilePath { get { return String.Concat("~", _appRelativePath); } }
            public override string PathInfo { get { return _pathInfo; } }
        }

        #endregion

        public RouteData RouteData { get; private set; }

        public string GetSpecificUrl(string culture)
        {
            RouteData.Values[MultiCultureMvcRouteHandler.CultureKey] = culture;
            var httpContextBase = new HttpContextWrapper(HttpContext.Current);
            var requestContext = new RequestContext(httpContextBase, RouteData);
            var urelHelper = new UrlHelper(requestContext);
            return urelHelper.RouteUrl(RouteData.Values);
        }
    }  

    public class MultiCultureMvcRouteHandler : MvcRouteHandler
    {
        public static List<string> Cultures = new List<string> { "fr", "es", "de", "nl", "en" };
        public const string CultureKey = "culture";
		public static Culture DefaultCulture = Culture.fr;

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
                    culture = Culture.fr;
                    break;
                case "es":
                    culture = Culture.es;
                    break;
                case "de":
                    culture = Culture.de;
                    break;
                case "nl":
                    culture = Culture.nl;
                    break;
                case "en":
                    culture = Culture.en;
                    break;
                default:
                    culture = DefaultCulture;
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
            if (url == null)
                return null;

            RouteInfo routeInfo = new RouteInfo(url, HttpContext.Current.Request.ApplicationPath);

            var newPathAndQuery = routeInfo.GetSpecificUrl(GetPrefix(lang));
            newPathAndQuery = HttpUtility.UrlDecode(newPathAndQuery);
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