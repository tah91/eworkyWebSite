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

		[AcceptVerbs(HttpVerbs.Post)]
		[ActionName("paypalnotification")]
		public virtual ActionResult PayPalInstantNotification()
		{
			string status = string.Empty;
			string requestId = string.Empty;

			List<string> errors = _PaymentService.ProcessPaypalIPNMessage(Request, out status, out requestId);
			switch (status)
			{
				case "COMPLETED":
					{
						var context = ModelFactory.GetUnitOfWork();
						var tRepo = ModelFactory.GetRepository<ITransactionRepository>(context);

						var type = tRepo.GetHandlerType(requestId);
						var paymentHandler = PaymentHandlerFactory.GetHandler(type);
                        paymentHandler.PaymentCompleted(requestId);

						Response.Clear();
						Response.ClearHeaders();
						Response.StatusCode = 200;
						Response.StatusDescription = "OK";
						Response.Flush();
						Response.End();

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
