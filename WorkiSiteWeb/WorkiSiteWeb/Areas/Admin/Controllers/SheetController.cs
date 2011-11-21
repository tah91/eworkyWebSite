using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure.Repository;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;
using Worki.Data.Models;

namespace Worki.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    [RequireHttpsRemote]
	public partial class SheetController : Controller
    {
        ILogger _Logger;

        public SheetController(ILogger logger)
        {
            _Logger = logger;
        }

        #region Admin Localisation

        /// <summary>
        /// Prepares a web page containing a paginated list of localisations
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult Index(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var pageValue = page ?? 1;
            var localisations = lRepo.Get((pageValue - 1) * MiscHelpers.Constants.PageSize, MiscHelpers.Constants.PageSize, l => l.ID);
            var viewModel = new PagingList<Localisation>()
            {
                List = localisations,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = lRepo.GetCount()
                }
            };
            return View(viewModel);
        }

        public virtual ActionResult OnOffline(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var loc = lRepo.Get(id);
            try
            {
                if (loc == null)
                {
                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.WorkplaceNotFound;
                    return RedirectToAction(MVC.Admin.Sheet.Index());
                }
                if (loc.MainLocalisation != null)
                {
                    loc.MainLocalisation.IsOffline = !loc.MainLocalisation.IsOffline;
                    context.Commit();
                }
            }
            catch (Exception ex)
            {
                context.Complete();
                _Logger.Error("OnOffline", ex);
            }

            return RedirectToAction(MVC.Admin.Sheet.Index());
        }

        public virtual ActionResult UpdateMainLocalisation(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var loc = lRepo.Get(id);
            try
            {
                if (loc == null)
                {
                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.WorkplaceNotFound;
                    return RedirectToAction(MVC.Admin.Sheet.Index());
                }
                if (loc.MainLocalisation != null)
                {
                    loc.MainLocalisation.IsMain = !loc.MainLocalisation.IsMain;
                    context.Commit();
                }
            }
            catch (Exception ex)
            {
                context.Complete();
                _Logger.Error("UpdateMainLocalisation", ex);
            }

            return RedirectToAction(MVC.Admin.Sheet.Index());
        }

        /// <summary>
        /// GET Action result to delete a localisation
        /// if the id is in db, ask for confirmation to delete the localiosation
        /// </summary>
        /// <param name="id">The id of the localisation to delete</param>
        /// <returns>the confirmation view</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult DeleteLocalisation(int id, string returnUrl = null)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var localisation = lRepo.Get(id);
            if (localisation == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.WorkplaceNotFound;
                return RedirectToAction(MVC.Admin.Sheet.Index());
            }
            else
            {
                TempData["returnUrl"] = returnUrl;
                return View(localisation);
            }
        }

        /// <summary>
        /// POST Action result to delete a localisation
        /// remove localistion from db
        /// <param name="id">The id of the localisation to delete</param>
        /// </summary>
        /// <returns>the deletetion success view</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteLocalisation(int id, string confirmButton, string returnUrl)
        {
            var context = ModelFactory.GetUnitOfWork();
            try
            {
                var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
                var localisation = lRepo.Get(id);
                if (localisation == null)
                {
                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.WorkplaceNotFound;
                    return RedirectToAction(MVC.Admin.Sheet.Index());
                }
                lRepo.Delete(id);
                context.Commit();
            }
            catch (Exception ex)
            {
                _Logger.Error("Delete", ex);
                context.Complete();
            }

            TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.LocHaveBeenDel;

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction(MVC.Admin.Sheet.Index());
            else
                return Redirect(returnUrl);
        }

        #endregion 

        #region Admin Rental

        /// <summary>
        /// Prepares a web page containing a paginated list of localisations
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult IndexRental(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
            var pageValue = page ?? 1;
            var rentals = rRepo.Get((pageValue - 1) * MiscHelpers.Constants.PageSize, MiscHelpers.Constants.PageSize, r => r.Id);
            var viewModel = new PagingList<Rental>()
            {
                List = rentals,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = rRepo.GetCount()
                }
            };
            return View(viewModel);
        }

        /// <summary>
        /// GET Action result to delete a rental
        /// if the id is in db, ask for confirmation to delete the rental
        /// </summary>
        /// <param name="id">The id of the rental to delete</param>
        /// <returns>the confirmation view</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult DeleteRental(int id, string returnUrl = null)
        {
            var context = ModelFactory.GetUnitOfWork();
            var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
            var rental = rRepo.Get(id);
            if (rental == null)
                return View(MVC.Shared.Views.Error);
            else
            {
                TempData["returnUrl"] = returnUrl;
                return View(rental);
            }
        }

        /// <summary>
        /// POST Action result to delete a rental
        /// remove rental from db
        /// <param name="id">The id of the rental to delete</param>
        /// </summary>
        /// <returns>the deletetion success view</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteRental(int id, string confirm, string returnUrl)
        {
            var context = ModelFactory.GetUnitOfWork();
            var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
            try
            {
                var rental = rRepo.Get(id);
                if (rental == null)
                    return View(MVC.Shared.Views.Error);
                rRepo.Delete(id);
                context.Commit();
            }
            catch (Exception ex)
            {
                _Logger.Error("Delete", ex);
                context.Complete();
            }

            TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Rental.RentalString.RentalHaveBeenDel;

            return RedirectToAction(MVC.Admin.Sheet.IndexRental());
            //return Redirect(returnUrl);
        }

        #endregion
    }
}
