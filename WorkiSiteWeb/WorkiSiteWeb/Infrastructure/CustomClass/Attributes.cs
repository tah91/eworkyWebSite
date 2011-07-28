﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace WorkiSiteWeb.Infrastructure
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

    public class ValidateOnlyIncomingValuesAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var modelState = filterContext.Controller.ViewData.ModelState;
            var incomingValues = filterContext.Controller.ValueProvider;

            var keys = modelState.Keys.Where(x => !incomingValues.ContainsPrefix(x));
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
            return WorkiResources.Validation.ValidationString.WebsiteNotGoodPattern;
        }
    }

    public class DateTimeStringAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return string.Format(WorkiResources.Validation.ValidationString.NoValideDate, name);
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
}