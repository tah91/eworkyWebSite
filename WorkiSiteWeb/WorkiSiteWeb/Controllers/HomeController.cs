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

namespace Worki.Web.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    public partial class HomeController : Controller
    {
        ILocalisationRepository _LocalisationRepository; 
        ILogger _Logger;
        IEmailService _EmailService;
        IWelcomePeopleRepository _WelcomePeopleRepository;
        IPressRepository _PressRepository;

        public HomeController(ILocalisationRepository localisationRepository, ILogger logger, IEmailService emailService, IWelcomePeopleRepository welcomePeopleRepository, IPressRepository pressRepository)
        {
            this._LocalisationRepository = localisationRepository;
            this._Logger = logger;
            this._EmailService = emailService;
            this._WelcomePeopleRepository = welcomePeopleRepository;
            this._PressRepository = pressRepository;
        }

        /// <summary>
        /// Return view of error
        /// </summary>
        /// <returns>View of error</returns>
        public virtual ActionResult Error()
        {
            return View();
        }

        const string _BlogPath = "http://blog.eworky.com/api/get_recent_posts/";
        public const string JTPath = "http://vimeo.com/29038745";
        public const string IndexViewModelContent = "IndexViewModel";

        IEnumerable<BlogPost> GetBlogPosts()
        {
            var toRet = new List<BlogPost>();
            using (var client = new WebClient())
            {
                try
                {
                    string textString = client.DownloadString(_BlogPath);
                    JObject blogJson = JObject.Parse(textString);
                    var posts = blogJson["posts"];
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
                    }
                    _Logger.Info("blog get_recent_posts ");
                }
                catch (WebException ex)
                {
                    _Logger.Error(ex.Message);
                }
            }
            return toRet;
        }

        /// <summary>
        /// Prepare the home page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("index")]
        public virtual ActionResult Index()
        {
            var indexModel = new IndexViewModel()
            {
                LocalisationCount = _LocalisationRepository.GetCount(),
                WelcomePeople = _WelcomePeopleRepository.GetAll().OrderByDescending(wp => wp.Id).ToList(),
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
        public virtual ActionResult Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    dynamic contactMail = new Email(MiscHelpers.EmailView);
                    contactMail.From = contact.FirstName + " " + contact.LastName + "<" + contact.EMail + ">";
                    contactMail.To = MiscHelpers.ContactMail;
                    contactMail.Subject = contact.Subject;
                    contactMail.ToName = MiscHelpers.ContactDisplayName;
                    contactMail.Content = contact.Message;
                    contactMail.Send();
                }
                catch (Exception ex)
                {
                    _Logger.Error("Contact", ex);
                }
                return View(MVC.Home.Views.message_envoye);
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
            var pressList = _PressRepository.GetAll().OrderByDescending(p => p.Date).ToList();
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

        //public virtual ActionResult Partners()
        //{
        //    return View();
        //}

        //public virtual ActionResult SiteMap()
        //{
        //    return View();
        //}
    }
}
