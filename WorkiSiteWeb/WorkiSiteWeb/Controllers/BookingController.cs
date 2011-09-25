using System;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Data.Repository;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Web.Helpers;
using Worki.Service;
using Worki.Infrastructure;
using Postal;
using System.Linq;

namespace Worki.Web.Controllers
{
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
	public partial class BookingController : Controller
	{
		#region Private

		ILogger _Logger;
		IEmailService _EmailService;

		#endregion

		public BookingController(	ILogger logger,
									IEmailService emailService)
		{
			_Logger = logger;
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
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var member = mRepo.Get(memberId);
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
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var member = mRepo.Get(memberId);
			try
			{
				formData.MemberBooking.MemberId = memberId;
				formData.MemberBooking.LocalisationId = id;

				//set phone number to the one from form
				member.MemberMainData.PhoneNumber = formData.PhoneNumber;
				member.MemberBookings.Add(formData.MemberBooking);
				context.Commit();

				//send mail to team
				dynamic teamMail = new Email(MiscHelpers.EmailView);
				teamMail.From = MiscHelpers.ContactDisplayName + "<" + MiscHelpers.ContactMail + ">";
				teamMail.To = MiscHelpers.BookingMail;
				teamMail.Subject = Worki.Resources.Email.BookingString.BookingMailSubject;
				teamMail.ToName = MiscHelpers.ContactDisplayName;
				teamMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingMailBody,
													string.Format("{0} {1}", member.MemberMainData.FirstName, member.MemberMainData.LastName),
													formData.PhoneNumber,
													member.Email,
													Localisation.GetOfferType(formData.MemberBooking.Offer),
													string.Format("{0:dd/MM/yyyy HH:MM}", formData.MemberBooking.FromDate),
													string.Format("{0:dd/MM/yyyy HH:MM}", formData.MemberBooking.ToDate),
													formData.MemberBooking.Message);
				teamMail.Send();

				return Redirect(formData.ReturnUrl);
			}
			catch (Exception ex)
			{
				_Logger.Error("Create", ex);
				context.Complete();
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
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var booking = bRepo.Get(id);
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
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var booking = bRepo.Get(id);
			var member = mRepo.Get(memberId);
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
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			try
			{
				UpdateModel(formData);
				if (ModelState.IsValid)
				{
					var b = bRepo.Get(id);
					UpdateModel(b, "MemberBooking");
					context.Commit();
					return Redirect(formData.ReturnUrl);
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("Edit", ex);
				context.Complete();
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
		public virtual ActionResult HandleBooking(int id,int memberId, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			try
			{
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
                var m = mRepo.Get(memberId);
                foreach (var b in m.MemberBookings.ToList())
                {
                    if (b.Id != id)
                        continue;
                    b.Handled = true;
                }
				//send email
				var booking = bRepo.Get(b => b.Id == id);

                dynamic handleMail = new Email(MiscHelpers.EmailView);
                handleMail.From = MiscHelpers.ContactDisplayName + "<" + MiscHelpers.BookingMail + ">";
                handleMail.To = booking.Member.Email;
                handleMail.Subject = Worki.Resources.Email.BookingString.HandleMailSubject;
                handleMail.ToName = booking.Member.MemberMainData.FirstName;
                handleMail.Content = string.Format( Worki.Resources.Email.BookingString.HandleMailBody,
                                                    Localisation.GetOfferType(booking.Offer),
                                                    string.Format("{0:dd/MM/yyyy HH:MM}", booking.FromDate),
                                                    string.Format("{0:dd/MM/yyyy HH:MM}", booking.ToDate),
                                                    booking.Localisation.Name,
                                                    booking.Localisation.Adress + ", " + booking.Localisation.PostalCode + " " + booking.Localisation.City);
                handleMail.Send();
				context.Commit();
			}
			catch (Exception ex)
			{
				_Logger.Error("HandleBooking", ex);
				context.Complete();
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
		public virtual ActionResult ConfirmBooking(int id, int memberId, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			try
			{
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
				var m = mRepo.Get(memberId);
				foreach (var b in m.MemberBookings.ToList())
				{
					if (b.Id != id)
						continue;
					b.Confirmed = true;
				}
				//send email
				var booking = bRepo.Get(b => b.Id == id);

				//send email
				dynamic confirmMail = new Email(MiscHelpers.EmailView);
				confirmMail.From = MiscHelpers.ContactDisplayName + "<" + MiscHelpers.BookingMail + ">";
				confirmMail.To = booking.Member.Email;
				confirmMail.Subject = Worki.Resources.Email.BookingString.ConfirmMailSubject;
				confirmMail.ToName = booking.Member.MemberMainData.FirstName;
				confirmMail.Content = string.Format(Worki.Resources.Email.BookingString.ConfirmMailBody,
													Localisation.GetOfferType(booking.Offer),
													string.Format("{0:dd/MM/yyyy HH:MM}", booking.FromDate),
													string.Format("{0:dd/MM/yyyy HH:MM}", booking.ToDate),
													booking.Localisation.Name,
													booking.Localisation.Adress + ", " + booking.Localisation.PostalCode + " " + booking.Localisation.City,
													booking.Price);
				confirmMail.Send();
				context.Commit();
			}
			catch (Exception ex)
			{
				_Logger.Error("ConfirmBooking", ex);
				context.Complete();
			}
			return Redirect(returnUrl);
		}
	}
}
