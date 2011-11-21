using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;
using Worki.Web.Helpers;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;

namespace Worki.Web.Areas.Backoffice.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    [Authorize]
    public partial class ClientController : Controller
    {
        ILogger _Logger;

        public ClientController(ILogger logger)
        {
            _Logger = logger;
        }

        public const int PageSize = 5;

        /// <summary>
        /// Get action method to show clients of the owner
        /// </summary>
        /// <returns>View containing the bookings</returns>
        public virtual ActionResult Clients(int? page)
        {
            var id = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var p = page ?? 1;
            try
            {
                var member = mRepo.Get(id);
                Member.Validate(member);
                var clientIds = member.GetClients();
                var skipped = clientIds.Skip((p - 1) * PageSize).Take(PageSize);

                var clients = mRepo.GetMany(m => skipped.Contains(m.MemberId));
                var model = new PagingList<Member>
                {
                    List = clients,
                    PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = clientIds.Count }
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _Logger.Error("Clients", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

    }
}
