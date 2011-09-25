using System;
using System.Linq;
using Worki.Infrastructure.Repository;
using System.Collections.Generic;
using Worki.Infrastructure.Logging;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;

namespace Worki.Data.Models
{
	public interface IVisitorRepository : IRepository<Visitor>
	{
		void ValidateVisitor(string email);
	}

	public class VisitorRepository : RepositoryBase<Visitor>, IVisitorRepository
	{
		public VisitorRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

		#region IVisitorRepository

		public void ValidateVisitor(string email)
		{
			var visitors = (from item in _Context.Visitors where string.Compare(item.Email, email, StringComparison.InvariantCultureIgnoreCase) == 0 select item);
			foreach (var item in visitors)
			{
				item.IsValid = true;
			}
			//db.SaveChanges();
		}

		#endregion
	}
}