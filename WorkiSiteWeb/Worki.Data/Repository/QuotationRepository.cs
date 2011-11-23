using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;

namespace Worki.Data.Models
{
	public interface IQuotationRepository : IRepository<MemberQuotation>
	{
        IList<MemberQuotation> GetLocalisationQuotations(int localisationId);
        IList<MemberQuotation> GetOwnerQuotations(int memberId);
	}

	public class QuotationRepository : RepositoryBase<MemberQuotation>, IQuotationRepository
	{
		public QuotationRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

        #region Quotation

        public IList<MemberQuotation> GetLocalisationQuotations(int id)
        {
            var quotations = from item in _Context.MemberQuotations where item.LocalisationId == id select item;
            return quotations.ToList();
        }

        public IList<MemberQuotation> GetOwnerQuotations(int id)
        {
            var quotations = from item in _Context.MemberQuotations where item.Offer.Localisation.OwnerID == id select item;
            return quotations.ToList();
        }

        #endregion
	}
}