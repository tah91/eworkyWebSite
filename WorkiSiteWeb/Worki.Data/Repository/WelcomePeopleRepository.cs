using System;
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
        WelcomePeople GetWelcomePeople(string key);
	}

	public class WelcomePeopleRepository : RepositoryBase<WelcomePeople>, IWelcomePeopleRepository
    {
		public WelcomePeopleRepository(ILogger logger, IUnitOfWork context)
			: base(logger, context)
		{
		}

        public WelcomePeople GetWelcomePeople(string key)
        {
            WelcomePeople member = _Context.WelcomePeoples.SingleOrDefault(m => m.Member.Username == key);

            return member;
        }

        #region IWelcomePeopleRepository

        #endregion
    }
}