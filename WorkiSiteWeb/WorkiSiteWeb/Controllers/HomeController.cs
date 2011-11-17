using System;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Data.Repository;
using Worki.Infrastructure;
using Worki.Infrastructure.Logging;
using Worki.Service;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using Postal;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Repository;
using Microsoft.ApplicationServer.Caching;
using Worki.Web.Singletons;
using Worki.Web.Helpers;

namespace Worki.Web.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    public partial class HomeController : Controller
    {
        ILogger _Logger;
        IEmailService _EmailService;

        public HomeController(ILogger logger, IEmailService emailService)
        {
            this._Logger = logger;
            this._EmailService = emailService;
        }

        /// <summary>
        /// Return view of error
        /// </summary>
        /// <returns>View of error</returns>
        public virtual ActionResult Error()
        {
            return View();
        }

        public const string IndexViewModelContent = "IndexViewModel";

		IEnumerable<BlogPost> GetBlogPostsFromApi()
		{
			var toRet = new List<BlogPost>();

			using (var client = new WebClient())
			{
				try
				{
					string textString = client.DownloadString(MiscHelpers.UrlConstants.BlogApiPath);
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

        IEnumerable<BlogPost> GetBlogPosts()
        {
			try
			{
				var fromCache = DataCacheSingleton.Instance.Cache.Get(MiscHelpers.BlogConstants.BlogCacheKey);
				if (fromCache != null)
					return (IEnumerable<BlogPost>)fromCache;

				var toRet = GetBlogPostsFromApi();
				DataCacheSingleton.Instance.Cache.Add(MiscHelpers.BlogConstants.BlogCacheKey, toRet, new TimeSpan(MiscHelpers.BlogConstants.CacheDaySpan, 0, 0, 0));
				return toRet;
			}
			catch (Exception ex)
			{
				_Logger.Error("GetBlogPosts", ex);
				return GetBlogPostsFromApi();
			}
        }

        /// <summary>
        /// Prepare the home page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("index")]
        public virtual ActionResult Index()
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
            var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
            var indexModel = new IndexViewModel()
            {
                LocalisationCount = lRepo.GetCount() + rRepo.GetCount(),
                WelcomePeople = wpRepo.GetMany(wp => wp.Online == true).OrderByDescending(wp => wp.Id).ToList(),
                BlogPosts = GetBlogPosts()
            };

            ViewData[IndexViewModelContent] = indexModel;
            return View(new SearchCriteriaFormViewModel());
        }

        /// <summary>
        /// GET action method to send a demand to contact
        /// </summary>
        /// <returns>The contact form to fill.</returns>
        [ActionName("contact")]
        public virtual ActionResult Contact()
        {
            return View(new Contact());
        }

        /// <summary>
        /// POST action method to send a demand to contact
        /// return message sent view
        /// </summary>
        /// <param name="contact">The contact data from the form</param>
        /// <returns>message sent view if ok, the form with errors else </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [ActionName("contact")]
        public virtual ActionResult Contact(Contact contact, string myCaptcha, string attempt)
        {
            //check capatcha
            if (!CaptchaHelper.VerifyAndExpireSolution(HttpContext, myCaptcha, attempt))
            {
                ModelState.AddModelError("attempt", Worki.Resources.Validation.ValidationString.VerificationLettersWrong);
            }
            //check model validity
            else if (ModelState.IsValid)
            {
                try
                {
                    dynamic contactMail = new Email(MVC.Emails.Views.Email);
                    contactMail.From = contact.FirstName + " " + contact.LastName + "<" + contact.EMail + ">";
                    contactMail.To = MiscHelpers.EmailConstants.ContactMail;
                    contactMail.Subject = contact.Subject;
					contactMail.ToName = MiscHelpers.EmailConstants.ContactDisplayName;
                    contactMail.Content = contact.Message;
                    contactMail.Send();
                }
                catch (Exception ex)
                {
                    _Logger.Error("Contact", ex);
                }

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Home.HomeString.MailWellSent;

                return RedirectToAction(MVC.Home.Index());
            }
            return View(contact);
        }

        /// <summary>
        /// Prepares the faq page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("faq")]
        public virtual ActionResult Faq()
        {
            return View();
        }

        /// <summary>
        /// Prepares the press page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("presse")]
        public virtual ActionResult Press()
        {
			var context = ModelFactory.GetUnitOfWork();
			var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
			var pressList = pRepo.GetAll().OrderByDescending(p => p.Date).ToList();
            return View(pressList);
        }

        /// <summary>
        /// Prepares the job page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("jobs")]
        public virtual ActionResult Jobs()
        {
            return View();
        }

        /// <summary>
        /// Prepares the about page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("a-propos")]
        public virtual ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Prepares the cgu page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("cgu")]
        public virtual ActionResult CGU()
        {
            return View();
        }

        /// <summary>
        /// Prepares the legal page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("mentions-legales")]
        public virtual ActionResult Legal()
        {
            return View();
        }

        /// <summary>
        /// It's just setting new culture to session dictionary
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public virtual ActionResult ChangeCulture(string lang, string returnUrl)
        {
            Session["Culture"] = new CultureInfo(lang);
            return Redirect(returnUrl);
        }

        [ActionName("ajouter-espace")]
        public virtual ActionResult AddSpace()
        {
            return View();
        }

        //public virtual ActionResult Partners()
        //{
        //    return View();
        //}

        //public virtual ActionResult SiteMap()
        //{
        //    return View();
        //}

		[ChildActionOnly]
		public virtual ActionResult AboutMenu(int id)
		{
			return PartialView(MVC.Home.Views._AboutMenu, new MenuModelBase { MenuItem = id });
		}
    }
}
