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
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Repository;
using Microsoft.ApplicationServer.Caching;
using Worki.Web.Singletons;
using Worki.Web.Helpers;
using System.Web;
using System.Web.Security;
using Worki.Web.Model;
using Worki.Infrastructure.Email;
using System.Net.Mail;

namespace Worki.Web.Controllers
{
	[HandleError]
	[CompressFilter(Order = 1)]
	[CacheFilter(Order = 2)]
	[DontRequireHttps]
	public abstract class ControllerBase : Controller
	{
        protected ILogger _Logger;
        protected IObjectStore _ObjectStore;
        protected IEmailService _EmailService;

        public ControllerBase()
        {
        }

        public ControllerBase(ILogger logger,IObjectStore objectStore, IEmailService emailService)
        {
            this._Logger = logger;
            this._ObjectStore = objectStore;
            this._EmailService = emailService;
        }
	}

    public partial class DummyController : ControllerBase
    {
        public DummyController()
            : base()
        {
        }
    }

	public partial class HomeController : ControllerBase
    {
		IBlogService _IBlogService;

        public HomeController(ILogger logger, IObjectStore objectStore, Worki.Infrastructure.Email.IEmailService emailService, IBlogService blogService)
            : base(logger,objectStore,emailService)
        {
			this._IBlogService = blogService;
        }

		[ChildActionOnly]
		public virtual ActionResult UserMenu()
		{
			var displayName = User.Identity.Name;
			var memberId = 0;
			FormsIdentity ident = User.Identity as FormsIdentity;
			if (ident != null)
			{
				displayName = WebHelper.GetIdentityDisplayName(User.Identity);
				memberId = WebHelper.GetIdentityId(User.Identity);
			}
			var dropDown = new DropDownModel
			{
				Id = DropDownModel.ProfilDD,
				Title = string.Format(Worki.Resources.Views.Home.HomeString.Welcome + " {0} !", displayName),
				Items = new List<DropDownItem>
				{
					new DropDownItem{ DisplayName = Worki.Resources.Menu.Menu.UserSpace, Link = Url.Action(MVC.Dashboard.Home.Index())}
				}
			};
			if (User.IsInRole(MiscHelpers.BackOfficeConstants.BackOfficeRole))
			{
				dropDown.Items.Add(new DropDownItem { DisplayName = Worki.Resources.Menu.Menu.OwnerSpace, Link = Url.Action(MVC.Backoffice.Home.Index()) });
				var context = ModelFactory.GetUnitOfWork();
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var member = mRepo.Get(memberId);

				foreach (var item in member.Localisations)
				{
					dropDown.Items.Add(new DropDownItem { DisplayName = item.Name, Link = Url.Action(MVC.Backoffice.Localisation.Index(item.ID)) });
				}
			}
			if (User.IsInRole(MiscHelpers.AdminConstants.AdminRole))
			{
				dropDown.Items.Add(new DropDownItem { DisplayName = Worki.Resources.Menu.Menu.AdminSpace, Link = Url.Action(MVC.Admin.Sheet.Index()) });
			}
			dropDown.Items.Add(new DropDownItem { DisplayName = Worki.Resources.Views.Shared.SharedString.Deconnect, Link = Url.Action(MVC.Account.LogOff()) });

			return PartialView(MVC.Shared.Views._DropDownList, dropDown);
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

        IEnumerable<BlogPost> GetBlogPosts(Culture culture)
        {
			try
			{
				var key = BlogService.GetBlogCacheKey(culture);
				object fromCache = DataCacheSingleton.Instance.Cache.Get(key);
				if (fromCache != null)
					return (IEnumerable<BlogPost>)fromCache;

				var toRet = _IBlogService.GetBlogPosts(culture);
				DataCacheSingleton.Instance.Cache.Add(key, toRet, new TimeSpan(MiscHelpers.BlogConstants.CacheDaySpan, 0, 0, 0));
				return toRet;
			}
			catch (Exception ex)
			{
				_Logger.Error("GetBlogPosts", ex);
				return _IBlogService.GetBlogPosts(culture);
			}
        }

        /// <summary>
        /// Child action to create index head
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public virtual ActionResult IndexHead()
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);

            return PartialView(MVC.Home.Views._IndexHead, lRepo.GetCount() + rRepo.GetCount());
        }

        /// <summary>
        /// Child action to create index slider
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public virtual ActionResult PeopleSlider(Culture culture)
        {
            var context = ModelFactory.GetUnitOfWork();
            var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
			var siteVersion = (int)WelcomePeople.GetVersion(culture);

			return PartialView(MVC.Home.Views._PeopleSlider, wpRepo.GetMany(wp => wp.Online == true && wp.SiteVersion == siteVersion).OrderByDescending(wp => wp.Id).ToList());
        }

        /// <summary>
        /// Child action to create index blog container
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
		public virtual ActionResult BlogContainer(Culture culture)
        {
			return PartialView(MVC.Home.Views._BlogContainer, GetBlogPosts(culture));
        }


        /// <summary>
        /// Prepare the home page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("index")]
        public virtual ActionResult Index()
        {
            return View(MVC.Home.Views.Index, new SearchCriteriaFormViewModel());
        }

        /// <summary>
        /// GET action method to send a demand to contact
        /// </summary>
        /// <returns>The contact form to fill.</returns>
        [ActionName("contact")]
        public virtual ActionResult Contact()
        {
            return View(MVC.Home.Views.Contact, new Contact());
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
            else 
                if (ModelState.IsValid)
            {
                try
                {
                    var displsayName = contact.FirstName + " " + contact.LastName;
                    var mail = _EmailService.PrepareMessageToDefault(new System.Net.Mail.MailAddress(contact.EMail, displsayName), contact.Subject, WebHelper.RenderEmailToString(displsayName, contact.Message));
                    _EmailService.Deliver(mail);
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
        [ActionName("press")]
        public virtual ActionResult Press()
        {
			var context = ModelFactory.GetUnitOfWork();
			var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
			var pressList = pRepo.GetAll().OrderByDescending(p => p.Date).ToList();
            return View(MVC.Home.Views.Press, pressList);
        }

        /// <summary>
        /// Prepares the job page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("jobs")]
        public virtual ActionResult Jobs()
        {
            return View(MVC.Home.Views.Jobs);
        }

        /// <summary>
        /// Prepares the about page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("about")]
        public virtual ActionResult About()
        {
            return View(MVC.Home.Views.About);
        }

		[ActionName("team")]
        public virtual ActionResult Team()
        {
			return View(MVC.Home.Views.Team);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("pricing")]
        public virtual ActionResult Pricing()
        {
            return View(MVC.Home.Views.Pricing, new BOAccept());
        }

        [AcceptVerbs(HttpVerbs.Post), Authorize]
        [ActionName("pricing")]
        public virtual ActionResult Pricing(BOAccept model)
        {
            var id = WebHelper.GetIdentityId(User.Identity);
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var member = mRepo.Get(id);
            if (member == null)
                return View(MVC.Shared.Views.Error);


            if (ModelState.IsValid)
            {
                try
                {
                    if (member.MemberMainData.BOStatus == (int)eBOStatus.None && !Roles.IsUserInRole(member.Username, MiscHelpers.BackOfficeConstants.BackOfficeRole))
                        member.MemberMainData.BOStatus = (int)eBOStatus.Pending;

                    context.Commit();

                    //send mail to team
                    var teamMailContent = string.Format(Worki.Resources.Email.Activation.BOTeamAsk,
                                                     string.Format("{0} {1}", member.MemberMainData.FirstName, member.MemberMainData.LastName),
                                                     member.Email);

                    var teamMail = _EmailService.PrepareMessageFromDefault(new MailAddress(MiscHelpers.EmailConstants.BookingMail, MiscHelpers.EmailConstants.ContactDisplayName),
                        Worki.Resources.Email.Activation.BOTeamAskSubject,
                        WebHelper.RenderEmailToString(member.MemberMainData.FirstName, teamMailContent));

                    _EmailService.Deliver(teamMail);

                    //email to tell ask is pending
                    var confirmationMailContent = Worki.Resources.Email.Activation.BOAskContent;

                    var confirmationMail = _EmailService.PrepareMessageFromDefault(new MailAddress(member.Email,member.GetDisplayName()),
                        Worki.Resources.Email.Activation.BOAskSubject,
                        WebHelper.RenderEmailToString(member.MemberMainData.FirstName, confirmationMailContent));

                    _EmailService.Deliver(confirmationMail);

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.BackOfficeAsked;
                    return RedirectToAction(MVC.Home.Index());
                }
                catch (Exception ex)
                {
                    context.Complete();
                    _Logger.Error("Pricing", ex);
                }
            }
            return View(MVC.Home.Views.Pricing, model);
        }

        /// <summary>
        /// Prepares the cgu page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("tos")]
        public virtual ActionResult CGU()
        {
            return View(MVC.Home.Views.CGU);
        }

        /// <summary>
        /// Prepares the legal page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("legal")]
        public virtual ActionResult Legal()
        {
            return View(MVC.Home.Views.Legal);
        }

		/// <summary>
		/// Prepares the how it works page
		/// </summary>
		/// <returns>The action result.</returns>
		[ActionName("how-it-works")]
		public virtual ActionResult HowItWorks()
		{
			return View(MVC.Home.Views.HowItWorks);
		}

		/// <summary>
		/// Prepares the owner tutorial page
		/// </summary>
		/// <returns>The action result.</returns>
		[ActionName("owner-notice")]
		public virtual ActionResult OwnerTutorial(bool boLink = false)
		{
			return View(MVC.Home.Views.OwnerTutorial, boLink);
		}

		/// <summary>
		/// Prepares the user tutorial page
		/// </summary>
		/// <returns>The action result.</returns>
		[ActionName("user-notice")]
		public virtual ActionResult UserTutorial()
		{
			return View(MVC.Home.Views.UserTutorial);
		}

        /// <summary>
        /// Prepares the share office page
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("share-office")]
        public virtual ActionResult ShareOffice()
        {
            return View(MVC.Home.Views.ShareOffice);
        }

        /// <summary>
        /// It's just setting new culture to session dictionary
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public virtual ActionResult ChangeCulture(string lang)
        {
            var newUrl = MultiCultureMvcRouteHandler.SetDomainPrefix(Request.UrlReferrer, lang);
            if (string.IsNullOrEmpty(newUrl))
            {
                if (Request.UrlReferrer != null)
                {
                    return Redirect(Request.UrlReferrer.PathAndQuery);
                }
                else
                {
                    return RedirectToAction(MVC.Home.Index());
                }
            }
            return Redirect(newUrl);
        }

        [ActionName("add-space")]
        public virtual ActionResult AddSpace()
        {
            return View(MVC.Home.Views.AddSpace);
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
