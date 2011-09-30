﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Worki.Infrastructure.Helpers
{
    public static class MiscHelpers
    {
        #region Email

        public const string EmailView = "Email";
        public const string EmailOwnerView = "EmailOwner";
        public const string ContactDisplayName = "eWorky Team";
        //public const string ContactMail = "t.iftikhar@hotmail.fr";
        //public const string BookingMail = "t.iftikhar@hotmail.fr";
        public const string ContactMail = "contact@eworky.com";
        public const string BookingMail = "team@eworky.com";
        public const string DummyPassword = "000000";

        #endregion

        public const string AdminRole = "AdminRole";
		public const string AdminUser = "Admin";
		public const string AdminPass = "Admin_Pass";
		public const string AdminMail = "admin@eworky.com";

		public const string DataConnectionString = "DataConnectionString";

		public const int MinRange = 1;
		public const int MaxRange = 10000;
        public const int MaxLengh = 256;
		public const string jquery = "";//"https://ajax.googleapis.com/ajax/libs/jquery/1.6.1/jquery.min.js";
		public const string jqueryui = "https://ajax.googleapis.com/ajax/libs/jqueryui/1.8.13/jquery-ui.min.js";
		public const int MaxFileSize = 3145728;
		public const int OneMo = 1048576;
		public const int MinRequiredPasswordLength = 6;

        public const int UnselectedItem = -1;

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

			if (scale == 1.0 && fs.Length < OneMo)
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

        const string _FeaturePrefix = "f_";

        /// <summary>
        /// return a string for the feature
        /// to put in url query string
        /// </summary>
        /// <param name="featureId">the feature id</param>
        /// <returns>a string</returns>
        public static string FeatureToString(int featureId)
        {
            return _FeaturePrefix + featureId.ToString();
        }

        /// <summary>
        /// get the feature id from the string of query string
        /// </summary>
        /// <param name="featureStr">the string to recover</param>
        /// <returns>the feature id</returns>
        public static int FeatureFromString(string featureStr)
        {
            var toRet = -1;
            if (string.IsNullOrEmpty(featureStr))
                return toRet;
            var idStr = featureStr.Replace(_FeaturePrefix, string.Empty);
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
        public static IEnumerable<int> GetFeatureIds(List<string> strings)
        {
            foreach (var item in strings)
            {
                var id = FeatureFromString(item);
                if (id != -1)
                    yield return id;
            }
        }
    }
}
