using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Worki.Infrastructure 
{
    public static class TranslatedRouteCollectionExtensions
    {
        public static TranslatedRoute MapTranslatedRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, object dataToken, object routeValueTranslationProviders)
        {
            TranslatedRoute route = new TranslatedRoute(
                url,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(constraints),
                new RouteValueDictionary(dataToken),
                new RouteValueDictionary(routeValueTranslationProviders),
                new MultiCultureMvcRouteHandler());
            routes.Add(name, route);
            return route;
        }

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
        public static Route CultureMapRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces, object routeValueTranslationProviders = null)
        {
            //other languages
            var clUrl = "{culture}/" + url;
            var clConstraint = constraints != null ? new RouteValueDictionary(constraints) : new RouteValueDictionary();
            clConstraint.Add("culture", new Worki.Infrastructure.CultureConstraint());
            var clRoute = routes.MapTranslatedRoute(
                name,
                clUrl,
                defaults,
                constraints,
                new { Namespaces = namespaces },
                routeValueTranslationProviders
            );
            clRoute.Constraints = clConstraint;
            clRoute.RouteHandler = new Worki.Infrastructure.MultiCultureMvcRouteHandler();

            //default language
            var frDefault = defaults != null ? new RouteValueDictionary(defaults) : new RouteValueDictionary();
            frDefault.Add("culture", Worki.Infrastructure.Culture.fr.ToString());
            var frRoute = routes.MapTranslatedRoute(
                "",
                url,
                defaults,
                constraints,
                new { Namespaces = namespaces },
                routeValueTranslationProviders
            );
            frRoute.Defaults = frDefault;
            frRoute.RouteHandler = new Worki.Infrastructure.MultiCultureMvcRouteHandler();
            return frRoute;
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

        public static Route SpecificCultureMapRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces, string culture, object routeValueTranslationProviders = null)
        {
            //other languages
            var clUrl = "{culture}/" + url;
            var clConstraint = constraints != null ? new RouteValueDictionary(constraints) : new RouteValueDictionary();
            clConstraint.Add("culture", new Worki.Infrastructure.CultureConstraint(culture));
            var clRoute = routes.MapTranslatedRoute(
                name,
                clUrl,
                defaults,
                constraints,
                new { Namespaces = namespaces },
                routeValueTranslationProviders
            );
            clRoute.Constraints = clConstraint;
            clRoute.RouteHandler = new Worki.Infrastructure.MultiCultureMvcRouteHandler();
            return clRoute;
        }

        public static Route CultureMapRoute(this AreaRegistrationContext areaContext, string name, string url, object defaults, object constraints, string[] namespaces)
        {
            //other languages
            var clUrl = "{culture}/" + url;
            var clConstraint = constraints != null ? new RouteValueDictionary(constraints) : new RouteValueDictionary();
            clConstraint.Add("culture", new Worki.Infrastructure.CultureConstraint());
            var clRoute = areaContext.MapRoute(
                name,
                clUrl,
                defaults,
                constraints,
                namespaces
            );
            clRoute.Constraints = clConstraint;
            clRoute.RouteHandler = new Worki.Infrastructure.MultiCultureMvcRouteHandler();

            //default language
            var frDefault = defaults != null ? new RouteValueDictionary(defaults) : new RouteValueDictionary();
            frDefault.Add("culture", Worki.Infrastructure.Culture.fr.ToString());
            var frRoute = areaContext.MapRoute(
                "",
                url,
                defaults,
                constraints,
                namespaces
            );
            frRoute.Defaults = frDefault;
            frRoute.RouteHandler = new Worki.Infrastructure.MultiCultureMvcRouteHandler();
            return frRoute;
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
    }
}
