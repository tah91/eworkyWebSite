using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Service;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;

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
        public virtual ActionResult PayWithPayPal(int memberBookingId)
        {
            string returnUrl = Url.ActionAbsolute(MVC.Payment.PayPalAccepted(memberBookingId)) + "/" + memberBookingId.ToString();
            string cancelUrl = Url.ActionAbsolute(MVC.Payment.PayPalCancelled(memberBookingId)) + "/" + memberBookingId.ToString();
            string ipnUrl = Url.ActionAbsolute(MVC.Payment.PayPalInstantNotification());

            string paypalApprovalUrl = _PaymentService.PayWithPayPal(1000, 10,
                                            returnUrl, 
                                            cancelUrl, 
                                            ipnUrl,
                                            "ulysse_1321956805_per@hotmail.com",
                                            "ulysse_1321039676_per@hotmail.com",
                                            "ulysse_1321039527_biz@hotmail.com");

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
        public virtual ActionResult PayPalInstantNotification()
        {
            List<string> errors = _PaymentService.ProcessPaypalIPNMessage(Request);

            
            

            return View();
        }


        


    }
}
