using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;
using Worki.Web.Helpers;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;

namespace Worki.Web.Areas.Dashboard.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    [Authorize]
	public partial class HistoricController : Controller
    {
        ILogger _Logger;

        public HistoricController(ILogger logger)
		{
			_Logger = logger;
		}

        /// <summary>
        /// Get action method to show past bookings of the member
        /// </summary>
        /// <returns>View containing the bookings</returns>
		public virtual ActionResult Booking(int? page)
        {
            var id = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var p = page ?? 1;
            try
            {
                var member = mRepo.Get(id);
                Member.Validate(member);
                var list = member.MemberBookings.Where(mb => mb.Expired);
                var model = new PagingList<MemberBooking>
                {
                    List = list.Skip((p - 1) * PagedListViewModel.PageSize).Take(PagedListViewModel.PageSize).ToList(),
                    PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PagedListViewModel.PageSize, TotalItems = list.Count() }
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _Logger.Error("Booking", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

        /// <summary>
        /// Get action method to show past quotations of the member
        /// </summary>
        /// <returns>View containing the quotations</returns>
        public virtual ActionResult Quotation(int? page)
        {
            var id = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var p = page ?? 1;
            try
            {
                var member = mRepo.Get(id);
                Member.Validate(member);
                var list = member.MemberQuotations.Where(q => !q.Unknown);
                var model = new PagingList<MemberQuotation>
                {
                    List = list.Skip((p - 1) * PagedListViewModel.PageSize).Take(PagedListViewModel.PageSize).ToList(),
                    PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PagedListViewModel.PageSize, TotalItems = list.Count() }
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _Logger.Error("Quotation", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

    }
}
