using System;
using WorkiSiteWeb.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WorkiSiteWeb.Infrastructure.Repository
{
    public interface IVisitorRepository : IRepository<Visitor, int>
    {
        Visitor GetVisitor(string email);
        void ValidateVisitor(string email);
		void DeleteVisitor(string email);
    }
}
