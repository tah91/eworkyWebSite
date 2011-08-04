using System;
using System.Linq;
using WorkiSiteWeb.Infrastructure.Repository;
using System.Collections.Generic;
using System.Data.Objects;
using WorkiSiteWeb.Infrastructure.Logging;
using System.Web.Security;
using WorkiSiteWeb.Helpers;

namespace WorkiSiteWeb.Models
{
	public class BookingRepository : IRepository<MemberBooking>
    {
		ILogger _Logger;

		public BookingRepository(ILogger logger)
		{
			_Logger = logger;
		}

        #region IRepository

		public MemberBooking Get(int key)
		{
			var db = new WorkiDBEntities();
			//using (var db = new WorkiDBEntities())
			{
				MemberBooking booking = db.MemberBookings.SingleOrDefault(b => b.Id == key);

				return booking;
			}
		}

		public void Add(MemberBooking toAdd)
        {
            using (var db = new WorkiDBEntities())
            {
				db.MemberBookings.AddObject(toAdd);
                db.SaveChanges();
            }
        }

        public void Delete(int key)
        {
            using (var db = new WorkiDBEntities())
            {
				MemberBooking booking = db.MemberBookings.SingleOrDefault(b => b.Id == key);
				if (booking == null)
                    return;
				db.MemberBookings.DeleteObject(booking);
                db.SaveChanges();
            }
        }

		public void Update(int id, Action<MemberBooking> actionToPerform)
        {
            using (var db = new WorkiDBEntities())
            {
				MemberBooking booking = db.MemberBookings.SingleOrDefault(b => b.Id == id);
				if (booking != null)
                {
					actionToPerform.Invoke(booking);
                    db.SaveChanges();
                }
            }
        }

		public IList<MemberBooking> GetAll()
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
				return db.MemberBookings.ToList();
            }
        }

		public IList<MemberBooking> Get(int start, int pageSize)
        {
            var db = new WorkiDBEntities();
            //using (var db = new WorkiDBEntities())
            {
				return db.MemberBookings.OrderByDescending(b => b.Id).Skip(start).Take(pageSize).ToList();
            }
        }

        public int GetCount()
        {
            using (var db = new WorkiDBEntities())
            {
				return db.MemberBookings.Count();
            }
        }

        #endregion
    }
}