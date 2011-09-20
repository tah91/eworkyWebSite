using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;

namespace Worki.Data.Models
{
    public interface IRentalRepository : IRepository<Rental>
    {

    }

    public class RentalRepository : RepositoryBase<Rental>, IRentalRepository
    {
        public RentalRepository(ILogger logger)
            : base(logger)
        {
        }
    }
}