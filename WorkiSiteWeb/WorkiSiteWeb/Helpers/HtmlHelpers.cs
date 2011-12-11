using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;
using System.Configuration;
using System.Reflection;
using System.Collections.Generic;

namespace Worki.Web.Helpers
{
    public static class PagingHelpers
    {
		static string PageLink(int pageIndex, int currentPage, Func<int, string> pageUrl, string a_class)
		{
			TagBuilder tag = new TagBuilder("a");
			var link = "javascript:void();";
			if (pageUrl != null)
				link = pageUrl(pageIndex);
			tag.MergeAttribute("href", link);
			if (!string.IsNullOrEmpty(a_class))
				tag.AddCssClass(a_class);
			tag.InnerHtml = pageIndex.ToString();
			if (pageIndex == currentPage)
				tag.AddCssClass("selected");
			return tag.ToString();
		}

		static string NextLink(int pageIndex, bool next, Func<int, string> pageUrl, string a_class)
		{
			TagBuilder tag = new TagBuilder("a");
			var link = "javascript:void();";
			if (pageUrl != null)
				link = pageUrl(pageIndex);
			tag.MergeAttribute("href", link);
			tag.InnerHtml = next ? ">" : "<";
			return tag.ToString();
		}

		public static MvcHtmlString PageLinks(this HtmlHelper html, PagingInfo pagingInfo, Func<int, string> pageUrl, string a_class = null)
		{
			StringBuilder result = new StringBuilder();
			if (pagingInfo.TotalPages < 2)
				return MvcHtmlString.Create(result.ToString());

			var first = 1;
			var current = pagingInfo.CurrentPage;
			var total = pagingInfo.TotalPages;

			//previous
			if (current > first && string.IsNullOrEmpty(a_class))
				result.AppendLine(NextLink(current - 1, false, pageUrl, a_class));

			//print first
			result.AppendLine(PageLink(first, current, pageUrl, a_class));

			//far from first
			if (current > 2)
			{
				result.AppendLine("...");
				if (current == total && total > 3)
					result.AppendLine(PageLink(current - 2, current, pageUrl, a_class));
				result.AppendLine(PageLink(current - first, current, pageUrl, a_class));
			}

			//current
			if (current != 1 && current != total)
				result.AppendLine(PageLink(current, current, pageUrl, a_class));

			//far from last
			if (current < total - 1)
			{
				result.AppendLine(PageLink(current + 1, current, pageUrl, a_class));
				if (current == first && total > 3)
					result.AppendLine(PageLink(current + 2, current, pageUrl, a_class));
				result.AppendLine("...");
			}

			//last
			result.AppendLine(PageLink(total, current, pageUrl, a_class));

			//next
			if (current < total && string.IsNullOrEmpty(a_class))
				result.AppendLine(NextLink(current + 1, true, pageUrl, a_class));

			return MvcHtmlString.Create(result.ToString());
		}

        public static MvcHtmlString PageDetailLinks(this HtmlHelper html, int currentIndex, int itemCount, Func<int, string> pageUrl, string previous, string next, string divClass)
        {
            StringBuilder result = new StringBuilder();
            if (currentIndex > 0)
            {
                var previousIndex = currentIndex - 1;
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(divClass);
                TagBuilder tag = new TagBuilder("a");
                tag.MergeAttribute("href", pageUrl(previousIndex));
                tag.InnerHtml = previous;
                div.InnerHtml = tag.ToString();
                result.AppendLine(div.ToString());
            }
            if (currentIndex < itemCount - 1)
            {
                var nextIndex = currentIndex + 1;
                TagBuilder div = new TagBuilder("div");
                div.AddCssClass(divClass);
                TagBuilder tag = new TagBuilder("a");
                tag.MergeAttribute("href", pageUrl(nextIndex));
                tag.InnerHtml = next;
                div.InnerHtml = tag.ToString();
				if (result.Length > 0)
					result.Append(" | ");
                result.AppendLine(div.ToString());
            }
            return MvcHtmlString.Create(result.ToString());
        }

		/// <summary>
		/// Create a label for a field
		/// </summary>
		/// <param name="html">HtmlHelper instance</param>
		/// <param name="content">content of the label</param>
		/// <param name="forContent">id of the for field</param>
		/// <returns>the build label</returns>
        public static MvcHtmlString LabelFor(this HtmlHelper html, string content, string forContent)
        {
            StringBuilder result = new StringBuilder();
            TagBuilder tag = new TagBuilder("label");
            tag.MergeAttribute("for", forContent);
            tag.InnerHtml = content;
            result.AppendLine(tag.ToString());
            return MvcHtmlString.Create(result.ToString());
        }

		public static MvcHtmlString FeatureLabelFor(this HtmlHelper html, Feature value, IFeatureProvider offer)
        {
			return html.LabelFor(FeatureHelper.GetFeatureDisplayName(value), FeatureHelper.GetStringId(value, offer.GetPrefix()));
        }

		public static MvcHtmlString FeatureCheckBox(this HtmlHelper html, Feature value, IFeatureProvider offer)
		{
            return html.CheckBox(FeatureHelper.GetStringId(value, offer.GetPrefix()), offer.HasFeature(value));
		}

		public static MvcHtmlString FeatureChecboxLabel(this HtmlHelper html, Feature value, IFeatureProvider offer)
		{
			StringBuilder result = new StringBuilder();
			TagBuilder tag = new TagBuilder("div");
			tag.AddCssClass("editor-field");
			var checkbox = html.FeatureCheckBox(value, offer);
			var label = html.FeatureLabelFor(value,offer);
			tag.InnerHtml = checkbox.ToHtmlString() + label.ToHtmlString();
			result.AppendLine(tag.ToString());
			return MvcHtmlString.Create(result.ToString());
		}

		public static MvcHtmlString FeatureStringTextBox(this HtmlHelper html, Feature value, IFeatureProvider offer, object htmlAttributes = null)
		{
            return html.TextBox(FeatureHelper.GetStringId(value, offer.GetPrefix()), offer.GetStringFeature(value), htmlAttributes);
		}

		public static MvcHtmlString FeatureNumberTextBox(this HtmlHelper html, Feature value, IFeatureProvider offer, object htmlAttributes = null)
		{
            return html.TextBox(FeatureHelper.GetStringId(value, offer.GetPrefix()), offer.GetNumberFeature(value), htmlAttributes);
		}

        public static MvcHtmlString FeatureHidden(this HtmlHelper html, Feature value, IFeatureProvider offer)
        {
            return html.Hidden(FeatureHelper.GetStringId(value, offer.GetPrefix()), offer.HasFeature(value));
        }

        public static MvcHtmlString WorkiTitle(this HtmlHelper html, string content)
        {
            return MvcHtmlString.Create(content + " | eWorky");
        }

        public static MvcHtmlString NullableTimeDisplay(this HtmlHelper html, System.Nullable<System.DateTime> time)
        {
            var toRet = "-";
            if (!time.HasValue)
                return MvcHtmlString.Create(toRet);
            else
                return MvcHtmlString.Create(time.Value.ToString("t", CultureInfo.CreateSpecificCulture("fr-FR")));
        }

        //to improve
        public static MvcHtmlString TruncateAtWord(this HtmlHelper html, string str, int length)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= length)
                return MvcHtmlString.Create(str);
            var firstSpaceAfterLimit = str.IndexOf(" ", length);
            if (firstSpaceAfterLimit == -1)
                firstSpaceAfterLimit = str.LastIndexOf(" ");
            if (firstSpaceAfterLimit == -1)
                return MvcHtmlString.Create("...");

            var truncated = str.Substring(0, firstSpaceAfterLimit);
            if (string.IsNullOrEmpty(truncated) || truncated.Length <= length)
                return MvcHtmlString.Create(string.Format("{0}...", truncated));
            truncated = truncated.Substring(0, truncated.LastIndexOf(" "));
            return MvcHtmlString.Create(string.Format("{0}...", truncated));
        }

        public static MvcHtmlString ActionImage(this HtmlHelper html, object routeValues, string imagePath, string alt, ActionResult action)
        {
            action.AddRouteValues(routeValues);

            var url = new UrlHelper(html.ViewContext.RequestContext);

            var imgBuilder = new TagBuilder("img");
            imgBuilder.MergeAttribute("src", url.Content(imagePath));
            imgBuilder.MergeAttribute("alt", alt);
            string imgHtml = imgBuilder.ToString(TagRenderMode.SelfClosing);
            
            var anchorBuilder = new TagBuilder("a");
            anchorBuilder.MergeAttribute("href", url.Action(action)); 
            anchorBuilder.InnerHtml = imgHtml; 
            string anchorHtml = anchorBuilder.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(anchorHtml);
        }


        public static string GetFileFullUrl(this HtmlHelper html, string path, bool forceHttps)
        {
            try
            {
				return WebHelper.ResolveServerUrl(VirtualPathUtility.ToAbsolute(path), forceHttps);
            }
            catch (Exception)
            {
                return path;
            }
        }

        public static string GetPhoneFormat(this HtmlHelper html, string number)
        {
            try
            {
                return Regex.Replace(number, @"(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})", "$1 $2 $3 $4 $5");
            }
            catch (Exception)
            {
                return null;
            }
        }

		public static MvcHtmlString Nl2Br(this HtmlHelper htmlHelper, string text)
		{
			return MvcHtmlString.Create(MiscHelpers.Nl2Br(text));
		}

        public static string JSEscape(this HtmlHelper htmlHelper, string s)
        {
            s = s.Replace(Environment.NewLine, " ");
            s = s.Replace("'", "\\'");
            return s;
        }

        /// <summary>
        /// Append build dependend id at the end of css/ js file
        /// to force ctrl f5 on each build
        /// </summary>
        /// <param name="scriptFileName">file name</param>
        /// <returns>correct version of file</returns>
        public static string VersionedContent(this UrlHelper instance, string fileName)
        {
            Assembly asy = Assembly.GetExecutingAssembly();
            var hash = string.Empty;
            if (asy != null)
                hash = asy.GetHashCode().ToString("x");
            return string.Format("{0}?v={1}", instance.Content(fileName), hash);
        }

		const string _AreaString = "area";
		const string _AdminString = "admin";
		const string _DashboardString = "dashboard";
		const string _BackofficeString = "backoffice";

		/// <summary>
		/// Tell if the url should be secured
		/// </summary>
		/// <param name="instance">helper instance</param>
		/// <returns>true if should be</returns>
		public static bool IsSecuredUrl(this UrlHelper instance)
		{
			if (!instance.RequestContext.RouteData.DataTokens.ContainsKey(_AreaString))
				return false;

			var area = ((string)instance.RequestContext.RouteData.DataTokens[_AreaString]).ToLower();
			if (area != _AdminString && area != _DashboardString && area != _BackofficeString)
				return false;

			return true;
		}

		/// <summary>
		/// Display date with specified format
		/// </summary>
		/// <param name="date">date to display</param>
		/// <param name="format">datetime format</param>
		/// <returns>correct date string</returns>
		public static MvcHtmlString DisplayDate(this HtmlHelper instance, DateTime? date, CultureHelpers.TimeFormat format = CultureHelpers.TimeFormat.Date)
		{
			return MvcHtmlString.Create(CultureHelpers.GetSpecificFormat(date, format));
		}

		/// <summary>
		/// Display date with local shift, format is long general
		/// </summary>
		/// <param name="date">date to display</param>
		/// <param name="relative">tell if we need relative date display</param>
		/// <returns>correct date string</returns>
		public static MvcHtmlString DisplayLocalDate(this HtmlHelper instance, DateTime? date, bool relative = false)
		{
			if (!date.HasValue)
				return null;
			var str = date.Value.ToString("MM/dd/yyyy HH:mm:ss"); 
			var tag = new TagBuilder("span");
			tag.AddCssClass("utcdate");
			if (relative)
			{
				tag.AddCssClass("timeago");
				tag.MergeAttribute("title", date.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
			}
			tag.SetInnerText(str);
			return MvcHtmlString.Create(tag.ToString());
		}

		/// <summary>
		/// Create a link
		/// </summary>
		/// <param name="text">text of the link</param>
		/// <param name="href">href of the string</param>
		/// <returns>link</returns>
		public static MvcHtmlString CreateLink(this HtmlHelper instance, string text, string href)
		{
			var tag = new TagBuilder("a");
			tag.MergeAttribute("href", href);
			tag.SetInnerText(text);
			return MvcHtmlString.Create(tag.ToString());
		}

		#region BeginCollectionItem

		private const string idsToReuseKey = "__htmlPrefixScopeExtensions_IdsToReuse_";

		public static IDisposable BeginCollectionItem(this HtmlHelper html, string collectionName)
		{
			var idsToReuse = GetIdsToReuse(html.ViewContext.HttpContext, collectionName);
			string itemIndex = idsToReuse.Count > 0 ? idsToReuse.Dequeue() : Guid.NewGuid().ToString();

			// autocomplete="off" is needed to work around a very annoying Chrome behaviour whereby it reuses old values after the user clicks "Back", which causes the xyz.index and xyz[...] values to get out of sync.
			html.ViewContext.Writer.WriteLine(string.Format("<input type=\"hidden\" name=\"{0}.index\" autocomplete=\"off\" value=\"{1}\" />", collectionName, html.Encode(itemIndex)));

			return BeginHtmlFieldPrefixScope(html, string.Format("{0}[{1}]", collectionName, itemIndex));
		}

		public static IDisposable BeginHtmlFieldPrefixScope(this HtmlHelper html, string htmlFieldPrefix)
		{
			return new HtmlFieldPrefixScope(html.ViewData.TemplateInfo, htmlFieldPrefix);
		}

		private static Queue<string> GetIdsToReuse(HttpContextBase httpContext, string collectionName)
		{
			// We need to use the same sequence of IDs following a server-side validation failure,  
			// otherwise the framework won't render the validation error messages next to each item.
			string key = idsToReuseKey + collectionName;
			var queue = (Queue<string>)httpContext.Items[key];
			if (queue == null)
			{
				httpContext.Items[key] = queue = new Queue<string>();
				var previouslyUsedIds = httpContext.Request[collectionName + ".index"];
				if (!string.IsNullOrEmpty(previouslyUsedIds))
					foreach (string previouslyUsedId in previouslyUsedIds.Split(','))
						queue.Enqueue(previouslyUsedId);
			}
			return queue;
		}

		private class HtmlFieldPrefixScope : IDisposable
		{
			private readonly TemplateInfo templateInfo;
			private readonly string previousHtmlFieldPrefix;

			public HtmlFieldPrefixScope(TemplateInfo templateInfo, string htmlFieldPrefix)
			{
				this.templateInfo = templateInfo;

				previousHtmlFieldPrefix = templateInfo.HtmlFieldPrefix;
				templateInfo.HtmlFieldPrefix = htmlFieldPrefix;
			}

			public void Dispose()
			{
				templateInfo.HtmlFieldPrefix = previousHtmlFieldPrefix;
			}
		}

		#endregion
    }
}