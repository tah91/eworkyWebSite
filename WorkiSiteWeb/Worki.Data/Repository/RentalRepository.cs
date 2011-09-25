using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;

namespace Worki.Data.Models
{
    public interface IRentalRepository : IRepository<Rental>
    {

    }

    public class RentalRepository : RepositoryBase<Rental>, IRentalRepository
    {
		public RentalRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}
    }
}