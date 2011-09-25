using System;
using System.Linq;
using Worki.Infrastructure.Repository;
using System.Collections.Generic;
using Worki.Infrastructure.Logging;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;

namespace Worki.Data.Models
{
    public interface IPressRepository : IRepository<Press>
    {

    }

    public class PressRepository : RepositoryBase<Press>, IPressRepository
    {
		public PressRepository(ILogger logger, IUnitOfWork context)
            : base(logger,context)
        {
        }

        #region IPressRepository

        #endregion
    }
}
