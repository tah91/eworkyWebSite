using System;
using System.Web.Mvc;
using Worki.Data.Repository;
using Worki.Data.Models;
using Worki.Web.Helpers;
using Worki.Infrastructure;
using Postal;
using Worki.Infrastructure.Repository;

namespace Worki.Web.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    public partial class VisitorController : Controller
    {
        IEmailService _EmailService;

		public VisitorController(IEmailService emailService)
		{
			this._EmailService = emailService;
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
			var context = ModelFactory.GetUnitOfWork();
			var vRepo = ModelFactory.GetRepository<IVisitorRepository>(context);
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			if (ModelState.IsValid)
			{
				try
				{
					var fromDB = vRepo.Get(item => string.Compare(item.Email, visitor.Email, StringComparison.InvariantCultureIgnoreCase) == 0);
					var user = mRepo.GetMember(visitor.Email);
					//already registered
					if (user != null)
					{
						TempData["AlreadyRegistered"] = "Vous êtes déjà inscrit !";
						return RedirectToAction(MVC.Account.LogOn());
					}
					//already added and validated from admin
					else if (fromDB != null && fromDB.IsValid)
					{
						//this.SendVisitorMail(_EmailService, fromDB);
					}
					else
					{
						vRepo.Add(visitor);
					}
					context.Commit();
				}
				catch (Exception)
				{
					context.Complete();
				}


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
