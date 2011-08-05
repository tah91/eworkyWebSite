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

namespace Worki.Web.Helpers
{
    /// <summary>
    /// static classe containings utility methods for controllers
    /// </summary>
    public static class ControllerHelpers
    {
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

        public static string GetSeoTitle(string title)
        {
            // make it all lower case
            title = title.ToLower();
            // remove entities
            title = Regex.Replace(title, @"&\w+;", "");
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
            if (controller == null || visitor==null)
                return;

            var urlHelper = new UrlHelper(controller.ControllerContext.RequestContext);
            var registerUrl = urlHelper.Action(MVC.Account.ActionNames.Register, MVC.Account.Name, new { id = visitor.Id }, "http");
            var createUrl = urlHelper.Action(MVC.Localisation.ActionNames.Create, MVC.Localisation.Name, null, "http");
            var searchUrl = urlHelper.Action(MVC.Search.ActionNames.FullSearch, MVC.Search.Name, null, "http");

            var subject = Worki.Resources.Models.Account.AccountModels.EWBegining;

            TagBuilder line = new TagBuilder("p");
            line.InnerHtml = Worki.Resources.Models.Account.AccountModels.Hello;
            var content = line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = Worki.Resources.Models.Account.AccountModels.EWFirstOnWorkplaceSearch;
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = Worki.Resources.Models.Account.AccountModels.BothAdding;
            content += line.ToString();

            line = new TagBuilder("p");
            TagBuilder link = new TagBuilder("a");
            link.MergeAttribute("href", registerUrl);
            link.InnerHtml = Worki.Resources.Models.Account.AccountModels.CompletYourProfil;
            line.InnerHtml += String.Format(Worki.Resources.Models.Account.AccountModels.BePartOfGroupe,link.ToString());
            content += line.ToString();

            line = new TagBuilder("p");
            link = new TagBuilder("a");
            link.MergeAttribute("href", createUrl);
            link.InnerHtml = Worki.Resources.Models.Account.AccountModels.AddWorkplace;
            line.InnerHtml += String.Format(Worki.Resources.Models.Account.AccountModels.IfYouFindThePlaceToWork,link.ToString());
            content += line.ToString();

            line = new TagBuilder("p");
            link = new TagBuilder("a");
            link.MergeAttribute("href", searchUrl);
            link.InnerHtml = Worki.Resources.Models.Account.AccountModels.SearhPlace;
            line.InnerHtml = String.Format(Worki.Resources.Models.Account.AccountModels.CommentWhatYouTest,link.ToString());
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = Worki.Resources.Models.Account.AccountModels.KeepInTouch;
            content += line.ToString();

            line = new TagBuilder("p");
            link = new TagBuilder("a");
            link.MergeAttribute("href", "http://www.facebook.com/pages/eWorky/226917517335276");
            link.InnerHtml = Worki.Resources.Models.Account.AccountModels.Facebook;
            
            TagBuilder link2 = new TagBuilder("a");
            link2 = new TagBuilder("a");
            link2.MergeAttribute("href", "http://twitter.com/#!/eWorky");
            link2.InnerHtml = Worki.Resources.Models.Account.AccountModels.Twitter;
            line.InnerHtml = String.Format(Worki.Resources.Models.Account.AccountModels.FollowUs, link.ToString(), link2.ToString());
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = Worki.Resources.Models.Account.AccountModels.SeeYouSoon;
            content += line.ToString();

            emailService.Send(EmailService.ContactMail, "eWorky Team", subject, content, true, visitor.Email);
        }

        public static void SendRegisterEmail(this Controller controller, IEmailService emailService, Member member)
        {
            if (controller == null || member == null)
                return;

            var urlHelper = new UrlHelper(controller.ControllerContext.RequestContext);
            var activationLink = urlHelper.Action(MVC.Account.ActionNames.Activate, MVC.Account.Name, new { userName = member.Email, key = member.EmailKey }, "http");

            var subject = Worki.Resources.Models.Account.AccountModels.ActivationAccount;
            TagBuilder line = new TagBuilder("p");
            line.InnerHtml = String.Format(Worki.Resources.Models.Account.AccountModels.HelloMember, member.MemberMainData.FirstName);
            var content = line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = Worki.Resources.Models.Account.AccountModels.MemberAfterActivation;
            content += line.ToString();

            line = new TagBuilder("p");

            TagBuilder link = new TagBuilder("a");
            link.MergeAttribute("href", activationLink);
            link.InnerHtml = activationLink;
            line.InnerHtml = String.Format(Worki.Resources.Models.Account.AccountModels.ValidationInscription, link.ToString());
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = String.Format(Worki.Resources.Models.Account.AccountModels.ConnectionEasy, member.Email);
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = Worki.Resources.Models.Account.AccountModels.SeeYouSoon;
            content += line.ToString();

            emailService.Send(EmailService.ContactMail, "eWorky Team", subject, content, true, member.Email);
        }

        public static void SendResetPasswordEmail(this Controller controller, IEmailService emailService, Member member, string password)
        {
            if (controller == null || member == null)
                return;

            var urlHelper = new UrlHelper(controller.ControllerContext.RequestContext);
            var changePassLink = urlHelper.Action(MVC.Account.ActionNames.ChangePassword, MVC.Account.Name, new { userName = member.Email, key = member.EmailKey }, "http");

            var subject = Worki.Resources.Models.Account.AccountModels.ResetPassword;

            TagBuilder line = new TagBuilder("p");
            line.InnerHtml = String.Format(Worki.Resources.Models.Account.AccountModels.HelloMember, member.MemberMainData.FirstName);
            var content = line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = Worki.Resources.Models.Account.AccountModels.YourNewLogin;
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = Worki.Resources.Models.Account.AccountModels.LoginForm;
            line.InnerHtml += member.Email;
            line.InnerHtml += "<br />";
            line.InnerHtml += Worki.Resources.Models.Account.AccountModels.PasswordForm;
            line.InnerHtml += password;
            content += line.ToString();

            line = new TagBuilder("p");
            TagBuilder link = new TagBuilder("a");
            link.MergeAttribute("href", changePassLink);
            link.InnerHtml = Worki.Resources.Models.Account.AccountModels.ChangePassword;
            line.InnerHtml = String.Format(Worki.Resources.Models.Account.AccountModels.AfterConnection, link.ToString());
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = Worki.Resources.Models.Account.AccountModels.SeeYouSoon;
            content += line.ToString();

            emailService.Send(EmailService.ContactMail, "eWorky Team", subject, content, true, member.Email);
        }

        /// <summary>
        /// Upload File to destination folder
        /// </summary>
        /// <param name="controller">Controller to get the folder Path</param>
        /// <param name="postedFile">File to upload</param>
        /// <returns>Name of the file created on server side</returns>
        public static string UploadFile(this Controller controller, HttpPostedFileBase postedFile)
        {
            var userImgFolder = ConfigurationManager.AppSettings["UserImageFolder"];
            var destinationFolder = controller.Server.MapPath(userImgFolder);
            return UploadFile(postedFile, destinationFolder);
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
				using (var bmp = MiscHelpers.Resize(fs, maxWidth, maxWidth))
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
    }
}