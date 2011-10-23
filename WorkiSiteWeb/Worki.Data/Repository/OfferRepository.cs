using System;
using System.Linq;
using Worki.Infrastructure.Repository;
using System.Collections.Generic;
using Worki.Infrastructure.Logging;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;

namespace Worki.Data.Models
{
    public interface IOfferRepository : IRepository<Offer>
    {

    }

	public class OfferRepository : RepositoryBase<Offer>, IOfferRepository
    {
		public OfferRepository(ILogger logger, IUnitOfWork context)
            : base(logger,context)
        {
		}

		#region IOfferRepository

		#endregion
	}
}
