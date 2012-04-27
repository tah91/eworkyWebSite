using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace Worki.Data.Models
{
    public interface IQuotationRepository : IRepository<MemberQuotation>, IProductRepository<MemberQuotation>
	{

	}

	public class QuotationRepository : RepositoryBase<MemberQuotation>, IQuotationRepository
	{
		public QuotationRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

        public IList<MemberQuotation> GetOwnerProducts(int id)
        {
            return GetMany(b => b.Offer.Localisation.OwnerID == id);
        }

        public IList<MemberQuotation> GetLocalisationProducts(int id)
        {
            return GetMany(b => b.Offer.LocalisationId == id);
        }
	}
}