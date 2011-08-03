using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkiSiteWeb.Models;
using WorkiSiteWeb.Helpers;
using WorkiSiteWeb.Infrastructure.Email;
using WorkiSiteWeb.Infrastructure.Repository;
using WorkiSiteWeb.Infrastructure.Logging;
using System.Web.Security;

namespace WorkiSiteWeb.Controllers
{
	public partial class BookingController : Controller
	{
		#region Private

		IMemberRepository _MemberRepository;
		ILocalisationRepository _LocalisationRepository;
		IRepository<MemberBooking, int> _BookingRepository;
		ILogger _Logger;
		IEmailService _EmailService;

		#endregion

		public BookingController(	IMemberRepository memberRepository, 
									ILogger logger, 
									ILocalisationRepository localisationRepository,
									IRepository<MemberBooking, int> bookingRepository,
									IEmailService emailService)
		{
			_MemberRepository = memberRepository;
			_Logger = logger;
			_LocalisationRepository = localisationRepository;
			_BookingRepository = bookingRepository;
			_EmailService = emailService;
		}

		/// <summary>
		/// GET Action result to show booking form
		/// </summary>
		/// <param name="id">id of localisation to book</param>
		/// <returns>View containing booking form</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		public virtual ActionResult Create(int id, string returnUrl)
		{
			var memberId = ControllerHelpers.GetIdentityId(User.Identity);
			if (memberId == 0)
				return View(MVC.Shared.Views.Error);
			var member = _MemberRepository.Get(memberId);
			var formModel = new MemberBookingFormViewModel { PhoneNumber = member.MemberMainData.PhoneNumber, ReturnUrl = returnUrl };

			return View(formModel);
		}

		/// <summary>
		/// Post Action result to add booking request
		/// </summary>
		/// <returns>View containing booking form</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		public virtual ActionResult Create(int id, MemberBookingFormViewModel formData)
		{
			var memberId = ControllerHelpers.GetIdentityId(User.Identity);
			if (memberId == 0)
				return View(MVC.Shared.Views.Error);
			var member = _MemberRepository.Get(memberId);
			try
			{
				formData.MemberBooking.MemberId = memberId;
				formData.MemberBooking.LocalisationId = id;
				_MemberRepository.Update(memberId, m =>
				{
					//set phone number to the one from form
					m.MemberMainData.PhoneNumber = formData.PhoneNumber;
					m.MemberBookings.Add(formData.MemberBooking);
				});

				//send mail to team
				var subject = WorkiResources.Views.Booking.BookingString.BookingMailSubject;
				var content = string.Format(WorkiResources.Views.Booking.BookingString.BookingMailBody,
											string.Format("{0} {1}", member.MemberMainData.FirstName, member.MemberMainData.LastName),
											formData.PhoneNumber,
											member.Email,
											Localisation.GetOfferType(formData.MemberBooking.Offer),
											string.Format("{0:dd/MM/yyyy HH:MM}", formData.MemberBooking.FromDate),
											string.Format("{0:dd/MM/yyyy HH:MM}", formData.MemberBooking.ToDate),
											formData.MemberBooking.Message);

				content = MiscHelpers.Nl2Br(content);

				_EmailService.Send(EmailService.ContactMail, "eWorky Team", subject, content, true, EmailService.BookingMail);

				return Redirect(formData.ReturnUrl);
			}
			catch (Exception ex)
			{
				_Logger.Error("Create", ex);
				ModelState.AddModelError("", ex.Message);
			}
			return View(formData);
		}

		/// <summary>
		/// GET Action result to show booking data
		/// </summary>
		/// <param name="id">id of booking</param>
		/// <returns>View containing booking data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminRole)]
		public virtual ActionResult Details(int id)
		{
			var booking = _BookingRepository.Get(id);
			return View(booking);
		}

		/// <summary>
		/// GET Action result to edit booking data
		/// </summary>
		/// <param name="id">id of booking</param>
		/// <returns>View containing booking data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminRole)]
		public virtual ActionResult Edit(int id, int memberId, string returnUrl)
		{
			var booking = _BookingRepository.Get(id);
			var member = _MemberRepository.Get(memberId);
			var formModel = new MemberBookingFormViewModel { PhoneNumber = member.MemberMainData.PhoneNumber, ReturnUrl = returnUrl, MemberBooking = booking };
			return View(MVC.Booking.Views.Create, formModel);
		}

		/// <summary>
		/// Post Action result to edit booking data
		/// </summary>
		/// <param name="id">id of booking</param>
		/// <returns>View containing booking data</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize(Roles = MiscHelpers.AdminRole)]
		public virtual ActionResult Edit(int id)
		{
			var formData = new MemberBookingFormViewModel();
			try
			{
				UpdateModel(formData);
				if (ModelState.IsValid)
				{
					_BookingRepository.Update(id, b =>
					{
						UpdateModel(b, "MemberBooking");
					});
					return Redirect(formData.ReturnUrl);
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("Edit", ex);
				ModelState.AddModelError("", ex.Message);
			}
			return View(MVC.Booking.Views.Create, formData);
		}

		/// <summary>
		/// GET Action result to handle booking
		/// </summary>
		/// <param name="id">id of booking to handle</param>
		/// <param name="returnUrl">url to redirect</param>
		/// <returns>Redirect to url</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminRole)]
		public virtual ActionResult HandleBooking(int id, string returnUrl)
		{
			try
			{
				_BookingRepository.Update(id, b =>
				{
					b.Handled = true;
				});
				//send email
				var booking = _BookingRepository.Get(id);
				var subject = WorkiResources.Views.Booking.BookingString.HandleMailSubject;
				var content = string.Format(WorkiResources.Views.Booking.BookingString.HandleMailBody,
											Localisation.GetOfferType(booking.Offer),
											string.Format("{0:dd/MM/yyyy HH:MM}", booking.FromDate),
											string.Format("{0:dd/MM/yyyy HH:MM}", booking.ToDate),
											booking.Localisation.Name,
											booking.Localisation.Adress + ", " + booking.Localisation.PostalCode + " " + booking.Localisation.City);

				content = MiscHelpers.Nl2Br(content);

				_EmailService.Send(EmailService.BookingMail, "eWorky Team", subject, content, true, booking.Member.Email);
			}
			catch (Exception ex)
			{
				_Logger.Error("HandleBooking", ex);
			}
			return Redirect(returnUrl);
		}

		/// <summary>
		/// GET Action result to confirm booking
		/// </summary>
		/// <param name="id">id of booking to confirm</param>
		/// <param name="returnUrl">url to redirect</param>
		/// <returns>Redirect to url</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminRole)]
		public virtual ActionResult ConfirmBooking(int id, string returnUrl)
		{
			try
			{
				_BookingRepository.Update(id, b =>
				{
					b.Confirmed = true;
				});
				//send email
				var booking = _BookingRepository.Get(id);
				var subject = WorkiResources.Views.Booking.BookingString.ConfirmMailSubject;
				var content = string.Format(WorkiResources.Views.Booking.BookingString.ConfirmMailBody,
											Localisation.GetOfferType(booking.Offer),
											string.Format("{0:dd/MM/yyyy HH:MM}", booking.FromDate),
											string.Format("{0:dd/MM/yyyy HH:MM}", booking.ToDate),
											booking.Localisation.Name,
											booking.Localisation.Adress + ", " + booking.Localisation.PostalCode + " " + booking.Localisation.City,
											booking.Price);

				content = MiscHelpers.Nl2Br(content);

				_EmailService.Send(EmailService.BookingMail, "eWorky Team", subject, content, true, booking.Member.Email);
			}
			catch (Exception ex)
			{
				_Logger.Error("ConfirmBooking", ex);
			}
			return Redirect(returnUrl);
		}
	}
}
