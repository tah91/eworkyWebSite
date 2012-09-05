using System;
using System.Collections.Generic;
using System.Net.Mail;
using Worki.Infrastructure.Email;
using Worki.Infrastructure.Helpers;
using System.Linq;
using SendGridMail;
using SendGridMail.Transport;
using System.Net;


namespace Worki.Service
{
    public class SendGridEmailService : IEmailService
    {
        public Object PrepareMessage(MailAddress from, IEnumerable<MailAddress> to, string subject, string body)
        {
            //create a new message object
            var message = SendGrid.GenerateInstance();

            //set the message recipients
            foreach (var recipient in to)
            {
                message.AddTo(recipient.Address);
            }

            //set the sender
            message.From = from;

            //set the message body
            message.Html = body;

            //set the message subject
            message.Subject = subject;

            message.InitializeFilters();
            // true indicates that links in plain text portions of the email 
            // should also be overwritten for link tracking purposes. 
            message.EnableClickTracking(true);
            message.EnableOpenTracking();

            return message;
        }

        public Object PrepareMessage(MailAddress from, MailAddress to, string subject, string body)
        {
            return PrepareMessage(from, new List<MailAddress> { to }, subject, body);
        }

        public Object PrepareMessageFromDefault(MailAddress to, string subject, string body)
        {
            return PrepareMessage(new MailAddress(MiscHelpers.EmailConstants.ContactMail, MiscHelpers.EmailConstants.ContactDisplayName), to, subject, body);
        }

        public Object PrepareMessageToDefault(MailAddress from, string subject, string body)
        {
            return PrepareMessage(from, new MailAddress(MiscHelpers.EmailConstants.ContactMail, MiscHelpers.EmailConstants.ContactDisplayName), subject, body);
        }

        public bool Deliver(Object message)
        {
            // Create credentials, specifying your user name and password.
            var credentials = new NetworkCredential("tah91", "eWorky_2011");

            // Create an SMTP transport for sending email.
            var transportSMTP = SMTP.GenerateInstance(credentials);

            // Send the email.
            transportSMTP.Deliver((SendGrid)message);
            return true;
        }
    }
}
