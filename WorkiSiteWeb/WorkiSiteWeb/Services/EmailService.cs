﻿using System;
using WorkiSiteWeb.Infrastructure.Email;
using System.Net.Mail;

namespace WorkiSiteWeb.Models
{
    public class EmailService : IEmailService
    {
        public const string ContactMail = "contact@eworky.com";

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
