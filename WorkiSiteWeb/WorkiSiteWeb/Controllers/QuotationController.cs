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
using Worki.Infrastructure.Email;
using System.Linq;
using Worki.Memberships;
using System.Collections.Generic;
using Worki.Section;
using System.Net.Mail;

namespace Worki.Web.Controllers
{
	public partial class QuotationController : ControllerBase
	{
		#region Private

        IMembershipService _MembershipService;
        IPaymentService _PaymentService;

		#endregion

        public QuotationController( ILogger logger,
                                    IObjectStore objectStore, 
                                    IEmailService emailService,
                                    IMembershipService membershipService,
                                    IPaymentService paymentService)
            : base(logger, objectStore, emailService)
		{
            _MembershipService = membershipService;
            _PaymentService = paymentService;
		}

		/// <summary>
		/// GET Action result to show Quotation form
		/// </summary>
		/// <param name="id">id of localisation to book</param>
		/// <returns>View containing Quotation form</returns>
        [AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult Create(int id)
        {
			var memberId = WebHelper.GetIdentityId(User.Identity);
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var offer = oRepo.Get(id);
            var member = mRepo.Get(memberId);

			var formModel = new MemberQuotationFormViewModel(member, offer);

            return View(formModel);
        }

		/// <summary>
		/// Post Action result to add Quotation request
		/// </summary>
		/// <returns>View containing Quotation form</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create(int id, MemberQuotationFormViewModel formData)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);

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
                        var localisation = lRepo.Get(offer.LocalisationId);
                        var hasOwner = localisation.HasOwner();

                        if (hasOwner)
                        {
                            formData.MemberQuotation.MemberId = memberId;
                            formData.MemberQuotation.OfferId = id;
                            formData.MemberQuotation.StatusId = (localisation.DirectlyReceiveQuotation == true) ? (int)MemberQuotation.Status.Unknown : (int)MemberQuotation.Status.Pending;
                            member.MemberQuotations.Add(formData.MemberQuotation);

                            formData.MemberQuotation.MemberQuotationLogs.Add(new MemberQuotationLog
                            {
                                CreatedDate = DateTime.UtcNow,
                                Event = "Quotation Created",
                                EventType = (int)MemberQuotationLog.QuotationEvent.Creation,
                                LoggerId = memberId
                            });
                        }
                        //set phone number to the one from form
                        member.MemberMainData.PhoneNumber = formData.PhoneNumber;

                        dynamic newMemberMail = null;
                        if (sendNewAccountMail)
                        {
                            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
                            var editpasswordUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Profil.ChangePassword());
                            TagBuilder editpasswordLink = new TagBuilder("a");
                            editpasswordLink.MergeAttribute("href", editpasswordUrl);
                            editpasswordLink.InnerHtml = Worki.Resources.Views.Account.AccountString.ChangeMyPassword;
                            var editprofilUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit());
                            TagBuilder editprofilLink = new TagBuilder("a");
                            editprofilLink.MergeAttribute("href", editprofilUrl);
                            editprofilLink.InnerHtml = Worki.Resources.Views.Account.AccountString.EditMyProfile;

                            var newMemberMailContent = string.Format(Worki.Resources.Email.BookingString.QuotationNewMemberBody,
                                                                    Localisation.GetOfferType(offer.Type), //0
                                                                    offer.Localisation.Name, //1
                                                                    offer.Localisation.Adress, //2
                                                                    formData.Email, //3
                                                                    _MembershipService.GetPassword(formData.Email, null), //4
                                                                    editpasswordLink, //5
                                                                    editprofilLink); //6;

                            newMemberMail = _EmailService.PrepareMessageFromDefault(new MailAddress(formData.Email, formData.FirstName),
                                Worki.Resources.Email.BookingString.QuotationNewMemberSubject,
                                WebHelper.RenderEmailToString(formData.FirstName, newMemberMailContent));
                        }

                        //send mail to team
                        var teamMailContent = string.Format(Worki.Resources.Email.BookingString.QuotationMailBody,
                                                         string.Format("{0} {1}", member.MemberMainData.FirstName, member.MemberMainData.LastName), //0
                                                         formData.PhoneNumber, //1
                                                         member.Email, //2
                                                         locName, //3
                                                         Localisation.GetOfferType(offer.Type), //4
                                                         formData.MemberQuotation.Message, //5
                                                         locUrl); //6

                        var teamMail = _EmailService.PrepareMessageFromDefault(new MailAddress(MiscHelpers.EmailConstants.BookingMail, MiscHelpers.EmailConstants.ContactDisplayName),
                            hasOwner ? Worki.Resources.Email.BookingString.QuotationMailSubject : Worki.Resources.Email.BookingString.QuotationMailSubject + " (sans gérant)",
                            WebHelper.RenderEmailToString(MiscHelpers.EmailConstants.ContactDisplayName, teamMailContent));

                        //send mail to quoation client
                        var clientMailContent = string.Format(Worki.Resources.Email.BookingString.CreateQuotationClient,
                                                         Localisation.GetOfferType(offer.Type), //0
                                                         locName, //1
                                                         offer.Localisation.Adress); //2

                        var clientMail = _EmailService.PrepareMessageFromDefault(new MailAddress(member.Email, member.MemberMainData.FirstName),
                             Worki.Resources.Email.BookingString.CreateQuotationClientSubject,
                            WebHelper.RenderEmailToString(member.MemberMainData.FirstName, clientMailContent));

                        context.Commit();

                        if (hasOwner && formData.MemberQuotation.StatusId == (int)MemberQuotation.Status.Unknown)
                        {
                            //we set the information for the mail after commit in order to get the id of the MemberQuotation that have been inserted
                            var context2 = ModelFactory.GetUnitOfWork();

                            var oRepo2 = ModelFactory.GetRepository<IOfferRepository>(context2);
                            var lRepo2 = ModelFactory.GetRepository<ILocalisationRepository>(context2);
                            var mRepo2 = ModelFactory.GetRepository<IMemberRepository>(context2);

                            var offer2 = oRepo2.Get(id);
                            var localisation2 = lRepo2.Get(offer.LocalisationId);
                            var member2 = mRepo2.Get(localisation2.OwnerID);

                            var urlHelp = new UrlHelper(ControllerContext.RequestContext);
                            //we get the ownerUrl from the id of the created MemberQuotation
                            var ownerUrl = urlHelp.ActionAbsolute(MVC.Backoffice.Localisation.QuotationDetail(formData.MemberQuotation.Id));
                            TagBuilder ownerLink = new TagBuilder("a");
                            ownerLink.MergeAttribute("href", ownerUrl);
                            ownerLink.InnerHtml = Worki.Resources.Views.Account.AccountString.OwnerSpace;

                            var ownerMailContent = string.Format(Worki.Resources.Email.BookingString.CreateQuotationOwner,
                                                            Localisation.GetOfferType(offer.Type),
                                                            localisation2.Name,
                                                            localisation2.Adress,
                                                            ownerLink);

                            var ownerMail = _EmailService.PrepareMessageFromDefault(new MailAddress(member2.Email, localisation2.Member.MemberMainData.FirstName),
                                  string.Format(Worki.Resources.Email.BookingString.CreateQuotationOwnerSubject, localisation2.Name),
                                  WebHelper.RenderEmailToString(member2.Email, ownerMailContent));

                            _EmailService.Deliver(ownerMail);
                        }
                        if (sendNewAccountMail)
                        {
                            _EmailService.Deliver(newMemberMail);
                        }
                        _EmailService.Deliver(clientMail);
                        _EmailService.Deliver(teamMail);
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex.Message);
                        context.Complete();
                        throw ex;
                    }

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Booking.BookingString.QuotationConfirmed;
                    return Redirect(offer.Localisation.GetDetailFullUrl(Url));
                }
                catch (Exception ex)
                {
                    _Logger.Error("Create", ex);
                    ModelState.AddModelError("", ex.Message);
                }
            }
            formData.QuotationOffer = offer;
            return View(formData);
        }

		/// <summary>
		/// GET Action result to show Quotation data
		/// </summary>
		/// <param name="id">id of Quotation</param>
		/// <returns>View containing Quotation data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult Details(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
			var quotation = qRepo.Get(id);
			return View(quotation);
		}

        /// <summary>
        /// GET Action result to edit Quotation data
        /// </summary>
        /// <param name="id">id of Quotation</param>
        /// <returns>View containing Quotation data</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
        public virtual ActionResult Edit(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
            var quotation = qRepo.Get(id);
            return View(quotation);
        }

        /// <summary>
        /// POST Action result to edit Quotation data
        /// </summary>
        /// <param name="id">id of Quotation</param>
        /// <returns>View containing Quotation data</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(int id, MemberQuotation formModel)
        {
            if (ModelState.IsValid)
            {
                var context = ModelFactory.GetUnitOfWork();
                var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
                try
                {
                    var quotation = qRepo.Get(id);
                    TryUpdateModel(quotation);

                    context.Commit();
                }
                catch (Exception ex)
                {
                    _Logger.Error("Edit", ex);
                    context.Complete();
                    ModelState.AddModelError("", ex.Message);
                }

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.QuotationEdited;

                return RedirectToAction(MVC.Quotation.Details(id));
            }
            return View(formModel);
        }

        [AcceptVerbs(HttpVerbs.Get), Authorize]
        [ActionName("paywithpaypal")]
        public virtual ActionResult PayWithPayPal(int id)
        {
            var memberId = WebHelper.GetIdentityId(User.Identity);
			var context = ModelFactory.GetUnitOfWork();
			var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);

            var member = mRepo.Get(memberId);
            var quotation = qRepo.Get(id);

            //case of first quotation, give it for free, or if it is shared office
            var first = qRepo.GetOwnerProducts(memberId).OrderBy(q => q.CreationDate).FirstOrDefault();
            if ((first == null || first.Id == id) || quotation.Offer.Localisation.ShouldNotPayQuotation())
            {
                try
                {
                    quotation.StatusId = (int)MemberQuotation.Status.Paid;

                    quotation.MemberQuotationLogs.Add(new MemberQuotationLog
                    {
                        CreatedDate = DateTime.UtcNow,
                        Event = "Payment completed",
                        EventType = (int)MemberQuotationLog.QuotationEvent.Payment,
                        LoggerId = memberId
                    });

                    context.Commit();
                }
                catch (Exception ex)
                {
                    _Logger.Error("PayWithPayPal", ex);
                    context.Complete();

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Booking.BookingString.PaymentError;
                }

                return RedirectToAction(MVC.Backoffice.Localisation.QuotationDetail(id));
            }
			
			var localisation = quotation.Offer.Localisation;

            string returnUrl = Url.ActionAbsolute(MVC.Backoffice.Localisation.QuotationDetail(id, true));
			string cancelUrl = Url.ActionAbsolute(MVC.Backoffice.Localisation.QuotationCancelled(id));
            string ipnUrl = Url.ActionAbsolute(MVC.Payment.PayPalInstantNotification());

            var paymentHandler = PaymentHandlerFactory.GetHandler(PaymentHandlerFactory.HandlerType.Quotation) as MemberQuotationPaymentHandler;

            decimal eworkyAmount = localisation.GetQuotationPrice();
            var payments = new List<PaymentItem>
            {
                new PaymentItem{  Index = 0, Amount = eworkyAmount, Email = PaymentConfiguration.Instance.PaypalMail},
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

            TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Booking.BookingString.PaymentError;

            return RedirectToAction(MVC.Home.Index());
        }
    }
}
