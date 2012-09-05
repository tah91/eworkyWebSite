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

        /// <summary>
        /// Get facebook namespace
        /// </summary>
        /// <returns>facebook namespace</returns>
        public static string GetFacebookNamespace()
        {
            if (IsDebug())
                return "eworky_localhost";
            else
                return "eworky_ns";
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

		/// <summary>
		/// Maps the specified URL route and sets default route values, constraints, and namespaces.
		/// </summary>
		/// <param name="routes">A collection of routes for the application.</param>
		/// <param name="name">The name of the route to map.</param>
		/// <param name="url">The URL pattern for the route.</param>
		/// <param name="defaults">An object that contains default route values.</param>
		/// <param name="constraints">A set of expressions that specify values for the url parameter.</param>
		/// <param name="namespaces">A set of namespaces for the application.</param>
		/// <returns>A reference to the mapped route.</returns>
		public static Route CultureMapRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces)
		{


			var route = routes.MapRoute(
				"",
				url,
				defaults,
				constraints,
				namespaces
			);

			route.RouteHandler = new Worki.Infrastructure.MultiCultureMvcRouteHandler();

			//other languages
			var clUrl = "fr/" + url;
			var clRoute = routes.MapRoute(
				"",
				clUrl,
				defaults,
				constraints,
				namespaces
			);
			clRoute.RouteHandler = new Worki.Infrastructure.MultiCultureMvcRouteHandler();

			return route;
		}

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

		public static Route CultureMapRoute(this AreaRegistrationContext areaContext, string name, string url, object defaults, object constraints, string[] namespaces)
		{
			var route = areaContext.MapRoute(
				"",
				url,
				defaults,
				constraints,
				namespaces
			);

			route.RouteHandler = new Worki.Infrastructure.MultiCultureMvcRouteHandler();

			return route;
		}

		public static Route CultureMapRoute(this AreaRegistrationContext areaContext, string name, string url, object defaults, string[] namespaces)
		{
			return areaContext.CultureMapRoute(
				name,
				url,
				defaults,
				null,
				namespaces
			);
		}

		public static Route CultureMapRoute(this AreaRegistrationContext areaContext, string name, string url, object defaults)
		{
			return areaContext.CultureMapRoute(
				name,
				url,
				defaults,
				null,
				null
			);
		}

		public static Worki.Infrastructure.Culture GetCulture(this UrlHelper instance)
		{
            Worki.Infrastructure.Culture culture;
            Worki.Infrastructure.MultiCultureMvcRouteHandler.GetCulture(instance.RequestContext.HttpContext.Request.Url, out culture);
            return culture;
		}

        public static string GetFBCulture(this UrlHelper instance)
        {
            var culture = instance.GetCulture();
            var fbCulture = string.Empty;
            switch (culture)
            {
                case Worki.Infrastructure.Culture.en:
                    fbCulture = "en_US";
                    break;
                case Worki.Infrastructure.Culture.fr:
                    fbCulture = "fr_FR";
                    break;
                case Worki.Infrastructure.Culture.es:
                    fbCulture = "es_ES";
                    break;
                case Worki.Infrastructure.Culture.de:
                    fbCulture = "de_DE";
                    break;
                case Worki.Infrastructure.Culture.nl:
                    fbCulture = "nl_NL";
                    break;
            }
            return fbCulture;
        }

        public static bool IsOfCulture(this UrlHelper instance, Infrastructure.Culture culture)
        {
            return GetCulture(instance) == culture;
        }

		public static bool IsFrench(this UrlHelper instance)
		{
            return instance.IsOfCulture(Infrastructure.Culture.fr);
		}

        public static bool IsEnglish(this UrlHelper instance)
        {
            return instance.IsOfCulture(Infrastructure.Culture.en);
        }

        public static string GetSuffix(this UrlHelper instance)
        {
            return Worki.Infrastructure.MultiCultureMvcRouteHandler.ExtractDomainSuffix(instance.RequestContext.HttpContext.Request.Url);
        }

		public static string GetFileFullUrl(this UrlHelper html, string path, bool forceHttps = false)
		{
			try
			{
				if (path.StartsWith("http://") || path.StartsWith("https://"))
				{
					return path;
				}
				return WebHelper.ResolveServerUrl(VirtualPathUtility.ToAbsolute(path), forceHttps);
			}
			catch (Exception)
			{
				return path;
			}
		}

        public static string GetQueryParam(this UrlHelper instance, string key)
        {
            return instance.RequestContext.HttpContext.Request.QueryString[key];
        }

		#endregion

        #region Theme

        public static string GetThemeCss(this UrlHelper html, string theme)
        {
            switch (theme)
            {
                case MiscHelpers.WidgetConstants.SwcClass:
                    return Links.Content.scss.widget_swc_css;
                case MiscHelpers.WidgetConstants.EworkyClass:
                default:
                    return Links.Content.scss.widget_eworky_css;
            }
        }
        #endregion

        public static string RenderEmailToString(string displayName, string body)
        {
            // Get the HttpContext
            HttpContextBase httpContextBase = new HttpContextWrapper(HttpContext.Current);
            // Build the route data, pointing to the dummy controller
            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", typeof(Worki.Web.Controllers.DummyController).Name);
            // Create the controller context
            Controller controller = new Worki.Web.Controllers.DummyController();
            ControllerContext controllerContext = new ControllerContext(new RequestContext(httpContextBase, routeData), controller);
            controller.ControllerContext = controllerContext;

            return controller.RenderRazorViewToString(MVC.Emails.Views.NewEmail, new EmailContentModel { ToName = displayName, Content = body });
        }
    }
}