using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;

namespace Worki.Data.Models
{
	public interface IBookingRepository : IRepository<MemberBooking>
	{

	}

	public class BookingRepository : RepositoryBase<MemberBooking>, IBookingRepository
    {
		public BookingRepository(ILogger logger)
			: base(logger)
		{
		}
    }
}