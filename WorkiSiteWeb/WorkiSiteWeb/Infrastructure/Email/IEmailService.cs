using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Worki.Web.Infrastructure.Email
{
    public interface IEmailService
    {
        string LastErrorMessage { get; }
        bool Send(string fromAddress, string fromName, string subject, string body, bool isHtml, string toAddress);
        bool Send(string fromAddress, string fromName, string subject, string body, bool isHtml, string[] toAddresses);
    }
}
