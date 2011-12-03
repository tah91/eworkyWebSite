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
using Worki.Memberships;
using System.Collections.Generic;
using Worki.Section;

namespace Worki.Web.Controllers
{
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
	public partial class BookingController : Controller
	{
		#region Private

		ILogger _Logger;
		IEmailService _EmailService;
        IMembershipService _MembershipService;
        IPaymentService _PaymentService;

		#endregion

		public BookingController(	ILogger logger,
									IEmailService emailService,
                                    IMembershipService membershipService,
                                    IPaymentService paymentService)
		{
			_Logger = logger;
			_EmailService = emailService;
            _MembershipService = membershipService;
            _PaymentService = paymentService;
		}

		/// <summary>
		/// GET Action result to show booking form
		/// </summary>
		/// <param name="id">id of offer to book</param>
		/// <returns>View containing booking form</returns>
		//[AcceptVerbs(HttpVerbs.Get), Authorize]
        [AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult Create(int id)
        {
			var memberId = WebHelper.GetIdentityId(User.Identity);
            //if (memberId == 0)
            //    return View(MVC.Shared.Views.Error);
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var offer = oRepo.Get(id);
            var member = mRepo.Get(memberId);
            var membetExists = member != null;
            var formModel = new MemberBookingFormViewModel
            {
                PhoneNumber = membetExists ? member.MemberMainData.PhoneNumber : string.Empty,
                NeedNewAccount = (memberId == 0),
                FirstName = membetExists ? member.MemberMainData.FirstName : string.Empty,
                LastName = membetExists ? member.MemberMainData.LastName : string.Empty,
                Email = membetExists ? member.Email : string.Empty,
            };

            return View(formModel);
        }

		/// <summary>
		/// Post Action result to add booking request
		/// </summary>
		/// <returns>View containing booking form</returns>
        //[AcceptVerbs(HttpVerbs.Post), Authorize]
		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult Create(int id, MemberBookingFormViewModel formData)
		{
			if (ModelState.IsValid)
			{
				var memberId = WebHelper.GetIdentityId(User.Identity);
				var sendNewAccountMail = false;
				try
				{
					var memberData = new MemberMainData
					{
						FirstName = formData.FirstName,
						LastName = formData.LastName,
						PhoneNumber = formData.PhoneNumber,
					};
					sendNewAccountMail = _MembershipService.TryCreateAccount(formData.Email, memberData, out memberId);

					var context = ModelFactory.GetUnitOfWork();
					var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
					var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
					var member = mRepo.Get(memberId);
					var offer = oRepo.Get(id);
					var locName = offer.Localisation.Name;
					try
					{
						formData.MemberBooking.MemberId = memberId;
						formData.MemberBooking.OfferId = id;
						formData.MemberBooking.StatusId = (int)MemberBooking.Status.Unknown;
						//set phone number to the one from form
						member.MemberMainData.PhoneNumber = formData.PhoneNumber;
						member.MemberBookings.Add(formData.MemberBooking);

						formData.MemberBooking.MemberBookingLogs.Add(new MemberBookingLog
						{
							CreatedDate = DateTime.UtcNow,
							Event = "Booking Created",
							EventType = (int)MemberBookingLog.BookingEvent.Creation
						});

						dynamic newMemberMail = null;
						if (sendNewAccountMail)
						{
							var urlHelper = new UrlHelper(ControllerContext.RequestContext);
							var profilUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Home.Index());
							TagBuilder profilLink = new TagBuilder("a");
							profilLink.MergeAttribute("href", profilUrl);
							profilLink.InnerHtml = profilUrl;

							newMemberMail = new Email(MVC.Emails.Views.Email);
							newMemberMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
							newMemberMail.To = formData.Email;
							newMemberMail.ToName = formData.FirstName;

							newMemberMail.Subject = Worki.Resources.Email.BookingString.BookingNewMemberSubject;
							newMemberMail.Content = "TODO MAIL";
						}

						//send mail to team
						dynamic teamMail = new Email(MVC.Emails.Views.Email);
						teamMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
						teamMail.To = MiscHelpers.EmailConstants.BookingMail;
						teamMail.Subject = Worki.Resources.Email.BookingString.BookingMailSubject;
						teamMail.ToName = MiscHelpers.EmailConstants.ContactDisplayName;
						teamMail.Content = string.Format(Worki.Resources.Email.BookingString.CreateBookingTeam,
														 string.Format("{0} {1}", member.MemberMainData.FirstName, member.MemberMainData.LastName),
														 formData.PhoneNumber,
														 member.Email,
														 locName,
														 Localisation.GetOfferType(offer.Type),
														 CultureHelpers.GetSpecificFormat(formData.MemberBooking.FromDate, CultureHelpers.TimeFormat.Date),
														 CultureHelpers.GetSpecificFormat(formData.MemberBooking.ToDate, CultureHelpers.TimeFormat.Date),
														 formData.MemberBooking.Message);

						//send mail to booking member
						dynamic clientMail = new Email(MVC.Emails.Views.Email);
						clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
						clientMail.To = member.Email;
						clientMail.Subject = Worki.Resources.Email.BookingString.BookingMailSubject;
						clientMail.ToName = member.MemberMainData.FirstName;
						clientMail.Content = string.Format(Worki.Resources.Email.BookingString.CreateBookingClient,
														 Localisation.GetOfferType(offer.Type),
														 CultureHelpers.GetSpecificFormat(formData.MemberBooking.FromDate, CultureHelpers.TimeFormat.Date),
														 CultureHelpers.GetSpecificFormat(formData.MemberBooking.ToDate, CultureHelpers.TimeFormat.Date),
														 locName,
														 offer.Localisation.Adress);

						//send mail to localisation member
						dynamic ownerMail = new Email(MVC.Emails.Views.Email);
						ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
						ownerMail.To = offer.Localisation.Member.Email;
						ownerMail.Subject = Worki.Resources.Email.BookingString.BookingMailSubject;
						ownerMail.ToName = offer.Localisation.Member.MemberMainData.FirstName;
						ownerMail.Content = string.Format(Worki.Resources.Email.BookingString.CreateBookingOwner,
														Localisation.GetOfferType(offer.Type),
														 CultureHelpers.GetSpecificFormat(formData.MemberBooking.FromDate, CultureHelpers.TimeFormat.Date),
														 CultureHelpers.GetSpecificFormat(formData.MemberBooking.ToDate, CultureHelpers.TimeFormat.Date),
														 locName,
														 offer.Localisation.Adress);

						context.Commit();

						if (sendNewAccountMail)
						{
							newMemberMail.Send();
						}
						clientMail.Send();
						teamMail.Send();
						ownerMail.Send();
					}
					catch (Exception ex)
					{
						_Logger.Error(ex.Message);
						context.Complete();
						throw ex;
					}

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Booking.BookingString.Confirmed;
					return Redirect(offer.Localisation.GetDetailFullUrl(Url));
				}
				catch (Exception ex)
				{
					_Logger.Error("Create", ex);
					ModelState.AddModelError("", ex.Message);
				}
			}
			return View(formData);
		}

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("paywithpaypal")]
        public virtual ActionResult PayWithPayPal(int id)
        {
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);

			var booking = bRepo.Get(id);

            string returnUrl = Url.ActionAbsolute(MVC.Dashboard.Home.BookingPaymentAccepted(id));
            string cancelUrl = Url.ActionAbsolute(MVC.Dashboard.Home.BookingPaymentCancelled(id));
            //string returnUrl = Url.ActionAbsolute(MVC.Payment.PayPalAccepted(memberBookingId));
            //string cancelUrl = Url.ActionAbsolute(MVC.Payment.PayPalCancelled(memberBookingId));
            string ipnUrl = Url.ActionAbsolute(MVC.Payment.PayPalInstantNotification());

            decimal ownerAmount, eworkyAmount;
            var paymentHandler = PaymentHandlerFactory.GetHandler(PaymentHandlerFactory.HandlerType.Booking) as MemberBookingPaymentHandler;
            paymentHandler.GetAmounts(booking.Price, out ownerAmount, out eworkyAmount);
            var payments = new List<PaymentItem>
            {
                new PaymentItem{  Index = 0, Amount = ownerAmount, Email = "t.ifti_1322172136_biz@hotmail.fr"},
                new PaymentItem{  Index = 1, Amount = eworkyAmount, Email = "t.ifti_1322171616_biz@hotmail.fr"},
            };

            string paypalApprovalUrl = _PaymentService.PayWithPayPal(id,
                                                                    returnUrl,
                                                                    cancelUrl,
                                                                    ipnUrl,
                                                                    "",
                                                                    payments,
                                                                    paymentHandler,
                                                                    PaymentConfiguration.Constants);

            if (paypalApprovalUrl != null)
            {
                return Redirect(paypalApprovalUrl);
            }

            TempData[MiscHelpers.TempDataConstants.Info] = "Une erreur emepeche le paymenent, veuillez nous contacter à support@eworky.com";

            return RedirectToAction(MVC.Home.Index());
        }

		///// <summary>
		///// GET Action result to show booking data
		///// </summary>
		///// <param name="id">id of booking</param>
		///// <returns>View containing booking data</returns>
		//[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		//public virtual ActionResult Details(int id)
		//{
		//    var context = ModelFactory.GetUnitOfWork();
		//    var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
		//    var booking = bRepo.Get(id);
		//    return View(booking);
		//}

		///// <summary>
		///// GET Action result to edit booking data
		///// </summary>
		///// <param name="id">id of booking</param>
		///// <returns>View containing booking data</returns>
		//[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		//public virtual ActionResult Edit(int id, int memberId, string returnUrl)
		//{
		//    var context = ModelFactory.GetUnitOfWork();
		//    var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
		//    var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
		//    var booking = bRepo.Get(id);
		//    var member = mRepo.Get(memberId);
		//    var formModel = new MemberBookingFormViewModel
		//    {
		//        PhoneNumber = member.MemberMainData.PhoneNumber,
		//        MemberBooking = booking,
		//        FirstName = member.MemberMainData.FirstName,
		//        LastName = member.MemberMainData.LastName,
		//        Email = member.Email,
		//        NeedNewAccount = false
		//    };
		//    return View(MVC.Booking.Views.Create, formModel);
		//}

		///// <summary>
		///// Post Action result to edit booking data
		///// </summary>
		///// <param name="id">id of booking</param>
		///// <returns>View containing booking data</returns>
		//[AcceptVerbs(HttpVerbs.Post), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		//public virtual ActionResult Edit(int id, string returnUrl, MemberBookingFormViewModel formData)
		//{
		//    var context = ModelFactory.GetUnitOfWork();
		//    var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
		//    try
		//    {
		//        UpdateModel(formData);
		//        if (ModelState.IsValid)
		//        {
		//            var b = bRepo.Get(id);
		//            UpdateModel(b, "MemberBooking");
		//            context.Commit();
		//            return Redirect(returnUrl);
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        _Logger.Error("Edit", ex);
		//        context.Complete();
		//        ModelState.AddModelError("", ex.Message);
		//    }
		//    return View(MVC.Booking.Views.Create, formData);
		//}

		///// <summary>
		///// GET Action result to handle booking
		///// </summary>
		///// <param name="id">id of booking to handle</param>
		///// <param name="returnUrl">url to redirect</param>
		///// <returns>Redirect to url</returns>
		//[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		//public virtual ActionResult HandleBooking(int id, int memberId, string returnUrl)
		//{
		//    var context = ModelFactory.GetUnitOfWork();
		//    try
		//    {
		//        var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
		//        var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
		//        var m = mRepo.Get(memberId);
		//        var booking = bRepo.Get(id);
		//        //booking.Handled = true;

		//        //send email

		//        dynamic handleMail = new Email(MVC.Emails.Views.Email);
		//        handleMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
		//        handleMail.To = booking.Member.Email;
		//        handleMail.ToName = booking.Member.MemberMainData.FirstName;

		//        handleMail.Subject = Worki.Resources.Email.BookingString.BookingHandleMailSubject;
		//        handleMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingHandleMailBody,
		//                                            Localisation.GetOfferType(booking.Offer.Type),
		//                                            CultureHelpers.GetSpecificFormat(booking.FromDate, CultureHelpers.TimeFormat.General),
		//                                            CultureHelpers.GetSpecificFormat(booking.ToDate, CultureHelpers.TimeFormat.General),
		//                                            booking.Offer.Localisation.Name,
		//                                            booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City);

		//        handleMail.Send();
		//        context.Commit();
		//    }
		//    catch (Exception ex)
		//    {
		//        _Logger.Error("HandleBooking", ex);
		//        context.Complete();
		//    }
		//    return Redirect(returnUrl);
		//}

		///// <summary>
		///// GET Action result to confirm booking
		///// </summary>
		///// <param name="id">id of booking to confirm</param>
		///// <param name="returnUrl">url to redirect</param>
		///// <returns>Redirect to url</returns>
		//[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		//public virtual ActionResult ConfirmBooking(int id, int memberId, string returnUrl)
		//{
		//    var context = ModelFactory.GetUnitOfWork();
		//    try
		//    {
		//        var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
		//        var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
		//        var m = mRepo.Get(memberId);
		//        var booking = bRepo.Get(id);
		//        booking.StatusId = (int)MemberBooking.Status.Accepted;

		//        //send email
		//        dynamic confirmMail = new Email(MVC.Emails.Views.Email);
		//        confirmMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
		//        confirmMail.To = booking.Member.Email;
		//        confirmMail.Subject = Worki.Resources.Email.BookingString.ConfirmMailSubject;
		//        confirmMail.ToName = booking.Member.MemberMainData.FirstName;
		//        confirmMail.Content = string.Format(Worki.Resources.Email.BookingString.ConfirmMailBody,
		//                                            Localisation.GetOfferType(booking.Offer.Type),
		//                                            CultureHelpers.GetSpecificFormat(booking.FromDate, CultureHelpers.TimeFormat.General),
		//                                            CultureHelpers.GetSpecificFormat(booking.ToDate, CultureHelpers.TimeFormat.General),
		//                                            booking.Offer.Localisation.Name,
		//                                            booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City,
		//                                            booking.Price);
		//        confirmMail.Send();
		//        context.Commit();
		//    }
		//    catch (Exception ex)
		//    {
		//        _Logger.Error("ConfirmBooking", ex);
		//        context.Complete();
		//    }
		//    return Redirect(returnUrl);
		//}

		///// <summary>
		///// GET Action result to refuse booking
		///// </summary>
		///// <param name="id">id of booking to refuse</param>
		///// <param name="returnUrl">url to redirect</param>
		///// <returns>Redirect to url</returns>
		//[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		//public virtual ActionResult RefuseBooking(int id, int memberId, string returnUrl)
		//{
		//    var context = ModelFactory.GetUnitOfWork();
		//    try
		//    {
		//        var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
		//        var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
		//        var m = mRepo.Get(memberId);
		//        var booking = bRepo.Get(id);
		//        booking.StatusId = (int)MemberBooking.Status.Accepted;

		//        //send email
		//        dynamic refuseMail = new Email(MVC.Emails.Views.Email);
		//        refuseMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
		//        refuseMail.To = booking.Member.Email;
		//        refuseMail.Subject = Worki.Resources.Email.BookingString.RefuseMailSubject;
		//        refuseMail.ToName = booking.Member.MemberMainData.FirstName;
		//        refuseMail.Content = string.Format(Worki.Resources.Email.BookingString.RefuseMailBody,
		//                                            Localisation.GetOfferType(booking.Offer.Type),
		//                                            CultureHelpers.GetSpecificFormat(booking.FromDate, CultureHelpers.TimeFormat.General),
		//                                            CultureHelpers.GetSpecificFormat(booking.ToDate, CultureHelpers.TimeFormat.General),
		//                                            booking.Offer.Localisation.Name,
		//                                            booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City);
		//        refuseMail.Send();
		//        context.Commit();
		//    }
		//    catch (Exception ex)
		//    {
		//        _Logger.Error("RefuseBooking", ex);
		//        context.Complete();
		//    }
		//    return Redirect(returnUrl);
		//}
    }
}
