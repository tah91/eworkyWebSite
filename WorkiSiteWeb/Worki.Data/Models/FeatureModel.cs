using System;
using System.Collections.Generic;
using System.Reflection;

namespace Worki.Data.Models
{
	public static class FeatureHelper
	{
		public enum FeatureField
		{
			Bool,
			String,
			Number
		}

		public static FeatureField GetFieldType(Feature feature)
		{
			var intVal = (int)feature;
			if (intVal < 1000)
				return FeatureField.Bool;
			else if (intVal < 2000)
				return FeatureField.String;
			else
				return FeatureField.Number;
		}

		public static List<Feature> AvoidPeriods = new List<Feature>()
		{
			Feature.AvoidMorning,
			Feature.AvoidLunch, 
			Feature.AvoidAfternoom, 
			Feature.AvoidEvening
		};

		public static List<Feature> Characteristics = new List<Feature>()
		{
			Feature.Handicap,
			Feature.Wifi_Free, 
			Feature.Wifi_Not_Free, 
			Feature.Parking,
			Feature.Outlet,
			Feature.FastInternet, 
			Feature.SafeStorage, 
			Feature.Coffee,
			Feature.Restauration,
			Feature.AC, 
			Feature.ErgonomicFurniture, 
			Feature.Shower,
			Feature.Newspaper,
			Feature.TV,
			Feature.CoffeePrice,
            Feature.Heater
		};

		public static List<Feature> Services = new List<Feature>()
		{
			Feature.Domiciliation,
			Feature.Secretariat, 
			Feature.Courier, 
			Feature.Printer,
			Feature.Computers,
			Feature.Archiving, 
			Feature.Concierge, 
			Feature.Pressing,
			Feature.ComputerHelp,
			Feature.RoomService, 
			Feature.Community,
			Feature.RelaxingArea,
            Feature.PhoneLine,
            Feature.Kitchen,
            Feature.SharedMeetingRoom
		};

        public static List<Feature> Sectors = new List<Feature>()
		{
			Feature.Architects,
			Feature.Associative,
			Feature.Artists,
			Feature.Lawyers,
			Feature.BusinessDevelopers,
			Feature.Commercial,
			Feature.CommunicationMedia,
			Feature.Accountants,
			Feature.Consultants,
			Feature.Designers,
			Feature.Developers,
			Feature.Writers,
            Feature.Contractors,
			Feature.Independent,
			Feature.Investors,
			Feature.Journalists,
			Feature.Photographers,
			Feature.Nomads
		};

		public static List<Feature> BuisnessLounge = new List<Feature>()
        {
            Feature.OpenToAll,
            Feature.ReservedToClients,
            Feature.ForCardOwner
        };

		public static List<Feature> Workstation = new List<Feature>()
        {
            Feature.NotSectorial,
            Feature.CoworkingVibe
        };

		public static List<Feature> Desktop = new List<Feature>()
        {
            Feature.Desktop10_25,
            Feature.Desktop25_50,
            Feature.Desktop50_100,
            Feature.Desktop100Plus,
            Feature.Equipped,
            Feature.AvailableNow,
            Feature.AllInclusive,
            Feature.FlexibleContract,
            Feature.MinimalPeriod
        };

		public static List<Feature> MeetingRoom = new List<Feature>() 
        {
            Feature.Room2_7,
            Feature.Room7_20,
            Feature.Room20_plus,
            Feature.Audio,
            Feature.Visio,
            Feature.Projector,
            Feature.VideoProj,
            Feature.Paperboard,
            Feature.Internet,
            Feature.PhoneJack,
            Feature.Drinks
        };

		public static List<Feature> SeminarRoom = new List<Feature>() 
        {
            Feature.Room20_100,
            Feature.Room100_250,
            Feature.Room250_1000,
            Feature.Room1000_plus,
            Feature.Scene,
            Feature.Micro,
            Feature.Picturesque,
            Feature.Welcoming,
            Feature.Accommodation,
            Feature.Lighting,
            Feature.Sound,
            Feature.Catering
        };

		public static List<Feature> VisioRoom = new List<Feature>() 
        {
            Feature.Room1_4,
            Feature.Room5_10,
            Feature.Room10_Plus,
            Feature.Visio,
            Feature.VisioHD,
            Feature.Telepresence,
            Feature.Paperboard,
            Feature.Internet,
            Feature.PhoneJack,
            Feature.Drinks
        };

		public const string LocalisationPrefix = "f_";
		public const string OfferPrefix = "o_";

        /// <summary>
        /// Get an unique id for form purpose
        /// </summary>
        /// <param name="feature"></param>
        public static string GetStringId(Feature feature, string prefix)
        {
            return prefix + Enum.GetName(typeof(Feature), feature);
        }

        /// <summary>
        /// Get corresponding Feature from stringId
        /// </summary>
        /// <param name="feature"></param>
        public static string ParseStringId(string strId, string prefix)
        {
            var str = strId.Replace(prefix, string.Empty);
            Feature parsedEnum;
            if (!Enum.TryParse<Feature>(str, true, out parsedEnum))
                return null;
            return str;
        }

		/// <summary>
		/// return a string for the feature
		/// to put in url query string
		/// </summary>
		/// <param name="featureId">the feature id</param>
		/// <param name="prefix">the prefix</param>
		/// <returns>a string</returns>
		public static string FeatureToString(int featureId, string prefix)
		{
			return prefix + featureId.ToString();
		}

		/// <summary>
		/// get the feature id from the string of query string
		/// </summary>
		/// <param name="featureStr">the string to recover</param>
		/// <param name="prefix">the prefix</param>
		/// <returns>the feature id</returns>
		public static int FeatureFromString(string featureStr, string prefix)
		{
			var toRet = -1;
			if (string.IsNullOrEmpty(featureStr))
				return toRet;
			var idStr = featureStr.Replace(prefix, string.Empty);
			try
			{
				toRet = int.Parse(idStr);
			}
			catch (Exception)
			{

			}
			return toRet;
		}

		/// <summary>
		/// convert a list of feature url to the corresponding feature ids
		/// </summary>
		/// <param name="strings">the feature urls</param>
		/// <returns>list of feature ids</returns>
		public static IEnumerable<int> GetFeatureIds(List<string> strings, string prefix)
		{
			foreach (var item in strings)
			{
				var id = FeatureFromString(item, prefix);
				if (id != -1)
					yield return id;
			}
		}

        /// <summary>
        /// convert a list of feature url to the corresponding feature
        /// </summary>
        /// <param name="strings">the feature urls</param>
        /// <returns>list of feature</returns>
        public static IEnumerable<string> GetFeatureStrings(List<string> strings, string prefix)
        {
            foreach (var item in strings)
            {
                var str = ParseStringId(item, prefix);
                if (!string.IsNullOrEmpty(str))
                    yield return str;
            }
        }

		/// <summary>
		/// Get feature display name from resources
		/// </summary>
		/// <param name="feature">the feature</param>
		/// <returns>the display name</returns>
		public static string GetFeatureDisplayName(Feature feature)
		{
			var enumType = typeof(Feature);
			var enumResxType = typeof(Worki.Resources.Models.Localisation.LocalisationFeatures);
			var enumStr = Enum.GetName(enumType, feature);

			var nameProperty = enumResxType.GetProperty(enumStr, BindingFlags.Static | BindingFlags.Public);
			if (nameProperty != null)
			{
				enumStr = (string)nameProperty.GetValue(nameProperty.DeclaringType, null);
			}
			return enumStr;
		}

		/// <summary>
		/// Get displayed value of a feature
		/// </summary>
		/// <param name="feature">feature to display</param>
		/// <param name="container">feature container</param>
		/// <returns>the display value</returns>
		public static string Display(IFeatureContainer feature)
		{
			switch(feature.Feature)
			{
				case Feature.CoffeePrice:
					return FeatureHelper.GetFeatureDisplayName(feature.Feature) + " : " + string.Format(new System.Globalization.CultureInfo("fr-FR", false), "{0:C}", feature.DecimalValue);
				case Feature.MinimalPeriod:
				case Feature.ForCardOwner:
					return FeatureHelper.GetFeatureDisplayName(feature.Feature) + " : " + feature.StringValue;
				default:
					return GetFeatureDisplayName(feature.Feature);
			}
		}
	}

	/// <summary>
	/// interface for handling features (for localisation and offer)
	/// </summary>
	public interface IFeatureProvider
	{
        /// <summary>
        /// Get internal prefix depending on container
        /// </summary>
        /// <returns>the prefix</returns>
        string GetPrefix();

		/// <summary>
		/// Tell if it has a given feature
		/// </summary>
		/// <param name="feature">feature to check</param>
		/// <returns>true if have it</returns>
		bool HasFeature(Feature feature);

		/// <summary>
		/// Get the string value of a feature
		/// </summary>
		/// <param name="feature">the feature to check</param>
		/// <returns>string value, null if don't have the feature</returns>
		string GetStringFeature(Feature feature);

		/// <summary>
		/// Get the decimal value of a feature
		/// </summary>
		/// <param name="feature">the feature to check</param>
		/// <returns>decimal value, null if don't have the feature</returns>
		decimal? GetNumberFeature(Feature feature);
	}

	/// <summary>
	/// interface for handling features (for localisation and offer)
	/// </summary>
	public interface IFeatureContainer
	{
		/// <summary>
		/// The feature
		/// </summary>
		Feature Feature { get; }

		/// <summary>
		/// Get the string value of a feature
		/// </summary>
		/// <returns>string value</returns>
		string StringValue { get; }

		/// <summary>
		/// Get the decimal value of a feature
		/// </summary>
		/// <returns>decimal value</returns>
		decimal? DecimalValue { get; }
	}
}
