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
	public partial class QuotationController : Controller
	{
		#region Private

		ILogger _Logger;
		IEmailService _EmailService;
        IMembershipService _MembershipService;

		#endregion

		public QuotationController(	ILogger logger,
									IEmailService emailService,
                                    IMembershipService membershipService)
		{
			_Logger = logger;
			_EmailService = emailService;
            _MembershipService = membershipService;
		}

		/// <summary>
		/// GET Action result to show Quotation form
		/// </summary>
		/// <param name="id">id of localisation to book</param>
		/// <returns>View containing Quotation form</returns>
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
            var formModel = new MemberQuotationFormViewModel
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
		/// Post Action result to add Quotation request
		/// </summary>
		/// <returns>View containing Quotation form</returns>
        //[AcceptVerbs(HttpVerbs.Post), Authorize]
		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult Create(int id, int localisationId, MemberQuotationFormViewModel formData)
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
					try
					{
						formData.MemberQuotation.MemberId = memberId;
						formData.MemberQuotation.LocalisationId = localisationId;
						formData.MemberQuotation.OfferId = id;

						//set phone number to the one from form
						member.MemberMainData.PhoneNumber = formData.PhoneNumber;
						member.MemberQuotations.Add(formData.MemberQuotation);

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
					dynamic teamMail = new Email(MVC.Emails.Views.Email);
					teamMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                    teamMail.To = MiscHelpers.EmailConstants.BookingMail;
                    teamMail.Subject = Worki.Resources.Email.BookingString.QuotationMailSubject;
					teamMail.ToName = MiscHelpers.EmailConstants.ContactDisplayName;
                    teamMail.Content = string.Format(Worki.Resources.Email.BookingString.QuotationMailBody,
					                                 string.Format("{0} {1}", member.MemberMainData.FirstName, member.MemberMainData.LastName),
					                                 formData.PhoneNumber,
					                                 member.Email,
					                                 locName,
					                                 Localisation.GetOfferType(offer.Type),
					                                 formData.MemberQuotation.Surface,
					                                 formData.MemberQuotation.Message);
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
		/// GET Action result to show Quotation data
		/// </summary>
		/// <param name="id">id of Quotation</param>
		/// <returns>View containing Quotation data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult Details(int id, int memberId, int offerId, int localisationId)
		{
			var context = ModelFactory.GetUnitOfWork();
			var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
			var quotation = qRepo.Get(id, memberId, localisationId, offerId);
			return View(quotation);
		}

		/// <summary>
		/// GET Action result to edit quotation data
		/// </summary>
		/// <param name="id">id of quotation</param>
		/// <returns>View containing quotation data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult Edit(int id, int memberId, int offerId, int localisationId, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
			var quotation = qRepo.Get(id, memberId, localisationId, offerId);
			var member = mRepo.Get(memberId);
            var formModel = new MemberQuotationFormViewModel
            {
                PhoneNumber = member.MemberMainData.PhoneNumber,
				MemberQuotation = quotation,
                FirstName = member.MemberMainData.FirstName,
                LastName = member.MemberMainData.LastName,
                Email = member.Email,
                NeedNewAccount = false
            };
			return View(MVC.Quotation.Views.Create, formModel);
		}

		/// <summary>
		/// Post Action result to edit quotation data
		/// </summary>
		/// <param name="id">id of quotation</param>
		/// <returns>View containing quotation data</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult Edit(int id, int memberId, int offerId, int localisationId, string returnUrl, MemberQuotationFormViewModel formData)
		{
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
			try
			{
				UpdateModel(formData);
				if (ModelState.IsValid)
				{
					var b = bRepo.Get(id, memberId, localisationId, offerId);
					UpdateModel(b, "MemberQuotation");
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
			return View(MVC.Quotation.Views.Create, formData);
		}

		/// <summary>
		/// GET Action result to handle quotation
		/// </summary>
		/// <param name="id">id of quotation to handle</param>
		/// <param name="returnUrl">url to redirect</param>
		/// <returns>Redirect to url</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult HandleBooking(int id, int memberId, int offerId, int localisationId, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			try
			{
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
				var m = mRepo.Get(memberId);
				var quotation = qRepo.Get(id, memberId, localisationId, offerId);
				quotation.Handled = true;

				//send email

				dynamic handleMail = new Email(MVC.Emails.Views.Email);
				handleMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
				handleMail.To = quotation.Member.Email;
				handleMail.ToName = quotation.Member.MemberMainData.FirstName;

				handleMail.Subject = Worki.Resources.Email.BookingString.QuotationHandleMailSubject;
				handleMail.Content = string.Format(Worki.Resources.Email.BookingString.QuotationHandleMailBody,
													Localisation.GetOfferType(quotation.Offer.Type),
													quotation.Surface,
													quotation.Offer.Localisation.Name,
													quotation.Offer.Localisation.Adress + ", " + quotation.Offer.Localisation.PostalCode + " " + quotation.Offer.Localisation.City);

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

        #region Private

        /// <summary>
        /// send mail for new member
        /// </summary>
        /// <param name="formData">form data containing quotation data</param>
        /// <param name="member">member data</param>
        /// <param name="loc">localisation data</param>
		void SendCreationAccountMail(MemberQuotationFormViewModel formData, Member member, Offer offer)
		{
			var urlHelper = new UrlHelper(ControllerContext.RequestContext);
			var profilUrl = urlHelper.ActionAbsolute(MVC.Profil.Dashboard());
			TagBuilder profilLink = new TagBuilder("a");
			profilLink.MergeAttribute("href", profilUrl);
			profilLink.InnerHtml = profilUrl;

			dynamic newMemberMail = new Email(MVC.Emails.Views.Email);
			newMemberMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
			newMemberMail.To = formData.Email;
			newMemberMail.ToName = formData.FirstName;

			newMemberMail.Subject = Worki.Resources.Email.BookingString.QuotationNewMemberSubject;
			newMemberMail.Content = string.Format(Worki.Resources.Email.BookingString.QuotationNewMemberBody,
												Localisation.GetOfferType(offer.Type),
												formData.MemberQuotation.Surface,
												offer.Localisation.Name,
												offer.Localisation.Adress + ", " + offer.Localisation.PostalCode + " " + offer.Localisation.City,
												member.Email,
												_MembershipService.GetPassword(member.Email, null),
												profilLink.ToString(),
												urlHelper.ActionAbsolute(MVC.Profil.Edit()));

			newMemberMail.Send();
		}

        #endregion
    }
}
