using System.Globalization;

namespace Worki.Infrastructure
{
    public interface IRouteValueTranslationProvider
    {
        RouteValueTranslation TranslateToRouteValue(string translatedValue, CultureInfo culture);
        RouteValueTranslation TranslateToTranslatedValue(string routeValue, CultureInfo culture);
    }
}
