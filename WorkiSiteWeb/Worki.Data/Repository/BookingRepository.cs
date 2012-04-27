using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace Worki.Data.Models
{
    public interface IProductRepository<T>
    {
        /// <summary>
        /// Get all the products of an owner
        /// </summary>
        /// <param name="id">owner id</param>
        /// <returns>list of the elements</returns>
        IList<T> GetOwnerProducts(int id);

        /// <summary>
        /// Get all the products of a localisation
        /// </summary>
        /// <param name="id">localisation id</param>
        /// <returns>list of the elements</returns>
        IList<T> GetLocalisationProducts(int id);
    }

    public interface IBookingRepository : IRepository<MemberBooking>, IProductRepository<MemberBooking>
	{

 	}

	public class BookingRepository : RepositoryBase<MemberBooking>, IBookingRepository
    {
		public BookingRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

        public IList<MemberBooking> GetOwnerProducts(int id)
        {
            return GetMany(q => q.Offer.Localisation.OwnerID == id && q.StatusId != (int)MemberQuotation.Status.Pending);
        }

        public IList<MemberBooking> GetLocalisationProducts(int id)
        {
            return GetMany(q => q.Offer.LocalisationId == id && q.StatusId != (int)MemberQuotation.Status.Pending);
        }
    }
}