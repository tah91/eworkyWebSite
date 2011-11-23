using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;
using Worki.Infrastructure;

namespace Worki.Web.Areas.Backoffice.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    [Authorize]
    public partial class HomeController : Controller
    {
        ILogger _Logger;

        public HomeController(ILogger logger)
        {
            _Logger = logger;
        }

        /// <summary>
        /// Get action result to show recent activities of the owner
        /// </summary>
        /// <returns>View with recent activities</returns>
        public virtual ActionResult Index(int? page)
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
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("Booking", ex);
				return View(MVC.Shared.Views.Error);
			}
        }

        public const int PageSize = 5;

        /// <summary>
        /// Get action method to show bookings of the owner
        /// </summary>
        /// <returns>View containing the bookings</returns>
        public virtual ActionResult Booking(int? page)
        {
            var id = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
            var p = page ?? 1;
            try
            {
                var member = mRepo.Get(id);
                Member.Validate(member);
				var bookings = bRepo.GetMany(b => b.Offer.Localisation.OwnerID == id);
                var model = new PagingList<MemberBooking>
                {
					List = bookings.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
					PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = bookings.Count }
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
        /// Get action method to show quotation of the owner
        /// </summary>
        /// <returns>View containing the quotations</returns>
        public virtual ActionResult Quotation(int? page)
        {
            var id = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
            var p = page ?? 1;
            try
            {
                var member = mRepo.Get(id);
                Member.Validate(member);
				var quotations = qRepo.GetMany(b => b.Offer.Localisation.OwnerID == id);
                var model = new PagingList<MemberQuotation>
                {
					List = quotations.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
					PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = quotations.Count }
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
