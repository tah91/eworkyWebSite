using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure.Repository;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure;
using Worki.Data.Models;
using Worki.Memberships;
using System.Web.Security;
using Worki.Web.Helpers;
using Worki.Infrastructure.Email;

namespace Worki.Web.Areas.Admin.Controllers
{
	public partial class MemberController : AdminControllerBase
    {
        IMembershipService _MembershipService;
        ILogger _Logger;

        public MemberController(IMembershipService memberShipservice,
                                ILogger logger)
        {
            _MembershipService = memberShipservice;
            _Logger = logger;
        }

        #region Members

        /// <summary>
        /// Prepares a web page containing a paginated list of members
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult IndexUser(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            int pageValue = page ?? 1;
            var members = mRepo.Get((pageValue - 1) * MiscHelpers.Constants.PageSize, MiscHelpers.Constants.PageSize, m => m.MemberId);
            var viewModel = new PagingList<MemberAdminModel>()
            {
                List = _MembershipService.GetAdminMapping(members).ToList(),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = mRepo.GetCount()
                }
            };
            return View(MVC.Admin.Member.Views.IndexUser, viewModel);
        }


        /// <summary>
        /// Action method to unlock an account for a member
        /// </summary>
        /// <param name="id">The id of account to unlock</param>
        /// <returns>Redirect to User index</returns>
        public virtual ActionResult UnlockUser(string username)
        {
            try
            {
                _MembershipService.UnlockMember(username);
            }
            catch (Exception ex)
            {
                _Logger.Error("UnlockUser", ex);
            }

            return RedirectToAction(MVC.Admin.Member.IndexUser());
        }

        /// <summary>
        /// POST Action method to update admin roles
        /// and redirect to user admin home
        /// </summary>
        /// <param name="collection">form containg the list of ids to push to admin role</param>
        /// <returns>Redirect to return url</returns>
        public virtual ActionResult ChangeUserRole(int page, int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var member = mRepo.Get(id);
            try
            {
                if (member == null)
                {
                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.UserNotFound;
                    return RedirectToAction(MVC.Admin.Member.IndexUser(page));
                }

                if (!Roles.IsUserInRole(member.Email, MiscHelpers.AdminConstants.AdminRole))
                {
                    Roles.AddUserToRole(member.Email, MiscHelpers.AdminConstants.AdminRole);
                }
                else
                {
                    Roles.RemoveUserFromRole(member.Email, MiscHelpers.AdminConstants.AdminRole);
                }

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.RoleHaveBeenSet;
            }
            catch (Exception e)
            {
                _Logger.Error(e.Message);
            }

            return RedirectToAction(MVC.Admin.Member.IndexUser(page));
        }


        /// <summary>
        /// Action method to delete a member
        /// </summary>
        /// <param name="username">user to delete</param>
        /// <returns>View to confirm the delete</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("supprimer-utilisateur")]
        public virtual ActionResult DeleteUser(string username, string returnUrl)
        {
            var user = _MembershipService.GetUser(username);
            if (user == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.UserNotFound;
                return RedirectToAction(MVC.Admin.Member.IndexUser());
            }
            TempData["returnUrl"] = returnUrl;
            return View(new User { UserName = user.UserName });
        }

        /// <summary>
        /// POST Action method to delete a member
        /// and redirect to user admin home
        /// </summary>
        /// <param name="user">user to delete</param>
        /// <param name="returnUrl">url to redirect to</param>
        /// <returns>Redirect to return url</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("supprimer-utilisateur")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteUser(User user, string confirmButton, string returnUrl)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var member = mRepo.GetMember(user.UserName);
            if (member == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.UserNotFound;
                return RedirectToAction(MVC.Admin.Member.IndexUser());
            }
            else
            {
                try
                {
                    mRepo.Delete(member.MemberId);
                    context.Commit();
                }
                catch (Exception ex)
                {
                    _Logger.Error("", ex);
                    context.Complete();
                }

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.UserHaveBeenDel;

                return Redirect(returnUrl);
            }
        }

        #endregion

        #region Admin Members

        public virtual ActionResult AdminList(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            int pageValue = page ?? 1;
            var admins = mRepo.GetMembers(MiscHelpers.AdminConstants.AdminRole);
            var viewModel = new PagingList<Member>()
            {
				List = admins.Skip((pageValue - 1) * MiscHelpers.Constants.PageSize).Take(MiscHelpers.Constants.PageSize).ToList(),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = admins.Count
                }
            };
            return View(MVC.Admin.Member.Views.Admins, viewModel);
        }

        #endregion

		#region Admin Members

        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult AddOwnerPlace()
        {
            var model = new OwnerLocalisationModel();
            return View(model);
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult AddOwnerPlace(OwnerLocalisationModel model)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);

            if (ModelState.IsValid)
            {
                MemberMainData maindata = new MemberMainData();
                maindata.FirstName = model.Firstname;
                maindata.LastName = model.Name;
                maindata.PhoneNumber = model.PhoneNumber;

                try
                {
                    var loc = lRepo.Get(model.LocalisationId);
                    if (loc == null)
                    {
                        throw new Exception(Worki.Resources.Models.Localisation.Localisation.LocalisationError);
                    }
                    if (loc.OwnerID != 1)
                    {
                        throw new Exception(Worki.Resources.Models.Localisation.Localisation.OwnerAllreadyExist);
                    }

                    int memberId;
                    bool sendNewAccountMail = _MembershipService.TryCreateAccount(model.Email.ToString(), maindata, out memberId, false);

                    if (memberId == 0)
                    {
                        throw new Exception("Utilisateur non crée");
                    }

                    var member = mRepo.Get(memberId);
                    member.MemberMainData.BOStatus = (int)eBOStatus.Done;
                    Roles.AddUserToRole(member.Username, MiscHelpers.BackOfficeConstants.BackOfficeRole);
                    loc.OwnerID = memberId;
                    context.Commit();

                    if (sendNewAccountMail)
                    {
                        var urlHelper = new UrlHelper(ControllerContext.RequestContext);
                        var activationLink = urlHelper.ActionAbsolute(MVC.Account.Activate(member.Email, member.EmailKey));
                        TagBuilder link = new TagBuilder("a");
                        link.MergeAttribute("href", activationLink);
                        link.InnerHtml = activationLink;

                        dynamic ownerMail = null;

                        ownerMail = new Email(MVC.Emails.Views.Email);
                        ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                        ownerMail.To = model.Email;
                        ownerMail.ToName = model.Firstname;

                        ownerMail.Subject = string.Format(Worki.Resources.Email.Common.OwnershipSubject, loc.Name);
                        ownerMail.Content = string.Format(Worki.Resources.Email.Common.AdminOwnershipAndAccount,
                                                            loc.Name,
                                                            activationLink.ToString(),
                                                            loc.GetDetailFullUrl(Url),
                                                            model.Email,
                                                            _MembershipService.GetPassword(model.Email, null));

                        ownerMail.Send();
                    }
                    else
                    {
                        var urlHelper = new UrlHelper(ControllerContext.RequestContext);
                        var boLink = urlHelper.ActionAbsolute(MVC.Backoffice.Localisation.Index(loc.ID));
                        TagBuilder link = new TagBuilder("a");
                        link.MergeAttribute("href", boLink);
                        link.InnerHtml = Worki.Resources.Views.Account.AccountString.OwnerSpace;

                        dynamic ownerMail = null;

                        ownerMail = new Email(MVC.Emails.Views.Email);
                        ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                        ownerMail.To = model.Email;
                        ownerMail.ToName = model.Firstname;

                        ownerMail.Subject = string.Format(Worki.Resources.Email.Common.OwnershipSubject, loc.Name);
                        ownerMail.Content = string.Format(Worki.Resources.Email.Common.AdminOwnership,
                                                            loc.Name,
                                                            boLink.ToString());

                        ownerMail.Send();
                    }

                    return RedirectToAction(MVC.Admin.Member.IndexOwner());
                }
                catch (Exception ex)
                {
                    _Logger.Error("", ex);
                    ModelState.AddModelError("", ex.Message);
                    context.Complete();
                }
            }

            return View(model);
        }

		public virtual ActionResult IndexOwner(int? page)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			int pageValue = page ?? 1;
			var owners = mRepo.GetOwners();
			var viewModel = new PagingList<Member>()
			{
				List = owners.Skip((pageValue - 1) * MiscHelpers.Constants.PageSize).Take(MiscHelpers.Constants.PageSize).ToList(),
				PagingInfo = new PagingInfo
				{
					CurrentPage = pageValue,
					ItemsPerPage = MiscHelpers.Constants.PageSize,
					TotalItems = owners.Count()
				}
			};
			return View(viewModel);
		}

		public virtual ActionResult SetBackoffice(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var member = mRepo.Get(id);
			try
			{
				var roleToGive = !Roles.IsUserInRole(member.Username, MiscHelpers.BackOfficeConstants.BackOfficeRole);
				if (roleToGive)
				{
                    member.MemberMainData.BOStatus = (int)eBOStatus.Done;
					Roles.AddUserToRole(member.Username,MiscHelpers.BackOfficeConstants.BackOfficeRole);
				}
				else
				{
                    member.MemberMainData.BOStatus = (int)eBOStatus.None;
					Roles.RemoveUserFromRole(member.Username,MiscHelpers.BackOfficeConstants.BackOfficeRole);
				}
                context.Commit();
                //email to tell bo has been given
                if (roleToGive)
                {
                    var urlHelper = new UrlHelper(ControllerContext.RequestContext);
                    var boHomeUrl = urlHelper.ActionAbsolute(MVC.Backoffice.Home.Index());
                    TagBuilder boHomeLink = new TagBuilder("a");
                    boHomeLink.MergeAttribute("href", boHomeUrl);
                    boHomeLink.InnerHtml = Worki.Resources.Email.Activation.BOLink;

                    dynamic confirmationMail = new Email(MVC.Emails.Views.Email);
                    confirmationMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                    confirmationMail.To = member.Email;
                    confirmationMail.ToName = member.GetDisplayName();
                    confirmationMail.Subject = Worki.Resources.Email.Activation.BOConfirmSubject;
                    confirmationMail.Content = string.Format(Worki.Resources.Email.Activation.BOConfirmContent, boHomeLink);
                    confirmationMail.Send();
                }
			}
			catch (Exception ex)
			{
                context.Complete();
				_Logger.Error("SetBackoffice", ex);
			}

			return RedirectToAction(MVC.Admin.Member.IndexOwner());
		}

		#endregion

        #region Member Leaderboard

        public virtual ActionResult Leaderboard(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            int pageValue = page ?? 1;
            var members = mRepo.GetLeaders();
            foreach (var member in members)
            {
                member.Score = mRepo.Get(member.MemberId).ComputeScore();
            }
            var viewModel = new PagingList<MemberAdminModel>()
            {
                List = members.OrderByDescending(x => x.Score).Skip((pageValue - 1) * MiscHelpers.Constants.PageSize).ToList(),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = members.Count
                }
            };
            return View(MVC.Admin.Member.Views.UsersLeaderboard, viewModel);
        }

        #endregion

        #region Member PendingBO

        public virtual ActionResult PendingBO(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            int pageValue = page ?? 1;
            var askers = mRepo.GetMany(m => m.MemberMainData.BOStatus == (int)eBOStatus.Pending);
            var viewModel = new PagingList<Member>()
            {
                List = askers.Skip((pageValue - 1) * MiscHelpers.Constants.PageSize).Take(MiscHelpers.Constants.PageSize).ToList(),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = askers.Count()
                }
            };
            return View(viewModel);
        }

        public virtual ActionResult ActivatedBO(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            int pageValue = page ?? 1;
            var activated = mRepo.GetMany(m => m.MemberMainData.BOStatus == (int)eBOStatus.Done);
            var viewModel = new PagingList<Member>()
            {
                List = activated.Skip((pageValue - 1) * MiscHelpers.Constants.PageSize).Take(MiscHelpers.Constants.PageSize).ToList(),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = activated.Count()
                }
            };
            return View(viewModel);
        }

        #endregion

        #region Member API

        public virtual ActionResult MemberWithApi(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            int pageValue = page ?? 1;
            var members = mRepo.GetMany(m => !string.IsNullOrEmpty(m.MemberMainData.ApiKey));
            var viewModel = new PagingList<Member>()
            {
                List = members.Skip((pageValue - 1) * MiscHelpers.Constants.PageSize).Take(MiscHelpers.Constants.PageSize).ToList(),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = members.Count()
                }
            };
            return View(viewModel);
        }

        /// <summary>
        /// Prepares a web page containing the form to create a new member api
        /// </summary>
        /// <returns>The action result.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult CreateMemberApi()
        {
            return View(new User());
        }

        /// <summary>
        /// Add the member api from the form to the repository, then redirect to index
        /// </summary>
        /// <param name="welcomePeople">data from the form</param>
        /// <returns>redirect to index</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult CreateMemberApi(User formModel)
        {
            if (ModelState.IsValid)
            {
                var context = ModelFactory.GetUnitOfWork();
                var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                try
                {
                    var member = mRepo.GetMember(formModel.UserName);
                    if (member == null)
                        throw new NullReferenceException();

                    member.MemberMainData.ApiKey = Guid.NewGuid().ToString();
                    context.Commit();

                    return RedirectToAction(MVC.Admin.Member.MemberWithApi());
                }
                catch (Exception ex)
                {
                    context.Complete();
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(formModel);
        }

        /// <summary>
        /// Remove the member api from the form to the repository, then redirect to index
        /// </summary>
        /// <param name="welcomePeople">data from the form</param>
        /// <returns>redirect to index</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult DeleteMemberApi(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            try
            {
                var member = mRepo.Get(id);
                if (member == null)
                    throw new NullReferenceException();

                member.MemberMainData.ApiKey = null;
                context.Commit();
            }
            catch (Exception ex)
            {
                context.Complete();
                ModelState.AddModelError("", ex.Message);
            }
            return RedirectToAction(MVC.Admin.Member.MemberWithApi());
        }

        #endregion
    }
}
