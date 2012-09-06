using System;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Worki.Data.Models;
using Worki.Infrastructure.Email;
using Worki.Infrastructure.Helpers;
using Worki.Service;
using Worki.Web.Singletons;
using System.Drawing;
using System.Net;

namespace Worki.Web.Helpers
{
    /// <summary>
    /// static classe containings utility methods for controllers
    /// </summary>
    public static class ControllerHelpers
    {
		const string _PathSeparator = "/";
        const string Thumbnail = "thumbnail";
        static int _MaxWidth;
        static int _ThumbMaxWidth;
        static string _UserImgFolder;
        static ImageCodecInfo _JpegCodecInfo;
        static EncoderParameters _EncoderParameters;

        static ControllerHelpers()
        {
            _MaxWidth = int.Parse(ConfigurationManager.AppSettings["UploadFileMaxWidth"]);
            _ThumbMaxWidth = int.Parse(ConfigurationManager.AppSettings["ThumbFileMaxWidth"]);
            _UserImgFolder = ConfigurationManager.AppSettings["UserImageFolder"];
            _JpegCodecInfo = ImageCodecInfo.GetImageEncoders().Where(codec => codec.MimeType == "image/jpeg").First();
            var qualityEncoder = Encoder.Quality;
            var quality = 100;
            var ratio = new EncoderParameter(qualityEncoder, quality);
            _EncoderParameters = new EncoderParameters(1);
            _EncoderParameters.Param[0] = ratio;
        }

		static string BuildPath(string path, string folder, bool thumb)
		{
			if (thumb)
				path = string.Join(_PathSeparator, Thumbnail, path);
			if (!string.IsNullOrEmpty(folder))
				path = string.Join(_PathSeparator, folder, path);

			return path;
		}

        /// <summary>
        /// Get the path of an image on a server
        /// if on azure, get the corresponding url
        /// </summary>
        /// <param name="image">name of the image</param>
        /// <returns>full path of the image</returns>
		public static string GetUserImagePath(string image, bool thumb = false, string folder = null)
        {
            if (string.IsNullOrEmpty(image))
                return null;

            if (image.StartsWith("http://") || image.StartsWith("https://"))
            {
                return image;
            }

			var fileName = BuildPath(image, folder, thumb);
            if (RoleEnvironment.IsAvailable)
            {
                var blobContainer = CloudBlobContainerSingleton.Instance.BlobContainer;
                if (blobContainer == null)
                    return null;
                var blob = blobContainer.GetBlobReference(fileName);
                return blob.Uri.AbsoluteUri;
            }
            else
            {
				return string.Join(_PathSeparator, _UserImgFolder, fileName);
            }
        }

		static Stream CopyAndClose(Stream inputStream)
		{
			const int readSize = 256;
			byte[] buffer = new byte[readSize];
			MemoryStream ms = new MemoryStream();

			int count = inputStream.Read(buffer, 0, readSize);
			while (count > 0)
			{
				ms.Write(buffer, 0, count);
				count = inputStream.Read(buffer, 0, readSize);
			}
			ms.Position = 0;
			inputStream.Close();
			return ms;
		}

		/// <summary>
		/// Upload file to server (file system or blob if azure)
		/// and return the name of the file created
		/// </summary>
		/// <param name="stream">stream to upload</param>
		/// <returns>the file name</returns>
		public static string UploadFile(this Controller controller, string url, MiscHelpers.ImageSize imageSize, string folder = null)
		{
			if (string.IsNullOrEmpty(url))
				return string.Empty;

			var ext = ".jpg";
			var contentType = "image/jpeg";
			var fileName = String.Format("{0:yyyy-MM-dd_hh-mm-ss-ffff}", DateTime.UtcNow) + ext;

			//Create a WebRequest to get the file
			HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(url);

			//Create a response for this request
			HttpWebResponse fileResp = (HttpWebResponse)fileReq.GetResponse();

			if (fileReq.ContentLength > 0)
				fileResp.ContentLength = fileReq.ContentLength;

			using (var fs = CopyAndClose(fileResp.GetResponseStream()))
			{
				SaveFile(controller, fs, fileName, ext, contentType, folder, imageSize.Width, imageSize.Height);
				fs.Position = 0;
				SaveFile(controller, fs, fileName, ext, contentType, folder, imageSize.TWidth, imageSize.THeight, true);
			}
			return fileName;
		}

        /// <summary>
        /// Upload file to server (file system or blob if azure)
        /// and return the name of the file created
        /// </summary>
        /// <param name="postedFile">file to upload</param>
        /// <returns>the file name</returns>
        public static string UploadFile(this Controller controller, HttpPostedFileBase postedFile, MiscHelpers.ImageSize imageSize, string folder = null)
        {
            if (postedFile == null)
                return string.Empty;

            var ext = Path.GetExtension(postedFile.FileName);
            var fileName = String.Format("{0:yyyy-MM-dd_hh-mm-ss-ffff}", DateTime.UtcNow) + ext;
            using (var fs = postedFile.InputStream)
            {
				SaveFile(controller, fs, fileName, ext, postedFile.ContentType, folder, imageSize.Width, imageSize.Height);
				SaveFile(controller, fs, fileName, ext, postedFile.ContentType, folder, imageSize.TWidth, imageSize.THeight, true);
            }
			return fileName;
        }

		static void SaveFile(Controller controller, Stream stream, string nameToSave, string ext, string contentType, string folder, int width, int height, bool thumb = false)
        {
			var fileName = BuildPath(nameToSave, folder, thumb);
            using (var image = Image.FromStream(stream))
            {
				var formatted = MiscHelpers.Format(image, width, height);

                if (RoleEnvironment.IsAvailable)
                {
                    if (CloudBlobContainerSingleton.Instance.BlobContainer == null)
                        return;

                    var blob = CloudBlobContainerSingleton.Instance.BlobContainer.GetBlobReference(fileName);

					using (var ms = new MemoryStream())
					{
						formatted.Save(ms, _JpegCodecInfo, _EncoderParameters);
						ms.Seek(0, SeekOrigin.Begin);
						blob.UploadFromStream(ms);
						blob.Metadata["FileName"] = fileName;
						blob.Metadata["FileExtension"] = ext;
						blob.SetMetadata();
						blob.Properties.ContentType = contentType;
						blob.SetProperties();
					}
                }
                else
                {
                    var destinationFolder = controller.Server.MapPath(_UserImgFolder);
					var path = string.Join(_PathSeparator, destinationFolder, fileName);
					var dir = Path.GetDirectoryName(path);
					Directory.CreateDirectory(dir);
					formatted.Save(path, _JpegCodecInfo, _EncoderParameters);
                }
            }
        }

        public static string RenderRazorViewToString(this Controller controller, string viewName, object model, bool isPartial = true)
        {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = isPartial ?    ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName) : 
                                                ViewEngines.Engines.FindView(controller.ControllerContext, viewName, "");
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}