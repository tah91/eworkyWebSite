using System;
using System.Collections.Generic;
using System.Net.Mail;
using Worki.Infrastructure.Email;
using Worki.Infrastructure.Helpers;
using System.Linq;

namespace Worki.Service
{
    public class EmailService : IEmailService
    {
        public Object PrepareMessage(MailAddress from, IEnumerable<MailAddress> to, string subject, string body)
        {
            SmtpClient smtpClient = new SmtpClient();
            var mailMessage = new MailMessage();
            mailMessage.From = from;
            foreach (var item in to)
            {
                mailMessage.To.Add(item);
            }

            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;

            return mailMessage;
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
            SmtpClient smtpClient = new SmtpClient();
            var mailMessage = (MailMessage)message;
            smtpClient.Send(mailMessage);
            return true;
        }
    }
}
