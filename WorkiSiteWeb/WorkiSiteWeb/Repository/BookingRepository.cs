using System;
using System.Linq;
using Worki.Web.Infrastructure.Repository;
using System.Collections.Generic;
using System.Data.Objects;
using Worki.Web.Infrastructure.Logging;
using System.Web.Security;
using Worki.Web.Helpers;

namespace Worki.Web.Models
{
	public class BookingRepository : RepositoryBase<MemberBooking>, IBookingRepository
    {
		public BookingRepository(ILogger logger)
			: base(logger)
		{
		}
    }
}