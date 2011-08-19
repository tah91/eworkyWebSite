using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;
using Worki.Data.Repository;
using Worki.Data.Models;
using System.Linq;
using Worki.Infrastructure;

namespace Worki.Web.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
	public partial class RentalController : Controller
	{
		#region Private

		IRentalRepository _RentalRepository;
		ILogger _Logger;

		#endregion

        public RentalController(IRentalRepository rentalRepository, ILogger logger)
		{
            _RentalRepository = rentalRepository;
			_Logger = logger;
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
	}
}
