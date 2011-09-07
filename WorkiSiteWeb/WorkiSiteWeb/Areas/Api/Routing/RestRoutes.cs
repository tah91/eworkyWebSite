using System.Web.Mvc;
using System.Web.Routing;

namespace Worki.Rest.Routing
{
    public static class RestRoutes
    {
        public static void RegisterRoutes(RouteCollection routes, string apiDirectory)
        {

            routes.Add(new OptionsRoute());

            routes.MapRoute(
               "API-LIST",                                              // Route name
               "API/{controller}",                           // URL with parameters
               new { action = "List" },  // Parameter defaults
               new
               {
                   path = new PathStartsWith(apiDirectory),
                   httpMethod = new HttpMethodConstraint(new string[] { "GET" })
               });

            routes.MapRoute(
               "API-GET",                                              // Route name
               "API/{controller}/{id}",                           // URL with parameters
               new { action = "Read" },  // Parameter defaults
               new
               {
                   path = new PathStartsWith(apiDirectory),
                   httpMethod = new HttpMethodConstraint(new string[] { "GET" })
               });

            routes.MapRoute(
               "API-UPDATE",                                              // Route name
               "API/{controller}",                           // URL with parameters
               new { action = "Update" },  // Parameter defaults
               new
               {
                   path = new PathStartsWith(apiDirectory),
                   httpMethod = new HttpMethodConstraint(new string[] { "PUT" })
               });

            routes.MapRoute(
               "API-CREATE",                                              // Route name
               "API/{controller}",                           // URL with parameters
               new { action = "Create" },  // Parameter defaults
               new
               {
                   path = new PathStartsWith(apiDirectory),
                   httpMethod = new HttpMethodConstraint(new string[] { "POST" })
               });

            routes.MapRoute(
               "API-DELETE",                                              // Route name
               "API/{controller}/{id}",                           // URL with parameters
               new { action = "Delete" },  // Parameter defaults
               new
               {
                   path = new PathStartsWith(apiDirectory),
                   httpMethod = new HttpMethodConstraint(new string[] { "DELETE" })
               });



            routes.MapRoute(
                "API",                                              // Route name
                "API/{controller}/{action}/{name}",                           // URL with parameters
                new { },  // Parameter defaults
                new { path = new PathStartsWith(apiDirectory), });           


        }

    }
}
