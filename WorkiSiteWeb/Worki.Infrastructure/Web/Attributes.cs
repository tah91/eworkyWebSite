using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.IO.Compression;
using System.Reflection;
using Worki.Infrastructure.Helpers;
using System.Web.Routing;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Worki.Infrastructure
{
	//public class PrivateBetaAttribute : AuthorizeAttribute
	//{
	//    public bool AlwaysAllow = bool.Parse(ConfigurationManager.AppSettings["AlwaysAllow"]);
	//    protected override bool AuthorizeCore(HttpContextBase httpContext)
	//    {
	//        if (AlwaysAllow)
	//            return true;

	//        return base.AuthorizeCore(httpContext);
	//    }

	//    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
	//    {
	//        var urlHelper = new UrlHelper(filterContext.RequestContext);
	//        //var returnUrl = filterContext.HttpContext.Request.Url.PathAndQuery;
	//        var url = urlHelper.Action(MVC.Visitor.ActionNames.Index, MVC.Visitor.Name);
	//        filterContext.Result = new RedirectResult(url);
	//    }
	//}

    /// <summary>
    /// Remote only require https
    /// </summary>
	public class RequireHttpsRemoteAttribute : RequireHttpsAttribute
	{
		static bool _DisableHttps = false;

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			if (_DisableHttps)
				return;

			if (filterContext == null || filterContext.HttpContext == null)
			{
				throw new ArgumentNullException("filterContext");
			}

			if (filterContext.IsChildAction || filterContext.HttpContext.Request.IsAjaxRequest() || filterContext.HttpContext.Request.IsLocal)
			{
				return;
			}

			base.OnAuthorization(filterContext);
		}
	}

    /// <summary>
    /// Don't require https
    /// </summary>
    public class DontRequireHttpsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
			if (filterContext == null || filterContext.HttpContext == null)
			{
				throw new ArgumentNullException("filterContext");
			}

			if (!filterContext.IsChildAction)
			{
				var request = filterContext.HttpContext.Request;
				var response = filterContext.HttpContext.Response;

				if (request.IsSecureConnection && !request.IsLocal && !request.IsAjaxRequest())
				{
					string redirectUrl = request.Url.ToString().Replace("https:", "http:");
					response.Redirect(redirectUrl);
				}
			}
            base.OnActionExecuting(filterContext);
        }
    }

    //The class is taken from http://aspnetmobilesamples.codeplex.com/
    public class RedirectMobileDevicesToMobileAreaAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            // Only redirect on the first request in a session
            if (!httpContext.Session.IsNewSession)
                return true;

            // Don't redirect non-mobile browsers, or mobile but tablet
			var isMobile = false;
			var isTablet = false;
			try
			{
				isMobile = httpContext.Request.Browser.IsMobileDevice;
				isTablet = bool.Parse(httpContext.Request.Browser["is_tablet"]);
			}
			catch(Exception)
			{
			
			}
			if (!isMobile || isTablet)
				return true;

            // Don't redirect requests for the Mobile area
			if (Regex.IsMatch(httpContext.Request.Url.PathAndQuery, "/mobile($|/)") || Regex.IsMatch(httpContext.Request.Url.PathAndQuery, "/account($|/)"))
                return true;

			// Don't redirect requests from eworky
			if (httpContext.Request.UrlReferrer != null && httpContext.Request.UrlReferrer.IsFromThisSite())
				return true;

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var redirectionRouteValues = GetRedirectionRouteValues(filterContext.RequestContext);
            filterContext.Result = new RedirectToRouteResult(redirectionRouteValues);
        }

        // Override this method if you want to customize the controller/action/parameters to which
        // mobile users would be redirected. This lets you redirect users to the mobile equivalent
        // of whatever resource they originally requested.
        protected virtual RouteValueDictionary GetRedirectionRouteValues(RequestContext requestContext)
        {
            return new RouteValueDictionary(new { area = "Mobile", controller = "Home", action = "Index" });
        }
    }

    public class ValidateOnlyIncomingValuesAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Gets or sets a comma-delimited list of property names for which validation is excluded
        /// </summary>
        public string Exclude { get; set; }

        /// <summary>
        /// Gets or sets a prefix for properties to exclude
        /// </summary>
        public string Prefix { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var modelState = filterContext.Controller.ViewData.ModelState;
            var incomingValues = filterContext.Controller.ValueProvider;
            var excludedProperties = !string.IsNullOrEmpty(Exclude) ? Exclude.Split(',').Select(e => Prefix + "." + e).ToList() : null;

            var keys = modelState.Keys.Where(x => (!incomingValues.ContainsPrefix(x) || (!string.IsNullOrEmpty(Exclude) && excludedProperties.Contains(x))));
            foreach (var key in keys) // These keys don't match any incoming value
                modelState[key].Errors.Clear();
        }
    }

    public class ValidateOnlyOnSubmitAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var actionParameters = filterContext.ActionParameters;
            if (actionParameters.Count == 0 || !actionParameters.ContainsKey(ButtonName) || actionParameters[ButtonName] != null)
            {
                _IsMatching = false;
                base.OnActionExecuting(filterContext);
                return;
            }

            var modelState = filterContext.Controller.ViewData.ModelState;
            var keys = modelState.Keys;
            _IsMatching = true;
            foreach (var key in keys)
                modelState[key].Errors.Clear();
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (_IsMatching)
            {
                var viewResult = filterContext.Result as ViewResult;
                if (viewResult != null)
                {
                    var modelState = viewResult.ViewData.ModelState;
                    var keys = modelState.Keys;
                    foreach (var key in keys)
                    {
                        if (string.IsNullOrEmpty(key))
                            continue;
                        modelState[key].Errors.Clear();
                    }
                }
            }
            base.OnActionExecuted(filterContext);
        }

        private bool _IsMatching = false;
        public string ButtonName { get; set; }
    }

    public class EmailAttribute : RegularExpressionAttribute
    {
        public EmailAttribute()
            : base(@"^([A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4})")
        {
        }
    }

    public class WebSiteAttribute : ValidationAttribute
    {
        public WebSiteAttribute()
            : base()
        {
        }

        public override bool IsValid(object value)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str))
                return false;
            try
            {
                var uri = new Uri(str);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return Worki.Resources.Validation.ValidationString.WebsiteNotGoodPattern;
        }
    }

    public class SelectValidationAttribute : ValidationAttribute
    {
        public SelectValidationAttribute()
            : base()
        {

        }

        public override bool IsValid(object value)
        {
            if (!(value is int))
                return false;
            var val = (int)value;
            if (val != MiscHelpers.Constants.UnselectedItem)
                return true;
            return false;
        }
    }

    public class SelectStringValidationAttribute : ValidationAttribute
    {
        public SelectStringValidationAttribute()
            : base()
        {

        }

        public override bool IsValid(object value)
        {
            if (!(value is string))
                return false;
            if (string.IsNullOrEmpty((string)value))
                return false;
            return true;
        }
    }

    public class DateTimeStringAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return string.Format(Worki.Resources.Validation.ValidationString.NoValideDate, name);
        }

        public override bool IsValid(object value)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str))
                return true;
            try
            {
                IFormatProvider culture = new CultureInfo("fr-FR", true);
                var date = DateTime.Parse(str, culture);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }

    public class LocalizedEnumAttribute : Attribute
    {
        private Type _resourceType;

        public LocalizedEnumAttribute()
        {

        }

        public Type ResourceType
        {
            get
            {
                return _resourceType;
            }
            set
            {
                _resourceType = value;
            }
        }
    }

	/// <summary>
	/// Represents errors that occur due to invalid application model state.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class HandleModelStateExceptionAttribute : FilterAttribute, IExceptionFilter
	{
		/// <summary>
		/// Called when an exception occurs and processes <see cref="ModelStateException"/> object.
		/// </summary>
		/// <param name="filterContext">Filter context.</param>
		public void OnException(ExceptionContext filterContext)
		{
			if (filterContext == null)
			{
				throw new ArgumentNullException("filterContext");
			}

			// handle modelStateException
			if (filterContext.Exception != null && typeof(ModelStateException).IsInstanceOfType(filterContext.Exception) && !filterContext.ExceptionHandled)
			{
				filterContext.ExceptionHandled = true;
				filterContext.HttpContext.Response.Clear();
				filterContext.HttpContext.Response.ContentEncoding = Encoding.UTF8;
				filterContext.HttpContext.Response.HeaderEncoding = Encoding.UTF8;
				filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
				filterContext.HttpContext.Response.StatusCode = 400;
				filterContext.Result = new ContentResult
				{
					Content = (filterContext.Exception as ModelStateException).Message,
					ContentEncoding = Encoding.UTF8,
				};
			}
		}
	}

    /// <summary>
    /// Filter to put cache on http header
    /// </summary>
    public class CacheFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Gets or sets the cache duration in seconds. The default is 10 seconds.
        /// </summary>
        /// <value>The cache duration in seconds.</value>
        public int Duration
        {
            get;
            set;
        }

        public CacheFilterAttribute()
        {
            Duration = -1;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (Duration <= 0) 
				return;

            HttpCachePolicyBase cache = filterContext.HttpContext.Response.Cache;
            TimeSpan cacheDuration = TimeSpan.FromSeconds(Duration);

            cache.SetCacheability(HttpCacheability.Public);
            cache.SetExpires(DateTime.UtcNow.Add(cacheDuration));
            cache.SetMaxAge(cacheDuration);
            cache.AppendCacheExtension("must-revalidate, proxy-revalidate");
        }
    }

    /// <summary>
    /// filter to compress response (gzip/deflate)
    /// </summary>
    public class CompressFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
			HttpRequestBase request = filterContext.HttpContext.Request;

			string acceptEncoding = request.Headers["Accept-Encoding"];

			if (string.IsNullOrEmpty(acceptEncoding)) return;

			acceptEncoding = acceptEncoding.ToUpperInvariant();

			HttpResponseBase response = filterContext.HttpContext.Response;

			if (response.Filter == null)
				return;

			if (acceptEncoding.Contains("GZIP"))
			{
				response.AppendHeader("Content-encoding", "gzip");
				response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
			}
			else if (acceptEncoding.Contains("DEFLATE"))
			{
				response.AppendHeader("Content-encoding", "deflate");
				response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
			}
        }
    }
}