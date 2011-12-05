﻿using System;
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

namespace Worki.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    [RequireHttpsRemote]
	public partial class MemberController : Controller
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
            var vRepo = ModelFactory.GetRepository<IVisitorRepository>(context);
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
                    vRepo.Delete(item => string.Compare(item.Email, user.UserName, StringComparison.InvariantCultureIgnoreCase) == 0);
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
				var hasRole = Roles.IsUserInRole(member.Username, MiscHelpers.BackOfficeConstants.BackOfficeRole);
				if (!hasRole)
				{
					Roles.AddUserToRole(member.Username,MiscHelpers.BackOfficeConstants.BackOfficeRole);
				}
				else
				{
					Roles.RemoveUserFromRole(member.Username,MiscHelpers.BackOfficeConstants.BackOfficeRole);
				}
			}
			catch (Exception ex)
			{
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
    }
}
