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
        public virtual ActionResult Index()
        {
			var id = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			try
			{
				var member = mRepo.Get(id);
				Member.Validate(member);

				var bookings = bRepo.GetMany(b => b.Offer.Localisation.OwnerID == id);
				var news = ModelHelper.GetNews(bookings, mb => { return Url.Action(MVC.Backoffice.Localisation.BookingDetail(mb.Id)); });
                news = news.OrderByDescending(n => n.Date).Take(BackOfficeConstants.NewsCount).ToList();

                var localisations = lRepo.GetMostBooked(id, BackOfficeConstants.LocalisationCount);

                return View(new BackOfficeHomeViewModel { Owner = member, Places = localisations, News = news });
			}
			catch (Exception ex)
			{
				_Logger.Error("Index", ex);
				return View(MVC.Shared.Views.Error);
			}
        }

		/// <summary>
		/// Get action result to show all the localisations of the owner
		/// </summary>
		/// <returns>View of owner localisations</returns>
		public virtual ActionResult Localisations(int? page)
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
					List = member.Localisations.Where(loc => !Localisation.FreeLocalisationTypes.Contains(loc.TypeValue)).Skip((p - 1) * PageSize).Take(PageSize).ToList(),
					PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = member.Localisations.Count }
				};
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("Localisations", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

        public const int PageSize = 6;

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
                var bookings = bRepo.GetMany(b => b.Offer.Localisation.OwnerID == id && b.StatusId == (int)MemberBooking.Status.Unknown);
                var model = new PagingList<MemberBooking>
                {
                    List = bookings.OrderByDescending(mb => mb.CreationDate).Skip((p - 1) * PageSize).Take(PageSize).ToList(),
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
