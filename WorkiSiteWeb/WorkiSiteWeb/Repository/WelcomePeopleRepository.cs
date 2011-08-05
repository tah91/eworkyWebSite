using System;
using System.Linq;
using Worki.Web.Infrastructure.Repository;
using System.Collections.Generic;
using Worki.Web.Infrastructure.Logging;

namespace Worki.Web.Models
{
	public class WelcomePeopleRepository : RepositoryBase<WelcomePeople>, IWelcomePeopleRepository
    {
		public WelcomePeopleRepository(ILogger logger)
			: base(logger)
		{
		}

        #region IWelcomePeopleRepository

        #endregion
    }
}