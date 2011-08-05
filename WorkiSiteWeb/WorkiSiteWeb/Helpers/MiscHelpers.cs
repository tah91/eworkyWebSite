using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Web.Models;
using System.IO;
using System.Net;
using System.Reflection;
using Worki.Web.Infrastructure;
using System.Net.Mail;
using System.Configuration;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using System.Data.Objects.DataClasses;
using System.Linq.Expressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Worki.Web.Helpers
{
    public static class EdmMethods
    {
        [EdmFunction("WorkiDBModel.Store", "DistanceBetween")]
        public static float? DistanceBetween(float Lat1, float Long1, float Lat2, float Long2)
        {
            throw new NotSupportedException("This function is only for L2E query.");
        }
    }

    public static class MiscHelpers
    {
		public const string AdminRole = "AdminRole";
		public const string AdminUser = "Admin";
		public const string AdminPass = "Admin_Pass";
		public const string AdminMail = "admin@eworky.com";

		public const string DataConnectionString = "DataConnectionString";

        public const int MaxLengh = 256;
		public const string jquery = "https://ajax.googleapis.com/ajax/libs/jquery/1.6.1/jquery.min.js";
		public const string jqueryui = "https://ajax.googleapis.com/ajax/libs/jqueryui/1.8.13/jquery-ui.min.js";
		public const int MaxFileSize = 3145728;
		public const int OneMo = 1048576;

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

        public static string GetFeatureDesc(Feature feat, FeatureType featType)
        {
            var first = Enum.GetName(typeof(Feature), feat);
            var sec = Enum.GetName(typeof(FeatureType), featType);
            return first + "-" + sec;
        }

        public static bool GetFeatureDesc(string str, out KeyValuePair<int,int> toFill)
        {
            toFill = new KeyValuePair<int, int>(0, 0);
            var strs = str.Split('-');
            if (strs.Length != 2)
                return false;
            var first = strs[0];
            var sec = strs[1];
            if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(sec))
                return false;
            try
            {
                var firstVal = (int)Enum.Parse(typeof(Feature), first);
                var secVal = (int)Enum.Parse(typeof(FeatureType), sec);
                toFill = new KeyValuePair<int, int>(firstVal, secVal);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
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
            foreach(var item in strings)
            {
                var id = FeatureFromString(item);
                if (id != -1)
                    yield return id;
            }
        }

        static CloudBlobContainer _BlobContainer = null;

		public static CloudBlobContainer GetBlobContainer(string containerName)
		{
			if (_BlobContainer != null)
				return _BlobContainer;

			if (string.IsNullOrEmpty(containerName))
				return null;
			var isDevStore = bool.Parse(ConfigurationManager.AppSettings["IsDevStorage"]);
			var blobStorageAccount = isDevStore ? CloudStorageAccount.DevelopmentStorageAccount : CloudStorageAccount.FromConfigurationSetting(MiscHelpers.DataConnectionString);
			var blobClient = blobStorageAccount.CreateCloudBlobClient();
			var blobContainer = blobClient.GetContainerReference(containerName);
			blobContainer.CreateIfNotExist();
			BlobContainerPermissions blobPermission = new BlobContainerPermissions();
			blobPermission.PublicAccess = BlobContainerPublicAccessType.Container;
			blobContainer.SetPermissions(blobPermission);

			_BlobContainer = blobContainer;
			return blobContainer;
		}

        public const char PathSeparator = '/';
        /// <summary>
        /// Get the path of an image on a server
        /// if on azure, get the corresponding url
        /// </summary>
        /// <param name="image">name of the image</param>
        /// <returns>full path of the image</returns>
        public static string GetUserImagePath(string image)
        {
            if (string.IsNullOrEmpty(image))
                return string.Empty;
			var isAzure = RoleEnvironment.IsAvailable;

            if (isAzure)
            {
                var blobContainerName = ConfigurationManager.AppSettings["AzureBlobContainer"];
                var blobContainer = GetBlobContainer(blobContainerName);
                if (blobContainer == null)
                    return string.Empty;
                var blob = blobContainer.GetBlobReference(image);
                return blob.Uri.AbsoluteUri;
            }
            else
            {
                var userImgFolder = ConfigurationManager.AppSettings["UserImageFolder"];
                return userImgFolder + PathSeparator + image;
            }
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

        /// <summary>
        /// Upload file to server (file system or blob if azure)
        /// and return the name of the file created
        /// </summary>
        /// <param name="file">file to upload</param>
        /// <param name="destinationFolder">destination folder</param>
        /// <returns>the file name</returns>
		public static string UploadFile(HttpPostedFileBase file, string destinationFolder)
		{
			if (file == null)
				return string.Empty;
			var ext = Path.GetExtension(file.FileName);
			var nameToSave = String.Format("{0:yyyy-MM-dd_hh-mm-ss-ffff}", DateTime.Now) + ext;
			var path = destinationFolder + PathSeparator + nameToSave;
			var isAzure = RoleEnvironment.IsAvailable;
			var maxWidth = int.Parse(ConfigurationManager.AppSettings["UploadFileMaxWidth"]);
			var qualityEncoder = Encoder.Quality;
			var quality = 100;
			var ratio = new EncoderParameter(qualityEncoder, quality);
			var codecParams = new EncoderParameters(1);
			codecParams.Param[0] = ratio;
			var jpegCodecInfo = ImageCodecInfo.GetImageEncoders().Where(codec => codec.MimeType == "image/jpeg").First();
			using (var fs = file.InputStream)
			{
				using (var bmp = Resize(fs, maxWidth, maxWidth))
				{
					if (isAzure)
					{
						var blobContainerName = ConfigurationManager.AppSettings["AzureBlobContainer"];
						var blobContainer = GetBlobContainer(blobContainerName);
						if (blobContainer == null)
							return string.Empty;
						var blob = blobContainer.GetBlobReference(nameToSave);

						var ms = new MemoryStream();
						bmp.Save(ms, jpegCodecInfo, codecParams);
						ms.Seek(0, SeekOrigin.Begin);
						blob.UploadFromStream(ms);
						blob.Metadata["FileName"] = nameToSave;
						blob.Metadata["FileExtension"] = ext;
						blob.SetMetadata();
						blob.Properties.ContentType = file.ContentType;
						blob.SetProperties();
					}
					else
					{
						bmp.Save(path, jpegCodecInfo, codecParams);
					}
				}
			}
			return nameToSave;
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
				builder.Append(HttpUtility.HtmlEncode(lines[i]));
			}
			return builder.ToString();
		}
    }
}
