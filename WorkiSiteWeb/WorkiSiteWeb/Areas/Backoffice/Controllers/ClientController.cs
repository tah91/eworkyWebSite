﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;
using Worki.Web.Helpers;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;

namespace Worki.Web.Areas.Backoffice.Controllers
{
	public partial class ClientController : BackofficeControllerBase
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
        /// <returns>View containing the clients</returns>
        public virtual ActionResult List(int? page)
        {
            var id = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var p = page ?? 1;
            try
            {
                var member = mRepo.Get(id);
                Member.Validate(member);

                var clients = mRepo.GetClients(id);
                var model = new PagingList<Member>
                {
                    List = clients.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
                    PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = clients.Count }
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _Logger.Error("List", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

        /// <summary>
        /// Get action method to add a client
        /// </summary>
        /// <returns>View containing the client data</returns>
        public virtual ActionResult Add()
        {
            return null;
        }

    }
}
