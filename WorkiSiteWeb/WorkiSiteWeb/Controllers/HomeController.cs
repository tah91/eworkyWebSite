using System;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Data.Repository;
using Worki.Infrastructure;
using Worki.Infrastructure.Email;
using Worki.Infrastructure.Logging;
using Worki.Service;

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

        public const string LocalisationCount = "locCount";
        public const string WelcomePeopleList = "welcomePeopleList";

        public HomeController(ILocalisationRepository localisationRepository, ILogger logger, IEmailService emailService, IWelcomePeopleRepository welcomePeopleRepository)
        {
            this._LocalisationRepository = localisationRepository;
            this._Logger = logger;
            this._EmailService = emailService;
            this._WelcomePeopleRepository = welcomePeopleRepository;
        }

        /// <summary>
        /// Return view of error
        /// </summary>
        /// <returns>View of error</returns>
        public virtual ActionResult Error()
        {
            return View();
        }

        /// <summary>
        /// Prepares the home page
        /// </summary>
        /// <returns>The action result.</returns>
		[ActionName("index")]
		public virtual ActionResult Index()
		{
			ViewData[LocalisationCount] = _LocalisationRepository.GetCount();
			ViewData[WelcomePeopleList] = _WelcomePeopleRepository.GetAll();
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
                    _EmailService.Send(contact.EMail, contact.FirstName + " " + contact.LastName, contact.Subject, contact.Message, false, EmailService.ContactMail);
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
            return View();
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

        public virtual ActionResult ChangeCulture(Culture lang, string returnUrl)
        {
            if (returnUrl.Length >= 3)
            {
                returnUrl = returnUrl.Substring(3);
            }
            return Redirect("/" + lang.ToString() + returnUrl);
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
