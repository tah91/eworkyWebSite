﻿using System;
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

namespace Worki.Web.Areas.Dashboard.Controllers
{
	public partial class ProfilController : DashboardControllerBase
	{
		#region Private

		ILogger _Logger;
		IMembershipService _MembershipService;

		#endregion

		public ProfilController(ILogger logger,
								IMembershipService membershipService)
		{
			_MembershipService = membershipService;
			_Logger = logger;
		}

		/// <summary>
		/// GET Action result to prepare the view to edit profil
		/// </summary>
		/// <param name="id">id of the member</param>
		/// <returns>the form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		[ActionName("editer")]
		[RequireHttpsRemote]
		public virtual ActionResult Edit()
		{
			var id = WebHelper.GetIdentityId(User.Identity);
			if (id == 0)
				return View(MVC.Shared.Views.Error);
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var item = mRepo.Get(id);
            if (item == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Profile.ProfileString.MemberNotFound;
				return RedirectToAction(MVC.Dashboard.Home.Index());
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
		[RequireHttpsRemote]
		public virtual ActionResult Edit(Member member)
		{
			var id = WebHelper.GetIdentityId(User.Identity);
			if (id == 0)
				return View(MVC.Shared.Views.Error);

			member.MemberId = id;
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

					return RedirectToAction(MVC.Dashboard.Home.Index());
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
		/// <param name="locId">Id of the favorite localisation</param>
		/// <param name="returnUrl">Url to redirect when action done</param>
		/// <returns>Redirect to returnUrl</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
		public virtual PartialViewResult AddToFavorite(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			try
			{
				var memberId = WebHelper.GetIdentityId(User.Identity);
                var member = mRepo.Get(memberId);
				if (member == null)
					return null;
                member.FavoriteLocalisations.Add(new FavoriteLocalisation { LocalisationId = id });

				context.Commit();
			}

			catch (Exception ex)
			{
				context.Complete();
				_Logger.Error("AddToFavorite", ex);
                return null;
			}
			ViewData[ProfilConstants.AddToFavorite] = false;
			ViewData[ProfilConstants.DelFavorite] = true;
			return PartialView(MVC.Shared.Views._AddToFavorite);
		}

		/// <summary>
		/// Action to remove a localisation from member favorites
		/// </summary>
		/// <param name="locId">Id of the favorite localisation to remove</param>
		/// <param name="returnUrl">Url to redirect when action done</param>
		/// <returns>Redirect to returnUrl</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		public virtual PartialViewResult RemoveFromFavorite(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			try
			{
				var memberId = WebHelper.GetIdentityId(User.Identity);
                var member = mRepo.Get(memberId);
				if (member == null)
					return null;

				foreach (var fav in member.FavoriteLocalisations.ToList())
				{
                    if (fav.LocalisationId == id)
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
                return null;
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
		public virtual ActionResult ChangePassword()
		{
			var id = WebHelper.GetIdentityId(User.Identity);
			if (id == 0)
				return View(MVC.Shared.Views.Error);

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
		public virtual ActionResult ChangePassword(ChangePasswordModel model)
		{
			var id = WebHelper.GetIdentityId(User.Identity);
			if (id == 0)
				return View(MVC.Shared.Views.Error);
			if (ModelState.IsValid)
			{
				var context = ModelFactory.GetUnitOfWork();
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var member = mRepo.Get(id);
				if (_MembershipService.ChangePassword(member.Username, model.OldPassword, model.NewPassword))
				{
                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Account.AccountString.YouChangeYourPassword;
					return RedirectToAction(MVC.Dashboard.Home.Index());
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
	}
}
