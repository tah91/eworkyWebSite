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
                return string.Empty;

            if (image.StartsWith("http://") || image.StartsWith("https://"))
            {
                return image;
            }

			var fileName = BuildPath(image, folder, thumb);
            if (RoleEnvironment.IsAvailable)
            {
                var blobContainer = CloudBlobContainerSingleton.Instance.BlobContainer;
                if (blobContainer == null)
                    return string.Empty;
                var blob = blobContainer.GetBlobReference(fileName);
                return blob.Uri.AbsoluteUri;
            }
            else
            {
				return string.Join(_PathSeparator, _UserImgFolder, fileName);
            }
        }

        /// <summary>
        /// Upload file to server (file system or blob if azure)
        /// and return the name of the file created
        /// </summary>
        /// <param name="postedFile">file to upload</param>
        /// <returns>the file name</returns>
        public static string UploadFile(this Controller controller, HttpPostedFileBase postedFile, string folder = null)
        {
            if (postedFile == null)
                return string.Empty;
            var ext = Path.GetExtension(postedFile.FileName);
            var nameToSave = String.Format("{0:yyyy-MM-dd_hh-mm-ss-ffff}", DateTime.Now) + ext;
            using (var fs = postedFile.InputStream)
            {
				SaveFile(controller, fs, nameToSave, ext, postedFile.ContentType, folder);
                SaveFile(controller, fs, nameToSave, ext, postedFile.ContentType,folder, true);
            }
            return nameToSave;
        }

        static void SaveFile(Controller controller, Stream fs, string nameToSave, string ext, string contentType,string folder, bool thumb = false)
        {
            var width = thumb ? _ThumbMaxWidth : _MaxWidth;
			var fileName = BuildPath(nameToSave, folder, thumb);
            using (var bmp = MiscHelpers.Resize(fs, width, width))
            {
                if (RoleEnvironment.IsAvailable)
                {
                    if (CloudBlobContainerSingleton.Instance.BlobContainer == null)
                        return;

                    var blob = CloudBlobContainerSingleton.Instance.BlobContainer.GetBlobReference(fileName);

                    var ms = new MemoryStream();
                    bmp.Save(ms, _JpegCodecInfo, _EncoderParameters);
                    ms.Seek(0, SeekOrigin.Begin);
                    blob.UploadFromStream(ms);
                    blob.Metadata["FileName"] = fileName;
                    blob.Metadata["FileExtension"] = ext;
                    blob.SetMetadata();
                    blob.Properties.ContentType = contentType;
                    blob.SetProperties();
                }
                else
                {
                    var destinationFolder = controller.Server.MapPath(_UserImgFolder);
					var path = string.Join(_PathSeparator, destinationFolder, fileName);
                    bmp.Save(path, _JpegCodecInfo, _EncoderParameters);
                }
            }
        }
    }
}