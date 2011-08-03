using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkiSiteWeb.Models;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Configuration;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using WorkiSiteWeb.Infrastructure.Email;
using System.Security.Principal;
using System.Web.Security;

namespace WorkiSiteWeb.Helpers
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

            var subject = WorkiResources.Models.Account.AccountModels.EWBegining;

            TagBuilder line = new TagBuilder("p");
            line.InnerHtml = WorkiResources.Models.Account.AccountModels.Hello;
            var content = line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = WorkiResources.Models.Account.AccountModels.EWFirstOnWorkplaceSearch;
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = WorkiResources.Models.Account.AccountModels.BothAdding;
            content += line.ToString();

            line = new TagBuilder("p");
            TagBuilder link = new TagBuilder("a");
            link.MergeAttribute("href", registerUrl);
            link.InnerHtml = WorkiResources.Models.Account.AccountModels.CompletYourProfil;
            line.InnerHtml += String.Format(WorkiResources.Models.Account.AccountModels.BePartOfGroupe,link.ToString());
            content += line.ToString();

            line = new TagBuilder("p");
            link = new TagBuilder("a");
            link.MergeAttribute("href", createUrl);
            link.InnerHtml = WorkiResources.Models.Account.AccountModels.AddWorkplace;
            line.InnerHtml += String.Format(WorkiResources.Models.Account.AccountModels.IfYouFindThePlaceToWork,link.ToString());
            content += line.ToString();

            line = new TagBuilder("p");
            link = new TagBuilder("a");
            link.MergeAttribute("href", searchUrl);
            link.InnerHtml = WorkiResources.Models.Account.AccountModels.SearhPlace;
            line.InnerHtml = String.Format(WorkiResources.Models.Account.AccountModels.CommentWhatYouTest,link.ToString());
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = WorkiResources.Models.Account.AccountModels.KeepInTouch;
            content += line.ToString();

            line = new TagBuilder("p");
            link = new TagBuilder("a");
            link.MergeAttribute("href", "http://www.facebook.com/pages/eWorky/226917517335276");
            link.InnerHtml = WorkiResources.Models.Account.AccountModels.Facebook;
            
            TagBuilder link2 = new TagBuilder("a");
            link2 = new TagBuilder("a");
            link2.MergeAttribute("href", "http://twitter.com/#!/eWorky");
            link2.InnerHtml = WorkiResources.Models.Account.AccountModels.Twitter;
            line.InnerHtml = String.Format(WorkiResources.Models.Account.AccountModels.FollowUs, link.ToString(), link2.ToString());
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = WorkiResources.Models.Account.AccountModels.SeeYouSoon;
            content += line.ToString();

            emailService.Send(EmailService.ContactMail, "eWorky Team", subject, content, true, visitor.Email);
        }

        public static void SendRegisterEmail(this Controller controller, IEmailService emailService, Member member)
        {
            if (controller == null || member == null)
                return;

            var urlHelper = new UrlHelper(controller.ControllerContext.RequestContext);
            var activationLink = urlHelper.Action(MVC.Account.ActionNames.Activate, MVC.Account.Name, new { userName = member.Email, key = member.EmailKey }, "http");

            var subject = WorkiResources.Models.Account.AccountModels.ActivationAccount;
            TagBuilder line = new TagBuilder("p");
            line.InnerHtml = String.Format(WorkiResources.Models.Account.AccountModels.HelloMember, member.MemberMainData.FirstName);
            var content = line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = WorkiResources.Models.Account.AccountModels.MemberAfterActivation;
            content += line.ToString();

            line = new TagBuilder("p");

            TagBuilder link = new TagBuilder("a");
            link.MergeAttribute("href", activationLink);
            link.InnerHtml = activationLink;
            line.InnerHtml = String.Format(WorkiResources.Models.Account.AccountModels.ValidationInscription, link.ToString());
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = String.Format(WorkiResources.Models.Account.AccountModels.ConnectionEasy, member.Email);
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = WorkiResources.Models.Account.AccountModels.SeeYouSoon;
            content += line.ToString();

            emailService.Send(EmailService.ContactMail, "eWorky Team", subject, content, true, member.Email);
        }

        public static void SendResetPasswordEmail(this Controller controller, IEmailService emailService, Member member, string password)
        {
            if (controller == null || member == null)
                return;

            var urlHelper = new UrlHelper(controller.ControllerContext.RequestContext);
            var changePassLink = urlHelper.Action(MVC.Account.ActionNames.ChangePassword, MVC.Account.Name, new { userName = member.Email, key = member.EmailKey }, "http");

            var subject = WorkiResources.Models.Account.AccountModels.ResetPassword;

            TagBuilder line = new TagBuilder("p");
            line.InnerHtml = String.Format(WorkiResources.Models.Account.AccountModels.HelloMember, member.MemberMainData.FirstName);
            var content = line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = WorkiResources.Models.Account.AccountModels.YourNewLogin;
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = WorkiResources.Models.Account.AccountModels.LoginForm;
            line.InnerHtml += member.Email;
            line.InnerHtml += "<br />";
            line.InnerHtml += WorkiResources.Models.Account.AccountModels.PasswordForm;
            line.InnerHtml += password;
            content += line.ToString();

            line = new TagBuilder("p");
            TagBuilder link = new TagBuilder("a");
            link.MergeAttribute("href", changePassLink);
            link.InnerHtml = WorkiResources.Models.Account.AccountModels.ChangePassword;
            line.InnerHtml = String.Format(WorkiResources.Models.Account.AccountModels.AfterConnection, link.ToString());
            content += line.ToString();

            line = new TagBuilder("p");
            line.InnerHtml = WorkiResources.Models.Account.AccountModels.SeeYouSoon;
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
            return MiscHelpers.UploadFile(postedFile, destinationFolder);
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
    }
}