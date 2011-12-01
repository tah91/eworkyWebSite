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
		PaymentHandlerFactory.HandlerType GetHandlerType(string requestId);
    }

    public class TransactionRepository : RepositoryBase<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ILogger logger, IUnitOfWork context)
            : base(logger, context)
        {
        }

		public PaymentHandlerFactory.HandlerType GetHandlerType(string requestId)
		{
			var booking = _Context.Transactions.Where(t => t.RequestId == requestId).Count();
			if (booking != 0)
				return PaymentHandlerFactory.HandlerType.Booking;
			var quotation = _Context.MemberQuotationTransactions.Where(t => t.RequestId == requestId).Count();
			if (quotation != 0)
				return PaymentHandlerFactory.HandlerType.Quotation;

			return PaymentHandlerFactory.HandlerType.Unknown;
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