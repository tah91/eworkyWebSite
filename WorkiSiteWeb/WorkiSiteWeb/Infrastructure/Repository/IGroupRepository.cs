using System;
using WorkiSiteWeb.Models;
using System.Linq;
using System.Collections.Generic;

namespace WorkiSiteWeb.Infrastructure.Repository
{
    public interface IGroupRepository : IRepository<Group>
    {
        void AddMembersInGroup(IEnumerable<MembersInGroup> membersInGroup);
        void DeleteMembersInGroup(IEnumerable<MembersInGroup> membersInGroup);
        IList<MembersInGroup> GetAllMembersInGroups();
        IList<string> GetGroupsForUser(string username);
    }
}
