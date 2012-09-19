using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Routing;

namespace Worki.Infrastructure
{
    public class TranslatedRoute : Route
    {
        public const string DetectedCultureKey = "__ROUTING_DETECTED_CULTURE";

        public RouteValueDictionary RouteValueTranslationProviders { get; private set; }

        public TranslatedRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataToken, RouteValueDictionary routeValueTranslationProviders, IRouteHandler routeHandler)
            : base(url, defaults, constraints, dataToken, routeHandler)
        {
            this.RouteValueTranslationProviders = routeValueTranslationProviders;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            RouteData routeData = base.GetRouteData(httpContext);

            // Translate route values
            foreach (KeyValuePair<string, object> pair in this.RouteValueTranslationProviders)
            {
                IRouteValueTranslationProvider translationProvider = pair.Value as IRouteValueTranslationProvider;
                if (translationProvider != null
                    && routeData != null
                    && routeData.Values.ContainsKey(pair.Key))
                {
                    RouteValueTranslation translation = translationProvider.TranslateToRouteValue(
                        routeData.Values[pair.Key].ToString(),
                        CultureInfo.CurrentCulture);

                    routeData.Values[pair.Key] = translation.RouteValue;

                    // Store detected culture
                    if (routeData.DataTokens[DetectedCultureKey] == null)
                    {
                        routeData.DataTokens.Add(DetectedCultureKey, translation.Culture);
                    }
                }
            }

            return routeData;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            RouteValueDictionary translatedValues = values;

            // Translate route values
            foreach (KeyValuePair<string, object> pair in this.RouteValueTranslationProviders)
            {
                IRouteValueTranslationProvider translationProvider = pair.Value as IRouteValueTranslationProvider;
                if (translationProvider != null
                    && translatedValues.ContainsKey(pair.Key))
                {

                    var routeCulture = values.ContainsKey(MultiCultureMvcRouteHandler.CultureKey) ? CultureInfo.GetCultureInfo(values[MultiCultureMvcRouteHandler.CultureKey].ToString()) : CultureInfo.CurrentCulture;
                    RouteValueTranslation translation = translationProvider.TranslateToTranslatedValue(translatedValues[pair.Key].ToString(), routeCulture);

                    translatedValues[pair.Key] = translation.TranslatedValue;
                }
            }

            return base.GetVirtualPath(requestContext, translatedValues);
        }
    }
}