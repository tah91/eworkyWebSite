using System;
using System.Linq;
using WorkiSiteWeb.Infrastructure.Repository;
using System.Collections.Generic;

namespace WorkiSiteWeb.Models
{
    public class VisitorRepository : IVisitorRepository
    {
        #region Private

        //WorkiDBEntities db = new WorkiDBEntities();

        #endregion

        #region IVisitorRepository

        public Visitor GetVisitor(string email)
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
                var visitor = (from item
                                       in db.Visitors
                               where string.Compare(item.Email, email, StringComparison.InvariantCultureIgnoreCase) == 0
                               select item).FirstOrDefault();

                return visitor;
            }
        }

        public void ValidateVisitor(string email)
        {
            using (var db = new WorkiDBEntities())
            {
                var visitors = (from item in db.Visitors where string.Compare(item.Email, email, StringComparison.InvariantCultureIgnoreCase) == 0 select item);
                foreach (var item in visitors)
                {
                    item.IsValid = true;
                }
                db.SaveChanges();
            }
        }

		public void DeleteVisitor(string email)
		{
			using (var db = new WorkiDBEntities())
			{
				var visitors = (from item in db.Visitors where string.Compare(item.Email, email, StringComparison.InvariantCultureIgnoreCase) == 0 select item);
				foreach (var item in visitors)
				{
					db.Visitors.DeleteObject(item);
				}
				db.SaveChanges();
			}
		}

        #endregion

        public Visitor Get(int key)
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
                return db.Visitors.SingleOrDefault(d => d.Id == key);
            }
        }

        public void Add(Visitor visitor)
        {
            using (var db = new WorkiDBEntities())
            {
                db.Visitors.AddObject(visitor);
                db.SaveChanges();
            }
        }

        public void Delete(int key)
        {
            using (var db = new WorkiDBEntities())
            {
                var visitor = db.Visitors.SingleOrDefault(v => v.Id == key);
                if (visitor == null)
                    return;
                db.Visitors.DeleteObject(visitor);
                db.SaveChanges();
            }
        }

        public void Update(int key, Action<Visitor> actionToPerform)
        {
            throw new NotImplementedException();
        }

        public IList<Visitor> GetAll()
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
                return db.Visitors.ToList();
            }
        }

        public IList<Visitor> Get(int start, int pageSize)
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
				return db.Visitors.OrderByDescending(v => v.Id).Skip(start).Take(pageSize).ToList();
            }
        }

        public int GetCount()
        {
            using (var db = new WorkiDBEntities())
            {
                return db.Visitors.Count();
            }
        }
    }
}