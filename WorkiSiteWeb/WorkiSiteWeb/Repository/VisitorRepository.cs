﻿using System;
using System.Linq;
using WorkiSiteWeb.Infrastructure.Repository;
using System.Collections.Generic;
using WorkiSiteWeb.Infrastructure.Logging;

namespace WorkiSiteWeb.Models
{
	public class VisitorRepository : RepositoryBase<Visitor>, IVisitorRepository
    {
        #region Private

        //WorkiDBEntities db = new WorkiDBEntities();

        #endregion

		public VisitorRepository(ILogger logger)
			: base(logger)
		{
		}

        #region IVisitorRepository

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

        #endregion
    }
}