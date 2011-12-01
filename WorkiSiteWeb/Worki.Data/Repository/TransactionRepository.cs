using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace Worki.Data.Models
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        
    }

    public class TransactionRepository : RepositoryBase<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ILogger logger, IUnitOfWork context)
            : base(logger, context)
        {
        }
    }

	public interface IQuotationTransactionRepository : IRepository<MemberQuotationTransaction>
	{

	}

	public class QuotationTransactionRepository : RepositoryBase<MemberQuotationTransaction>, IQuotationTransactionRepository
	{
		public QuotationTransactionRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}
	}
}