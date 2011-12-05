﻿using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Worki.Data.Models;
using Worki.Infrastructure;
using Worki.Infrastructure.Logging;
using Worki.Memberships;
using Worki.Web.Helpers;
using Postal;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Repository;
using Facebook;
using System.Collections.Generic;

namespace Worki.Web.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    public partial class AccountController : Controller
    {
        const string MemberDisplayNameString = "MemberDisplayName";

        IFormsAuthenticationService _FormsService;
        IMembershipService _MembershipService;
        ILogger _Logger;
        IEmailService _EmailService;

        public AccountController(   IFormsAuthenticationService formsService, 
                                    IMembershipService membershipService,
                                    ILogger logger,
                                    IEmailService emailService)
        {
            this._FormsService = formsService;
            this._MembershipService = membershipService;
            this._Logger = logger;
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
                try
                {
                    if (_MembershipService.ValidateUser(model.Login, model.Password))
                    {
                        var context = ModelFactory.GetUnitOfWork();
                        var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                        var member = mRepo.GetMember(model.Login);
                        var userData = member.GetUserData();
                        _FormsService.SignIn(model.Login, userData, /*model.RememberMe*/true, ControllerContext.HttpContext.Response);
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
                catch (Member.ValidationException ex)
                {
                    _Logger.Error("LogOn", ex);
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _Logger.Error("LogOn", ex);
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
            _FormsService.SignOut();
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(MemberDisplayNameString))
            {
                HttpCookie cookie = ControllerContext.HttpContext.Request.Cookies[MemberDisplayNameString];
                cookie.Expires = DateTime.UtcNow.AddDays(-1);
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
					var context = ModelFactory.GetUnitOfWork();
					var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
					var fromDB = mRepo.GetMember(model.Email);
					if (fromDB != null)
					{
						error = Worki.Resources.Validation.ValidationString.UsernameExistForThisMail;
						field = "Email";
						throw new Exception(error);
					}
					createStatus = _MembershipService.CreateUser(model.Email, model.Password, model.Email);
					createStatusSuccess = createStatus == MembershipCreateStatus.Success;
					if (!createStatusSuccess)
					{
						error = AccountValidation.ErrorCodeToString(createStatus);
						throw new Exception(error);
					}

					//add memberData
					context = ModelFactory.GetUnitOfWork();
					mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
					var created = mRepo.GetMember(model.Email);
					created.MemberMainData = model.MemberMainData;
					context.Commit();
					addMemberDataSuccess = true;
				}
				catch (Exception ex)
				{
					_Logger.Error(ex.Message);
					if (string.IsNullOrEmpty(error))
						error = Worki.Resources.Views.Account.InscriptionString.ErrorSave;
				}

				if (createStatusSuccess && addMemberDataSuccess)
				{
					//add them to private beta role
					var context = ModelFactory.GetUnitOfWork();
					var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
					var member = mRepo.GetMember(model.Email);
					//send mail to activate the account
					try
					{
                        var urlHelper = new UrlHelper(ControllerContext.RequestContext);
                        var activationLink = urlHelper.AbsoluteAction(MVC.Account.ActionNames.Activate, MVC.Account.Name, new { userName = member.Email, key = member.EmailKey });
                        TagBuilder link = new TagBuilder("a");
                        link.MergeAttribute("href", activationLink);
                        link.InnerHtml = activationLink;

                        dynamic activateMail = new Email(MVC.Emails.Views.Email);
						activateMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                        activateMail.To = member.Email;
                        activateMail.Subject = Worki.Resources.Email.Activation.ActivationSubject;
                        activateMail.ToName = member.MemberMainData.FirstName;
                        activateMail.Content = string.Format(Worki.Resources.Email.Activation.ActivationContent, link.ToString(), member.Email);
                        activateMail.Send();
					}
					catch (Exception ex)
					{
						_Logger.Error(ex.Message);
                    }
                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Account.AccountString.ConfirmationMail;
                    return RedirectToAction(MVC.Home.Index());
				}
				else
				{
					ModelState.AddModelError(field, error);
				}
			}
			// Si nous sommes arrivés là, quelque chose a échoué, réafficher le formulaire
			ViewData["PasswordLength"] = _MembershipService.MinPasswordLength;
			return View(model);
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
            if (_MembershipService.ActivateMember(username, key) == false)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Shared.SharedString.CorrectThenTryAgain;
                return RedirectToAction(MVC.Home.Index());
            }
            else
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Shared.SharedString.ActivationCongratz;
                return RedirectToAction(MVC.Account.LogOn());
            }
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
                try
                {
                    if (_MembershipService.ResetPassword(model.EMail))
                    {
                        //send mail to activate the account
                        var context = ModelFactory.GetUnitOfWork();
                        var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                        var member = mRepo.GetMember(model.EMail);
                        try
                        {
                            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
							var profilLink = urlHelper.ActionAbsolute(MVC.Dashboard.Home.Index());
                            TagBuilder link = new TagBuilder("a");
                            link.MergeAttribute("href", profilLink);
                            link.InnerHtml = Worki.Resources.Email.ResetPassword.ResetPasswordLink;

                            dynamic resetMail = new Email(MVC.Emails.Views.Email);
                            resetMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                            resetMail.To = member.Email;
                            resetMail.Subject = Worki.Resources.Email.ResetPassword.ResetPasswordSubject;
                            resetMail.ToName = member.MemberMainData.FirstName;
                            resetMail.Content = string.Format(Worki.Resources.Email.ResetPassword.ResetPasswordContent, member.Email, _MembershipService.GetPassword(member.Email, null), link.ToString());
                            resetMail.Send();
                        }
                        catch (Exception ex)
                        {
                            _Logger.Error("error", ex);
                        }

                        TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Account.AccountString.PasswordHaveBeenChanged;
                        return RedirectToAction(MVC.Home.Index());
                    }
                    else
                    {
                        ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.MailDoNotMatch);
                    }
                }
                catch (Member.ValidationException ex)
                {
                    _Logger.Error("LogOn", ex);
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _Logger.Error("LogOn", ex);
                    ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.MailDoNotMatch);
                }
            }
            return View(model);
        }


        /// <summary>
        /// Redirige vers le Auth Dialog de Facebook Connect avec les paramètres et permissions nécessaires sur les données utiles 
        /// à la création de compte
        /// </summary>
        /// <returns></returns>
        [ActionName("facebook-logon")]
        public virtual ActionResult FacebookLogOn()
        {
            string returnUrl = Request.UrlReferrer.AbsoluteUri;

            if (Request.UrlReferrer.AbsolutePath == Url.Action(this.ActionNames.LogOn) || Request.UrlReferrer.AbsolutePath == Url.Action(this.ActionNames.Register))
            {
                var parameters = HttpUtility.ParseQueryString(Request.UrlReferrer.Query);
                if (!string.IsNullOrEmpty(parameters["returnUrl"]))
                    returnUrl = parameters["returnUrl"];
                else
                    // Si on fait un facebook connect depuis les pages de login ou de création de compte, on atterrit sur la Home au retour 
                    returnUrl = Url.Action(MVC.Home.Index());
            }

            string redirectUrl = Url.ActionAbsolute(this.FacebookOAuth());
            
            // Liste des permissions demandées par eWorky sur le profil facebook du user
            // Pour la liste exhaustives des permissions, voir http://developers.facebook.com/docs/reference/api/permissions/
            string[] permissions = new[] { "user_location", "email", "offline_access" };

            var oAuthClient = new FacebookOAuthClient(FacebookApplication.Current);
            oAuthClient.RedirectUri = new Uri(redirectUrl);

            var scope = new System.Text.StringBuilder();
            
            if (permissions != null && permissions.Length > 0)
            {
                scope.Append(string.Join(",", permissions));
            }

            var loginUri = oAuthClient.GetLoginUrl(new Dictionary<string, object> { { "state", returnUrl }, { "scope", scope.ToString() } });
            return Redirect(loginUri.AbsoluteUri);
        }


        /// <summary>
        /// Callback appelé après l'authentification réussie via le Auth Dialog de Facebook Connect
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
		[ActionName("facebook-oauth")]
		public virtual ActionResult FacebookOAuth(string code, string state)
		{
			FacebookOAuthResult oauthResult;
			if (FacebookOAuthResult.TryParse(Request.Url, out oauthResult))
			{
				if (oauthResult.IsSuccess)
				{
					string redirectUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Url.Action(this.FacebookOAuth());

					var oAuthClient = new FacebookOAuthClient(FacebookApplication.Current);
					oAuthClient.RedirectUri = new Uri(redirectUrl);
					dynamic tokenResult = oAuthClient.ExchangeCodeForAccessToken(code);
					string accessToken = tokenResult.access_token;

					//DateTime expiresOn = DateTime.MaxValue;

					//if (tokenResult.ContainsKey("expires"))
					//{
					//    DateTimeConvertor.FromUnixTime(tokenResult.expires);
					//}

					FacebookClient fbClient = new FacebookClient(accessToken);
					dynamic me = fbClient.Get("me?fields=id,name,email,first_name,last_name,link");
					long facebookId = Convert.ToInt64(me.id);
					string faceBookEmail = ((string)me.email).ToString();
					string faceBookFirstName = ((string)me.first_name).ToString();
					string faceBookLastName = ((string)me.last_name).ToString();
					string faceBookLink = ((string)me.link).ToString();

					var context = ModelFactory.GetUnitOfWork();
					var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
					var member = mRepo.GetMember(faceBookEmail);

					if (member == null)
					{
						bool created = false;
						int memberId = 0;
						try
						{
							var memberData = new MemberMainData
							{
								Avatar = string.Format(MiscHelpers.FaceBookConstants.FacebookProfilePictureUrlPattern, facebookId),
								Facebook = faceBookLink,//string.Format(MiscHelpers.FaceBookConstants.FacebookProfileViewPattern, facebookId, facebookId),
								FirstName = faceBookFirstName,
								LastName = faceBookLastName
							};
							created = _MembershipService.TryCreateAccount(faceBookEmail, memberData, out memberId);
						}
						catch (Exception ex)
						{
							_Logger.Error(ex.Message);
						}

						if (created)
						{
							context = ModelFactory.GetUnitOfWork();
							mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
							member = mRepo.Get(memberId);
							var urlHelper = new UrlHelper(ControllerContext.RequestContext);

							// Send mail
							try
							{
								dynamic facebookMail = new Email(MVC.Emails.Views.Email);
								facebookMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
								facebookMail.To = member.Email;
								facebookMail.Subject = Worki.Resources.Email.Activation.ActivationSubject;
								facebookMail.ToName = member.MemberMainData.FirstName;
								facebookMail.Content = string.Format(Worki.Resources.Email.FacebookRegistration.Content,
																	urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit()), 
																	member.Email, 
																	_MembershipService.GetPassword(member.Email, null));
								facebookMail.Send();
							}
							catch (Exception ex)
							{
								_Logger.Error(ex.Message);
							}

							TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Account.InscriptionString.RegisterEworky;
						}
						else
						{
							return RedirectToAction(MVC.Home.Index());
						}
					}

					var userData = member.GetUserData();
					_FormsService.SignIn(member.Email, userData, /*model.RememberMe*/true, ControllerContext.HttpContext.Response);

					if (Url.IsLocalUrl(state))
					{
						return Redirect(state);
					}
					else
					{
						return RedirectToAction(MVC.Home.Index());
					}
				}
			}
			return RedirectToAction(MVC.Home.Index());
		}
    }
}
