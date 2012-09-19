using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Worki.Infrastructure
{
    public class DictionaryRouteValueTranslationProvider : IRouteValueTranslationProvider
    {
        public IList<RouteValueTranslation> Translations { get; private set; }

        public DictionaryRouteValueTranslationProvider(IList<RouteValueTranslation> translations)
        {
            this.Translations = translations;
        }

        public RouteValueTranslation TranslateToRouteValue(string translatedValue, CultureInfo culture)
        {
            RouteValueTranslation translation = null;

            // Find translation in specified CultureInfo
            translation = this.Translations.Where(
                t => t.TranslatedValue == translatedValue
                    && (t.Culture.ToString() == culture.ToString() || t.Culture.ToString().Substring(0, 2) == culture.ToString().Substring(0, 2)))
                .OrderByDescending(t => t.Culture)
                .FirstOrDefault();
            if (translation != null)
            {
                return translation;
            }

            // Find translation without taking account on CultureInfo
            translation = this.Translations.Where(t => t.TranslatedValue == translatedValue)
                .FirstOrDefault();
            if (translation != null)
            {
                return translation;
            }

            // Return the current values
            return new RouteValueTranslation
            {
                Culture = culture,
                RouteValue = translatedValue,
                TranslatedValue = translatedValue
            };
        }

        public RouteValueTranslation TranslateToTranslatedValue(string routeValue, CultureInfo culture)
        {
            RouteValueTranslation translation = null;

            // Find translation in specified CultureInfo
            translation = this.Translations.Where(
                t => t.RouteValue == routeValue
                    && (t.Culture.ToString() == culture.ToString() || t.Culture.ToString().Substring(0, 2) == culture.ToString().Substring(0, 2)))
                .OrderByDescending(t => t.Culture)
                .FirstOrDefault();
            if (translation != null)
            {
                return translation;
            }

            // Find translation without taking account on CultureInfo
            //translation = this.Translations.Where(t => t.RouteValue == routeValue)
            //    .FirstOrDefault();
            //if (translation != null)
            //{
            //    return translation;
            //}

            // Return the current values
            return new RouteValueTranslation
            {
                Culture = culture,
                RouteValue = routeValue,
                TranslatedValue = routeValue
            };
        }
    }
}
