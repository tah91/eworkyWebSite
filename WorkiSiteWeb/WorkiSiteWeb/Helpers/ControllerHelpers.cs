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
        const char _PathSeparator = '/';
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

        public static string GetJSDouble(double val)
        {
            var str = val.ToString();
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return str.Replace(',', '.');
        }

        public static double GetCSharpDouble(string val)
        {
            if (string.IsNullOrEmpty(val))
                return 0;
            try
            {
                return Convert.ToDouble(val);
            }
            catch
            {
                return 0;
            }
        }

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

        public static string ResolveServerUrl(string serverUrl, bool forceHttps)
        {
            if (serverUrl.IndexOf("://") > -1)
                return serverUrl;

            string newUrl = serverUrl;
            Uri originalUri = HttpContext.Current.Request.Url;
            newUrl = (forceHttps ? "https" : originalUri.Scheme) +
                     "://" + originalUri.Authority + newUrl;
            return newUrl;
        }

        public static void SendVisitorMail(this Controller controller, IEmailService emailService, Visitor visitor)
        {
            if (controller == null || visitor == null)
                return;

            var urlHelper = new UrlHelper(controller.ControllerContext.RequestContext);
            var registerUrl = urlHelper.AbsoluteAction(MVC.Account.ActionNames.Register, MVC.Account.Name, new { id = visitor.Id });
            var createUrl = urlHelper.AbsoluteAction(MVC.Localisation.ActionNames.Create, MVC.Localisation.Name, null);
			var searchUrl = urlHelper.AbsoluteAction(MVC.Localisation.ActionNames.FullSearch, MVC.Localisation.Name, null);

            //var subject = Worki.Resources.Models.Account.AccountModels.EWBegining;

            //TagBuilder line = new TagBuilder("p");
            //line.InnerHtml = Worki.Resources.Models.Account.AccountModels.Hello;
            //var content = line.ToString();

            //line = new TagBuilder("p");
            //line.InnerHtml = Worki.Resources.Models.Account.AccountModels.EWFirstOnWorkplaceSearch;
            //content += line.ToString();

            //line = new TagBuilder("p");
            //line.InnerHtml = Worki.Resources.Models.Account.AccountModels.BothAdding;
            //content += line.ToString();

            //line = new TagBuilder("p");
            //TagBuilder link = new TagBuilder("a");
            //link.MergeAttribute("href", registerUrl);
            //link.InnerHtml = Worki.Resources.Models.Account.AccountModels.CompletYourProfil;
            //line.InnerHtml += String.Format(Worki.Resources.Models.Account.AccountModels.BePartOfGroupe, link.ToString());
            //content += line.ToString();

            //line = new TagBuilder("p");
            //link = new TagBuilder("a");
            //link.MergeAttribute("href", createUrl);
            //link.InnerHtml = Worki.Resources.Models.Account.AccountModels.AddWorkplace;
            //line.InnerHtml += String.Format(Worki.Resources.Models.Account.AccountModels.IfYouFindThePlaceToWork, link.ToString());
            //content += line.ToString();

            //line = new TagBuilder("p");
            //link = new TagBuilder("a");
            //link.MergeAttribute("href", searchUrl);
            //link.InnerHtml = Worki.Resources.Models.Account.AccountModels.SearhPlace;
            //line.InnerHtml = String.Format(Worki.Resources.Models.Account.AccountModels.CommentWhatYouTest, link.ToString());
            //content += line.ToString();

            //line = new TagBuilder("p");
            //line.InnerHtml = Worki.Resources.Models.Account.AccountModels.KeepInTouch;
            //content += line.ToString();

            //line = new TagBuilder("p");
            //link = new TagBuilder("a");
            //link.MergeAttribute("href", "http://www.facebook.com/pages/eWorky/226917517335276");
            //link.InnerHtml = Worki.Resources.Models.Account.AccountModels.Facebook;

            //TagBuilder link2 = new TagBuilder("a");
            //link2 = new TagBuilder("a");
            //link2.MergeAttribute("href", "http://twitter.com/#!/eWorky");
            //link2.InnerHtml = Worki.Resources.Models.Account.AccountModels.Twitter;
            //line.InnerHtml = String.Format(Worki.Resources.Models.Account.AccountModels.FollowUs, link.ToString(), link2.ToString());
            //content += line.ToString();

            //line = new TagBuilder("p");
            //line.InnerHtml = Worki.Resources.Models.Account.AccountModels.SeeYouSoon;
            //content += line.ToString();

            //emailService.Send(EmailService.ContactMail, "eWorky Team", subject, content, true, visitor.Email);
        }

        /// <summary>
        /// Get member display name from authentication data
        /// </summary>
        /// <param name="ident">the identity containing authentication data</param>
        /// <returns>display name</returns>
        public static string GetIdentityDisplayName(IIdentity ident)
        {
            FormsIdentity formIdent = ident as FormsIdentity;
            if (formIdent == null)
                return string.Empty;
            var ticket = formIdent.Ticket;
            return Member.GetNameFromUserData(ticket.UserData);
        }

        /// <summary>
        /// Get member id from authentication data
        /// </summary>
        /// <param name="ident">the identity containing authentication data</param>
        /// <returns>member id</returns>
        public static int GetIdentityId(IIdentity ident)
        {
            FormsIdentity formIdent = ident as FormsIdentity;
            if (formIdent == null)
                return 0;
            var ticket = formIdent.Ticket;
            return Member.GetIdFromUserData(ticket.UserData);
        }

        /// <summary>
        /// tell if the identiry match the member id
        /// </summary>
        /// <param name="ident">the identity containing authentication data</param>
        /// <param name="id">id of the member to match</param>
        /// <returns>true if match</returns>
        public static bool MatchIdentity(IIdentity ident, int id)
        {
            FormsIdentity formIdent = ident as FormsIdentity;
            if (formIdent == null)
                return false;
            var ticket = formIdent.Ticket;
            return Member.GetIdFromUserData(ticket.UserData) == id;
        }

        /// <summary>
        /// Get the path of an image on a server
        /// if on azure, get the corresponding url
        /// </summary>
        /// <param name="image">name of the image</param>
        /// <returns>full path of the image</returns>
        public static string GetUserImagePath(string image, bool thumb = false)
        {
            if (string.IsNullOrEmpty(image))
                return string.Empty;

            if (image.StartsWith("http://") || image.StartsWith("https://"))
            {
                return image;
            }

            var fileName = ThumbPath(image, thumb); 
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
                return _UserImgFolder + _PathSeparator + fileName;
            }
        }

        /// <summary>
        /// Upload file to server (file system or blob if azure)
        /// and return the name of the file created
        /// </summary>
        /// <param name="postedFile">file to upload</param>
        /// <returns>the file name</returns>
        public static string UploadFile(this Controller controller, HttpPostedFileBase postedFile)
        {
            if (postedFile == null)
                return string.Empty;
            var ext = Path.GetExtension(postedFile.FileName);
            var nameToSave = String.Format("{0:yyyy-MM-dd_hh-mm-ss-ffff}", DateTime.Now) + ext;
            using (var fs = postedFile.InputStream)
            {
                SaveFile(controller, fs, nameToSave, ext, postedFile.ContentType);
                SaveFile(controller, fs, nameToSave, ext, postedFile.ContentType, true);
            }
            return nameToSave;
        }

        static string ThumbPath(string path, bool thumb)
        {
            return thumb ? Thumbnail + _PathSeparator + path : path;
        }

        static void SaveFile(Controller controller, Stream fs, string nameToSave, string ext, string contentType, bool thumb = false)
        {
            var width = thumb ? _ThumbMaxWidth : _MaxWidth;
            var fileName = ThumbPath(nameToSave, thumb);
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
                    var path = destinationFolder + _PathSeparator + fileName;
                    bmp.Save(path, _JpegCodecInfo, _EncoderParameters);
                }
            }
        }
    }
}