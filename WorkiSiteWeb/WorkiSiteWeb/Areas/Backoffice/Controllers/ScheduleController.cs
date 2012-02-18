using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Web.Helpers;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;
using Worki.Infrastructure.Helpers;
using Postal;
using Worki.Web.Model;
using System.Web.Security;
using System.IO;
using Worki.Service;

namespace Worki.Web.Areas.Backoffice.Controllers
{
	public partial class ScheduleController : BackofficeControllerBase
    {
        ILogger _Logger;

		public ScheduleController(ILogger logger)
        {
            _Logger = logger;
        }

		/// <summary>
		/// Get action method to show offer schedule
		/// </summary>
		/// <param name="id">localisation id</param>
		/// <param name="offerId">offer id</param>
		/// <returns>view with a calandar</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult OfferSchedule(int id, int offerId = 0)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				Offer offer;
				//case no offer selected, take the first one
				if (offerId == 0)
				{
					var loc = lRepo.Get(id);
					offer = loc.Offers.Where(o => o.CanHaveBooking).FirstOrDefault();
					if (offer == null)
					{
						TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.DoNotHaveOnlineBooking;
						return RedirectToAction(MVC.Backoffice.Offer.Configure(id));
					}
				}
				else
				{
					offer = oRepo.Get(offerId);
				}

				Member.ValidateOwner(member, offer.Localisation);

				return View(offer);
			}
			catch (Exception ex)
			{
				_Logger.Error("OfferSchedule", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		public virtual ActionResult LocalisationSchedule(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var p = 1;
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				var loc = lRepo.Get(id);
				Member.ValidateOwner(member, loc);

				var bookings = bRepo.GetMany(b => b.Offer.LocalisationId == id);
				var model = new LocalisationBookingViewModel
				{
					Item = loc,
					List = new PagingList<MemberBooking>
					{
						List = bookings.ToList(),
						PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PagedListViewModel.PageSize, TotalItems = bookings.Count() }
					}
				};
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("LocalisationSchedule", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		#region Event feed

		/// <summary>
		/// provid a json array of all booking events
		/// </summary>
		/// <param name="id">id of the offer</param>
		/// <returns>json of booking events</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult BookingEvents(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

			var offer = oRepo.Get(id);

			var events = new List<CalandarJson>();
			foreach (var item in offer.MemberBookings)
			{
				events.Add(item.GetCalandarEvent(Url));
			}

			return Json(events);
		}

		#endregion

		#region Handlers

		/// <summary>
		/// Ajax action to handle dropevent
		/// </summary>
		/// <param name="id">id of the booking</param>
		/// <param name="dayDelta">day delta</param>
		/// <param name="minuteDelta">minute delta</param>
		[AcceptVerbs(HttpVerbs.Post)]
		[HandleModelStateException]
		public virtual ActionResult DropEvent(int id, int dayDelta, int minuteDelta)
		{
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var booking = bRepo.Get(id);

			if (booking != null)
            {
                var newFromDate = booking.FromDate.AddDays(dayDelta).AddMinutes(minuteDelta);
                if (!booking.CanModify(newFromDate))
                    throw new ModelStateException(ModelState);

				try
				{
					booking.FromDate = booking.FromDate.AddDays(dayDelta).AddMinutes(minuteDelta);
					booking.ToDate = booking.ToDate.AddDays(dayDelta).AddMinutes(minuteDelta);

					//send mail to booking client
					var urlHelper = new UrlHelper(ControllerContext.RequestContext);
					var dashboardUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Home.Index());
					TagBuilder dashboardLink = new TagBuilder("a");
					dashboardLink.MergeAttribute("href", dashboardUrl);
					dashboardLink.InnerHtml = dashboardUrl;

					var localisationUrl = booking.Offer.Localisation.GetDetailFullUrl(Url);
					TagBuilder localisationLink = new TagBuilder("a");
					localisationLink.MergeAttribute("href", localisationUrl);
					localisationLink.InnerHtml = localisationUrl;

					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
					clientMail.To = booking.Client.Email;
					clientMail.Subject = string.Format(Worki.Resources.Email.BookingString.CalandarBookingModificationSubject, booking.Offer.Localisation.Name);
					clientMail.ToName = booking.Client.MemberMainData.FirstName;
					clientMail.Content = string.Format(Worki.Resources.Email.BookingString.CalandarBookingModification,
														booking.Offer.Localisation.Name,
														booking.GetStartDate(),
														booking.GetEndDate(),
														dashboardLink,
														localisationLink);

					context.Commit();

					clientMail.Send();

					return Json("Drop success");
				}
				catch (Exception ex)
				{
					_Logger.Error("DropEvent", ex);
					context.Complete();
					throw new ModelStateException(ModelState);
				}
			}
			else
			{
				throw new ModelStateException(ModelState);
			}
		}

		/// <summary>
		/// Ajax action to handle Resizeevent
		/// </summary>
		/// <param name="id">id of the booking</param>
		/// <param name="dayDelta">day delta</param>
		/// <param name="minuteDelta">minute delta</param>
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleModelStateException]
        public virtual ActionResult ResizeEvent(int id, int dayDelta, int minuteDelta)
        {
            var context = ModelFactory.GetUnitOfWork();
            var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
            var booking = bRepo.Get(id);

            if (booking != null)
            {
                var newToDate = booking.ToDate.AddDays(dayDelta).AddMinutes(minuteDelta);
                if (!booking.CanModify(newToDate))
                    throw new ModelStateException(ModelState);

                try
                {
                    booking.ToDate = booking.ToDate.AddDays(dayDelta).AddMinutes(minuteDelta);

					//send mail to booking client
					var urlHelper = new UrlHelper(ControllerContext.RequestContext);
					var dashboardUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Home.Index());
					TagBuilder dashboardLink = new TagBuilder("a");
					dashboardLink.MergeAttribute("href", dashboardUrl);
					dashboardLink.InnerHtml = dashboardUrl;

					var localisationUrl = booking.Offer.Localisation.GetDetailFullUrl(Url);
					TagBuilder localisationLink = new TagBuilder("a");
					localisationLink.MergeAttribute("href", localisationUrl);
					localisationLink.InnerHtml = localisationUrl;

					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
					clientMail.To = booking.Client.Email;
					clientMail.Subject = string.Format(Worki.Resources.Email.BookingString.CalandarBookingModificationSubject, booking.Offer.Localisation.Name);
					clientMail.ToName = booking.Client.MemberMainData.FirstName;
					clientMail.Content = string.Format(	Worki.Resources.Email.BookingString.CalandarBookingModification,
														booking.Offer.Localisation.Name,
														booking.GetStartDate(),
														booking.GetEndDate(),
														dashboardLink,
														localisationLink);

                    context.Commit();

					clientMail.Send();

                    return Json("Resize success");
                }
                catch (Exception ex)
                {
                    _Logger.Error("ResizeEvent", ex);
                    context.Complete();
                    throw new ModelStateException(ModelState);
                }
            }
            else
            {
                throw new ModelStateException(ModelState);
            }
        }

        /// <summary>
        /// Action to show booking summary
        /// </summary>
        /// <param name="id">id of the booking</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual PartialViewResult BookingSummary(int id)
        {
            try
            {
                var context = ModelFactory.GetUnitOfWork();
                var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
                var booking = bRepo.Get(id);

                return PartialView(MVC.Backoffice.Schedule.Views._BookingSummary, booking);
            }
            catch (Exception ex)
            {
                _Logger.Error("BookingSummary", ex);
                return null;
            }
        }

		/// <summary>
		/// Action to create booking creation partial view for an offer
		/// </summary>
		/// <param name="id">id of the offer</param>
		/// <returns></returns>
		[ChildActionOnly]
		public virtual ActionResult CreateEvent(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			try
			{
				var context = ModelFactory.GetUnitOfWork();
				var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

				var offer = oRepo.Get(id);
				var clients = offer.Localisation.LocalisationClients.ToDictionary(mc => mc.ClientId, mc => mc.Member.GetFullDisplayName());
				var model = new CreateBookingModel(offer.Localisation);
				model.Booking.OfferId = id;

				return PartialView(MVC.Backoffice.Schedule.Views._CreateBooking, model);
			}
			catch (Exception ex)
			{
				_Logger.Error("CreateEvent", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Ajax action to handle Create event
		/// </summary>
		/// <param name="id">id of the booking</param>
		/// <param name="start">start date</param>
		/// <param name="end">end date</param>
		[AcceptVerbs(HttpVerbs.Post)]
		[HandleModelStateException]
		[ValidateOnlyIncomingValues]
		public virtual ActionResult CreateEvent(CreateBookingModel createBookingModel)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

			if (ModelState.IsValid)
			{
				try
				{
					var offer = oRepo.Get(createBookingModel.Booking.OfferId);

					MemberBookingLog log = new MemberBookingLog();
					log.CreatedDate = DateTime.UtcNow;
					log.EventType = (int)MemberBookingLog.BookingEvent.Creation;
					log.Event = "Booking Created From Calandar";
					createBookingModel.Booking.MemberBookingLogs.Add(log);

					offer.MemberBookings.Add(createBookingModel.Booking);

					context.Commit();

					var newContext = ModelFactory.GetUnitOfWork();
					var bRepo = ModelFactory.GetRepository<IBookingRepository>(newContext);
					var booking = bRepo.Get(createBookingModel.Booking.Id);

					//send mail to client
					var urlHelper = new UrlHelper(ControllerContext.RequestContext);
					var dashboardUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Home.Index());
					TagBuilder dashboardLink = new TagBuilder("a");
					dashboardLink.MergeAttribute("href", dashboardUrl);
					dashboardLink.InnerHtml = dashboardUrl;

					var localisationUrl = booking.Offer.Localisation.GetDetailFullUrl(Url);
					TagBuilder localisationLink = new TagBuilder("a");
					localisationLink.MergeAttribute("href", localisationUrl);
					localisationLink.InnerHtml = localisationUrl;

					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
					clientMail.To = booking.Client.Email;
					clientMail.Subject = string.Format(Worki.Resources.Email.BookingString.CalandarBookingCreationSubject, booking.Offer.Localisation.Name);
					clientMail.ToName = booking.Client.MemberMainData.FirstName;
					clientMail.Content = string.Format(Worki.Resources.Email.BookingString.CalandarBookingCreation,
														Localisation.GetOfferType(booking.Offer.Type),
														booking.Offer.Localisation.Name,
														booking.GetStartDate(),
														booking.GetEndDate(),
														dashboardLink,
														booking.Price,
														localisationLink);

					clientMail.Send();

					return Json(booking.GetCalandarEvent(Url));
				}
				catch (Exception ex)
				{
					_Logger.Error("CreateEvent", ex);
					context.Complete();
					throw new ModelStateException(ModelState);
				}
			}
			else
			{
				throw new ModelStateException(ModelState);
			}
		}

		/// <summary>
		/// Action to create booking
		/// </summary>
		/// <param name="id">id of the localisation</param>
		/// <returns></returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult CreateBooking(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			try
			{
				var context = ModelFactory.GetUnitOfWork();
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
				var member = mRepo.Get(memberId);
				var localisation = lRepo.Get(id);
				Member.Validate(member);

				var model = new CreateBookingModel(localisation);
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("CreateBooking", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Ajax action to handle Create event
		/// </summary>
		/// <param name="id">id of the booking</param>
		/// <param name="start">start date</param>
		/// <param name="end">end date</param>
		[AcceptVerbs(HttpVerbs.Post)]
		[HandleModelStateException]
		[ValidateOnlyIncomingValues]
		public virtual ActionResult CreateBooking(CreateBookingModel createBookingModel)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

			if (ModelState.IsValid)
			{
				try
				{
					var offer = oRepo.Get(createBookingModel.Booking.OfferId);

					MemberBookingLog log = new MemberBookingLog();
					log.CreatedDate = DateTime.UtcNow;
					log.EventType = (int)MemberBookingLog.BookingEvent.Creation;
					log.Event = "Booking Created From Calandar";
					createBookingModel.Booking.MemberBookingLogs.Add(log);

					offer.MemberBookings.Add(createBookingModel.Booking);

					context.Commit();

					var newContext = ModelFactory.GetUnitOfWork();
					var bRepo = ModelFactory.GetRepository<IBookingRepository>(newContext);
					var booking = bRepo.Get(createBookingModel.Booking.Id);

					//send mail to client
					var urlHelper = new UrlHelper(ControllerContext.RequestContext);
					var dashboardUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Home.Index());
					TagBuilder dashboardLink = new TagBuilder("a");
					dashboardLink.MergeAttribute("href", dashboardUrl);
					dashboardLink.InnerHtml = dashboardUrl;

					var localisationUrl = booking.Offer.Localisation.GetDetailFullUrl(Url);
					TagBuilder localisationLink = new TagBuilder("a");
					localisationLink.MergeAttribute("href", localisationUrl);
					localisationLink.InnerHtml = localisationUrl;

					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
					clientMail.To = booking.Client.Email;
					clientMail.Subject = string.Format(Worki.Resources.Email.BookingString.CalandarBookingCreationSubject, booking.Offer.Localisation.Name);
					clientMail.ToName = booking.Client.MemberMainData.FirstName;
					clientMail.Content = string.Format(Worki.Resources.Email.BookingString.CalandarBookingCreation,
														Localisation.GetOfferType(booking.Offer.Type),
														booking.Offer.Localisation.Name,
														booking.GetStartDate(),
														booking.GetEndDate(),
														dashboardLink,
														booking.Price,
														localisationLink);

					clientMail.Send();

					return RedirectToAction(MVC.Backoffice.Schedule.CreateBooking(createBookingModel.Booking.Offer.LocalisationId));
				}
				catch (Exception ex)
				{
					_Logger.Error("CreateBooking", ex);
					context.Complete();
				}
			}
			return View(createBookingModel);
		}

		#endregion	
	}
}
