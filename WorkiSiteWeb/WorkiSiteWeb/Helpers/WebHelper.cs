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
using System.Web.Routing;

namespace Worki.Web.Helpers
{
    /// <summary>
    /// static classe containings utility methods for web
    /// </summary>
    public static class WebHelper
    {
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

		static bool _IsAzureDebug = bool.Parse(ConfigurationManager.AppSettings["IsAzureDebug"]);

		/// <summary>
		/// Tell if it is in debug or prod
		/// </summary>
		/// <returns>true if in debug</returns>
		public static bool IsDebug()
		{
			return _IsAzureDebug;
		}

		static string _FacebookId = null;

		/// <summary>
		/// Get facebook appid
		/// </summary>
		/// <returns>facebook appid</returns>
		public static string GetFacebookId()
		{
			if (!string.IsNullOrEmpty(_FacebookId))
				return _FacebookId;
			var fbSettings = ConfigurationManager.GetSection("facebookSettings") as Facebook.FacebookConfigurationSection;
			_FacebookId= fbSettings != null ? fbSettings.AppId : string.Empty;
			return _FacebookId;
		}

		public static RouteValueDictionary GetRVD(WebViewPage page)
		{
			RouteValueDictionary rvd = new RouteValueDictionary(page.ViewContext.RouteData.Values);
			foreach (string key in page.Request.QueryString.Keys)
			{
				rvd[key] = page.Request.QueryString[key].ToString();
			}
			return rvd;
		}
    }
}