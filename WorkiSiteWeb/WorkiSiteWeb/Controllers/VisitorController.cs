using System;
using System.Web.Mvc;
using Worki.Infrastructure.Email;
using Worki.Data.Repository;
using Worki.Data.Models;
using Worki.Web.Helpers;
using Worki.Infrastructure;

namespace Worki.Web.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    public partial class VisitorController : Controller
    {
        IVisitorRepository _VisitorRepository;
        IEmailService _EmailService;
        IMemberRepository _MemberRepository;

        public VisitorController(   IVisitorRepository visitorRepository,
                                    IEmailService emailService,
                                    IMemberRepository memberRepository)
        {
            this._VisitorRepository = visitorRepository;
            this._EmailService = emailService;
            this._MemberRepository = memberRepository;
        }

        /// <summary>
        /// GET Action result to prepare the visitor page (for beta private)
        /// one form for logon, in case user has already an account (handled by account controller)
        /// one form to ask for an account
        /// </summary>
        /// <returns>the two forms to fill</returns>
		[HttpGet]
		public virtual ActionResult Index()
		{
            var url = Url.ActionAbsolute(MVC.Home.Index());
            return RedirectPermanent(url);
			//return View(new VisitorFormViewModel());
		}

        /// <summary>
        /// POST Action result to handle the asking for an acocunt
        /// </summary>
        /// <returns>redirect to visitor index</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Index(Visitor visitor)
        {
            if (ModelState.IsValid)
            {
				var fromDB = _VisitorRepository.Get(item => string.Compare(item.Email, visitor.Email, StringComparison.InvariantCultureIgnoreCase) == 0);
                var user = _MemberRepository.GetMember(visitor.Email);
                //already registered
                if (user != null)
                {
                    TempData["AlreadyRegistered"] = "Vous êtes déjà inscrit !";
                    return RedirectToAction(MVC.Account.LogOn());
                }
                //already added and validated from admin
                else if (fromDB != null && fromDB.IsValid)
                {
                    this.SendVisitorMail(_EmailService, fromDB);
                }
                else
                {
                    _VisitorRepository.Add(visitor);
                }
                //visitorRepository.Save();
                return RedirectToAction(MVC.Visitor.AskForAccountSuccess());
            }
            return View(new VisitorFormViewModel { Visitor = visitor });
        }

        /// <summary>
        /// Action method when asking for an account is ok
        /// </summary>
        /// <returns>the view</returns>
        [ActionName("demande-reussie")]
        public virtual ActionResult AskForAccountSuccess()
        {
            return View();
        }
    }
}
