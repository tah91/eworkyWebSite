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
		IList<MemberBooking> GetLocalisationBookings(int localisationId);
		IList<MemberBooking> GetOwnerBookings(int memberId);
	}

	public class BookingRepository : RepositoryBase<MemberBooking>, IBookingRepository
    {
		public BookingRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

		#region Booking

		public IList<MemberBooking> GetLocalisationBookings(int id)
		{
			var bookings = from item in _Context.MemberBookings where item.LocalisationId == id select item;
			return bookings.ToList();
		}

		public IList<MemberBooking> GetOwnerBookings(int id)
		{
			var bookings = from item in _Context.MemberBookings where item.Offer.Localisation.OwnerID == id select item;
			return bookings.ToList();
		}

		#endregion
    }
}