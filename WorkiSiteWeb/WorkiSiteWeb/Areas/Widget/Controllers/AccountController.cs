using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Service;
using Worki.Memberships;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;
using Worki.Data.Models;
using Worki.Infrastructure.Repository;

namespace Worki.Web.Areas.Widget.Controllers
{
    public partial class AccountController : ControllerBase
    {
        IFormsAuthenticationService _FormsService;
        IMembershipService _MembershipService;

        public AccountController(ILogger logger,
                                IObjectStore objectStore,
                                IFormsAuthenticationService formsService, 
                                IMembershipService membershipService)
            : base(logger, objectStore)
        {
           _FormsService = formsService;
           _MembershipService = membershipService;
        }

        public virtual ActionResult LogOn()
        {
            return PartialView(MVC.Widget.Account.Views._Login, new LogOnModel());
        }

        /// <summary>
        /// POST action method to login to an account, add cookie for display name
        /// </summary>
        /// <param name="model">The logon data from the form</param>
        /// <param name="returnUrl">The url to redirect to in case of sucess</param>
        /// <returns>Redirect to return url if any, if not to home page</returns>
        [HttpPost]
        [HandleModelStateException]
        public virtual ActionResult LogOn(LogOnModel model)
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

                        return Json(Url.RequestContext.HttpContext.Request.UrlReferrer);
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
            throw new ModelStateException(ModelState);
        }

        const string MemberDisplayNameString = "MemberDisplayName";

        public virtual ActionResult LogOff()
        {
            _FormsService.SignOut();
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(MemberDisplayNameString))
            {
                HttpCookie cookie = ControllerContext.HttpContext.Request.Cookies[MemberDisplayNameString];
                cookie.Expires = DateTime.UtcNow.AddDays(-1);
                this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
            }
            return RedirectToAction(MVC.Widget.Localisation.Index());
        }

    }
}
