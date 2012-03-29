using System;
using System.Linq;
using Worki.Infrastructure.Repository;
using System.Collections.Generic;
using Worki.Infrastructure.Logging;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;

namespace Worki.Data.Models
{
	public interface IInvoiceRepository : IRepository<Invoice>
	{
        IEnumerable<Invoice> GetInvoices(int id, MonthYear monthYear, out DateTime initial);
	}

    public class InvoiceRepository : RepositoryBase<Invoice>, IInvoiceRepository
    {
		public InvoiceRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
        }

        #region IInvoiceRepository

        #endregion

        public IEnumerable<Invoice> GetInvoices(int id, MonthYear monthYear, out DateTime initial)
        {
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(_Context);
            var localisation = lRepo.Get(id);

            //from booking
            var bookingInvoices = localisation.GetPaidBookings().Select(b => new Invoice(b));

            //from invoices
            var invoices = bookingInvoices.Concat(localisation.Invoices);
            
            //first item
            initial = invoices.Count() != 0 ? invoices.Where(b => b.CreationDate != DateTime.MinValue).Select(b => b.CreationDate).Min() : DateTime.Now;

            //filter and order by date
            return invoices.Where(b => monthYear.EqualDate(b.CreationDate)).OrderByDescending(mb => mb.CreationDate).ToList();
        }
    }
}