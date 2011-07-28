using System;
using System.Linq;
using WorkiSiteWeb.Infrastructure.Repository;
using System.Collections.Generic;

namespace WorkiSiteWeb.Models
{
    public class WelcomePeopleRepository : IWelcomePeopleRepository
    {
        #region IWelcomePeopleRepository

        #endregion

        public WelcomePeople Get(int key)
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
                return db.WelcomePeoples.SingleOrDefault(d => d.Id == key);
            }
        }

        public void Add(WelcomePeople welcomePeople)
        {
            using (var db = new WorkiDBEntities())
            {
                db.WelcomePeoples.AddObject(welcomePeople);
                db.SaveChanges();
            }
        }

        public void Delete(int key)
        {
            using (var db = new WorkiDBEntities())
            {
                var welcomePeople = db.WelcomePeoples.SingleOrDefault(v => v.Id == key);
                if (welcomePeople == null)
                    return;
                db.WelcomePeoples.DeleteObject(welcomePeople);
                db.SaveChanges();
            }
        }

        public void Update(int key, Action<WelcomePeople> actionToPerform)
        {
            using (var db = new WorkiDBEntities())
            {
                var people = db.WelcomePeoples.SingleOrDefault(p => p.Id == key);
                if (people != null)
                {
                    actionToPerform.Invoke(people);
                    db.SaveChanges();
                }
            }
        }

        public IList<WelcomePeople> GetAll()
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
                return db.WelcomePeoples.ToList();
            }
        }

        public IList<WelcomePeople> Get(int start, int pageSize)
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
				return db.WelcomePeoples.OrderByDescending(v => v.Id).Skip(start).Take(pageSize).ToList();
            }
        }

        public int GetCount()
        {
            using (var db = new WorkiDBEntities())
            {
                return db.WelcomePeoples.Count();
            }
        }
    }
}