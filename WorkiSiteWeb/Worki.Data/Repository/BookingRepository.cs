using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace Worki.Data.Models
{
	public interface IBookingRepository : IRepository<MemberBooking>
	{

 	}

	public class BookingRepository : RepositoryBase<MemberBooking>, IBookingRepository
    {
		public BookingRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}
    }
}