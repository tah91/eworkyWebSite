using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;
using System.Collections.Generic;

namespace Worki.Data.Models
{
    public interface IRentalRepository : IRepository<Rental>
    {
        IList<Rental> FindByCriteria(RentalSearchCriteria criteria);
    }

    public class RentalRepository : RepositoryBase<Rental>, IRentalRepository
    {
		public RentalRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

        public IList<Rental> FindByCriteria(RentalSearchCriteria criteria)
        {
            return null;
        }
    }
}