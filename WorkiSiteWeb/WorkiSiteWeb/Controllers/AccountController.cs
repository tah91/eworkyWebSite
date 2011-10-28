using System;
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
					var context = ModelFactory.GetUnitOfWork();
					var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
					var fromDB = mRepo.GetMember(model.Email);
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
					context = ModelFactory.GetUnitOfWork();
					mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
					var created = mRepo.GetMember(model.Email);
					created.MemberMainData = model.MemberMainData;
					context.Commit();
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

                        dynamic activateMail = new Email(MiscHelpers.EmailConstants.EmailView);
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
			ViewData["PasswordLength"] = MembershipService.MinPasswordLength;
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
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            if (mRepo.ActivateMember(username, key) == false)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Shared.SharedString.CorrectThenTryAgain; 
                return RedirectToAction(MVC.Home.Index());
            }
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
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var member = mRepo.GetMember(username);
            //link not ok, redirect to home
            if (member == null || string.Compare(key, member.EmailKey) != 0)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Account.AccountString.ChangePasswordError; ;
                return RedirectToAction(MVC.Home.Index());
            }

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
						var context = ModelFactory.GetUnitOfWork();
						var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
						var member = mRepo.GetMember(model.UserName);
                        var userData = member.GetUserData();
                        FormsService.SignIn(model.UserName, userData, /*model.RememberMe*/true, ControllerContext.HttpContext.Response);
                        TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Account.AccountString.PasswordHaveBeenChanged;
                        return RedirectToAction(MVC.Home.ActionNames.Index, MVC.Home.Name);
                    }
                    else
                    {
                        TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Account.AccountString.PasswordHaveBeenChanged;
                        return RedirectToAction(MVC.Home.Index());
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
					var context = ModelFactory.GetUnitOfWork();
					var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
					var member = mRepo.GetMember(model.EMail);
                    try
                    {
                        var urlHelper = new UrlHelper(ControllerContext.RequestContext);
                        var changePassLink = urlHelper.AbsoluteAction(MVC.Account.ActionNames.ChangePassword, MVC.Account.Name, new { userName = member.Email, key = member.EmailKey });
                        TagBuilder link = new TagBuilder("a");
                        link.MergeAttribute("href", changePassLink);
                        link.InnerHtml = Worki.Resources.Email.ResetPassword.ResetPasswordLink;

                        dynamic resetMail = new Email(MiscHelpers.EmailConstants.EmailView);
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
            return View(model);
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
                // Si on fait un facebook connect depuis les pages de login ou de création de compte, on atterrit sur la Home au retour 
                returnUrl = Url.Action(MVC.Home.Index());
            }

            string redirectUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Url.Action(this.FacebookOAuth());
            
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
                    dynamic me = fbClient.Get("me?fields=id,name,email,first_name,last_name");
                    long facebookId = Convert.ToInt64(me.id);
                    string faceBookEmail = ((string)me.email).ToString();
                    string faceBookFirstName = ((string)me.first_name).ToString();
                    string faceBookLastName = ((string)me.last_name).ToString();

                    var context = ModelFactory.GetUnitOfWork();
                    var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                    var member = mRepo.GetMember(faceBookEmail);
                    
                    if (member != null)
                    {
                        var userData = member.GetUserData();
                        FormsService.SignIn(faceBookEmail, userData, /*model.RememberMe*/true, ControllerContext.HttpContext.Response);

                        if (Url.IsLocalUrl(state))
                        {
                            return Redirect(state);
                        }
                        else
                        {
                            return RedirectToAction(MVC.Home.Index());
                        }
                    }
                    else
                    {
                        bool createStatusSuccess = false;
                        bool addMemberDataSuccess = false;

                        try
                        {
                            MembershipCreateStatus createStatus = MembershipService.CreateUser(faceBookEmail, MiscHelpers.GeneratePassword(12,true), faceBookEmail);
                            createStatusSuccess = createStatus == MembershipCreateStatus.Success;
                            if (!createStatusSuccess)
                            {
                                throw new Exception(AccountValidation.ErrorCodeToString(createStatus));
                            }

                            //add memberData
                            context = ModelFactory.GetUnitOfWork();
                            mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                            var created = mRepo.GetMember(faceBookEmail);
                            created.MemberMainData = new MemberMainData();
                            created.MemberMainData.Avatar = string.Format(MiscHelpers.FaceBookConstants.FacebookProfilePictureUrlPattern, facebookId);
                            created.MemberMainData.Facebook = string.Format(MiscHelpers.FaceBookConstants.FacebookProfileViewPattern, facebookId, facebookId);
                            created.MemberMainData.FirstName = faceBookFirstName;
                            created.MemberMainData.LastName = faceBookLastName;

                            // pour un compte facebook, pas besoin d'activation
                            created.IsApproved = true;
                            created.LastActivityDate = DateTime.Now;
                            created.EmailKey = null;
                            
                            context.Commit();
                            addMemberDataSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            _Logger.Error(ex.Message);
                        }

                        if (createStatusSuccess && addMemberDataSuccess)
                        {
                            //add them to private beta role
                            context = ModelFactory.GetUnitOfWork();
                            mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                            member = mRepo.GetMember(faceBookEmail);
                            var urlHelper = new UrlHelper(ControllerContext.RequestContext);

                            // Send mail
                            try
                            {
								dynamic facebookMail = new Email(MiscHelpers.EmailConstants.EmailView);
								facebookMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                                facebookMail.To = member.Email;
                                facebookMail.Subject = Worki.Resources.Email.Activation.ActivationSubject;
                                facebookMail.ToName = member.MemberMainData.FirstName;
                                facebookMail.Content = string.Format(Worki.Resources.Email.FacebookRegistration.Content, urlHelper.AbsoluteAction(MVC.Profil.ActionNames.Edit, MVC.Profil.Name, new { id = member.MemberId }), member.Email, _MembershipService.GetPassword(member.Email, null));
                                facebookMail.Send();
                            }
                            catch (Exception ex)
                            {
                                _Logger.Error(ex.Message);
                            }

                            var userData = member.GetUserData();
                            FormsService.SignIn(member.Email, userData, /*model.RememberMe*/true, ControllerContext.HttpContext.Response);

                            if (Url.IsLocalUrl(state))
                            {
                                return Redirect(state);
                            }
                            else
                            {
                                return RedirectToAction(MVC.Home.Index());
                            }
                        }
                        else
                        {
                            // TODO Gérer l'erreur de création
                            
                        }
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }

    }
}
