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

		#endregion

		public BookingController(	ILogger logger,
									IEmailService emailService,
                                    IMembershipService membershipService)
		{
			_Logger = logger;
			_EmailService = emailService;
            _MembershipService = membershipService;
		}

		/// <summary>
		/// GET Action result to show booking form
		/// </summary>
		/// <param name="id">id of localisation to book</param>
		/// <returns>View containing booking form</returns>
		//[AcceptVerbs(HttpVerbs.Get), Authorize]
        [AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult Create(int id, int localisationId)
        {
			var memberId = WebHelper.GetIdentityId(User.Identity);
            //if (memberId == 0)
            //    return View(MVC.Shared.Views.Error);
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var offer = oRepo.Get(id,localisationId);
            var member = mRepo.Get(memberId);
            var membetExists = member != null;
            var formModel = new MemberBookingFormViewModel
            {
                PhoneNumber = membetExists ? member.MemberMainData.PhoneNumber : string.Empty,
                NeedNewAccount = (memberId == 0),
                FirstName = membetExists ? member.MemberMainData.FirstName : string.Empty,
                LastName = membetExists ? member.MemberMainData.LastName : string.Empty,
                Email = membetExists ? member.Email : string.Empty,
				NeedQuotation = offer.NeedQuotation()
            };

            return View(formModel);
        }

		/// <summary>
		/// Post Action result to add booking request
		/// </summary>
		/// <returns>View containing booking form</returns>
        //[AcceptVerbs(HttpVerbs.Post), Authorize]
		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult Create(int id, int localisationId, MemberBookingFormViewModel formData)
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
					var offer = oRepo.Get(id, localisationId);
					var locName = offer.Localisation.Name;
					var needQuotation = offer.NeedQuotation();
					try
					{
						formData.MemberBooking.MemberId = memberId;
						formData.MemberBooking.LocalisationId = localisationId;
						formData.MemberBooking.OfferId = id;

						//set phone number to the one from form
						member.MemberMainData.PhoneNumber = formData.PhoneNumber;
						member.MemberBookings.Add(formData.MemberBooking);

						context.Commit();
					}
					catch (Exception ex)
					{
						_Logger.Error(ex.Message);
						context.Complete();
						throw ex;
					}

					if (sendNewAccountMail)
					{
						SendCreationAccountMail(formData, member, offer);
					}

					//send mail to team

					dynamic teamMail = new Email(MiscHelpers.EmailConstants.EmailView);
					teamMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
					teamMail.To = MiscHelpers.EmailConstants.BookingMail;
					teamMail.Subject = Worki.Resources.Email.BookingString.BookingMailSubject;
					teamMail.ToName = MiscHelpers.EmailConstants.ContactDisplayName;
					teamMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingMailBody,
													 string.Format("{0} {1}", member.MemberMainData.FirstName, member.MemberMainData.LastName),
													 formData.PhoneNumber,
													 member.Email,
													 locName,
													 Localisation.GetOfferType(offer.Type),
													 string.Format("{0:dd/MM/yyyy HH:MM}", formData.MemberBooking.FromDate),
													 string.Format("{0:dd/MM/yyyy HH:MM}", formData.MemberBooking.ToDate),
													 formData.MemberBooking.Message);
					teamMail.Send();

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

		/// <summary>
		/// GET Action result to show booking data
		/// </summary>
		/// <param name="id">id of booking</param>
		/// <returns>View containing booking data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult Details(int id, int memberId, int offerId, int localisationId)
		{
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var booking = bRepo.Get(id, memberId, localisationId, offerId);
			return View(booking);
		}

		/// <summary>
		/// GET Action result to edit booking data
		/// </summary>
		/// <param name="id">id of booking</param>
		/// <returns>View containing booking data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult Edit(int id, int memberId, int offerId, int localisationId, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var booking = bRepo.Get(id, memberId, localisationId, offerId);
			var member = mRepo.Get(memberId);
            var formModel = new MemberBookingFormViewModel
            {
                PhoneNumber = member.MemberMainData.PhoneNumber,
                MemberBooking = booking,
                FirstName = member.MemberMainData.FirstName,
                LastName = member.MemberMainData.LastName,
                Email = member.Email,
                NeedNewAccount = false
            };
			return View(MVC.Booking.Views.Create, formModel);
		}

		/// <summary>
		/// Post Action result to edit booking data
		/// </summary>
		/// <param name="id">id of booking</param>
		/// <returns>View containing booking data</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult Edit(int id, int memberId, int offerId, int localisationId, string returnUrl, MemberBookingFormViewModel formData)
		{
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			try
			{
				UpdateModel(formData);
				if (ModelState.IsValid)
				{
					var b = bRepo.Get(id, memberId, localisationId, offerId);
					UpdateModel(b, "MemberBooking");
					context.Commit();
					return Redirect(returnUrl);
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
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult HandleBooking(int id, int memberId, int offerId, int localisationId, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			try
			{
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
                var m = mRepo.Get(memberId);
				var booking = bRepo.Get(id, memberId, localisationId, offerId);
                booking.Handled = true;

				//send email

				dynamic handleMail = new Email(MiscHelpers.EmailConstants.EmailView);
				handleMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
                handleMail.To = booking.Member.Email;
				handleMail.ToName = booking.Member.MemberMainData.FirstName;

				if (booking.Offer.NeedQuotation())
				{
					handleMail.Subject = Worki.Resources.Email.BookingString.QuotationHandleMailSubject;
					handleMail.Content = string.Format(Worki.Resources.Email.BookingString.QuotationHandleMailBody,
														Localisation.GetOfferType(booking.Offer.Type),
														booking.Surface,
														booking.Offer.Localisation.Name,
														booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City);
				}
				else 
				{
					handleMail.Subject = Worki.Resources.Email.BookingString.BookingHandleMailSubject;
					handleMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingHandleMailBody,
														Localisation.GetOfferType(booking.Offer.Type),
														string.Format("{0:dd/MM/yyyy HH:MM}", booking.FromDate),
														string.Format("{0:dd/MM/yyyy HH:MM}", booking.ToDate),
														booking.Offer.Localisation.Name,
														booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City);
				}

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
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult ConfirmBooking(int id, int memberId, int offerId, int localisationId, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			try
			{
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
				var m = mRepo.Get(memberId);
				var booking = bRepo.Get(id, memberId, localisationId, offerId);
                booking.Confirmed = true;

				if (booking.Offer.NeedQuotation())
					throw new Exception("No confirmation for Quotation");

				//send email
				dynamic confirmMail = new Email(MiscHelpers.EmailConstants.EmailView);
				confirmMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
				confirmMail.To = booking.Member.Email;
				confirmMail.Subject = Worki.Resources.Email.BookingString.ConfirmMailSubject;
				confirmMail.ToName = booking.Member.MemberMainData.FirstName;
				confirmMail.Content = string.Format(Worki.Resources.Email.BookingString.ConfirmMailBody,
													Localisation.GetOfferType(booking.Offer.Type),
													string.Format("{0:dd/MM/yyyy HH:MM}", booking.FromDate),
													string.Format("{0:dd/MM/yyyy HH:MM}", booking.ToDate),
													booking.Offer.Localisation.Name,
													booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City,
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

        /// <summary>
        /// GET Action result to refuse booking
        /// </summary>
        /// <param name="id">id of booking to refuse</param>
        /// <param name="returnUrl">url to redirect</param>
        /// <returns>Redirect to url</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult RefuseBooking(int id, int memberId, int offerId, int localisationId, string returnUrl)
        {
            var context = ModelFactory.GetUnitOfWork();
            try
            {
                var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
                var m = mRepo.Get(memberId);
				var booking = bRepo.Get(id, memberId, localisationId, offerId);
                booking.Refused = true;

				if (booking.Offer.NeedQuotation())
					throw new Exception("No confirmation for Quotation");

                //send email
				dynamic refuseMail = new Email(MiscHelpers.EmailConstants.EmailView);
				refuseMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
                refuseMail.To = booking.Member.Email;
                refuseMail.Subject = Worki.Resources.Email.BookingString.RefuseMailSubject;
                refuseMail.ToName = booking.Member.MemberMainData.FirstName;
                refuseMail.Content = string.Format(Worki.Resources.Email.BookingString.RefuseMailBody,
                                                    Localisation.GetOfferType(booking.Offer.Type),
                                                    string.Format("{0:dd/MM/yyyy HH:MM}", booking.FromDate),
                                                    string.Format("{0:dd/MM/yyyy HH:MM}", booking.ToDate),
													booking.Offer.Localisation.Name,
													booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City);
                refuseMail.Send();
                context.Commit();
            }
            catch (Exception ex)
            {
                _Logger.Error("RefuseBooking", ex);
                context.Complete();
            }
            return Redirect(returnUrl);
        }

        #region Private

        /// <summary>
        /// send mail for new member
        /// </summary>
        /// <param name="formData">form data containing booking data</param>
        /// <param name="member">member data</param>
        /// <param name="loc">localisation data</param>
        void SendCreationAccountMail(MemberBookingFormViewModel formData, Member member, Offer offer)
        {
            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
			var profilUrl = urlHelper.AbsoluteAction(MVC.Profil.ActionNames.Dashboard, MVC.Profil.Name, new { id = member.MemberId });
			TagBuilder profilLink = new TagBuilder("a");
			profilLink.MergeAttribute("href", profilUrl);
			profilLink.InnerHtml = profilUrl;

			dynamic newMemberMail = new Email(MiscHelpers.EmailConstants.EmailView);
			newMemberMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
            newMemberMail.To = formData.Email;
			newMemberMail.ToName = formData.FirstName;

			if (offer.NeedQuotation())
			{
				newMemberMail.Subject = Worki.Resources.Email.BookingString.QuotationNewMemberSubject;
				newMemberMail.Content = string.Format(Worki.Resources.Email.BookingString.QuotationNewMemberBody,
													Localisation.GetOfferType(offer.Type),
													formData.MemberBooking.Surface,
													offer.Localisation.Name,
													offer.Localisation.Adress + ", " + offer.Localisation.PostalCode + " " + offer.Localisation.City,
													member.Email,
													_MembershipService.GetPassword(member.Email, null),
													profilLink.ToString(),
													urlHelper.AbsoluteAction(MVC.Profil.ActionNames.Edit, MVC.Profil.Name, new { id = member.MemberId }));
			}
			else
			{
				newMemberMail.Subject = Worki.Resources.Email.BookingString.BookingNewMemberSubject;
				newMemberMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingNewMemberBody,
													string.Format("{0:dd/MM/yyyy HH:MM}", formData.MemberBooking.FromDate),
													string.Format("{0:dd/MM/yyyy HH:MM}", formData.MemberBooking.ToDate),
													offer.Localisation.Name,
													offer.Localisation.Adress + ", " + offer.Localisation.PostalCode + " " + offer.Localisation.City,
													member.Email,
													_MembershipService.GetPassword(member.Email, null),
													profilLink.ToString(),
													urlHelper.AbsoluteAction(MVC.Profil.ActionNames.Edit, MVC.Profil.Name, new { id = member.MemberId }));
			}
           
            newMemberMail.Send();
        }

        #endregion
    }
}
