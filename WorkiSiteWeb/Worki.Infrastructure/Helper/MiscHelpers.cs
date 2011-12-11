using System;
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
        #region Email

		public static class EmailConstants
		{
			public const string ContactDisplayName = "eWorky";
			public const string WebsiteAddress = "www.eworky.com";
			public static string ContactMail = "contact@eworky.com";
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

		public static class UrlConstants
		{
			public const string BlogApiPath = "http://blog.eworky.com/api/get_recent_posts/";
			public const string BlogUrl = "http://blog.eworky.com";
			public const string JTPath = "http://vimeo.com/29038745";
			public const string eWorkyFacebook = "http://www.facebook.com/pages/eWorky/226917517335276";
			public const string eWorkyTwitter = "http://www.twitter.com/#!/eWorky";
			public const string jquery = "";//"https://ajax.googleapis.com/ajax/libs/jquery/1.6.1/jquery.min.js";
			public const string jqueryui = "https://ajax.googleapis.com/ajax/libs/jqueryui/1.8.13/jquery-ui.min.js";
            public const string paypal = "https://www.paypal.com";
		}

		public static class BlogConstants
		{
			public const string BlogCacheKey = "BlogCacheKey";
			public const int CacheDaySpan = 1;
			public const int MaxBlogItem = 4;
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
			public const int MaxFileSize = 3145728;
			public const int MinRequiredPasswordLength = 6;
			public const int UnselectedItem = -1;
			public const int OneMo = 1048576;
            public const int PageSize = 25;
		}

		public static class TempDataConstants
		{
			public const string Info = "info";
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
		/// Resize a file stream of image to desired height and width
		/// </summary>
		/// <param name="fs">stream containing the image</param>
		/// <param name="width">target width</param>
		/// <param name="height">target height</param>
		/// <returns>resized image</returns>
		public static Image Resize(Stream fs, int width, int height)
		{
			float scale;
			var image = Image.FromStream(fs);
			float scaleWidth = image.Width < width ? 1 : ((float)width / (float)image.Width);
			float scaleHeight = image.Height < height ? 1 : ((float)height / (float)image.Height);
			if (scaleHeight < scaleWidth)
			{
				scale = scaleHeight;
			}
			else
			{
				scale = scaleWidth;
			}

			if (scale == 1.0 && fs.Length < Constants.OneMo)
				return new Bitmap(image);

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

            var dDistance = EarthRadius * c;
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
    }
}
