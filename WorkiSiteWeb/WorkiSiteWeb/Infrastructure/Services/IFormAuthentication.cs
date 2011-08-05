using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Worki.Web.Infrastructure.Membership
{
    public interface IFormsAuthenticationService
    {
        void SignIn(string userName, string userData, bool createPersistentCookie, HttpResponseBase response);
        void SignOut();
    }
}
