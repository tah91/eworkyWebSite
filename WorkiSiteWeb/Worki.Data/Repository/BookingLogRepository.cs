using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace Worki.Data.Models
{
    public interface IBookingLogRepository : IRepository<MemberBookingLog>
    {

    }

    public class BookingLogRepository : RepositoryBase<MemberBookingLog>, IBookingLogRepository
    {
        public BookingLogRepository(ILogger logger, IUnitOfWork context)
            : base(logger, context)
        {
        }
    }
}