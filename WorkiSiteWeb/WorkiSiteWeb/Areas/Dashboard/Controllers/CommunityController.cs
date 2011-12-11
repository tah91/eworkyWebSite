using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure;
using Worki.Web.Helpers;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;
using Worki.Infrastructure.Logging;

namespace Worki.Web.Areas.Dashboard.Controllers
{
	public partial class CommunityController : DashboardControllerBase
    {
		ILogger _Logger;

		public CommunityController(ILogger logger)
		{
			_Logger = logger;
		}

		public const int PageSize = 9;

		/// <summary>
		/// Get action result to get places of the member
		/// </summary>
		/// <returns>View containing member places</returns>
		public virtual ActionResult Places(int? page)
        {
			var id = WebHelper.GetIdentityId(User.Identity);	

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var p = page ?? 1;
			try
			{
				var member = mRepo.Get(id);
				Member.Validate(member);
				var model = new PagingList<Localisation>
				{
					List = member.Localisations.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
					PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = member.Localisations.Count }
				};
				Func<int, string> func = x => Url.Action(MVC.Dashboard.Community.Places(x));
				ViewData[ProfilConstants.PageUrl] = func;
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("Places", ex);
				return View(MVC.Shared.Views.Error);
			}
        }

		/// <summary>
		/// Get action result to get favorite places of the member
		/// </summary>
		/// <returns>View containing member favorite places</returns>
		public virtual ActionResult FavoritePlaces(int? page)
		{
			var id = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var p = page ?? 1;
			try
			{
				var member = mRepo.Get(id);
				Member.Validate(member);
				var model = new PagingList<Localisation>
				{
					List = member.FavoriteLocalisations.Skip((p - 1) * PageSize).Take(PageSize).Select(fl=>fl.Localisation).ToList(),
					PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = member.FavoriteLocalisations.Count }
				};
				Func<int, string> func = x => Url.Action(MVC.Dashboard.Community.FavoritePlaces(x));
				ViewData[ProfilConstants.PageUrl] = func;
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("Places", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Get action result to get places added by the member
		/// </summary>
		/// <returns>View containing added by the member</returns>
		public virtual ActionResult AddedPlaces(int? page)
		{
			var id = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var p = page ?? 1;
			try
			{
				var member = mRepo.Get(id);
				Member.Validate(member);
				var model = new PagingList<Localisation>
				{
					List = member.MemberEditions.Where(me => me.ModificationType == (int)EditionType.Creation).Skip((p - 1) * PageSize).Take(PageSize).Select(me => me.Localisation).ToList(),
					PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = member.MemberEditions.Where(me => me.ModificationType == (int)EditionType.Creation).Count() }
				};
				Func<int, string> func = x => Url.Action(MVC.Dashboard.Community.AddedPlaces(x));
				ViewData[ProfilConstants.PageUrl] = func;
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("Places", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Get action result to get places commented by the member
		/// </summary>
		/// <returns>View containing comments by the member</returns>
		public virtual ActionResult CommentedPlaces(int? page)
		{
			var id = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var p = page ?? 1;
			try
			{
				var member = mRepo.Get(id);
				Member.Validate(member);
				var model = new PagingList<Comment>
				{
					List = member.Comments.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
					PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = member.Comments.Count }
				};
				Func<int, string> func = x => Url.Action(MVC.Dashboard.Community.CommentedPlaces(x));
				ViewData[ProfilConstants.PageUrl] = func;
				ViewData[Comment.DisplayRelatedLocalisation] = true;
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("Places", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

    }
}
