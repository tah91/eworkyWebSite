﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;
using Worki.Infrastructure;
using Worki.Infrastructure.Helpers;
using Worki.Web.Model;
using Worki.Service;
using System.Linq.Expressions;
using Worki.Infrastructure.Email;

namespace Worki.Web.Areas.Backoffice.Controllers
{
	[HandleError]
	[CompressFilter(Order = 1)]
	[CacheFilter(Order = 2)]
	[Authorize(Roles = MiscHelpers.BackOfficeConstants.BackOfficeRole)]
	[RequireHttpsRemote]
	public abstract class BackofficeControllerBase : Controller
	{
        protected ILogger _Logger;
        protected IObjectStore _ObjectStore;
        protected IEmailService _EmailService;

        public BackofficeControllerBase()
        {
        }

        public BackofficeControllerBase(ILogger logger, IObjectStore objectStore, IEmailService emailService)
        {
            this._Logger = logger;
            this._ObjectStore = objectStore;
            this._EmailService = emailService;
        }
	}

	public partial class HomeController : BackofficeControllerBase
    {
        public HomeController(ILogger logger, IObjectStore objectStore, IEmailService emailService)
            : base(logger, objectStore, emailService)
        {

        }

        /// <summary>
        /// Get action result to show recent activities of the owner
        /// </summary>
        /// <returns>View with recent activities</returns>
		public virtual ActionResult Index()
		{
            _Logger.Error("BO INDEX");
			var id = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
			try
			{
				var member = mRepo.Get(id);
				Member.Validate(member);

				var bookings = bRepo.GetOwnerProducts(id);
                var quotations = qRepo.GetOwnerProducts(id);
				var news = ModelHelper.GetNews(id, bookings, mbl => { return Url.Action(MVC.Backoffice.Localisation.ReadBookingLog(mbl.Id)); });
				news = news.Concat(ModelHelper.GetNews(id, quotations, ql => { return Url.Action(MVC.Backoffice.Localisation.ReadQuotationLog(ql.Id)); })).ToList();

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
                var localisations = member.Localisations.Where(loc => !Localisation.FreeLocalisationTypes.Contains(loc.TypeValue));
				var model = new PagingList<Localisation>
				{
                    List = localisations.Skip((p - 1) * PagedListViewModel.PageSize).Take(PagedListViewModel.PageSize).ToList(),
                    PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PagedListViewModel.PageSize, TotalItems = localisations.Count() }
				};
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("Localisations", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

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
                var bookings = bRepo.GetMany(b => b.Offer.Localisation.OwnerID == id && b.StatusId == (int)MemberBooking.Status.Unknown).Where(b => !b.Expired);
                var model = new PagingList<MemberBooking>
                {
                    List = bookings.OrderByDescending(mb => mb.CreationDate).Skip((p - 1) * PagedListViewModel.PageSize).Take(PagedListViewModel.PageSize).ToList(),
					PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PagedListViewModel.PageSize, TotalItems = bookings.Count() }
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
                var quotations = qRepo.GetMany(q => q.Offer.Localisation.OwnerID == id && q.StatusId == (int)MemberQuotation.Status.Unknown);
                var model = new PagingList<MemberQuotation>
                {
                    List = quotations.OrderByDescending(mq => mq.CreationDate).Skip((p - 1) * PagedListViewModel.PageSize).Take(PagedListViewModel.PageSize).ToList(),
					PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PagedListViewModel.PageSize, TotalItems = quotations.Count }
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
		/// Get action method to get read booking log
		/// </summary>
		/// <returns>redirect to booking detail</returns>
		[DontRequireHttps]
		public virtual ActionResult GetAlertSummary()
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);

				var bookings = bRepo.GetOwnerProducts(memberId);
                var quotations = qRepo.GetOwnerProducts(memberId);
				var news = ModelHelper.GetNews(memberId, bookings, mbl => { return Url.Action(MVC.Backoffice.Localisation.ReadBookingLog(mbl.Id)); });
				news = news.Concat(ModelHelper.GetNews(memberId, quotations, ql => { return Url.Action(MVC.Backoffice.Localisation.ReadQuotationLog(ql.Id)); })).ToList();

				news = news.OrderByDescending(n => n.Date).Take(BackOfficeConstants.NewsCount).ToList();

				var count = news.Count(n => !n.Read);
				if (count == 0)
					return null;

				var urlHelper = new UrlHelper(ControllerContext.RequestContext);
				var boHomeUrl = urlHelper.Action(MVC.Backoffice.Home.Index());

				var model = new AlertSumaryModel
				{
					AlertCount = count,
					AlertLink = boHomeUrl,
					AlertTitle = string.Format(Worki.Resources.Views.BackOffice.BackOfficeString.BOAlerts, count)
				};

				return PartialView(MVC.Backoffice.Shared.Views._AlertSummary, model);
			}
			catch (Exception ex)
			{
				_Logger.Error("GetAlertSummary", ex);
				return null;
			}
		}
    }
}
