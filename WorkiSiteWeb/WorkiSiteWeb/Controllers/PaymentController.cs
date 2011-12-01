using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Service;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;
using Postal;
using Worki.Infrastructure.Helpers;
using System.Text;

namespace Worki.Web.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    public partial class PaymentController : Controller
    {
        ILogger _Logger;
        IPaymentService _PaymentService;

        public PaymentController(ILogger logger, IPaymentService paymentService)
        {
            this._Logger = logger;
            this._PaymentService = paymentService;
        }

        
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("paywithpaypal")]
        public virtual ActionResult PayWithPayPal(int id)
        {
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);

			var booking = bRepo.Get(id);

			string returnUrl = Url.ActionAbsolute(MVC.Dashboard.Home.BookingAccepted(id));
			string cancelUrl = Url.ActionAbsolute(MVC.Dashboard.Home.BookingCancelled(id));
			//string returnUrl = Url.ActionAbsolute(MVC.Payment.PayPalAccepted(memberBookingId));
			//string cancelUrl = Url.ActionAbsolute(MVC.Payment.PayPalCancelled(memberBookingId));
            string ipnUrl = Url.ActionAbsolute(MVC.Payment.PayPalInstantNotification());

			double ownerAmount, eworkyAmount;
            var paymentHandler = PaymentHandlerFactory.GetHandler(PaymentHandlerFactory.HandlerType.Booking) as MemberBookingPaymentHandler;
            paymentHandler.GetAmounts(booking.Price, out ownerAmount, out eworkyAmount);
            string paypalApprovalUrl = _PaymentService.PayWithPayPal(id,
																	ownerAmount,
																	eworkyAmount,
																	returnUrl, 
																	cancelUrl, 
																	ipnUrl,
																	"",
																	"t.ifti_1322172136_biz@hotmail.fr",
																	"t.ifti_1322171616_biz@hotmail.fr",
                                                                    paymentHandler);

            if (paypalApprovalUrl != null)
            {
                return Redirect(paypalApprovalUrl);
            }

			TempData[MiscHelpers.TempDataConstants.Info] = "Une erreur emepeche le paymenent, veuillez nous contacter à support@eworky.com";

			return RedirectToAction(MVC.Home.Index());
        }

		[AcceptVerbs(HttpVerbs.Post)]
		[ActionName("paypalnotification")]
		public virtual ActionResult PayPalInstantNotification()
		{
			string status = string.Empty;
			string requestId = string.Empty;
			var context = ModelFactory.GetUnitOfWork();
			var tRepo = ModelFactory.GetRepository<ITransactionRepository>(context);

			List<string> errors = _PaymentService.ProcessPaypalIPNMessage(Request, out status, out requestId);
			switch (status)
			{
				case "COMPLETED":
					{
						var transaction = tRepo.Get(trx => trx.RequestId == requestId);
						var booking = transaction.MemberBooking;
						var offer = booking.Offer;
						var localisation = offer.Localisation;

						//send mail to owner 
						dynamic ownerMail = new Email(MVC.Emails.Views.Email);
						ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
						ownerMail.To = booking.Owner.Email;
						ownerMail.Subject = Worki.Resources.Email.BookingString.PayementSubject;
						ownerMail.ToName = booking.Owner.MemberMainData.FirstName;
						ownerMail.Content = string.Format(Worki.Resources.Email.BookingString.PayementOwner,
														Localisation.GetOfferType(offer.Type),
														CultureHelpers.GetSpecificFormat(booking.FromDate, CultureHelpers.TimeFormat.Date),
														CultureHelpers.GetSpecificFormat(booking.ToDate, CultureHelpers.TimeFormat.Date),
														localisation.Name,
														localisation.Adress);
						ownerMail.Send();

						//send mail to client 
						dynamic clientMail = new Email(MVC.Emails.Views.Email);
						clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
						clientMail.To = booking.Client.Email;
						clientMail.Subject = Worki.Resources.Email.BookingString.PayementSubject;
						clientMail.ToName = booking.Client.MemberMainData.FirstName;
						clientMail.Content = string.Format(Worki.Resources.Email.BookingString.PayementClient,
														Localisation.GetOfferType(offer.Type),
														CultureHelpers.GetSpecificFormat(booking.FromDate, CultureHelpers.TimeFormat.Date),
														CultureHelpers.GetSpecificFormat(booking.ToDate, CultureHelpers.TimeFormat.Date),
														localisation.Name,
														localisation.Adress);
						clientMail.Send();

						Response.Clear();
						Response.ClearHeaders();
						Response.StatusCode = 200;
						Response.StatusDescription = "OK";
						Response.Flush();
						Response.End();

						try
						{
							booking.MemberBookingLogs.Add(new MemberBookingLog
							{
								CreatedDate = DateTime.UtcNow,
								Event = "Payment completed",
								EventType = (int)MemberBookingLog.BookingEvent.Payment
							});

							context.Commit();
						}
						catch (Exception ex)
						{
							context.Complete();
							_Logger.Error("PayPalInstantNotification", ex);
						}
						break;
					}
				default:
					{
						//send mail to admin 
						dynamic adminMail = new Email(MVC.Emails.Views.Email);
						adminMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
						adminMail.To = MiscHelpers.AdminConstants.AdminMail;
						adminMail.Subject = status;
						adminMail.ToName = MiscHelpers.EmailConstants.ContactDisplayName;
						var strBuilder = new StringBuilder();
						foreach (var item in errors)
						{
							strBuilder.AppendLine(item);
						}
						adminMail.Content = strBuilder.ToString();
						adminMail.Send();

						break;
					}
			}


			return View();
		}
    }
}
