using System;
using System.Linq;
using WorkiSiteWeb.Infrastructure.Repository;
using System.Collections.Generic;
using System.Data.Objects;
using WorkiSiteWeb.Infrastructure.Logging;
using System.Web.Security;
using WorkiSiteWeb.Helpers;

namespace WorkiSiteWeb.Models
{
	public class BookingRepository : RepositoryBase<MemberBooking>, IBookingRepository
    {
		public BookingRepository(ILogger logger)
			: base(logger)
		{
		}
    }
}