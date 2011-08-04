using System;
using System.Linq;
using WorkiSiteWeb.Infrastructure.Repository;
using System.Collections.Generic;
using WorkiSiteWeb.Infrastructure.Logging;

namespace WorkiSiteWeb.Models
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