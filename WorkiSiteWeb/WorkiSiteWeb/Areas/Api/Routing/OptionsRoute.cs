using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Worki.Rest.Routing
{

    class OptionsRoute : Route
    {
        public OptionsRoute()
            : base(null, new MvcRouteHandler())
        {
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            if (httpContext.Request.HttpMethod == "OPTIONS")
            {
                httpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                httpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, PUT, POST, OPTIONS");
                httpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");
                httpContext.Response.End();
                return new RouteData();
            }
            else
            {
                return null;
            }
        }

    }
}
