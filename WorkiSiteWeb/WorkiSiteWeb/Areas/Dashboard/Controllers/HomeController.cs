﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure;
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;

namespace Worki.Web.Areas.Dashboard.Controllers
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
        /// Get action result to show recent activities of the member
        /// </summary>
        /// <returns>View with recent activities</returns>
        public virtual ActionResult Index()
        {
			var id = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);

			var member = mRepo.Get(id);
			var news = new List<NewsItem>();
			foreach (var booking in member.MemberBookings)
			{
				foreach (var log in booking.MemberBookingLogs)
				{
					if (log.EventType == (int)MemberBookingLog.BookingEvent.General)
						continue;

					news.Add(new NewsItem
						{
							Date = log.CreatedDate,
							DisplayName = log.GetDisplay(),
							Link = Url.Action(MVC.Dashboard.Home.Booking())
						});
				}
			}

			news = news.OrderByDescending(n => n.Date).Take(10).ToList();
			return View(news);
        }

        public const int PageSize = 5;

        /// <summary>
        /// Get action method to show bookings of the member
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
                var model = new PagingList<MemberBooking>
                {
                    List = member.MemberBookings.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
                    PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = member.MemberBookings.Count }
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
                var model = new PagingList<MemberQuotation>
                {
                    List = member.MemberQuotations.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
                    PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = member.MemberQuotations.Count }
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
		/// Get action method to show bookings is paid
		/// </summary>
		/// <param name="id">member booking id</param>
		/// <returns>View containing the booking</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult BookingAccepted(int id)
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
			catch(Exception ex)
			{
				_Logger.Error("BookingAccepted", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Get action method to show bookings is canceled
		/// </summary>
		/// <param name="id">member booking id</param>
		/// <returns>View containing the booking</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult BookingCancelled(int id)
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
					CreatedDate = DateTime.Now,
					Event = "Booking Payment Cancelled"
				});

				context.Commit();
				return View(booking);
			}
			catch (Exception ex)
			{
				context.Complete();
				_Logger.Error("BookingCancelled", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

    }
}
