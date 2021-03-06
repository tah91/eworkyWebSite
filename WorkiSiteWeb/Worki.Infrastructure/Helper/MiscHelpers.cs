﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Worki.Infrastructure.Helpers
{
    public static class MiscHelpers
	{
		#region Uri

		public const string ThisSite = "eworky";

		public static bool IsFromThisSite(this Uri url)
		{
			if (url != null &&
				!string.IsNullOrEmpty(url.Host) &&
				url.Host.Split('.').Contains(ThisSite))
				return true;

			return false;
		}

		#endregion

		#region Email

		public static class EmailConstants
		{
			public const string ContactDisplayName = "eWorky";
			public const string WebsiteAddress = "www.eworky.com";
            public static string _ContactMail = null;
            public static string ContactMail
            {
                get 
                {
                    if (!string.IsNullOrEmpty(_ContactMail))
                        return _ContactMail;
                    return Worki.Resources.Views.Shared.SharedString.ContactMail; 
                }

                set
                {
                    _ContactMail = value;
                }
            }

			public static string BookingMail = "team@eworky.com";
            public const string Tel = "01 77 17 04 21";
		}

        #endregion

        #region Facebook

        public static class FaceBookConstants
        {
            public const string FacebookProfilePictureUrlPattern = "https://graph.facebook.com/{0}/picture?type=large";
            public const string FacebookProfileViewPattern = "http://www.facebook.com/#!/profile.php?id={0}&viewas={1}&returnto=profile";
            public const string FacebookDefaultPassword = "Fcbkdfltpsswrd";
        }

        #endregion

        public static class SeoConstantsFr
        {
            public const string FreeAreas = "lieux-gratuits";
            public const string Telecentre = "telecentre";
            public const string BuisnessCenter = "centre-d'affaires";
            public const string CoworkingSpace = "espace-de-coworking";
            public const string OtherPlaces = "autres-lieux";
            public const string SharedOffice = "bureau-partage";
            public const string SpotWifi = "spot-wifi";
            public const string CoffeeResto = "cafe-resto";
            public const string Biblio = "biblioteque";
            public const string TravelerSpace = "espace-voyageurs";
            public const string WorkingHotel = "incubateur";
            public const string PrivateArea = "espace-evenementiel";
            public const string PublicSpace = "cybercafe";
            public const string Hotel = "hotel";
            public const string Type = "type";

            public static List<string> LocalisationTypes = new List<string>
            {
                FreeAreas ,
                Telecentre ,
                BuisnessCenter ,
                CoworkingSpace ,
                OtherPlaces ,
                SharedOffice ,
                SpotWifi ,
                CoffeeResto ,
                Biblio ,
                TravelerSpace,
                WorkingHotel,
                PrivateArea,
                PublicSpace,
                Hotel
            };

            public const string FreeArea = "espace-gratuit";
            public const string BuisnessLounge = "buisness-lounge";
            public const string Workstation = "poste-de-travail";
            public const string Desktop = "bureau";
            public const string MeetingRoom = "salle-de-reunion";
            public const string SeminarRoom = "salle-de-conference";
            public const string VisioRoom = "salle-de-visio";

            public static List<string> LocalisationOfferTypes = new List<string>
            {
                FreeArea ,
                BuisnessLounge,
                Workstation ,
                Desktop ,
                MeetingRoom ,
                SeminarRoom ,
                VisioRoom 
            };
        }

        public static class SeoConstants
        {
            public const string FreeAreas = "free-areas";
            public const string Telecentre = "smart-workcenter";
            public const string BuisnessCenter = "business-center";
            public const string CoworkingSpace = "coworking-space";
            public const string OtherPlaces = "other-places";
			public const string SharedOffice = "shared-office";
            public const string SpotWifi = "wifi-hotspot";
            public const string CoffeeResto = "pub-resto";
            public const string Biblio = "library";
            public const string TravelerSpace = "travel-lounge";
            public const string WorkingHotel = "incubator";
            public const string PrivateArea = "private-place";
            public const string PublicSpace = "cyber";
            public const string Hotel = "hotel";
            public const string Type = "type";
            public const string LocalisationType = "loc-type";

            public static List<string> AllLocalisationTypes = new List<string>
            {
                SpotWifi,
                CoffeeResto,
                Biblio,
                PublicSpace,
                TravelerSpace,
                Hotel,
                Telecentre,
                BuisnessCenter,
                CoworkingSpace,
                WorkingHotel,
                PrivateArea,
                SharedOffice,
                SeoConstantsFr.SpotWifi,
                SeoConstantsFr.CoffeeResto,
                SeoConstantsFr.Biblio,
                SeoConstantsFr.PublicSpace,
                SeoConstantsFr.TravelerSpace,
                SeoConstantsFr.Hotel,
                SeoConstantsFr.Telecentre,
                SeoConstantsFr.BuisnessCenter,
                SeoConstantsFr.CoworkingSpace,
                SeoConstantsFr.WorkingHotel,
                SeoConstantsFr.PrivateArea,
                SeoConstantsFr.SharedOffice
            };

            public static List<string> AllOfferTypes = new List<string>
            {
                FreeArea,
                BuisnessLounge,
                Workstation,
                Desktop,
                MeetingRoom,
                SeminarRoom,
                VisioRoom, 
                SeoConstantsFr.FreeArea,
                SeoConstantsFr.BuisnessLounge,
                SeoConstantsFr.Workstation,
                SeoConstantsFr.Desktop,
                SeoConstantsFr.MeetingRoom,
                SeoConstantsFr.SeminarRoom,
                SeoConstantsFr.VisioRoom
            };

            public class Coordinates
            {
                public double Latitude { get; set; }
                public double Longitude { get; set; }
            }

            public static Dictionary<string, Coordinates> Places = new Dictionary<string, Coordinates>
            {
                {"paris", new Coordinates{ Latitude = 48.856614, Longitude=2.35222190000002}},
                {"lyon", new Coordinates{ Latitude = 45.764043, Longitude=4.83565899999996}},
                {"lille", new Coordinates{ Latitude = 50.62925, Longitude=3.05725600000005}},
                {"marseille", new Coordinates{ Latitude = 43.296482, Longitude=5.36977999999999}},
                {"bordeaux", new Coordinates{ Latitude = 44.837789, Longitude=-0.579179999999951}},
                {"rennes", new Coordinates{ Latitude = 48.113475, Longitude=-1.67570799999999}},
                {"belgique", new Coordinates{ Latitude = 50.503887, Longitude=4.46993599999996}}
            };

            public static List<string> LocalisationTypes = new List<string>
            {
                FreeAreas ,
                Telecentre ,
                BuisnessCenter ,
                CoworkingSpace ,
                OtherPlaces ,
                SharedOffice ,
                SpotWifi ,
                CoffeeResto ,
                Biblio ,
                TravelerSpace,
                WorkingHotel,
                PrivateArea,
                PublicSpace,
                Hotel
            };

            public const string FreeArea = "free-area";
            public const string BuisnessLounge = "buisness-lounge";
            public const string Workstation = "workstation";
            public const string Desktop = "desktop";
            public const string MeetingRoom = "meeting-room";
            public const string SeminarRoom = "conf-room";
            public const string VisioRoom = "visio-room";

            public static List<string> LocalisationOfferTypes = new List<string>
            {
                FreeArea ,
                BuisnessLounge,
                Workstation ,
                Desktop ,
                MeetingRoom ,
                SeminarRoom ,
                VisioRoom 
            };

            public const string Page = "page";
            public const string Place = "place";
            public const string Boundary = "boundary";
            public const string OfferType = "offerType";
            public const string SearchOfferType = "offer-type";
            public const string GlobalType = "global-type";
            public const string Latitude = "lat";
            public const string Longitude = "lng";
            public const string PlaceName = "name";
            public const string Order = "order";
            public const string Search = "search";
			public const string View = "view";
        }

		public static class UrlConstants
		{
			public const string JTPath = "http://vimeo.com/29038745";
			public const string eWorkyFacebook = "http://www.facebook.com/eWorkypage";
			public const string eWorkyTwitter = "http://www.twitter.com/#!/eWorky";
			public const string jquery = "";//"https://ajax.googleapis.com/ajax/libs/jquery/1.6.1/jquery.min.js";
			public const string jqueryui = "https://ajax.googleapis.com/ajax/libs/jqueryui/1.8.13/jquery-ui.min.js";
            public const string paypal = "https://www.paypal.com";
		}

		public static class BlogConstants
		{
			public const int CacheDaySpan = 1;
			public const int MaxBlogItem = 4;
		}

        public static class ApiConstants
        {
            public static char[] ArrayTrim = { '[', ']' };
            public const int TakeCount = 20;
        }

		public static class AdminConstants
		{
			public const string AdminRole = "AdminRole";
			public const string AdminPass = "Admin_Pass";
			public const string AdminMail = "admin@eworky.com";
			public const string DummyPassword = "000000";
		}

        public static class BackOfficeConstants
        {
            public const string BackOfficeRole = "BackOfficeRole";
        }

		public static class AzureConstants
		{
			public const string DataConnectionString = "DataConnectionString";
		}

		public static class Constants
		{
			public const int MinRange = 1;
			public const int MaxRange = 10000;
			public const int MaxLengh = 256;
            public const int MaxFileSize = 10485760;
			public const int MinRequiredPasswordLength = 6;
			public const int UnselectedItem = -1;
			public const int OneMo = 1048576;
            public const int PageSize = 25;
		}

		public static class TempDataConstants
		{
			public const string Info = "info";
            public const string NewLocalisationId = "NewLocalisationId";
		}

        public static class WidgetConstants
        {
            #region Widget kind

            public const string KindFinder = "finder";
            public const string KindFinderFetched = "finder-fetched";
            public const string KindDetail = "detail";

            #endregion

            #region Data Keys

            public const string Theme = "theme";
            public const string Kind = "kind";
            public const string AppKey = "app-key";
            public const string Type = "filter-type";
            public const string Country = "filter-country";
            public const string ItemId = "item-id";
            public const string PlaceHolder = "placeholder";
            public const string LocaleCulture = "locale-culture";
            public const string ForceNonMobile = "force-non-mobile";
            public const string ParamToKeep = "theme,app-key,filter-type,filter-country,item-id,placeholder,locale-culture,force-non-mobile";

            #endregion

            #region Themes

            public const string EworkyClass = "eworky-widget";
            public const string SwcClass = "swc";

            #endregion
        }

        /// <summary>
        /// Get the display name of enums, from resource file
        /// </summary>
        /// <param name="enumType">the Type of the enum</param>
        /// <returns>a dictionary key : enum, value : displayName</returns>
        public static Dictionary<int, string> GetEnumDescriptors(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ApplicationException("GetEnumAttributes does not support non-enum types");

            var toRet = new Dictionary<int, string>();
            var attribute = enumType.GetCustomAttributes(typeof(LocalizedEnumAttribute), false);
            if (attribute == null || attribute.Count() == 0)
                return toRet;
            var localizeAttribute = attribute[0] as LocalizedEnumAttribute;
            if (localizeAttribute == null)
                return toRet;

            foreach (var field in enumType.GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public))
            {
                var enumVal = (int)field.GetValue(null);
                var enumStr = Enum.GetName(enumType, enumVal);

                var nameProperty = localizeAttribute.ResourceType.GetProperty(enumStr, BindingFlags.Static | BindingFlags.Public);
                if (nameProperty != null)
                {
                    enumStr = (string)nameProperty.GetValue(nameProperty.DeclaringType, null);
                }
                toRet.Add(enumVal, enumStr);
            }
            return toRet;
        }

        /// <summary>
        /// Get the display name of enums, from resource file
        /// </summary>
        /// <param name="enumType">the Type of the enum</param>
        /// <param name="resourceType">the Type of the resource</param>
        /// <returns>a dictionary key : enum, value : displayName</returns>
        public static Dictionary<string, string> GetEnumDisplayName(Type enumType, Type resourceType, string emptyOne)
        {
            if (!enumType.IsEnum)
                throw new ApplicationException("GetEnumDisplayName does not support non-enum types");

            var toRet = new Dictionary<string, string> { { "", emptyOne } };

            foreach (var field in enumType.GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public))
            {
                var enumVal = field.GetValue(null);
                var enumStr = Enum.GetName(enumType, enumVal);

                var nameProperty = resourceType.GetProperty(enumStr, BindingFlags.Static | BindingFlags.Public);
                if (nameProperty != null)
                {
                    enumStr = (string)nameProperty.GetValue(nameProperty.DeclaringType, null);
                }
                toRet.Add(enumVal.ToString(), enumStr);
            }
            return toRet;
        }

        public const double EarthRadius = 6376.5; //kms

        /// <summary>
        /// Compute distance between two points via haversine formula
        /// </summary>
        /// <param name="lat1">latitude of the first point</param>
        /// <param name="lng1">longitude of the first point</param>
        /// <param name="lat2">latitude of the second point</param>
        /// <param name="lng2">longitude of the second point</param>
        /// <returns>distance between the points</returns>
        public static double GetDistanceBetween(double lat1, double lng1, double lat2, double lng2)
        {
            var dLat1InRad = lat1 * Math.PI / 180.0;
            var dLng1InRad = lng1 * Math.PI / 180.0;
            var dLat2InRad = lat2 * Math.PI / 180.0;
            var dLng2InRad = lng2 * Math.PI / 180.0;

            var dLatitude = dLat2InRad - dLat1InRad;
            var dLongitude = dLng2InRad - dLng1InRad;

            /* Intermediate result a. */
            var a = Math.Pow(Math.Sin(dLatitude / 2.0), 2) + Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2.0), 2);
            /* Intermediate result c (great circle distance in Radians). */
            var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            var dDistance = EarthRadius * CultureHelpers.GetDistanceFactor() * c;
            return dDistance;
        }

		/// <summary>
		/// Get similitude between two string
		/// </summary>
		/// <param name="reference">the reference string</param>
		/// <param name="toCompare">the string to evaluate</param>
		/// <returns>index telling how similar the strings are</returns>
		public static int GetSimilitude(string reference, string toCompare)
		{
			var toRet = 0;
			if (string.IsNullOrEmpty(reference) || string.IsNullOrEmpty(toCompare))
				return -1;

			var splitted = reference.ToLower().Split(' ');
			toCompare = toCompare.ToLower();
			foreach (var part in splitted)
			{
				if (toCompare.Contains(part))
					toRet++;
			}

			return toRet;
		}

		public static string Nl2Br(string text)
		{
			var builder = new System.Text.StringBuilder();
			if (string.IsNullOrEmpty(text))
				return builder.ToString();
			string[] lines = text.Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				if (i > 0)
					builder.Append("<br/>");
				builder.Append(lines[i]);
			}
			return builder.ToString();
		}

        /// <summary>
        /// Get value corresponding to an url param key
        /// either in path or query string
        /// </summary>
        /// <param name="request">request to parse</param>
        /// <param name="key">parameter key</param>
        /// <param name="value">found value</param>
        /// <returns>true if have value for this param</returns>
        public static bool GetRequestValue(HttpRequestBase request, string key, ref string value)
        {
            value = string.Empty;
            if (request.Params[key] != null)
            {
                value = request.Params[key];
                return true;
            }
            else if (request.RequestContext.RouteData.Values[key] != null)
            {
                value = request.RequestContext.RouteData.Values[key] as string;
                return true;
            }
            else
                return false;
        }

		/// <summary>
		/// Get seo compliant version of a string
		/// </summary>
		/// <param name="title">string to transform</param>
		/// <returns>correct string</returns>
		public static string GetSeoString(string title)
		{
			// make it all lower case
			title = title.ToLower();
			// remove entities
			title = Regex.Replace(title, @"&\w+;", "");
			// remove tabs
			title = Regex.Replace(title, @"\t", "");
			// remove anything that is not letters, numbers, dash, or space, accent letter
			title = Regex.Replace(title, @"[^a-z0-9ÁÀÂÄÉÈÊËÍÌÎÏÓÒÔÖÚÙÛÜ0Çàáâãäåòóôõöøèéêëçìíîïùúûüÿñ\-\s]", "");
			// replace spaces
			title = title.Replace(' ', '-');
			// collapse dashes
			title = Regex.Replace(title, @"-{2,}", "-");
			// trim excessive dashes at the beginning
			title = title.TrimStart(new[] { '-' });
			// if it's too long, clip it
			if (title.Length > 80)
				title = title.Substring(0, 79);
			// remove trailing dashes
			title = title.TrimEnd(new[] { '-' });
			return title;
        }

        public static IList<T> Shuffle<T>(IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
		}

		#region Image

		public class ImageSize
		{
			public int Width { get; set; }
			public int Height { get; set; }
			public int TWidth { get; set; }
			public int THeight { get; set; }

            public static ImageSize WelcomePeople = new ImageSize
            {
                Width = 460,
                Height = 310,
                TWidth = 46,
                THeight = 31
            };

            public static ImageSize MemberAvatar = new ImageSize
            {
                Width = 250,
                Height = 250,
                TWidth = 80,
                THeight = 80
            };

            public static ImageSize Localisation = new ImageSize
            {
                Width = 600,
                Height = 400,
                TWidth = 180,
                THeight = 120
            };
		}
		/// <summary>
		/// Crop image to fit a rectangle
		/// </summary>
		/// <param name="img">image to crop</param>
		/// <param name="cropArea">rectangle to fit</param>
		/// <returns>cropped image</returns>
		static Image Crop(Image img, Rectangle cropArea)
		{
			Bitmap bmpImage = new Bitmap(img);
			Bitmap bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
			return (Image)(bmpCrop);
		}

		/// <summary>
		/// Resize an image to desired height and width
		/// </summary>
		/// <param name="fs">he image to resize</param>
		/// <param name="width">target width</param>
		/// <param name="height">target height</param>
		/// <returns>resized image</returns>
		public static Image Resize(Image image, int width, int height)
		{
			float scaleWidth = ((float)width / (float)image.Width);
			float scaleHeight = ((float)height / (float)image.Height);
			var scale = Math.Max(scaleWidth, scaleHeight);

			int destWidth = (int)((image.Width * scale) + 0.5);
			int destHeight = (int)((image.Height * scale) + 0.5);

			Bitmap bitmap = new Bitmap(destWidth, destHeight, PixelFormat.Format24bppRgb);
			bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.Clear(Color.White);
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

				graphics.DrawImage(image,
						new Rectangle(0, 0, destWidth, destHeight),
						new Rectangle(0, 0, image.Width, image.Height),
						GraphicsUnit.Pixel);
			}
			return bitmap;
		}

		/// <summary>
		/// format image to specified dimentions
		/// </summary>
		/// <param name="image">image to format</param>
		/// <param name="width">desired width</param>
		/// <param name="height">desired height</param>
		/// <returns>formated image</returns>
		public static Image Format(Image image, int width, int height)
		{
			//resize
			var resized = Resize(image, width, height);

			//crop
			var cropped = Crop(resized, new Rectangle(0, 0, width, height));

			return cropped;
		}

		#endregion

		/// <summary>
		/// Ensures that local times are converted to UTC times.  Unspecified kinds are recast to UTC with no conversion.
		/// </summary>
		/// <param name="value">The date-time to convert.</param>
		/// <returns>The date-time in UTC time.</returns>
		public static DateTime AsUtc(this DateTime value)
		{
			if (value.Kind == DateTimeKind.Unspecified)
			{
				return new DateTime(value.Ticks, DateTimeKind.Utc);
			}

			return value.ToUniversalTime();
		}

        public static string ToApiDate(this DateTime value)
        {
            return value.ToString("R");
        }

        public static string ToApiDate(this DateTime? value)
        {
            if (!value.HasValue)
                return "";
            return value.Value.ToString("R");
        }

        public static IEnumerable<int> SplitAndParse(this string array)
        {
            if (string.IsNullOrEmpty(array))
                return new List<int>();

            var typesArray = array.Trim(MiscHelpers.ApiConstants.ArrayTrim).Split(',').Select(ot =>
            {
                int value;
                bool success = int.TryParse(ot, out value);
                return new { value, success };
            }).Where(pair => pair.success)
              .Select(pair => pair.value).ToList();

            return typesArray;
        }

        public static string GetApiString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "N/A";

            return value;
        }
	}
}
