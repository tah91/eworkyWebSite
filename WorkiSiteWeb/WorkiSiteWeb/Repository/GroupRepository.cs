using System.Collections.Generic;
using System.Linq;
using WorkiSiteWeb.Infrastructure.Repository;
using WorkiSiteWeb.Infrastructure.Logging;

namespace WorkiSiteWeb.Models
{
    public class GroupRepository : RepositoryBase<Group>, IGroupRepository
    {
        #region Private

        //WorkiDBEntities db = new WorkiDBEntities();

        #endregion

		public GroupRepository(ILogger logger)
			: base(logger)
		{
		}

        #region IGroupRepository

        public void AddMembersInGroup(IEnumerable<MembersInGroup> membersInGroup)
        {
            using (var db = new WorkiDBEntities())
            {
                foreach (var item in membersInGroup)
                {
                    db.MembersInGroups.Add(item);
                }
                db.SaveChanges();
            }
        }

        public void DeleteMembersInGroup(IEnumerable<MembersInGroup> membersInGroup)
        {
            using (var db = new WorkiDBEntities())
            {	
				var ids = from item in membersInGroup select item.RelationId;
				var toDelete = db.MembersInGroups.Where(mig => ids.Contains(mig.RelationId));
				foreach (var item in toDelete)
                {
                    db.MembersInGroups.Remove(item);
                }
                db.SaveChanges();
            }
        }

        public IList<MembersInGroup> GetAllMembersInGroups()
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
                return db.MembersInGroups.ToList();
            }
        }

        public IList<string> GetGroupsForUser(string username)
        {
            using (var db = new WorkiDBEntities())
            {
                return (from mg in db.MembersInGroups where mg.Member.Username == username select mg.Group.Title).ToList();
            }
        }

        #endregion
    }
}