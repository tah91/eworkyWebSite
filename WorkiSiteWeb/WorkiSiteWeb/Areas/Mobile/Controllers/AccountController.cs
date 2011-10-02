using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Worki.Data.Models;
using Worki.Infrastructure.Email;
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;
using Worki.Data.Repository;
using Worki.Memberships;
using Worki.Infrastructure.Repository;


namespace Worki.Web.Areas.Mobile.Controllers
{
	public partial class AccountController : Controller
	{

		public const string MemberDisplayNameString = "MemberDisplayName";
		public IFormsAuthenticationService FormsService
		{
			get { return _FormsService; }
		}
		public IMembershipService MembershipService
		{
			get { return _MembershipService; }
		}

		IFormsAuthenticationService _FormsService;
		IMembershipService _MembershipService;
		ILogger _Logger;

		public AccountController(IFormsAuthenticationService formsService,
								IMembershipService membershipService,
								ILogger logger)
		{
			this._FormsService = formsService;
			this._MembershipService = membershipService;
			this._Logger = logger;
		}


		/// <summary>
		/// POST action method to login to an account, add cookie for display name
		/// </summary>
		/// <param name="model">The logon data from the form</param>
		/// <param name="returnUrl">The url to redirect to in case of sucess</param>
		/// <returns>Redirect to return url if any, if not to home page</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("connexion")]
		public virtual ActionResult LogOn(LogOnModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				if (MembershipService.ValidateUser(model.Login, model.Password))
				{
					var context = ModelFactory.GetUnitOfWork();
					var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
					var member = mRepo.GetMember(model.Login);
					var userData = member.GetUserData();
					FormsService.SignIn(model.Login, userData, /*model.RememberMe*/true, ControllerContext.HttpContext.Response);
					if (!String.IsNullOrEmpty(returnUrl))
					{
						return Redirect(returnUrl);
					}
					else
					{
						return RedirectToAction(MVC.Mobile.Home.ActionNames.Index, MVC.Mobile.Home.Name);
					}
				}
				else
				{
					ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.MailOrPasswordNotCorrect);
				}
			}

			// Si nous sommes arrivés là, quelque chose a échoué, réafficher le formulaire
			return View(model);
		}

		/// <summary>
		/// Action method to login off, remove the cookie of display name
		/// </summary>
		/// <returns>Redirect to home page</returns>
		[ActionName("deconnexion")]
		public virtual ActionResult LogOff()
		{
			FormsService.SignOut();
			if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(MemberDisplayNameString))
			{
				HttpCookie cookie = ControllerContext.HttpContext.Request.Cookies[MemberDisplayNameString];
				cookie.Expires = DateTime.Now.AddDays(-1);
				this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
			}
			return RedirectToAction(MVC.Mobile.Home.ActionNames.Index, MVC.Mobile.Home.Name);
		}
	}
}
