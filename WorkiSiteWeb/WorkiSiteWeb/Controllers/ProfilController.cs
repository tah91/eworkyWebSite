using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;
using Worki.Data.Repository;
using Worki.Data.Models;
using System.Linq;

namespace Worki.Web.Controllers
{
	public partial class ProfilController : Controller
	{
		#region Private

		IMemberRepository _MemberRepository;
		ILocalisationRepository _LocalisationRepository;
		ILogger _Logger;

		ProfilDashboardModel GetDashboard(Member member, bool isPrivate = false, int p1 = 1, int p2 = 1, int p3 = 1, int p4 = 1)
		{
			//get fav localisations
			var favLocs = new List<Localisation>();
			foreach (var item in member.FavoriteLocalisations.Skip((p1 - 1) * LocalisationPageSize).Take(LocalisationPageSize))
			{
				favLocs.Add(_LocalisationRepository.Get(item.LocalisationId));
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

		public ProfilController(IMemberRepository memberRepository, ILogger logger, ILocalisationRepository localisationRepository)
		{
			_MemberRepository = memberRepository;
			_Logger = logger;
			_LocalisationRepository = localisationRepository;
		}

		/// <summary>
		/// GET Action result to show profil details
		/// </summary>
		/// <param name="id">id of the member</param>
		/// <returns>View containing profil details</returns>
		[HttpGet]
		[ActionName("details")]
		public virtual ActionResult Detail(int id)
		{
			var member = _MemberRepository.Get(id);
			if (member == null || member.MemberMainData == null)
				return View(MVC.Shared.Views.Error);

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
		[HttpGet]
		[ActionName("dashboard")]
		public virtual ActionResult Dashboard(int id)
		{
			var member = _MemberRepository.Get(id);
			if (member == null || member.MemberMainData == null)
				return View(MVC.Shared.Views.Error);

			var dashboard = GetDashboard(member, true);
			return View(dashboard);
		}

		public virtual PartialViewResult AjaxDashboard(int id, int tabId, int p1, int p2, int p3, int p4)
		{
			var member = _MemberRepository.Get(id);
			if (member == null || member.MemberMainData == null)
				return null;

			var dashboard = GetDashboard(member, true, p1, p2, p3, p4);
			ViewData[ProfilDashboardModel.TabId] = tabId;
			ViewData[Comment.DisplayRelatedLocalisation] = true;
			var tab = (ProfilDashboardModel.DashboardTab)tabId;
			if (tab == ProfilDashboardModel.DashboardTab.FavLoc || tab == ProfilDashboardModel.DashboardTab.AddedLoc)
				return PartialView(MVC.Profil.Views._LocalisationTab, dashboard);
			else if (tab == ProfilDashboardModel.DashboardTab.PostedCom || tab == ProfilDashboardModel.DashboardTab.RelCom)
				return PartialView(MVC.Profil.Views._CommentTab, dashboard);
			else
				return null;
		}

		/// <summary>
		/// GET Action result to prepare the view to edit profil
		/// </summary>
		/// <param name="id">id of the member</param>
		/// <returns>the form to fill</returns>
		[HttpGet]
		[ActionName("editer")]
		public virtual ActionResult Edit(int id)
		{
			var item = _MemberRepository.Get(id);
			if (item == null)
				return View(MVC.Shared.Views.Error);
			return View(new ProfilFormViewModel { Member = item });
		}

		/// <summary>
		/// POST Action result to handle the edit of profil
		/// </summary>
		/// <param name="id">id of the member</param>
		/// <param name="member">member data from the form</param>
		/// <returns>redirect to profil page if succeed</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("editer")]
		public virtual ActionResult Edit(int id, Member member)
		{
			if (ModelState.IsValid)
			{
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
					_MemberRepository.Update(id, m =>
					{
						UpdateModel(m, "Member");
						if (!string.IsNullOrEmpty(member.MemberMainData.Avatar))
							m.MemberMainData.Avatar = member.MemberMainData.Avatar;
					});
					return RedirectToAction(MVC.Profil.ActionNames.Dashboard, new { id = id });
				}
				catch (Exception ex)
				{
					_Logger.Error("EditProfil", ex);
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
			var member = _MemberRepository.GetMember(id);
			if (member == null)
				return null;
			_MemberRepository.Update(member.MemberId, m =>
			{
				m.FavoriteLocalisations.Add(new FavoriteLocalisation { LocalisationId = locId });
			});
			ViewData[ProfilDashboardModel.AddToFavorite] = false;
			ViewData[ProfilDashboardModel.DelFavorite] = true;
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
			var member = _MemberRepository.GetMember(id);
			if (member == null)
				return null;

			_MemberRepository.Update(member.MemberId, m =>
			{
				foreach (var fav in m.FavoriteLocalisations.ToList())
				{
					if (fav.LocalisationId == locId)
					{
						m.FavoriteLocalisations.Remove(fav);
					}
				}
			});

			ViewData[ProfilDashboardModel.AddToFavorite] = true;
			ViewData[ProfilDashboardModel.DelFavorite] = false;
			return PartialView(MVC.Shared.Views._AddToFavorite);
		}
	}
}
