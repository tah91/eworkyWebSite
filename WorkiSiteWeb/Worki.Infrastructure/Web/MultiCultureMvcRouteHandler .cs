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
		public const string ConnexionPath = "/account/logon";

		/// <summary>
		/// Get culture type from url
		/// </summary>
		/// <param name="url">the url</param>
		/// <returns>the culture type</returns>
		public static Culture GetCulture(string url)
		{
			var cultureStr = ExtractCultureFromUrl(url);
			Culture culture;
			return Enum.TryParse<Culture>(cultureStr, out culture) ? culture : Culture.fr;
		}

		/// <summary>
		/// Extract culture from an url
		/// </summary>
		/// <param name="url">the url</param>
		/// <returns>the culture, null if no culture found</returns>
		public static string ExtractCultureFromUrl(string url)
		{
			if (string.IsNullOrEmpty(url))
				return null;

			var lang = url.Split('/').FirstOrDefault(str => _Languages.Contains(str));
			if (string.IsNullOrEmpty(lang))
				return null;

			return lang;
		}

        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            string langName = "fr";

			//check if culture in path
			var fromUrl = ExtractCultureFromUrl(HttpContext.Current.Request.Url.PathAndQuery);
			if (!string.IsNullOrEmpty(fromUrl))
			{
				langName = fromUrl;
			}
			else
			{
				//check if it is connexion path
				if (HttpContext.Current.Request.Path == ConnexionPath)
				{
					var returnUrl = HttpContext.Current.Request.Params["ReturnUrl"];
					//check if culture in returnUrl
					fromUrl = ExtractCultureFromUrl(returnUrl);
					if (!string.IsNullOrEmpty(fromUrl))
					{
						langName = fromUrl;
					}
				}
				//take culture from navi language
				else if (HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Length != 0)
				{
					langName = HttpContext.Current.Request.UserLanguages[0].Substring(0, 2);
				}
			}

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