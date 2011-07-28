using System.Web.Mvc;
using System.Web;
using System.Web.Routing;
using System.Globalization;
using System.Threading;
using System.Linq;

namespace WorkiSiteWeb.Infrastructure 
{
    public class MultiCultureMvcRouteHandler : MvcRouteHandler
    {
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var culture = requestContext.RouteData.Values["culture"].ToString();
            var ci = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
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

    class SingleCultureMvcRouteHandler
    {
        public SingleCultureMvcRouteHandler()
        {

        }
    }
}