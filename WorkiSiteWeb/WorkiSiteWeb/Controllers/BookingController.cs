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
	public partial class BookingController : ControllerBase
	{
		#region Private

		IEmailService _EmailService;
        IMembershipService _MembershipService;
        IPaymentService _PaymentService;

		#endregion

		public BookingController(	ILogger logger,
                                    IObjectStore objectStore,
									IEmailService emailService,
                                    IMembershipService membershipService,
                                    IPaymentService paymentService)
            : base(logger, objectStore)
		{
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

            var formModel = new MemberBookingFormViewModel(member, offer);

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
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
            var memberId = WebHelper.GetIdentityId(User.Identity);
            var member = mRepo.Get(memberId);
            var offer = oRepo.Get(id);

			if (ModelState.IsValid)
			{
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
					member = mRepo.Get(memberId);

					var locName = offer.Localisation.Name;
                    var locUrl = offer.Localisation.GetDetailFullUrl(Url);
					try
					{
						formData.MemberBooking.MemberId = memberId;
						formData.MemberBooking.OfferId = id;
						formData.MemberBooking.StatusId = (int)MemberBooking.Status.Unknown;
                        formData.AjustBookingPeriod();
                        formData.MemberBooking.Price = offer.GetDefaultPrice(   formData.MemberBooking.FromDate,
                                                                                formData.MemberBooking.ToDate,
																				formData.MemberBooking.PeriodType == (int)MemberBooking.ePeriodType.SpendUnit,
                                                                                (Offer.PaymentPeriod)formData.MemberBooking.TimeType,
                                                                                formData.MemberBooking.TimeUnits);
						//set phone number to the one from form
						member.MemberMainData.PhoneNumber = formData.PhoneNumber;
						member.MemberBookings.Add(formData.MemberBooking);

						formData.MemberBooking.MemberBookingLogs.Add(new MemberBookingLog
						{
							CreatedDate = DateTime.UtcNow,
							Event = "Booking Created",
							EventType = (int)MemberBookingLog.BookingEvent.Creation,
							LoggerId = memberId
						});

						formData.MemberBooking.InvoiceNumber = new InvoiceNumber();

						if (!offer.Localisation.HasClient(memberId))
						{
							offer.Localisation.LocalisationClients.Add(new LocalisationClient { ClientId = memberId });
						}

						dynamic newMemberMail = null;
						if (sendNewAccountMail)
						{
							var urlHelper = new UrlHelper(ControllerContext.RequestContext);
							var editprofilUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit());
							TagBuilder profilLink = new TagBuilder("a");
							profilLink.MergeAttribute("href", editprofilUrl);
							profilLink.InnerHtml = Worki.Resources.Views.Account.AccountString.EditMyProfile;

                            var editpasswordUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit());
                            TagBuilder passwordLink = new TagBuilder("a");
                            passwordLink.MergeAttribute("href", editpasswordUrl);
                            passwordLink.InnerHtml = Worki.Resources.Views.Account.AccountString.ChangeMyPassword;

							newMemberMail = new Email(MVC.Emails.Views.Email);
							newMemberMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
							newMemberMail.To = formData.Email;
							newMemberMail.ToName = formData.FirstName;

							newMemberMail.Subject = Worki.Resources.Email.BookingString.BookingNewMemberSubject;
                            newMemberMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingNewMember,
                                                                    Localisation.GetOfferType(offer.Type),
                                                                    formData.MemberBooking.GetStartDate(),
                                                                    formData.MemberBooking.GetEndDate(),
                                                                    locName,
                                                                    offer.Localisation.Adress,
                                                                    formData.Email,
                                                                    _MembershipService.GetPassword(formData.Email, null),
                                                                    passwordLink,
                                                                    profilLink);
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
														 formData.MemberBooking.GetStartDate(),
														 formData.MemberBooking.GetEndDate(),
														 formData.MemberBooking.Message,
                                                         locUrl);

						//send mail to booking member
						dynamic clientMail = new Email(MVC.Emails.Views.Email);
						clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
						clientMail.To = member.Email;
						clientMail.Subject = Worki.Resources.Email.BookingString.CreateBookingClientSubject;
						clientMail.ToName = member.MemberMainData.FirstName;
						clientMail.Content = string.Format(Worki.Resources.Email.BookingString.CreateBookingClient,
														 Localisation.GetOfferType(offer.Type),
														 formData.MemberBooking.GetStartDate(),
														 formData.MemberBooking.GetEndDate(),
														 locName,
														 offer.Localisation.Adress);

						//send mail to localisation member
                        var urlHelp = new UrlHelper(ControllerContext.RequestContext);
                        var ownerUrl = urlHelp.ActionAbsolute(MVC.Backoffice.Home.Booking());
						TagBuilder ownerLink = new TagBuilder("a");
                        ownerLink.MergeAttribute("href", ownerUrl);
                        ownerLink.InnerHtml = Worki.Resources.Views.Account.AccountString.OwnerSpace;

						dynamic ownerMail = new Email(MVC.Emails.Views.Email);
						ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
						ownerMail.To = offer.Localisation.Member.Email;
						ownerMail.Subject = string.Format(Worki.Resources.Email.BookingString.BookingOwnerSubject, locName);
						ownerMail.ToName = offer.Localisation.Member.MemberMainData.FirstName;
						ownerMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingOwnerBody,
														Localisation.GetOfferType(offer.Type),
														locName,
														offer.Localisation.Adress,
                                                        ownerLink);

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
            formData.Periods = new SelectList(Offer.GetPaymentPeriodTypes(offer.GetPricePeriods()), "Key", "Value");
			formData.BookingOffer = offer;
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
            string ipnUrl = Url.ActionAbsolute(MVC.Payment.PayPalInstantNotification());

            decimal ownerAmount, eworkyAmount;
            var paymentHandler = PaymentHandlerFactory.GetHandler(PaymentHandlerFactory.HandlerType.Booking) as MemberBookingPaymentHandler;
			var localisation = booking.Offer.Localisation;

			localisation.GetAmounts(booking.Price, out ownerAmount, out eworkyAmount);

			var ownerMail = WebHelper.IsDebug() ? "t.ifti_1322172136_biz@hotmail.fr" : booking.Owner.MemberMainData.PaymentAddress;

            var payments = new List<PaymentItem>
            {
                new PaymentItem{  Index = 0, Amount = ownerAmount, Email = ownerMail}
            };

			if (eworkyAmount != 0)
			{
				new PaymentItem { Index = 1, Amount = eworkyAmount, Email = PaymentConfiguration.Instance.PaypalMail };
			}

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

            TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Booking.BookingString.PaymentError;

            return RedirectToAction(MVC.Home.Index());
        }

		/// <summary>
		/// GET Action result to show booking data
		/// </summary>
		/// <param name="id">id of booking</param>
		/// <returns>View containing booking data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult Details(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var booking = bRepo.Get(id);
			return View(booking);
		}

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
