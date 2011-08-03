using System;
using WorkiSiteWeb.Infrastructure.Email;
using System.Net.Mail;
using System.Configuration;

namespace WorkiSiteWeb.Models
{
    public class EmailService : IEmailService
    {
        public static string ContactMail = ConfigurationManager.AppSettings["ContactMail"];
		public static string BookingMail = ConfigurationManager.AppSettings["BookingMail"];

        public string LastErrorMessage
        {
            get { throw new NotImplementedException(); }
        }

        public bool Send(string fromAddress, string fromName, string subject, string body, bool isHtml, string toAddress)
        {
            //SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            SmtpClient smtpClient = new SmtpClient();
            var mailMessage = new MailMessage();
            var from = new MailAddress(fromAddress, fromName);
            mailMessage.From = from;
            mailMessage.To.Add(toAddress);
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = isHtml;
            smtpClient.Send(mailMessage);
            return true;
        }

        public bool Send(string fromAddress, string fromName, string subject, string body, bool isHtml, string[] toAddresses)
        {
            throw new NotImplementedException();
        }
    }
}
