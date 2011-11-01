using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;
using Worki.Data.Repository;
using Worki.Data.Models;
using System.Linq;
using Worki.Infrastructure;
using Worki.Infrastructure.Repository;
using Worki.Infrastructure.Helpers;
using System.Web.Security;
using Worki.Memberships;

namespace Worki.Web.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
	public partial class ProfilController : Controller
	{
		#region Private

		ILogger _Logger;
		IMembershipService _MembershipService;

		ProfilDashboardModel GetDashboard(Member member, bool isPrivate = false, int p1 = 1, int p2 = 1, int p3 = 1, int p4 = 1)
		{
			//get fav localisations
			var favLocs = new List<Localisation>();
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			foreach (var item in member.FavoriteLocalisations.Skip((p1 - 1) * LocalisationPageSize).Take(LocalisationPageSize))
			{
				favLocs.Add(lRepo.Get(item.LocalisationId));
			}
			//added localisations
			var addedLoc = member.Localisations.Skip((p2 - 1) * LocalisationPageSize).Take(LocalisationPageSize).ToList();

			var profilDashboard = new ProfilDashboardModel
			{
				Member = member,
				FavoriteLocalisations = favLocs,
				FavoriteLocalisationsPI = new PagingInfo { CurrentPage = p1, ItemsPerPage = LocalisationPageSize, TotalItems = member.FavoriteLocalisations.Count },
				AddedLocalisations = addedLoc,
				AddedLocalisationsPI = new PagingInfo { CurrentPage = p2, ItemsPerPage = LocalisationPageSize, TotalItems = member.Localisations.Count },
				IsPrivate = isPrivate
			};

			if (isPrivate)
			{
				//posted comments
				var postedComments = member.Comments.Skip((p3 - 1) * CommentPageSize).Take(CommentPageSize).ToList();
				//related comments
				var relatedCom = new List<Comment>();
				foreach (var loc in addedLoc)
				{
					relatedCom.AddRange(loc.Comments.ToList());
				}
				relatedCom = relatedCom.Skip((p4 - 1) * CommentPageSize).Take(CommentPageSize).ToList();
				profilDashboard.PostedComments = postedComments;
				profilDashboard.PostedCommentsPI = new PagingInfo { CurrentPage = p3, ItemsPerPage = CommentPageSize, TotalItems = member.Comments.Count };
				profilDashboard.RelatedComments = relatedCom;
				profilDashboard.RelatedCommentsPI = new PagingInfo { CurrentPage = p4, ItemsPerPage = CommentPageSize, TotalItems = relatedCom.Count };
			}
			return profilDashboard;
		}

		#endregion

		public ProfilController(ILogger logger,
								IMembershipService membershipService)
		{
			_MembershipService = membershipService;
			_Logger = logger;
		}

		/// <summary>
		/// GET Action result to show profil details
		/// </summary>
		/// <param name="id">id of the member</param>
		/// <returns>View containing profil details</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		[ActionName("details")]
		public virtual ActionResult Detail(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var member = mRepo.Get(id);
			if (member == null || member.MemberMainData == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Profile.ProfileString.MemberNotFound;
                return RedirectToAction(MVC.Home.Index());
            }

			var dashboard = GetDashboard(member);
			return View(MVC.Profil.Views.dashboard, dashboard);
		}

		public const int LocalisationPageSize = 6;
		public const int CommentPageSize = 3;
		/// <summary>
		/// GET Action result to show profil dashboard
		/// </summary>
		/// <param name="id">id of the member</param>
		/// <returns>View containing profil dashboard</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		[ActionName("dashboard")]
		public virtual ActionResult Dashboard(int id)
		{
			if (!Roles.IsUserInRole(MiscHelpers.AdminConstants.AdminRole) && !WebHelper.MatchIdentity(User.Identity, id))
            {
                return View(MVC.Shared.Views.Error);
            }

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var member = mRepo.Get(id);
            if (member == null || member.MemberMainData == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Profile.ProfileString.MemberNotFound;
                return RedirectToAction(MVC.Home.Index());
            }

			var dashboard = GetDashboard(member, true);
			return View(dashboard);
		}

		public virtual PartialViewResult AjaxDashboard(int id, int tabId, int p1, int p2, int p3, int p4)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var member = mRepo.Get(id);
			if (member == null || member.MemberMainData == null)
				return null;

			var dashboard = GetDashboard(member, true, p1, p2, p3, p4);
			ViewData[ProfilConstants.TabId] = tabId;
			ViewData[Comment.DisplayRelatedLocalisation] = true;
			var tab = (ProfilConstants.DashboardTab)tabId;
			if (tab == ProfilConstants.DashboardTab.FavLoc || tab == ProfilConstants.DashboardTab.AddedLoc)
				return PartialView(MVC.Profil.Views._LocalisationTab, dashboard);
			else if (tab == ProfilConstants.DashboardTab.PostedCom || tab == ProfilConstants.DashboardTab.RelCom)
				return PartialView(MVC.Profil.Views._CommentTab, dashboard);
			else
				return null;
		}

		/// <summary>
		/// GET Action result to prepare the view to edit profil
		/// </summary>
		/// <param name="id">id of the member</param>
		/// <returns>the form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		[ActionName("editer")]
		public virtual ActionResult Edit(int id)
		{
			if (!Roles.IsUserInRole(MiscHelpers.AdminConstants.AdminRole) && !WebHelper.MatchIdentity(User.Identity, id))
            {
                return View(MVC.Shared.Views.Error);
            }

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var item = mRepo.Get(id);
            if (item == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Profile.ProfileString.MemberNotFound;
                return RedirectToAction(MVC.Profil.Dashboard(id));
            }
				
			return View(new ProfilFormViewModel { Member = item });
		}

		/// <summary>
		/// POST Action result to handle the edit of profil
		/// </summary>
		/// <param name="id">id of the member</param>
		/// <param name="member">member data from the form</param>
		/// <returns>redirect to profil page if succeed</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		[ValidateAntiForgeryToken]
		[ActionName("editer")]
		public virtual ActionResult Edit(int id, Member member)
		{
			if (ModelState.IsValid)
			{
				var context = ModelFactory.GetUnitOfWork();
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				try
				{
					//upload images and set image paths
					foreach (string name in Request.Files)
					{
						try
						{
							var postedFile = Request.Files[name];
							if (postedFile == null || string.IsNullOrEmpty(postedFile.FileName))
								continue;
							var uploadedFileName = this.UploadFile(postedFile);
							switch (name)
							{
								case "Avatar":
									member.MemberMainData.Avatar = uploadedFileName;
									break;
								default:
									break;
							}
						}
						catch (Exception ex)
						{
							ModelState.AddModelError("", ex.Message);
						}
					}
					var m = mRepo.Get(id);
					UpdateModel(m, "Member");
					if (!string.IsNullOrEmpty(member.MemberMainData.Avatar))
						m.MemberMainData.Avatar = member.MemberMainData.Avatar;

					context.Commit();

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Profile.ProfileString.ProfilHaveBeenEdit;

					return RedirectToAction(MVC.Profil.ActionNames.Dashboard, new { id = id });
				}
				catch (Exception ex)
				{
					_Logger.Error("EditProfil", ex);
					context.Complete();
					ModelState.AddModelError("EditProfil", ex);
				}
			}
			return View(new ProfilFormViewModel { Member = member });
		}

		/// <summary>
		/// Action to add a localisation to member favorites
		/// </summary>
		/// <param name="id">Id of the member</param>
		/// <param name="locId">Id of the favorite localisation</param>
		/// <param name="returnUrl">Url to redirect when action done</param>
		/// <returns>Redirect to returnUrl</returns>
		public virtual PartialViewResult AddToFavorite(string id, int locId)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			try
			{
				var member = mRepo.GetMember(id);
				if (member == null)
					return null;
				member.FavoriteLocalisations.Add(new FavoriteLocalisation { LocalisationId = locId });

				context.Commit();
			}

			catch (Exception ex)
			{
				context.Complete();
				_Logger.Error("AddToFavorite", ex);
			}
			ViewData[ProfilConstants.AddToFavorite] = false;
			ViewData[ProfilConstants.DelFavorite] = true;
			return PartialView(MVC.Shared.Views._AddToFavorite);
		}

		/// <summary>
		/// Action to remove a localisation from member favorites
		/// </summary>
		/// <param name="id">Id of the member</param>
		/// <param name="locId">Id of the favorite localisation to remove</param>
		/// <param name="returnUrl">Url to redirect when action done</param>
		/// <returns>Redirect to returnUrl</returns>
		public virtual PartialViewResult RemoveFromFavorite(string id, int locId)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			try
			{
				var member = mRepo.GetMember(id);
				if (member == null)
					return null;

				foreach (var fav in member.FavoriteLocalisations.ToList())
				{
					if (fav.LocalisationId == locId)
					{
						member.FavoriteLocalisations.Remove(fav);
					}
				}
				context.Commit();
			}
			catch (Exception ex)
			{
				context.Complete();
				_Logger.Error("RemoveFromFavorite", ex);
			}


			ViewData[ProfilConstants.AddToFavorite] = true;
			ViewData[ProfilConstants.DelFavorite] = false;
			return PartialView(MVC.Shared.Views._AddToFavorite);
		}

		// **************************************
		// URL: /Account/ChangePassword
		// **************************************

		/// <summary>
		/// GET Action method to change the password
		/// </summary>
		/// <param name="id">member id</param>
		/// <returns>the form to fill</returns>
		[ActionName("changer-mdp")]
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		public virtual ActionResult ChangePassword(int id)
		{
			if (!Roles.IsUserInRole(MiscHelpers.AdminConstants.AdminRole) && !WebHelper.MatchIdentity(User.Identity, id))
			{
				return View(MVC.Shared.Views.Error);
			}
			ViewData["PasswordLength"] = _MembershipService.MinPasswordLength;
			return View(new ChangePasswordModel { MemberId = id });
		}

		/// <summary>
		/// POST Action method to change the password
		/// </summary>
		/// <param name="model">The change password data from the form</param>
		/// <returns>Password change succes page if ok, the form with error if not</returns>
		[ActionName("changer-mdp")]
		[AcceptVerbs(HttpVerbs.Post),Authorize]
		[ValidateAntiForgeryToken]
		public virtual ActionResult ChangePassword(int id, ChangePasswordModel model)
		{
			if (ModelState.IsValid)
			{
				var context = ModelFactory.GetUnitOfWork();
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var member = mRepo.Get(id);
				if (_MembershipService.ChangePassword(member.Username, model.OldPassword, model.NewPassword))
				{
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Account.AccountString.PasswordHaveBeenChanged;
					return RedirectToAction(MVC.Profil.Dashboard(id));
				}
				else
				{
					ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.PasswordNotValide);
				}
			}

			// Si nous sommes arrivés là, quelque chose a échoué, réafficher le formulaire
			ViewData["PasswordLength"] = _MembershipService.MinPasswordLength;
			return View(model);
		}

		[ChildActionOnly]
		public virtual ActionResult ProfilMenu(int memberId, int type, bool isPrivate = true)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var member = mRepo.Get(memberId);

			return PartialView(MVC.Profil.Views._ProfilMenu, new ProfilMenuModel { Member = member, MenuItem = type, IsPrivate = isPrivate });
		}
	}
}
