using System;
using Worki.Web.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Worki.Web.Infrastructure.Repository
{
    public interface IVisitorRepository : IRepository<Visitor>
    {
        void ValidateVisitor(string email);
    }
}
