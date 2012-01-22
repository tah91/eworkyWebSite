using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure;
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;
using Postal;
using Worki.Infrastructure.Helpers;
using Worki.Web.Model;

namespace Worki.Web.Areas.Dashboard.Controllers
{
	[HandleError]
	[CompressFilter(Order = 1)]
	[CacheFilter(Order = 2)]
	[Authorize]
	[RequireHttpsRemote]
	public abstract class DashboardControllerBase : Controller
	{

	}

	public partial class HomeController : DashboardControllerBase
    {
        ILogger _Logger;

        public HomeController(ILogger logger)
        {
            _Logger = logger;
        }

        /// <summary>
        /// Get action result to show recent activities of the member
        /// </summary>
        /// <returns>View with recent activities</returns>
		public virtual ActionResult Index()
		{
			var id = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);

			var member = mRepo.Get(id);
			var news = ModelHelper.GetNews(id, member.MemberBookings, mbl => { return Url.Action(MVC.Backoffice.Localisation.ReadBookingLog(mbl.Id, false)); });
			news = news.Concat(ModelHelper.GetNews(id, member.MemberQuotations, ql => { return Url.Action(MVC.Backoffice.Localisation.ReadQuotationLog(ql.Id, false)); })).ToList();

			news = news.OrderByDescending(n => n.Date).Take(BackOfficeConstants.NewsCount).ToList();
			return View(new DashoboardHomeViewModel { News = news, Member = member });
		}

		/// <summary>
		/// Get action method to get read booking log
		/// </summary>
		/// <returns>redirect to booking detail</returns>
		[DontRequireHttps]
		public virtual ActionResult GetAlertSummary()
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);

				var news = ModelHelper.GetNews(memberId, member.MemberBookings, mbl => { return Url.Action(MVC.Backoffice.Localisation.ReadBookingLog(mbl.Id, false)); });
				news = news.Concat(ModelHelper.GetNews(memberId, member.MemberQuotations, ql => { return Url.Action(MVC.Backoffice.Localisation.ReadQuotationLog(ql.Id, false)); })).ToList();

				news = news.OrderByDescending(n => n.Date).Take(BackOfficeConstants.NewsCount).ToList();

				var count = news.Count(n => !n.Read);
				if (count == 0)
					return null;

				var urlHelper = new UrlHelper(ControllerContext.RequestContext);
				var dashboardHomeUrl = urlHelper.Action(MVC.Dashboard.Home.Index());	

				var model = new AlertSumaryModel
				{
					AlertCount = count,
					AlertLink = dashboardHomeUrl,
					AlertTitle = string.Format(Worki.Resources.Views.BackOffice.BackOfficeString.DashboardAlerts, count)
				};

				return PartialView(MVC.Backoffice.Shared.Views._AlertSummary, model);
			}
			catch (Exception ex)
			{
				_Logger.Error("GetAlertSummary", ex);
				return null;
			}
		}

		#region Booking

		/// <summary>
		/// Get action method to show current bookings of the member
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
                var list = member.MemberBookings.Where(mb => !mb.Expired).OrderByDescending(b=>b.CreationDate);
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
		/// Get action method to show booking detail
		/// </summary>
		/// <returns>View containing the booking</returns>
		public virtual ActionResult BookingDetail(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			try
			{
                var member = mRepo.Get(memberId);
				Member.Validate(member);
				var booking = bRepo.Get(id);
				if (memberId != booking.MemberId)
					throw new Exception(Worki.Resources.Validation.ValidationString.InvalidUser);

				return View(booking);
			}
			catch (Exception ex)
			{
				_Logger.Error("BookingDetail", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Get action method to show bookings is paid
		/// </summary>
		/// <param name="id">member booking id</param>
		/// <returns>View containing the booking</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult BookingPaymentAccepted(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			try
			{
				var booking = bRepo.Get(id);
				if (booking.MemberId != memberId)
					throw new Exception(Worki.Resources.Validation.ValidationString.InvalidUser);
				return View(booking);
			}
			catch (Exception ex)
			{
                _Logger.Error("BookingPaymentAccepted", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Get action method to show bookings is canceled
		/// </summary>
		/// <param name="id">member booking id</param>
		/// <returns>View containing the booking</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult BookingPaymentCancelled(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			try
			{
				var booking = bRepo.Get(id);
				if (booking.MemberId != memberId)
					throw new Exception(Worki.Resources.Validation.ValidationString.InvalidUser);
				booking.MemberBookingLogs.Add(new MemberBookingLog
				{
					CreatedDate = DateTime.UtcNow,
					Event = "Booking Payment Cancelled",
					LoggerId = memberId
				});

				context.Commit();
				return View(booking);
			}
			catch (Exception ex)
			{
				context.Complete();
                _Logger.Error("BookingPaymentCancelled", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

        /// <summary>
        /// GET Action result to cancel booking
        /// </summary>
        /// <param name="id">id of booking to confirm</param>
        /// <returns>View to fill booking data</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult CancelBooking(int id)
        {
            var memberId = WebHelper.GetIdentityId(User.Identity);
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);

            try
            {
                var member = mRepo.Get(memberId);
                var booking = bRepo.Get(id);
                Member.Validate(member);
                booking.StatusId = (int)MemberBooking.Status.Cancelled;
                booking.MemberBookingLogs.Add(new MemberBookingLog
                {
                    CreatedDate = DateTime.UtcNow,
                    Event = "Booking Cancelled",
                    EventType = (int)MemberBookingLog.BookingEvent.Cancellation,
					LoggerId = memberId
                });

                context.Commit();

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Booking.BookingString.BookingCanceled;
                return RedirectToAction(MVC.Dashboard.Home.BookingDetail(id));
            }
            catch (Exception ex)
            {
                _Logger.Error("CancelBooking", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

		#endregion

		#region Quotation

		/// <summary>
		/// Get action method to show quotation of the member
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
                var list = member.MemberQuotations.Where(q => q.Unknown || q.CreationDate > DateTime.UtcNow.AddDays(-15)).OrderByDescending(q => q.CreationDate); ;
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

		/// <summary>
		/// Get action method to show quotation detail
		/// </summary>
		/// <returns>View containing the quotation</returns>
		public virtual ActionResult QuotationDetail(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				var quotation = qRepo.Get(id);
				if (memberId != quotation.MemberId)
					throw new Exception(Worki.Resources.Validation.ValidationString.InvalidUser);

				return View(quotation);
			}
			catch (Exception ex)
			{
				_Logger.Error("QuotationDetail", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

        /// <summary>
        /// GET Action result to cancel quotation
        /// </summary>
        /// <param name="id">id of quotation to cancel</param>
        /// <returns>Redirect to quotation detail</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult CancelQuotation(int id)
        {
            var memberId = WebHelper.GetIdentityId(User.Identity);
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);

            try
            {
                var member = mRepo.Get(memberId);
                var quotation = qRepo.Get(id);
                Member.Validate(member);
                quotation.StatusId = (int)MemberQuotation.Status.Cancelled;
                quotation.MemberQuotationLogs.Add(new MemberQuotationLog
                {
                    CreatedDate = DateTime.UtcNow,
                    Event = "Quotation Cancelled",
                    EventType = (int)MemberQuotationLog.QuotationEvent.Cancellation,
					LoggerId = memberId
                });

                context.Commit();

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Booking.BookingString.QuotationCanceled;
                return RedirectToAction(MVC.Dashboard.Home.QuotationDetail(id));
            }
            catch (Exception ex)
            {
                _Logger.Error("CancelBooking", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

		#endregion
    }
}
