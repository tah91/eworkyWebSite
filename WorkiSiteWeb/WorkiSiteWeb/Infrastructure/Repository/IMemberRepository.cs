using System;
using WorkiSiteWeb.Models;
using System.Linq;
using System.Collections.Generic;

namespace WorkiSiteWeb.Infrastructure.Repository
{
    public interface IMemberRepository : IRepository<Member>
    {
        Member GetMember(string key);
        string GetUserName(string email);
        bool ActivateMember(string username, string key);
    }
}
