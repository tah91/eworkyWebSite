﻿using System;
using System.Linq;
using Worki.Infrastructure.Repository;
using System.Collections.Generic;
using Worki.Infrastructure.Logging;
using Worki.Data.Repository;
using Worki.Infrastructure.UnitOfWork;

namespace Worki.Data.Models
{
	public interface IWelcomePeopleRepository : IRepository<WelcomePeople>
	{
	}

	public class WelcomePeopleRepository : RepositoryBase<WelcomePeople>, IWelcomePeopleRepository
    {
		public WelcomePeopleRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

        #region IWelcomePeopleRepository

        #endregion
    }
}