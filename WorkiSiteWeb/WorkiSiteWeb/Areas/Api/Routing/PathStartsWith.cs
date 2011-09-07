using System;
using System.Web;
using System.Web.Routing;

namespace Worki.Rest.Routing
{
    public class PathStartsWith : IRouteConstraint
    {
        private string _match = String.Empty;

        public PathStartsWith(string match)
        {
            _match = match;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            return httpContext.Request.Url.AbsolutePath.StartsWith(_match, StringComparison.OrdinalIgnoreCase);
        }
    }
}
