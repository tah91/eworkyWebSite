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
        public ScheduleController(ILogger logger, IObjectStore objectStore)
            : base(logger, objectStore)
        {
            
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
				OfferController.GetOffer(id, offerId, out offer, lRepo, oRepo, () =>
				{
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.DoNotHaveOnlineBooking;
					return RedirectToAction(MVC.Backoffice.Localisation.Index(id));
				});

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
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				var loc = lRepo.Get(id);
				Member.ValidateOwner(member, loc);

				return View(id);
			}
			catch (Exception ex)
			{
				_Logger.Error("LocalisationSchedule", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		#region Event feed

		/// <summary>
		/// provid a json array of all booking events of a localisation
		/// </summary>
		/// <param name="id">id of the localisation</param>
		/// <returns>json of booking events</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult AllBookingEvents(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);

			var localisation = lRepo.Get(id);

			var events = new List<CalandarJson>();
			foreach (var item in localisation.GetBookings())
			{
				events.Add(item.GetCalandarEvent(Url, true));
			}

			return Json(events);
		}

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

		MemberBooking InternalCreateBooking(CreateBookingModel createBookingModel)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var memberId = WebHelper.GetIdentityId(User.Identity);
			var offer = oRepo.Get(createBookingModel.Booking.OfferId);

			try
			{
				createBookingModel.Booking.MemberBookingLogs.Add(new MemberBookingLog
					{
						CreatedDate = DateTime.UtcNow,
						EventType = (int)MemberBookingLog.BookingEvent.Creation,
						Event = "Booking Created From Calandar",
					}
				);

				if (createBookingModel.Booking.Paid)
				{
					createBookingModel.Booking.MemberBookingLogs.Add(new MemberBookingLog
					{
						CreatedDate = DateTime.UtcNow,
						Event = "Payment completed",
						EventType = (int)MemberBookingLog.BookingEvent.Payment,
						LoggerId = memberId
					});
				}

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

				return booking;
			}
			catch (Exception ex)
			{
				_Logger.Error("CreateEvent", ex);
				context.Complete();
				throw new ModelStateException(ModelState);
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
				var booking = InternalCreateBooking(createBookingModel);
				return Json(booking.GetCalandarEvent(Url));
			}
			else
			{
				throw new ModelStateException(ModelState);
			}
		}

		/// <summary>
		/// Ajax action to handle Create event then edit
		/// </summary>
		/// <param name="id">id of the booking</param>
		/// <param name="start">start date</param>
		/// <param name="end">end date</param>
		[AcceptVerbs(HttpVerbs.Post)]
		[HandleModelStateException]
		[ValidateOnlyIncomingValues]
		public virtual ActionResult CreateAndEditEvent(CreateBookingModel createBookingModel)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

			if (ModelState.IsValid)
			{
				var booking = InternalCreateBooking(createBookingModel);
				return Json(new { redirectToUrl = Url.Action(MVC.Backoffice.Schedule.EditBooking(booking.Id)) });
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

				var model = new LocalisationModel<CreateBookingModel> { InnerModel = new CreateBookingModel(localisation), LocalisationModelId = id };
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("CreateBooking", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// action to handle Create booking
		/// </summary>
		/// <param name="id">id of the booking</param>
		/// <param name="start">start date</param>
		/// <param name="end">end date</param>
		[AcceptVerbs(HttpVerbs.Post)]
		[HandleModelStateException]
		[ValidateOnlyIncomingValues]
		public virtual ActionResult CreateBooking(int id, LocalisationModel<CreateBookingModel> createBookingModel)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var booking = InternalCreateBooking(createBookingModel.InnerModel);
					return RedirectToAction(MVC.Backoffice.Schedule.OfferSchedule(booking.Offer.LocalisationId, booking.OfferId));
				}
				catch (Exception ex)
				{
					_Logger.Error("CreateBooking", ex);
				}
			}
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var localisation = lRepo.Get(id);
			createBookingModel.InnerModel.InitSelectLists(localisation);
			return View(createBookingModel);
		}

		/// <summary>
		/// Action to create booking
		/// </summary>
		/// <param name="id">id of the booking</param>
		/// <returns></returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult EditBooking(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			try
			{
				var context = ModelFactory.GetUnitOfWork();
				var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
				var booking = bRepo.Get(id);

				var model = new LocalisationModel<CreateBookingModel> { InnerModel = new CreateBookingModel(booking.Offer.Localisation, booking), LocalisationModelId = booking.Offer.LocalisationId };
				return View(MVC.Backoffice.Schedule.Views.CreateBooking, model);
			}
			catch (Exception ex)
			{
				_Logger.Error("EditBooking", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// action to handle Create booking
		/// </summary>
		/// <param name="id">id of the booking</param>
		[AcceptVerbs(HttpVerbs.Post)]
		[HandleModelStateException]
		[ValidateOnlyIncomingValues]
		public virtual ActionResult EditBooking(int id, LocalisationModel<CreateBookingModel> createBookingModel)
		{
			var context = ModelFactory.GetUnitOfWork();
			var memberId = WebHelper.GetIdentityId(User.Identity);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var booking = bRepo.Get(id);

			if (ModelState.IsValid)
			{
				try
				{
					TryUpdateModel(booking, "InnerModel.Booking");

					if (booking.Paid)
					{
						booking.MemberBookingLogs.Add(new MemberBookingLog
						{
							CreatedDate = DateTime.UtcNow,
							Event = "Payment completed",
							EventType = (int)MemberBookingLog.BookingEvent.Payment,
							LoggerId = memberId
						});
					}

					context.Commit();

					var newContext = ModelFactory.GetUnitOfWork();
					var newRepo = ModelFactory.GetRepository<IBookingRepository>(newContext);
					var newBooking = newRepo.Get(id);

					return RedirectToAction(MVC.Backoffice.Schedule.OfferSchedule(newBooking.Offer.LocalisationId, newBooking.OfferId));
				}
				catch (Exception ex)
				{
					_Logger.Error("CreateEvent", ex);
					context.Complete();
				}
			}
			createBookingModel.InnerModel.InitSelectLists(booking.Offer.Localisation);
			return View(MVC.Backoffice.Schedule.Views.CreateBooking, createBookingModel);
		}

		/// <summary>
		/// GET Action result to delete booking
		/// </summary>
		/// <param name="id">id of booking to delete</param>
		/// <returns>View to fill booking data</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult DeleteBooking(int id, string returnUrl)
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
				Member.ValidateOwner(member, booking.Offer.Localisation);

				var model = new RefuseBookingModel { BookingId = id, ReturnUrl = returnUrl };
				return View(new LocalisationModel<RefuseBookingModel> { InnerModel = model, LocalisationModelId = booking.Offer.LocalisationId });
			}
			catch (Exception ex)
			{
				_Logger.Error("RefuseBooking", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// POST Action result to confirm booking
		/// </summary>
		/// <param name="id">id of booking to confirm</param>
		/// <returns>View to fill booking data</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		[ValidateAntiForgeryToken]
		public virtual ActionResult DeleteBooking(int id, LocalisationModel<RefuseBookingModel> formModel, string confirm)
		{
			if (string.IsNullOrEmpty(confirm))
			{
				return Redirect(formModel.InnerModel.ReturnUrl);
			}

			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var memberId = WebHelper.GetIdentityId(User.Identity);

			if (ModelState.IsValid)
			{
				try
				{
					var booking = bRepo.Get(id);
					var offerId = booking.OfferId;
					var locId = booking.Offer.LocalisationId;
					bRepo.Delete(id);
					context.Commit();

					TempData[MiscHelpers.TempDataConstants.Info] = "Résa supprimée";

					return RedirectToAction(MVC.Backoffice.Schedule.OfferSchedule(locId, offerId));
				}
				catch (Exception ex)
				{
					_Logger.Error("DeleteBooking", ex);
					context.Complete();
				}
			}
			return View(formModel);
		}

		#endregion	
	}
}
