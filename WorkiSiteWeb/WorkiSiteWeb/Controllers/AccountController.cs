using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Worki.Data.Models;
using Worki.Infrastructure.Email;
/*using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.Messaging;*/
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;
using Worki.Data.Repository;
using Worki.Memberships;

namespace Worki.Web.Controllers
{
    [HandleError]
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
        IVisitorRepository _VisitorRepository;
        IMemberRepository _MemberRepository;
        IEmailService _EmailService;

        public AccountController(   IFormsAuthenticationService formsService, 
                                    IMembershipService membershipService,
                                    ILogger logger, 
                                    IVisitorRepository visitorRepository, 
                                    IMemberRepository memberRepository,
                                    IEmailService emailService)
        {
            this._FormsService = formsService;
            this._MembershipService = membershipService;
            this._Logger = logger;
            this._VisitorRepository = visitorRepository;
            this._MemberRepository = memberRepository;
            this._EmailService = emailService;
        }

        /// <summary>
        /// GET action method to login to an account
        /// </summary>
        /// <returns>The form to fill to logon</returns>
        [ActionName("connexion")]
        public virtual ActionResult LogOn()
        {
            if (TempData.ContainsKey("AlreadyRegistered"))
                ViewBag.AlreadyRegistered = TempData["AlreadyRegistered"];
            return View();
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
                    var member = _MemberRepository.GetMember(model.Login);
                    var userData = member.GetUserData();
                    FormsService.SignIn(model.Login, userData, /*model.RememberMe*/true, ControllerContext.HttpContext.Response);
                    if (!String.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction(MVC.Home.ActionNames.Index, MVC.Home.Name);
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
            return RedirectToAction(MVC.Home.ActionNames.Index, MVC.Home.Name);
        }

        /// <summary>
        /// GET action method to create a new account
        /// </summary>
        /// <returns>The form to fill to create the account</returns>
        [ActionName("inscription")]
        public virtual ActionResult Register()
        {
            return View(new RegisterModel());
        }

        /// <summary>
        /// POST action method to create a new account
        /// add member to db and send an email to the member to activate  his account
        /// </summary>
        /// <param name="model">The account data from the form</param>
        /// <param name="myCaptcha">The captcha to avoid spam</param>
        /// <param name="attempt">The user attempt to the captcha</param>
        /// <returns>Redirect to register succes page if succes, the form with errors if not</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("inscription")]
		public virtual ActionResult Register(RegisterModel model, string myCaptcha, string attempt)
		{
			//check capatcha
			if (!CaptchaHelper.VerifyAndExpireSolution(HttpContext, myCaptcha, attempt))
			{
				ModelState.AddModelError("attempt", Worki.Resources.Validation.ValidationString.VerificationLettersWrong);
			}
			//check model validity
			else if (ModelState.IsValid)
			{
				// Tentative d'inscription de l'utilisateur
				bool createStatusSuccess = false;
				bool addMemberDataSuccess = false;
				string error = string.Empty;
				string field = string.Empty;
				MembershipCreateStatus createStatus = MembershipCreateStatus.UserRejected;
				try
				{
					var fromDB = _MemberRepository.GetMember(model.Email);
					if (fromDB != null)
					{
						error = Worki.Resources.Validation.ValidationString.UsernameExistForThisMail;
						field = "Email";
						throw new Exception(error);
					}
					createStatus = MembershipService.CreateUser(model.Email, model.Password, model.Email);
					createStatusSuccess = createStatus == MembershipCreateStatus.Success;
					if (!createStatusSuccess)
					{
						error = AccountValidation.ErrorCodeToString(createStatus);
						throw new Exception(error);
					}

					//add memberData
					var created = _MemberRepository.GetMember(model.Email);
					_MemberRepository.Update(created.MemberId, m =>
					{
						m.MemberMainData = model.MemberMainData;
					});
					addMemberDataSuccess = true;
				}
				catch (Exception ex)
				{
					//TODO change this
					_Logger.Error(ex.Message);
					if (string.IsNullOrEmpty(error))
						error = "Une erreur est surevnue pendant la sauvegarde";
				}

				if (createStatusSuccess && addMemberDataSuccess)
				{
					//add them to private beta role
					var member = _MemberRepository.GetMember(model.Email);
					//send mail to activate the account
					try
					{
						this.SendRegisterEmail(_EmailService, member);
					}
					catch (Exception ex)
					{
						_Logger.Error(ex.Message);
					}
					return RedirectToAction(MVC.Account.RegisterSuccess());
				}
				else
				{
					ModelState.AddModelError(field, error);
				}
			}
			// Si nous sommes arrivés là, quelque chose a échoué, réafficher le formulaire
			ViewData["PasswordLength"] = MembershipService.MinPasswordLength;
			return View(model);
		}

        /// <summary>
        /// Action method for registration success
        /// </summary>
        /// <returns>The view of registration succes</returns>
        [ActionName("inscription-reussie")]
        public virtual ActionResult RegisterSuccess()
        {
            return View();
        }

        /// <summary>
        /// Action method to activate an account for a member
        /// </summary>
        /// <param name="username">The username to activate</param>
        /// <param name="returnUrl">The key provided by the user via email link</param>
        /// <returns>Redirect to Logon page if the key provided is matching the one in db, if not return to home page</returns>
        [ActionName("activer")]
        public virtual ActionResult Activate(string username, string key)
        {
            if (_MemberRepository.ActivateMember(username, key) == false)
                return RedirectToAction(MVC.Home.Index());
            else
                return RedirectToAction(MVC.Account.LogOn());
        }

        // **************************************
        // URL: /Account/ChangePassword
        // **************************************

        /// <summary>
        /// GET Action method to change the password
        /// </summary>
        /// <param name="username">The username of the account to modify</param>
        /// <param name="returnUrl">The key provided by the user via email link</param>
        /// <returns>the form to fill</returns>
        [ActionName("changer-mdp")]
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult ChangePassword(string username, string key)
        {
            ViewData["PasswordLength"] = MembershipService.MinPasswordLength;
            var member = _MemberRepository.GetMember(username);
            //link not ok, redirect to home
            if (member == null || string.Compare(key, member.EmailKey) != 0)
                return RedirectToAction(MVC.Home.Index());
            return View(new ChangePasswordModel { UserName = username });
        }

        /// <summary>
        /// POST Action method to change the password
        /// </summary>
        /// <param name="model">The change password data from the form</param>
        /// <returns>Password change succes page if ok, the form with error if not</returns>
        [ActionName("changer-mdp")]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ChangePassword(model.UserName, model.OldPassword, model.NewPassword))
                {
                    if (MembershipService.ValidateUser(model.UserName, model.NewPassword))
                    {
                        var member = _MemberRepository.GetMember(model.UserName);
                        var userData = member.GetUserData();
                        FormsService.SignIn(model.UserName, userData, /*model.RememberMe*/true, ControllerContext.HttpContext.Response);
                        return RedirectToAction(MVC.Home.ActionNames.Index, MVC.Home.Name);
                    }
                    else
                    {
                        return RedirectToAction(MVC.Account.ActionNames.ChangePasswordSuccess);
                    }
                }
                else
                {
                    ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.PasswordNotValide);
                }
            }

            // Si nous sommes arrivés là, quelque chose a échoué, réafficher le formulaire
            ViewData["PasswordLength"] = MembershipService.MinPasswordLength;
            return View(model);
        }

        /// <summary>
        /// GET Action method to reset password
        /// </summary>
        /// <returns>the form to fill</returns>
        [ActionName("reset-mdp")]
        public virtual ActionResult ResetPassword()
        {
            return View();
        }

        /// <summary>
        /// POST Action method to reset the password
        /// send email with credentials if ok
        /// </summary>
        /// <param name="model">The reset password data from the form</param>
        /// <returns>Password reset succes page if ok, the form with error if not</returns>
        [HttpPost] 
        [ValidateAntiForgeryToken]
        [ActionName("reset-mdp")]
        public virtual ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ResetPassword(model.EMail))
                {
                    //send mail to activate the account
                    var member = _MemberRepository.GetMember(model.EMail);
                    try
                    {
                        this.SendResetPasswordEmail(_EmailService, member, _MembershipService.GetPassword(member.Email, null));
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error("error", ex);
                    }
                    return RedirectToAction(MVC.Account.ActionNames.ResetPasswordSuccess);
                }
                else
                {
                    ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.MailDoNotMatch);
                }
            }
            return View(model);
        }

        /// <summary>
        /// Action method when password change is ok
        /// </summary>
        /// <returns>the wiew</returns>
        [ActionName("changer-mdp-reussi")]
        public virtual ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        /// <summary>
        /// Action method when password reset is ok
        /// </summary>
        /// <returns>the wiew</returns>
        [ActionName("reset-mdp-reussi")]
        public virtual ActionResult ResetPasswordSuccess()
        {
            return View();
        }

        //OpenIdRelyingParty openId = new OpenIdRelyingParty();
        //public virtual ActionResult OpenId(string openIdUrl)
        //{
        //    var response = openId.GetResponse();
        //    if (response == null)
        //    {
        //        // Stage 2: user submitting Identifier
        //        Identifier id;
        //        if (Identifier.TryParse(openIdUrl, out id))
        //        {
        //            try
        //            {
        //                var request = openId.CreateRequest(openIdUrl);
        //                var fetch = new FetchRequest();
        //                fetch.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
        //                fetch.Attributes.AddRequired(WellKnownAttributes.Name.First);
        //                fetch.Attributes.AddRequired(WellKnownAttributes.Name.Last);
        //                request.AddExtension(fetch);
        //                return request.RedirectingResponse.AsActionResult();
        //            }
        //            catch (ProtocolException ex)
        //            {
        //                _Logger.Error("OpenID Exception...", ex);
        //                return RedirectToAction(MVC.Account.ActionNames.LogOn);
        //            }
        //        }
        //        _Logger.Info("OpenID Error...invalid url. url='" + openIdUrl + "'");
        //        return RedirectToAction(MVC.Account.ActionNames.LogOn);
        //    }

        //    // Stage 3: OpenID Provider sending assertion response
        //    switch (response.Status)
        //    {
        //        case AuthenticationStatus.Authenticated:
        //            var fetch = response.GetExtension<FetchResponse>();
        //            string firstName = "";
        //            string lastName = "";
        //            string email = "";
        //            if (fetch != null)
        //            {
        //                firstName = fetch.GetAttributeValue(WellKnownAttributes.Name.First);
        //                lastName = fetch.GetAttributeValue(WellKnownAttributes.Name.Last);
        //                email = fetch.GetAttributeValue(WellKnownAttributes.Contact.Email);
        //            }
        //            return Content(response.ClaimedIdentifier.ToString() + firstName + lastName + email);
        //        case AuthenticationStatus.Canceled:
        //            _Logger.Info("OpenID: Cancelled at provider.");
        //            return RedirectToAction("Login");
        //        case AuthenticationStatus.Failed:
        //            _Logger.Error("OpenID Exception...", response.Exception);
        //            return RedirectToAction(MVC.Account.ActionNames.LogOn);
        //    }
        //    return RedirectToAction(MVC.Account.ActionNames.LogOn);
        //}
    }
}
