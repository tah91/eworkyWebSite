using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure.Repository;
using Worki.Infrastructure.Helpers;
using Worki.Memberships;
using Worki.Infrastructure.Logging;
using Worki.Rest;
using Worki.Web.Helpers;
using Worki.Infrastructure.Email;
using System.Net.Mail;

namespace Worki.Web.Areas.Api.Controllers
{
    public partial class AccountController : Controller
    {
        ILogger _Logger;
        IEmailService _EmailService;
        IMembershipService _MembershipService;

        public AccountController(ILogger logger,
                                 IEmailService emailService,
                                 IMembershipService membershipService)
        {
            _Logger = logger;
            _EmailService = emailService;
            _MembershipService = membershipService;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Register(MemberApiModel formData)
        {
            if (ModelState.IsValid)
            {
                var context = ModelFactory.GetUnitOfWork();
                var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                var sendNewAccountMailPass = false;
                var sendNewAccountMail = false;
                try
                {
                    int memberId;

                    var memberData = new MemberMainData
                    {
                        FirstName = formData.FirstName,
                        LastName = formData.LastName,
                        PhoneNumber = formData.PhoneNumber
                    };
                    if (formData.FacebookId != 0)
                    {
                        memberData.Avatar = this.UploadFile(string.Format(MiscHelpers.FaceBookConstants.FacebookProfilePictureUrlPattern, formData.FacebookId), MiscHelpers.ImageSize.MemberAvatar, Member.AvatarFolder);
                    }
                    if (formData.BirthDate > DateTime.MinValue)
                    {
                        memberData.BirthDate = formData.BirthDate;
                    }
                    if (!string.IsNullOrEmpty(formData.FacebookLink))
                    {
                        memberData.Facebook = formData.FacebookLink;
                    }
                    if (!string.IsNullOrEmpty(formData.PhoneNumber))
                    {
                        memberData.PhoneNumber = formData.PhoneNumber;
                    }

                    if (string.IsNullOrEmpty(formData.Password))
                    {
                        sendNewAccountMailPass = _MembershipService.TryCreateAccount(formData.Email, memberData, out memberId);
                    }
                    else
                    {
                        sendNewAccountMail = _MembershipService.TryCreateAccount(formData.Email, formData.Password, memberData, out memberId);
                    }
                    var member = mRepo.Get(memberId);

                    try
                    {
                        object newMemberMail = null;
                        if (sendNewAccountMailPass)
                        {
                            var urlHelper = new UrlHelper(ControllerContext.RequestContext);

                            var newMemberMailContent = string.Format(Worki.Resources.Email.FacebookRegistration.Content,
                                                                    urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit()),
                                                                    member.Email,
                                                                    _MembershipService.GetPassword(member.Email, null));

                            newMemberMail = _EmailService.PrepareMessageFromDefault(new MailAddress(member.Email, member.MemberMainData.FirstName),
                                Worki.Resources.Email.Activation.ActivationSubject,
                                WebHelper.RenderEmailToString(member.MemberMainData.FirstName, newMemberMailContent));
                          
                        }
                        if (sendNewAccountMail)
                        {
                            var urlHelper = new UrlHelper(ControllerContext.RequestContext);

                            var newMemberMailContent = string.Format(Worki.Resources.Email.FacebookRegistration.ContentWithoutPass,
                                                                    urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit()));

                            newMemberMail = _EmailService.PrepareMessageFromDefault(new MailAddress(member.Email, member.MemberMainData.FirstName),
                                Worki.Resources.Email.Activation.ActivationSubject,
                                WebHelper.RenderEmailToString(member.MemberMainData.FirstName, newMemberMailContent));
                        }
                        context.Commit();

                        if (sendNewAccountMail || sendNewAccountMailPass)
                        {
                            _EmailService.Deliver(newMemberMail);
                        }

                        return new ObjectResult<AuthJson>(ModelHelper.GetAuthData(_MembershipService, formData.Email));
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex.Message);
                        context.Complete();
                        throw ex;
                    }
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    return new ObjectResult<AuthJson>(null, 400, dbEx.GetErrors());
                }
                catch (Exception ex)
                {
                    _Logger.Error("Create", ex);
                    ModelState.AddModelError("", ex.Message);
                    return new ObjectResult<AuthJson>(null, 400, ex.Message);
                }
            }
            else
            {
                return new ObjectResult<AuthJson>(null, 400, ModelState.GetErrors());
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Login(LogOnModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _MembershipService.ValidateUser(model.Login, model.Password);
                    return new ObjectResult<AuthJson>(ModelHelper.GetAuthData(_MembershipService, model.Login));
                }
                catch (Exception ex)
                {
                    return new ObjectResult<AuthJson>(null, 400, ex.Message);
                }
            }
            else
            {
                return new ObjectResult<AuthJson>(null, 400, ModelState.GetErrors());
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult EditInfo(MemberApiModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var context = ModelFactory.GetUnitOfWork();
                    var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                    Member m = mRepo.GetMemberFromToken(model.Token);

                    try
                    {
                        model.UpdateMember(m);
                        context.Commit();
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error("EditInfo", ex);
                        context.Complete();
                        throw ex;
                    }

                    var newContext = ModelFactory.GetUnitOfWork();
                    mRepo = ModelFactory.GetRepository<IMemberRepository>(newContext);

                    m = mRepo.GetMemberFromToken(model.Token);
                    return new ObjectResult<AuthJson>(m.GetAuthData());
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    return new ObjectResult<AuthJson>(null, 400, dbEx.GetErrors());
                }
                catch (Exception ex)
                {
                    return new ObjectResult<AuthJson>(null, 400, ex.Message);
                }
            }
            else
            {
                return new ObjectResult<AuthJson>(null, 400, ModelState.GetErrors());
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult EditPassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var context = ModelFactory.GetUnitOfWork();
                    var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                    var member = mRepo.GetMemberFromToken(model.Token);

                    if (_MembershipService.ChangePassword(member.Username, model.OldPassword, model.NewPassword))
                    {
                        var newContext = ModelFactory.GetUnitOfWork();
                        mRepo = ModelFactory.GetRepository<IMemberRepository>(newContext);

                        member = mRepo.GetMemberFromToken(model.Token);
                        return new ObjectResult<AuthJson>(member.GetAuthData());
                    }
                    else
                    {
                        throw new Exception(Worki.Resources.Validation.ValidationString.PasswordNotValide);
                    }
                }
                catch (Exception ex)
                {
                    return new ObjectResult<AuthJson>(null, 400, ex.Message);
                }
            }
            else
            {
                return new ObjectResult<AuthJson>(null, 400,  ModelState.GetErrors());
            }
        }
    }
}
