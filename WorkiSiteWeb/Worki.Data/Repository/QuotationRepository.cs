using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;

namespace Worki.Data.Models
{
	public interface IQuotationRepository : IRepository<MemberQuotation>
	{

	}

	public class QuotationRepository : RepositoryBase<MemberQuotation>, IQuotationRepository
	{
		public QuotationRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}
	}
}