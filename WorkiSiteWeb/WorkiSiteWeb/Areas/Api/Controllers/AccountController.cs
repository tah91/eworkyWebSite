﻿using System;
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
using Postal;

namespace Worki.Web.Areas.Api.Controllers
{
    public partial class AccountController : Controller
    {
        ILogger _Logger;
        IMembershipService _MembershipService;

        public AccountController(ILogger logger,
                                 IMembershipService membershipService)
        {
            _Logger = logger;
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
                        member.MemberMainData.PhoneNumber = formData.PhoneNumber;
                        dynamic newMemberMail = null;
                        if (sendNewAccountMailPass)
                        {
                            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
                            var editprofilUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit());
                            TagBuilder profilLink = new TagBuilder("a");
                            profilLink.MergeAttribute("href", editprofilUrl);
                            profilLink.InnerHtml = Worki.Resources.Views.Account.AccountString.EditMyProfile;

                            var editpasswordUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit());
                            TagBuilder passwordLink = new TagBuilder("a");
                            passwordLink.MergeAttribute("href", editpasswordUrl);
                            passwordLink.InnerHtml = Worki.Resources.Views.Account.AccountString.ChangeMyPassword;

                            newMemberMail = new Email(MVC.Emails.Views.Email);
                            newMemberMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                            newMemberMail.To = formData.Email;
                            newMemberMail.ToName = formData.FirstName;

                            newMemberMail.Subject = Worki.Resources.Email.BookingString.BookingNewMemberSubject;
                            /*
                            newMemberMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingNewMember,
                                                                    Localisation.GetOfferType(offer.Type),
                                                                    formData.MemberBooking.GetStartDate(),
                                                                    formData.MemberBooking.GetEndDate(),
                                                                    locName,
                                                                    offer.Localisation.Adress,
                                                                    formData.Email,
                                                                    _MembershipService.GetPassword(formData.Email, null),
                                                                    passwordLink,
                                                                    profilLink);
                             */
                            newMemberMail.Content = "Vous vous êtes bien inscrit via mobile.\nVotre mot de passe est : " +
                                _MembershipService.GetPassword(formData.Email, null) + " (" + passwordLink + ").";
                        }
                        if (sendNewAccountMail)
                        {
                            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
                            var editprofilUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit());
                            TagBuilder profilLink = new TagBuilder("a");
                            profilLink.MergeAttribute("href", editprofilUrl);
                            profilLink.InnerHtml = Worki.Resources.Views.Account.AccountString.EditMyProfile;

                            newMemberMail = new Email(MVC.Emails.Views.Email);
                            newMemberMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                            newMemberMail.To = formData.Email;
                            newMemberMail.ToName = formData.FirstName;

                            newMemberMail.Subject = Worki.Resources.Email.BookingString.BookingNewMemberSubject;
                            /*
                            newMemberMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingNewMember,
                                                                    Localisation.GetOfferType(offer.Type),
                                                                    formData.MemberBooking.GetStartDate(),
                                                                    formData.MemberBooking.GetEndDate(),
                                                                    locName,
                                                                    offer.Localisation.Adress,
                                                                    formData.Email,
                                                                    _MembershipService.GetPassword(formData.Email, null),
                                                                    passwordLink,
                                                                    profilLink);
                             */
                            newMemberMail.Content = "Vous vous êtes bien inscrit via mobile.\n";
                        }
                        context.Commit();

                        if (sendNewAccountMail || sendNewAccountMailPass)
                        {
                            newMemberMail.Send();
                        }

                        var newContext = ModelFactory.GetUnitOfWork();
                        mRepo = ModelFactory.GetRepository<IMemberRepository>(newContext);
                        Member m = mRepo.GetMember(formData.Email);

                        return new ObjectResult<AuthJson>(m.GetAuthJson());
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex.Message);
                        context.Complete();
                        throw ex;
                    }
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
                    var context = ModelFactory.GetUnitOfWork();
                    var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                    Member m = mRepo.GetMember(model.Login);

                    return new ObjectResult<AuthJson>(m.GetAuthJson());
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
        public virtual ActionResult EditInfo(string id, MemberApiModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var context = ModelFactory.GetUnitOfWork();
                    var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                    Member m = mRepo.GetMemberFromToken(id);

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

                    m = mRepo.GetMemberFromToken(id);
                    return new ObjectResult<AuthJson>(m.GetAuthJson());
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
        public virtual ActionResult EditPassword(string id, ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var context = ModelFactory.GetUnitOfWork();
                    var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                    var member = mRepo.GetMemberFromToken(id);

                    if (_MembershipService.ChangePassword(member.Username, model.OldPassword, model.NewPassword))
                    {
                        var newContext = ModelFactory.GetUnitOfWork();
                        mRepo = ModelFactory.GetRepository<IMemberRepository>(newContext);

                        member = mRepo.GetMemberFromToken(id);
                        return new ObjectResult<AuthJson>(member.GetAuthJson());
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
