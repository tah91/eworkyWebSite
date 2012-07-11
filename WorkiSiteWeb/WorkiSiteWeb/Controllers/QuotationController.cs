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
	public partial class QuotationController : ControllerBase
	{
		#region Private

		IEmailService _EmailService;
        IMembershipService _MembershipService;
        IPaymentService _PaymentService;

		#endregion

        public QuotationController( ILogger logger,
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
        //[AcceptVerbs(HttpVerbs.Post), Authorize]
		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult Create(int id, MemberQuotationFormViewModel formData)
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
						formData.MemberQuotation.MemberId = memberId;
						formData.MemberQuotation.OfferId = id;
						formData.MemberQuotation.StatusId = (int)MemberQuotation.Status.Pending;

						//set phone number to the one from form
						member.MemberMainData.PhoneNumber = formData.PhoneNumber;
						member.MemberQuotations.Add(formData.MemberQuotation);

                        formData.MemberQuotation.MemberQuotationLogs.Add(new MemberQuotationLog
                        {
                            CreatedDate = DateTime.UtcNow,
                            Event = "Quotation Created",
                            EventType = (int)MemberQuotationLog.QuotationEvent.Creation,
							LoggerId = memberId
                        });

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

							newMemberMail = new Email(MVC.Emails.Views.Email);
							newMemberMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
							newMemberMail.To = formData.Email;
							newMemberMail.ToName = formData.FirstName;

							newMemberMail.Subject = Worki.Resources.Email.BookingString.QuotationNewMemberSubject;
                            newMemberMail.Content = string.Format(Worki.Resources.Email.BookingString.QuotationNewMemberBody,
                                                                    Localisation.GetOfferType(offer.Type),
                                                                    formData.MemberQuotation.Surface,
                                                                    offer.Localisation.Name,
                                                                    offer.Localisation.Adress,
                                                                    formData.Email,
                                                                    _MembershipService.GetPassword(formData.Email, null),
                                                                    editpasswordLink,
                                                                    editprofilLink);
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
														 formData.MemberQuotation.Message,
                                                         locUrl);

						//send mail to quoation client
                        dynamic clientMail = new Email(MVC.Emails.Views.Email);
                        clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                        clientMail.To = member.Email;
                        clientMail.Subject = Worki.Resources.Email.BookingString.CreateQuotationClientSubject;
                        clientMail.ToName = member.MemberMainData.FirstName;
                        clientMail.Content = string.Format(Worki.Resources.Email.BookingString.CreateQuotationClient,
                                                         Localisation.GetOfferType(offer.Type),
                                                         formData.MemberQuotation.Surface,
                                                         locName,
                                                         offer.Localisation.Adress);

						context.Commit();

						if (sendNewAccountMail)
						{
							newMemberMail.Send();
						}
						clientMail.Send();
						teamMail.Send();
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
                    UpdateModel(quotation);

                    context.Commit();
                }
                catch (Exception ex)
                {
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
            if ((first != null && first.Id == id) || quotation.Offer.Localisation.IsSharedOffice())
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
