using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace Worki.Infrastructure.Email
{
    public class EmailContentModel
    {
        public string ToName { get; set; }
        public string Content { get; set; }
    }

    public interface IEmailService
    {
        Object PrepareMessage(MailAddress from, IEnumerable<MailAddress> to, string subject, string body);
        Object PrepareMessage(MailAddress from, MailAddress to, string subject, string body);
        Object PrepareMessageFromDefault(MailAddress to, string subject, string body);
        Object PrepareMessageToDefault(MailAddress from, string subject, string body);
        bool Deliver(Object message);
    }
}
