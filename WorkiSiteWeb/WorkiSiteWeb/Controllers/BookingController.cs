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
        public virtual ActionResult Create(int id, string returnUrl)
        {
			var memberId = WebHelper.GetIdentityId(User.Identity);
            //if (memberId == 0)
            //    return View(MVC.Shared.Views.Error);
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var member = mRepo.Get(memberId);
            var membetExists = member != null;
            var formModel = new MemberBookingFormViewModel
            {
                PhoneNumber = membetExists ? member.MemberMainData.PhoneNumber : string.Empty,
                ReturnUrl = returnUrl,
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
                    if (memberId == 0)
                    {
                        FetchAccount(formData, ref  memberId, ref  sendNewAccountMail);
                    }

                    var context = ModelFactory.GetUnitOfWork();
                    var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                    var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
                    var member = mRepo.Get(memberId);
                    var loc = lRepo.Get(id);

                    try
                    {
                        formData.MemberBooking.MemberId = memberId;
                        formData.MemberBooking.LocalisationId = id;

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
                        SendCreationAccountMail(formData, member, loc);
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
                                                     loc.Name,
                                                     Localisation.GetOfferType(formData.MemberBooking.OfferId),
                                                     string.Format("{0:dd/MM/yyyy HH:MM}", formData.MemberBooking.FromDate),
                                                     string.Format("{0:dd/MM/yyyy HH:MM}", formData.MemberBooking.ToDate),
                                                     formData.MemberBooking.Message);
                    teamMail.Send();

                    return Redirect(formData.ReturnUrl);
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
		public virtual ActionResult Details(int id, int memberId)
		{
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
            var booking = bRepo.Get(id, memberId);
			return View(booking);
		}

		/// <summary>
		/// GET Action result to edit booking data
		/// </summary>
		/// <param name="id">id of booking</param>
		/// <returns>View containing booking data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult Edit(int id, int memberId, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
            var booking = bRepo.Get(id, memberId);
			var member = mRepo.Get(memberId);
            var formModel = new MemberBookingFormViewModel
            {
                PhoneNumber = member.MemberMainData.PhoneNumber,
                ReturnUrl = returnUrl,
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
		public virtual ActionResult Edit(int id, int memberId)
		{
			var formData = new MemberBookingFormViewModel();
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			try
			{
				UpdateModel(formData);
				if (ModelState.IsValid)
				{
                    var b = bRepo.Get(id, memberId);
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
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult HandleBooking(int id,int memberId, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			try
			{
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
                var m = mRepo.Get(memberId);
                var booking = bRepo.Get(id,memberId);
                booking.Handled = true;

				//send email

				dynamic handleMail = new Email(MiscHelpers.EmailConstants.EmailView);
				handleMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
                handleMail.To = booking.Member.Email;
                handleMail.Subject = Worki.Resources.Email.BookingString.HandleMailSubject;
                handleMail.ToName = booking.Member.MemberMainData.FirstName;
                handleMail.Content = string.Format( Worki.Resources.Email.BookingString.HandleMailBody,
                                                    Localisation.GetOfferType(booking.OfferId),
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
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult ConfirmBooking(int id, int memberId, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			try
			{
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
				var m = mRepo.Get(memberId);
                var booking = bRepo.Get(id, memberId);
                booking.Confirmed = true;

				//send email
				dynamic confirmMail = new Email(MiscHelpers.EmailConstants.EmailView);
				confirmMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
				confirmMail.To = booking.Member.Email;
				confirmMail.Subject = Worki.Resources.Email.BookingString.ConfirmMailSubject;
				confirmMail.ToName = booking.Member.MemberMainData.FirstName;
				confirmMail.Content = string.Format(Worki.Resources.Email.BookingString.ConfirmMailBody,
													Localisation.GetOfferType(booking.OfferId),
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

        /// <summary>
        /// GET Action result to refuse booking
        /// </summary>
        /// <param name="id">id of booking to refuse</param>
        /// <param name="returnUrl">url to redirect</param>
        /// <returns>Redirect to url</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
        public virtual ActionResult RefuseBooking(int id, int memberId, string returnUrl)
        {
            var context = ModelFactory.GetUnitOfWork();
            try
            {
                var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
                var m = mRepo.Get(memberId);
                var booking = bRepo.Get(id, memberId);
                booking.Refused = true;

                //send email
				dynamic refuseMail = new Email(MiscHelpers.EmailConstants.EmailView);
				refuseMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
                refuseMail.To = booking.Member.Email;
                refuseMail.Subject = Worki.Resources.Email.BookingString.RefuseMailSubject;
                refuseMail.ToName = booking.Member.MemberMainData.FirstName;
                refuseMail.Content = string.Format(Worki.Resources.Email.BookingString.RefuseMailBody,
                                                    Localisation.GetOfferType(booking.OfferId),
                                                    string.Format("{0:dd/MM/yyyy HH:MM}", booking.FromDate),
                                                    string.Format("{0:dd/MM/yyyy HH:MM}", booking.ToDate),
                                                    booking.Localisation.Name,
                                                    booking.Localisation.Adress + ", " + booking.Localisation.PostalCode + " " + booking.Localisation.City);
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
        /// check if an account exists for the given email
        /// if not create one an validate it
        /// </summary>
        /// <param name="formData">form data containing data for new member</param>
        /// <param name="memberId">memberId to fill from the fetched member</param>
        /// <param name="sendNewAccountMail">if new member created</param>
        void FetchAccount(MemberBookingFormViewModel formData, ref int memberId, ref bool sendNewAccountMail)
        {
            //check if email match an account
            var createAccountContext = ModelFactory.GetUnitOfWork();
            var createAccountmRepo = ModelFactory.GetRepository<IMemberRepository>(createAccountContext);
            try
            {
                var memberFromForm = createAccountmRepo.GetMember(formData.Email);
                if (memberFromForm != null)
                {
                    memberId = memberFromForm.MemberId;
                }
                else
                {
                    //if not create an account from form data
                    var status = _MembershipService.CreateUser(formData.Email, MiscHelpers.AdminConstants.DummyPassword, formData.Email, true);
                    if (status != System.Web.Security.MembershipCreateStatus.Success)
                    {
                        var error = AccountValidation.ErrorCodeToString(status);
                        throw new Exception(error);
                    }
                    var created = createAccountmRepo.GetMember(formData.Email);
                    created.MemberMainData = new MemberMainData();
                    created.MemberMainData.FirstName = formData.FirstName;
                    created.MemberMainData.LastName = formData.LastName;
                    created.MemberMainData.PhoneNumber = formData.PhoneNumber;
                    createAccountContext.Commit();

                    if (!_MembershipService.ResetPassword(formData.Email))
                    {
                        throw new Exception("ResetPassword failed");
                    }
                    //send email to new member
                    sendNewAccountMail = true;

                    memberId = created.MemberId;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.Message);
                createAccountContext.Complete();
                throw ex;
            }
        }

        /// <summary>
        /// send mail for new member
        /// </summary>
        /// <param name="formData">form data containing booking data</param>
        /// <param name="member">member data</param>
        /// <param name="loc">localisation data</param>
        void SendCreationAccountMail(MemberBookingFormViewModel formData, Member member, Localisation loc)
        {
            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
            var changePassUrl = urlHelper.AbsoluteAction(MVC.Account.ActionNames.ChangePassword, MVC.Account.Name, new { userName = member.Email, key = member.EmailKey });
            TagBuilder changePassLink = new TagBuilder("a");
            changePassLink.MergeAttribute("href", changePassUrl);
            changePassLink.InnerHtml = changePassUrl;

			dynamic newMemberMail = new Email(MiscHelpers.EmailConstants.EmailView);
			newMemberMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
            newMemberMail.To = formData.Email;
            newMemberMail.Subject = Worki.Resources.Email.BookingString.NewMemberSubject;
            newMemberMail.ToName = formData.FirstName;
            newMemberMail.Content = string.Format(Worki.Resources.Email.BookingString.NewMemberBody,
                                                Localisation.GetOfferType(formData.MemberBooking.OfferId),
                                                string.Format("{0:dd/MM/yyyy HH:MM}", formData.MemberBooking.FromDate),
                                                string.Format("{0:dd/MM/yyyy HH:MM}", formData.MemberBooking.ToDate),
                                                loc.Name,
                                                loc.Adress + ", " + loc.PostalCode + " " + loc.City,
                                                member.Email,
                                                _MembershipService.GetPassword(member.Email, null),
                                                changePassLink.ToString(),
                                                urlHelper.AbsoluteAction(MVC.Profil.ActionNames.Edit, MVC.Profil.Name, new { id = member.MemberId }));
            newMemberMail.Send();
        }

        #endregion
    }
}
