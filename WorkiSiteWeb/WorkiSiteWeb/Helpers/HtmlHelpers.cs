using System;
using System.Text;
using System.Web.Mvc;
using Worki.Web.Models;
using System.Globalization;
using System.Web.Mvc.Html;
using System.Web;
using System.Web.Routing;
using System.Text.RegularExpressions;
using Worki.Web.Infrastructure;

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

        public static MvcHtmlString PageDetailLinks(this HtmlHelper html, int currentIndex, int itemCount, Func<int, string> pageUrl, string previous, string next)
        {
            StringBuilder result = new StringBuilder();
            if (currentIndex > 0)
            {
                var previousIndex = currentIndex - 1;
                TagBuilder tag = new TagBuilder("a");
                tag.MergeAttribute("href", pageUrl(previousIndex));
                tag.InnerHtml = previous;
                result.AppendLine(tag.ToString());
            }
            if (currentIndex < itemCount - 1)
            {
                var nextIndex = currentIndex + 1;
                TagBuilder tag = new TagBuilder("a");
                tag.MergeAttribute("href", pageUrl(nextIndex));
                tag.InnerHtml = next;
                result.AppendLine(tag.ToString());
            }
            return MvcHtmlString.Create(result.ToString());
        }

        public static MvcHtmlString LabelFor(this HtmlHelper html, string content, string forContent)
        {
            StringBuilder result = new StringBuilder();
            TagBuilder tag = new TagBuilder("label");
            tag.MergeAttribute("for", forContent);
            tag.InnerHtml = content;
            result.AppendLine(tag.ToString());
            return MvcHtmlString.Create(result.ToString());
        }

        public static MvcHtmlString FeatureLabelFor(this HtmlHelper html, Feature value, FeatureType valueType)
        {
            return html.LabelFor(LocalisationBinder.LocalisationFeatureDict[(int)value], MiscHelpers.GetFeatureDesc(value, valueType));
        }

        public static MvcHtmlString FeatureCheckBox(this HtmlHelper html, Localisation localisation, Feature value, FeatureType valueType)
        {
            return html.CheckBox(MiscHelpers.GetFeatureDesc(value, valueType), localisation.HasFeature(value, valueType));
        }

        public static MvcHtmlString FeatureCheckBox(this HtmlHelper html, Localisation localisation, Feature value, FeatureType valueType, object htmlAttributes)
        {
            return html.CheckBox(MiscHelpers.GetFeatureDesc(value, valueType), localisation.HasFeature(value, valueType), htmlAttributes);
        }

        public static MvcHtmlString FeatureHidden(this HtmlHelper html, Localisation localisation, Feature value, FeatureType valueType)
        {
            return html.Hidden(MiscHelpers.GetFeatureDesc(value, valueType), localisation.HasFeature(value, valueType));
        }

        public static MvcHtmlString FeatureChecboxLabel(this HtmlHelper html, Localisation localisation, Feature value, FeatureType valueType)
        {
            StringBuilder result = new StringBuilder();
            TagBuilder tag = new TagBuilder("div");
            tag.AddCssClass("editor-field");
            var checkbox = html.FeatureCheckBox(localisation, value, valueType);
            var label = html.FeatureLabelFor(value, valueType);
            tag.InnerHtml = checkbox.ToHtmlString() + label.ToHtmlString();
            result.AppendLine(tag.ToString());
            return MvcHtmlString.Create(result.ToString());
        }

        public static MvcHtmlString FeatureAvailable(this HtmlHelper html, Localisation localisation, Feature value, FeatureType valueType)
        {
            StringBuilder result = new StringBuilder();
            if (!localisation.HasFeature(value, valueType))
                return MvcHtmlString.Create(result.ToString());

            TagBuilder li = new TagBuilder("li");
            TagBuilder div = new TagBuilder("div");
            div.AddCssClass("available");
            var lablel = html.FeatureLabelFor(value, valueType);
            div.InnerHtml = lablel.ToHtmlString();
            li.InnerHtml = div.ToString();
            result.AppendLine(li.ToString());
            return MvcHtmlString.Create(result.ToString());
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

        public static MvcHtmlString ReadOnlyRating(this HtmlHelper html, double value)
        {
            StringBuilder result = new StringBuilder();
            TagBuilder tag = new TagBuilder("div");
            tag.MergeAttribute("class", "rateit");
            tag.MergeAttribute("data-rateit-value", value.ToString(CultureInfo.InvariantCulture));
            tag.MergeAttribute("data-rateit-readonly", "true");
            result.AppendLine(tag.ToString());
            return MvcHtmlString.Create(result.ToString());
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

        public static string GetUrl(this HtmlHelper html, string action, string controller, object routeValues)
        {
            var urlHelper = new UrlHelper(html.ViewContext.RequestContext);
            var url = urlHelper.Action(action, controller, routeValues);
            return url;
        }

        public static string GetUrl(this HtmlHelper html, string action, string controller, RouteValueDictionary rvd)
        {
            var urlHelper = new UrlHelper(html.ViewContext.RequestContext);
            var url = urlHelper.Action(action, controller, rvd);
            return url;
        }

        public static string GetUrl(this HtmlHelper html, string action, string controller)
        {
            return html.GetUrl(action, controller, null);
        }

        public static string GetFullUrl(this HtmlHelper html, string action, string controller, object routeValues)
        {
            var urlHelper = new UrlHelper(html.ViewContext.RequestContext);
            var url = urlHelper.Action(action, controller, routeValues, "http");
            return url;
        }

        public static string GetFileFullUrl(this HtmlHelper html, string path, bool forceHttps)
        {
            try
            {
                return ControllerHelpers.ResolveServerUrl(VirtualPathUtility.ToAbsolute(path), forceHttps);
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

        public static MvcHtmlString CreateLinkMenu(this HtmlHelper html)// Create links of the pop up menu for " rechercher" 
        {
            var tagDiv3 = new TagBuilder("div");
            tagDiv3.MergeAttribute("class", "menuTaille");
            tagDiv3.MergeAttribute("id", "lienMenu");

            var tagDiv1 = new TagBuilder("div");
            tagDiv1.MergeAttribute("class", "float-left menuLeft");

            var tagDiv2 = new TagBuilder("div");
            tagDiv2.MergeAttribute("class", "float-right menuRight lienMenu");

            var ul1 = new TagBuilder("ul");
            ul1.MergeAttribute("id", "surligner");
            ul1.MergeAttribute("class", "menuMarge");

            var ul2 = new TagBuilder("ul");
            ul2.MergeAttribute("id", "surligner");
            ul2.MergeAttribute("class", "menuMarge");

            var index = -1;

            foreach (string item in Localisation.LocalisationOfferTypes.Values)
            {
                if (index < 3)
                {
                    var li = new TagBuilder("li");
                    var url = "";
                    if (index == 0)// For  "salon d'affaires" because of the ' it's doesn't working 
                    {
                        url += html.ActionLink("Salon d affaires", MVC.Search.FullSearchOffer(++index));
                    }
                    else
                    {
                        url += html.ActionLink(item, MVC.Search.FullSearchOffer(++index));
                    }

                    li.InnerHtml = url.ToString();
                    ul1.InnerHtml += li.ToString();
                }
                else
                {
                    var li = new TagBuilder("li");
                    var url = html.ActionLink(item, MVC.Search.FullSearchOffer(++index));
                    li.InnerHtml = url.ToString();
                    ul2.InnerHtml += li.ToString();
                }
            }

            tagDiv1.InnerHtml = ul1.ToString();
            tagDiv2.InnerHtml = ul2.ToString();

            tagDiv3.InnerHtml = tagDiv1.ToString();
            tagDiv3.InnerHtml += tagDiv2.ToString();

            return MvcHtmlString.Create(tagDiv3.ToString());
        }


        public static string JSEscape(this HtmlHelper htmlHelper, string s)
        {
            s = s.Replace(Environment.NewLine, " ");
            s = s.Replace("'", "\\'");
            return s;
        }
    }
}