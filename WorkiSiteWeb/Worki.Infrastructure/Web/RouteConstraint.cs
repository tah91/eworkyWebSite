using System.Web.Mvc;
using System.Web.Routing;
using System.Web;
using System.Linq;
using System.Collections.Generic;

namespace Worki.Infrastructure
{
    public class FromValuesListConstraint : IRouteConstraint
    {
        public FromValuesListConstraint(IEnumerable<string> values)
        {
            _Filtered = values;
        }

        private IEnumerable<string> _Filtered;

        public bool Match(HttpContextBase httpContext,
                          Route route,
                          string parameterName,
                          RouteValueDictionary values,
                          RouteDirection routeDirection)
        {
            // Get the value called "parameterName" from the 
            // RouteValueDictionary called "value"
            string value = values[parameterName].ToString();

            // Return true is the list of allowed values contains 
            // this value.
            return _Filtered.Contains(value);
        }
    }

    public class CultureConstraint : IRouteConstraint
    {
        private string[] _values;

        public CultureConstraint(params string[] values)
        {
            this._values = values;
        }

        public CultureConstraint()
        {
            this._values = MultiCultureMvcRouteHandler.Cultures.ToArray();
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
}