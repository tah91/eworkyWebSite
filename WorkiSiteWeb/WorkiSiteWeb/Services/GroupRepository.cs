using System.Collections.Generic;
using System.Linq;
using WorkiSiteWeb.Infrastructure.Repository;

namespace WorkiSiteWeb.Models
{
    public class GroupRepository : IGroupRepository
    {
        #region Private

        //WorkiDBEntities db = new WorkiDBEntities();

        #endregion

        #region IRepository

        public Group Get(string key)
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
                Group g = (from groups in db.Groups
                           where groups.Title == key
                           select groups).SingleOrDefault();

                return g;
            }
        }

        public void Add(Group toAdd)
        {
            using (var db = new WorkiDBEntities())
            {
                db.Groups.AddObject(toAdd);
                db.SaveChanges();
            }
        }

        public void Delete(string key)
        {
            using (var db = new WorkiDBEntities())
            {
                Group toDelete = (from groups in db.Groups where groups.Title == key select groups).SingleOrDefault();
                if (toDelete == null)
                    return;
                db.Groups.DeleteObject(toDelete);
                db.SaveChanges();
            }
        }

        public void Update(string key, System.Action<Group> actionToPerform)
        {
            throw new System.NotImplementedException();
        }

        public IList<Group> GetAll()
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
                return db.Groups.ToList();
            }
        }

        public IList<Group> Get(int start, int pageSize)
        {
            throw new System.NotImplementedException();
        }

        public int GetCount()
        {
            using (var db = new WorkiDBEntities())
            {
                return db.Groups.Count();
            }
        }

        #endregion

        #region IGroupRepository

        public void AddMembersInGroup(IEnumerable<MembersInGroup> membersInGroup)
        {
            using (var db = new WorkiDBEntities())
            {
                foreach (var item in membersInGroup)
                {
                    db.MembersInGroups.AddObject(item);
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
                    db.MembersInGroups.DeleteObject(item);
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

        public Group GetGroup(int id)
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
                Group g = (from groups in db.Groups
                           where groups.GroupId == id
                           select groups).SingleOrDefault();

                return g;
            }
        }


        #endregion

        //public void Save()
        //{
        //    db.SaveChanges();
        //}
    }
}