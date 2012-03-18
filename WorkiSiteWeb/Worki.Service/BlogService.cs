using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Worki.Data.Models;
using Worki.Data.Repository;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Repository;
using System.Web.Routing;
using System.Web;
using System.Net;
using Worki.Infrastructure.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Worki.Infrastructure;
using HtmlAgilityPack;

namespace Worki.Service
{
	public interface IBlogService
	{
		IEnumerable<BlogPost> GetBlogPosts(Culture culture);
	}

	public class BlogService : IBlogService
	{
		public const string WordpressApiPath = "http://blog.eworky.com/api/get_recent_posts/";
		public const string TumblrApiKey = "wJjB5BVwq0rJA22mrh8fnD6QLU02hG8mlJZsGs3uONY750RLgr";
		public const string TumblrApiPath = "http://api.tumblr.com/v2/blog/eworky.tumblr.com/posts/text?api_key={0}";
		public const string FrBlogUrl = "http://blog.eworky.com";
		public const string EnBlogUrl = "http://eworky.tumblr.com";
		public const string FrBlogCacheKey = "FrBlogCacheKey";
		public const string EnBlogCacheKey = "EnBlogCacheKey";

		#region static 

		public static string GetBlogUrl(Culture culture)
		{
			switch(culture)
			{
				case Culture.fr:
					return FrBlogUrl;
				case Culture.en:
				case Culture.es:
				default:
					return EnBlogUrl;
			}
		}

		public static string GetBlogCacheKey(Culture culture)
		{
			switch (culture)
			{
				case Culture.fr:
					return FrBlogCacheKey;
				case Culture.en:
				case Culture.es:
				default:
					return EnBlogCacheKey;
			}
		}

		#endregion

		#region private

		ILogger _Logger;

		#endregion

		public BlogService(ILogger logger)
		{
            _Logger = logger;
		}

		IEnumerable<BlogPost> GetFrBlogPosts()
		{
			var toRet = new List<BlogPost>();

			using (var client = new WebClient())
			{
				try
				{
					string textString = client.DownloadString(WordpressApiPath);
					JObject blogJson = JObject.Parse(textString);
					var posts = blogJson["posts"];
					var added = 0;
					foreach (var item in posts)
					{
						toRet.Add(new BlogPost()
						{
							Url = (string)item["url"],
							Title = (string)item["title"],
							Content = (string)item["excerpt"],
							Image = item["attachments"].Count() != 0 ? (string)item["attachments"][0]["images"]["medium"]["url"] : (string)item["thumbnail"],
							PublicationDate = DateTime.Parse((string)item["date"])
						});
						if (++added >= MiscHelpers.BlogConstants.MaxBlogItem)
							break;
					}
					_Logger.Info("blog get_recent_posts ");
				}
				catch (WebException ex)
				{
					_Logger.Error("GetBlogPostsFromApi", ex);
				}
				return toRet;
			}
		}

		string ExtractImage(string html)
		{
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(html);

			var nodes = doc.DocumentNode.SelectNodes("//img[@src]");
			if (nodes != null)
			{
				foreach (var node in nodes)
				{
					if (node.Attributes.Contains("src"))
					    return node.Attributes["src"].Value;
				}
			}
			return string.Empty;
		}

		IEnumerable<BlogPost> GetEnBlogPosts()
		{
			var toRet = new List<BlogPost>();

			using (var client = new WebClient())
			{
				try
				{
					var url = string.Format(TumblrApiPath, TumblrApiKey);
					string textString = client.DownloadString(url);
					JObject blogJson = JObject.Parse(textString);
					var posts = blogJson["response"]["posts"];
					var added = 0;
					foreach (var item in posts)
					{
						var body = (string)item["body"];
						var content = body;
						var image =  ExtractImage(body);

						toRet.Add(new BlogPost()
						{
							Url = (string)item["post_url"],
							Title = (string)item["title"],
							Content = content,
							Image = image,
							PublicationDate = DateTime.Parse((string)item["date"])
						});
						if (++added >= MiscHelpers.BlogConstants.MaxBlogItem)
							break;
					}
					_Logger.Info("blog get_recent_posts ");
				}
				catch (WebException ex)
				{
					_Logger.Error("GetBlogPostsFromApi", ex);
				}
				return toRet;
			}
		}

		public IEnumerable<BlogPost> GetBlogPosts(Culture culture)
		{
			switch (culture)
			{
				case Culture.fr:
					return GetFrBlogPosts();
				case Culture.en:
				case Culture.es:
				default:
					return GetEnBlogPosts();
			}
		}
	}
}