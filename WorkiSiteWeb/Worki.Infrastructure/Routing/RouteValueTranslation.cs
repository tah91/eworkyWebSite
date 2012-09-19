using System.Globalization;

namespace Worki.Infrastructure
{
    public class RouteValueTranslation
    {
        public CultureInfo Culture { get; set; }

        public string RouteValue { get; set; }

        public string TranslatedValue { get; set; }

        public RouteValueTranslation()
        {
        }

        public RouteValueTranslation(CultureInfo culture, string routeValue, string translatedValue)
        {
            this.Culture = culture;
            this.RouteValue = routeValue;
            this.TranslatedValue = translatedValue;
        }
    }
}