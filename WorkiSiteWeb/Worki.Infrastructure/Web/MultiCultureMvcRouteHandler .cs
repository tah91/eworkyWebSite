using System.Web.Mvc;
using System.Web;
using System.Web.Routing;
using System.Globalization;
using System.Threading;
using System.Linq;
using System;

namespace Worki.Infrastructure 
{
    public class MultiCultureMvcRouteHandler : MvcRouteHandler
    {
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            string langName = "fr";

            //first look in url
            var culture = requestContext.RouteData.Values["culture"];

            //then in session
            if (culture==null && requestContext.HttpContext.Session != null)
            {
                culture = requestContext.HttpContext.Session["Culture"];
                //Checking first if there is no value in session
                //and set default language
                //this can happen for first user's request
                if (culture == null)
                {
                    //Sets default culture to french invariant
                    
                    //Try to get values from Accept lang HTTP header
                    if (HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Length != 0)
					{
						//Gets accepted list
						langName = HttpContext.Current.Request.UserLanguages[0].Substring(0, 2);
					}
                    requestContext.HttpContext.Session["Culture"] = langName;
                }
            }

            //set it
            if (culture != null)
            {
                langName = culture.ToString();
            }
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
        en = 1,
        fr = 2
    }

    public class SingleCultureMvcRouteHandler
    {
        public SingleCultureMvcRouteHandler()
        {

        }
    }
}