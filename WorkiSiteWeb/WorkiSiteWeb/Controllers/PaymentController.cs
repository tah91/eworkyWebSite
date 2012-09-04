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
using Worki.Infrastructure.Email;
using Worki.Infrastructure.Helpers;
using System.Text;
using Worki.Section;
using System.Net.Mail;
using Worki.Web.Helpers;

namespace Worki.Web.Controllers
{
	public partial class PaymentController : ControllerBase
    {
        IPaymentService _PaymentService;

        public PaymentController(ILogger logger, IObjectStore objectStore, IEmailService emailService, IPaymentService paymentService)
            : base(logger, objectStore, emailService)
        {
            this._PaymentService = paymentService;
        }

		[AcceptVerbs(HttpVerbs.Post)]
		[ActionName("paypalnotification")]
		public virtual ActionResult PayPalInstantNotification()
		{
			string status = string.Empty;
			string requestId = string.Empty;

            List<string> errors = _PaymentService.ProcessPaypalIPNMessage(Request, out status, out requestId, PaymentConfiguration.Constants);
			switch (status)
			{
				case "COMPLETED":
					{
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
                        var strBuilder = new StringBuilder();
                        foreach (var item in errors)
                        {
                            strBuilder.AppendLine(item);
                        }
                        var adminMailContent = strBuilder.ToString();

                        var adminMail = _EmailService.PrepareMessageFromDefault(new MailAddress(MiscHelpers.AdminConstants.AdminMail, MiscHelpers.EmailConstants.ContactDisplayName),
                            status,
                            WebHelper.RenderEmailToString(MiscHelpers.AdminConstants.AdminMail, adminMailContent));

                        _EmailService.Deliver(adminMail);

						break;
					}
			}

			return View();
		}
    }
}
