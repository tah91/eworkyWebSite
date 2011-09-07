using System;
using System.Web;
using System.Web.Routing;

namespace Worki.Rest.Routing
{
    public class PathDoesNotStartWith : IRouteConstraint
    {
        private string _match = String.Empty;

        public PathDoesNotStartWith(string match)
        {
            _match = match;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            return !httpContext.Request.Url.AbsolutePath.StartsWith(_match, StringComparison.OrdinalIgnoreCase);
        }
    }
}