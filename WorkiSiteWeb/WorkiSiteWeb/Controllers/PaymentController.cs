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

			string returnUrl = Url.ActionAbsolute(MVC.Dashboard.Home.Booking());
			string cancelUrl = Url.ActionAbsolute(MVC.Dashboard.Home.Booking());
			//string returnUrl = Url.ActionAbsolute(MVC.Payment.PayPalAccepted(memberBookingId));
			//string cancelUrl = Url.ActionAbsolute(MVC.Payment.PayPalCancelled(memberBookingId));
            string ipnUrl = Url.ActionAbsolute(MVC.Payment.PayPalInstantNotification());

            string paypalApprovalUrl = _PaymentService.PayWithPayPal(id, 
																	booking.Price,
																	booking.Price,
																	returnUrl, 
																	cancelUrl, 
																	ipnUrl,
																	"",
																	"t.ifti_1322172136_biz@hotmail.fr",
																	"t.ifti_1322171616_biz@hotmail.fr");

            if (paypalApprovalUrl != null)
            {
                return Redirect(paypalApprovalUrl);
            }

            // TODO MQP Afficher le message d'erreur
            return Redirect("/");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("paypalaccepted")]
        public virtual ActionResult PayPalAccepted(int memberBookingId)
        {


            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("paypalcancelled")]
        public virtual ActionResult PayPalCancelled(int memberBookingId)
        {


            return View();
        }


        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("paypalnotification")]
        [HttpPost] 
        public virtual ActionResult PayPalInstantNotification()
        {
			string status = string.Empty;
			string requestId = string.Empty;
			var context = ModelFactory.GetUnitOfWork();
			var tRepo = ModelFactory.GetRepository<ITransactionRepository>(context);

            List<string> errors = _PaymentService.ProcessPaypalIPNMessage(Request, out status, out requestId);
			if (!string.IsNullOrEmpty(status))
			{
				switch (status)
				{
					case "COMPLETED":
						{
							var transaction = tRepo.Get(trx => trx.RequestId == requestId);
							var booking = transaction.MemberBooking;
							var offer = booking.Offer;
							var localisation = offer.Localisation;
							var client = booking.Member;
							var owner = transaction.Member;

							//send mail to owner 
							dynamic ownerMail = new Email(MVC.Emails.Views.Email);
							ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
							ownerMail.To = client.Email;
							ownerMail.Subject = Worki.Resources.Email.BookingString.BookingMailSubject;
							ownerMail.ToName = MiscHelpers.EmailConstants.ContactDisplayName;
							ownerMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingMailBody,
															 string.Format("{0} {1}", client.MemberMainData.FirstName, client.MemberMainData.LastName),
															 client.MemberMainData.PhoneNumber,
															 client.Email,
															 localisation.Name,
															 Localisation.GetOfferType(offer.Type),
															 string.Format("{0:dd/MM/yyyy HH:MM}", booking.FromDate),
															 string.Format("{0:dd/MM/yyyy HH:MM}", booking.ToDate),
															 booking.Message);
							ownerMail.Send();

							//send mail to client 
							dynamic clientMail = new Email(MVC.Emails.Views.Email);
							clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
							clientMail.To = client.Email;
							clientMail.Subject = Worki.Resources.Email.BookingString.BookingMailSubject;
							clientMail.ToName = MiscHelpers.EmailConstants.ContactDisplayName;
							clientMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingMailBody,
															 string.Format("{0} {1}", owner.MemberMainData.FirstName, owner.MemberMainData.LastName),
															 owner.MemberMainData.PhoneNumber,
															 owner.Email,
															 localisation.Name,
															 Localisation.GetOfferType(offer.Type),
															 string.Format("{0:dd/MM/yyyy HH:MM}", booking.FromDate),
															 string.Format("{0:dd/MM/yyyy HH:MM}", booking.ToDate),
															 booking.Message);
							clientMail.Send();

							break;
						}
					default:
						{
							//send mail to admin 
							dynamic adminMail = new Email(MVC.Emails.Views.Email);
							adminMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
							adminMail.To = MiscHelpers.AdminConstants.AdminMail;
							adminMail.Subject = Worki.Resources.Email.BookingString.BookingMailSubject;
							adminMail.ToName = MiscHelpers.EmailConstants.ContactDisplayName;
							adminMail.Content = status;
							adminMail.Send();

							break;
						}
				}
			}

            return View();
        }
    }
}
