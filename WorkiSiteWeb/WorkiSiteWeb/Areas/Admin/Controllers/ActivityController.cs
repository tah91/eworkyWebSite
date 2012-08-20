using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure.Repository;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure;
using Worki.Data.Models;
using Worki.Web.Singletons;
using Worki.Web.Helpers;
using Worki.Service;

namespace Worki.Web.Areas.Admin.Controllers
{
	public partial class ActivityController : AdminControllerBase
    {
        ILogger _Logger;

        public ActivityController(ILogger logger)
        {
            _Logger = logger;
        }

        #region Admin WelcomePeople	

        /// <summary>
        /// Prepares a web page containing a paginated list of the people on home page
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult IndexWelcomePeople(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
            int pageValue = page ?? 1;
            var welcomPeople = wpRepo.Get((pageValue - 1) * MiscHelpers.Constants.PageSize, MiscHelpers.Constants.PageSize, wp => wp.Id);
            var viewModel = new PagingList<WelcomePeople>()
            {
                List = welcomPeople,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = wpRepo.GetCount()
                }
            };
            return View(viewModel);
        }

        public virtual ActionResult OnOffline(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var wRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
            var welcomeppl = wRepo.Get(id);
            try
            {
                if (welcomeppl == null)
                {
                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleNotFound;
                    return RedirectToAction(MVC.Admin.Activity.IndexWelcomePeople());
                }
                welcomeppl.Online = !welcomeppl.Online;
                context.Commit();
            }
            catch (Exception ex)
            {
                context.Complete();
                _Logger.Error("OnOffline", ex);
            }

            return RedirectToAction(MVC.Admin.Activity.IndexWelcomePeople());
        }

        /// <summary>
        /// Prepares a web page containing the details of a WelcomePeople
        /// </summary>
        /// <param name="page">id of the WelcomePeople</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult DetailWelcomePeople(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
            var item = wpRepo.Get(id);
            if (item == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleNotFound;
                return RedirectToAction(MVC.Admin.Activity.IndexWelcomePeople());
            }
            return View(item);
        }

        /// <summary>
        /// Prepares a web page containing the form to create a new WelcomePeople
        /// </summary>
        /// <returns>The action result.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult CreateWelcomePeople()
        {
            return View(new WelcomePeopleFormViewModel());
        }

        /// <summary>
        /// Add the welcomepeople from the form to the repository, then redirect to index
        /// </summary>
        /// <param name="welcomePeople">data from the form</param>
        /// <returns>redirect to index</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult CreateWelcomePeople(WelcomePeopleFormViewModel formModel)
        {
            if (ModelState.IsValid)
            {
                var context = ModelFactory.GetUnitOfWork();
                var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
                var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
                var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
                try
                {
                    //upload images and set image paths
                    foreach (string name in Request.Files)
                    {
                        var postedFile = Request.Files[name];
                        if (postedFile == null || string.IsNullOrEmpty(postedFile.FileName))
                            continue;
						var uploadedFileName = this.UploadFile(postedFile, MiscHelpers.ImageSize.WelcomePeople);
                        formModel.WelcomePeople.LocalisationPicture = uploadedFileName;
                    }
                    //get localisation
                    var loc = lRepo.Get(l => string.Compare(l.Name, formModel.LocalisationName, StringComparison.InvariantCultureIgnoreCase) == 0);
                    var offer = loc.Offers.FirstOrDefault(o => string.Compare(o.Name, formModel.OfferName, StringComparison.InvariantCultureIgnoreCase) == 0);
                    formModel.WelcomePeople.OfferId = offer.Id;
                    wpRepo.Add(formModel.WelcomePeople);
                    context.Commit();

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleHaveBeenCreate;

                    return RedirectToAction(MVC.Admin.Activity.IndexWelcomePeople());
                }
                catch (Exception ex)
                {
                    context.Complete();
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(formModel);
        }

        /// <summary>
        /// prepare the view to edit a welcomePeople
        /// </summary>
        /// <param name="id">id of the welcomePeople to edit</param>
        /// <returns>the form</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult EditWelcomePeople(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
            var item = wpRepo.Get(id);
            if (item == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleNotFound;
                return RedirectToAction(MVC.Admin.Activity.IndexWelcomePeople());
            }
            return View(new WelcomePeopleFormViewModel(item));
        }

        /// <summary>
        /// Apply the modifications to a welcomePeople
        /// </summary>
        /// <param name="welcomePeople">data from the form</param>
        /// <returns>redirect to index</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult EditWelcomePeople(int id, WelcomePeopleFormViewModel formModel)
        {
            if (ModelState.IsValid)
            {
                var context = ModelFactory.GetUnitOfWork();
                var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
                var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
                try
                {
                    //upload images and set image paths
                    foreach (string name in Request.Files)
                    {

                        var postedFile = Request.Files[name];
                        if (postedFile == null || string.IsNullOrEmpty(postedFile.FileName))
                            continue;
                        var uploadedFileName = this.UploadFile(postedFile, MiscHelpers.ImageSize.WelcomePeople);
                        formModel.WelcomePeople.LocalisationPicture = uploadedFileName;
                    }

                    var wp = wpRepo.Get(id);

                    UpdateModel(wp, "WelcomePeople");
                    //get localisation
                    var loc = lRepo.Get(l => string.Compare(l.Name, formModel.LocalisationName, StringComparison.InvariantCultureIgnoreCase) == 0);
                    var offer = loc.Offers.FirstOrDefault(o => string.Compare(o.Name, formModel.OfferName, StringComparison.InvariantCultureIgnoreCase) == 0);
                    wp.OfferId = offer.Id;

                    if (!string.IsNullOrEmpty(formModel.WelcomePeople.LocalisationPicture))
                        wp.LocalisationPicture = formModel.WelcomePeople.LocalisationPicture;

                    context.Commit();
                }
                catch (Exception ex)
                {
                    context.Complete();
                    ModelState.AddModelError("", ex.Message);
                }

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleHaveBeenEdit;

                return RedirectToAction(MVC.Admin.Activity.IndexWelcomePeople());
            }
            return View(formModel);
        }

        /// <summary>
        /// Prepares a web page to delete a WelcomePeople
        /// </summary>
        /// <param name="page">id of the WelcomePeople</param>
        /// <returns>The action result.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("DeleteWelcomePeople")]
        public virtual ActionResult DeleteWelcomePeople(int id, string returnUrl)
        {
            var context = ModelFactory.GetUnitOfWork();
            var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
            var welcomePeople = wpRepo.Get(id);
            if (welcomePeople == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleNotFound;
                return RedirectToAction(MVC.Admin.Activity.IndexWelcomePeople());
            }
            TempData["returnUrl"] = returnUrl;

            return View(welcomePeople);
        }

        // supprimer-welcomePeople

        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("DeleteWelcomePeople")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteWelcomePeople(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
            var profil = wpRepo.Get(id);
            if (profil == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleNotFound;
                return RedirectToAction(MVC.Admin.Activity.IndexWelcomePeople());
            }
            else
            {
                wpRepo.Delete(profil.Id);
                context.Commit();

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleHaveBeenDel;

                return RedirectToAction(MVC.Admin.Activity.IndexWelcomePeople());
            }
        }

        /// <summary>
        /// POST Action method to update admin roles
        /// and redirect to user admin home
        /// </summary>
        /// <param name="collection">form containg the list of ids to push to admin role</param>
        /// <returns>Redirect to return url</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult WelcomePeopleLine(FormCollection collection, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var context = ModelFactory.GetUnitOfWork();
                var wRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
                try
                {
                    var listCollection = collection.AllKeys;
                    foreach (var placeName in listCollection)
                    {
                        var onlineCheck = collection[placeName].ToLower();
                        var welcomeppl = wRepo.Get(wp => string.Compare(wp.Offer.Localisation.Name, onlineCheck, StringComparison.InvariantCultureIgnoreCase) == 0);
                        if (welcomeppl == null)
                            continue;
                        welcomeppl.Online = onlineCheck.Contains("true");
                    }
                    context.Commit();
                }
                catch (Exception e)
                {
                    _Logger.Error(e.Message);
                    context.Complete();
                }

            }

            TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.RoleHaveBeenSet;

            // Redirection
            return Redirect(returnUrl);
        }

        #endregion

        #region Admin Press

        /// <summary>
        /// Prepares a web page containing a paginated list of the press on home page
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult IndexPress(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
            int pageValue = page ?? 1;
            var press = pRepo.Get((pageValue - 1) * MiscHelpers.Constants.PageSize, MiscHelpers.Constants.PageSize, p => p.ID);
            var viewModel = new PagingList<Press>()
            {
                List = press.OrderByDescending(x => x.Date).ToList(),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = pRepo.GetCount()
                }
            };
            return View(viewModel);
        }

        /// <summary>
        /// Prepares a web page containing the details of a Press
        /// </summary>
        /// <param name="page">id of the Press</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult DetailPress(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
            var item = pRepo.Get(id);
            if (item == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressNotFound;
                return RedirectToAction(MVC.Admin.Activity.IndexPress());
            }

            return View(item);
        }

        /// <summary>
        /// Prepares a web page containing the form to create a new Press
        /// </summary>
        /// <returns>The action result.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult CreatePress()
        {
            return View(new Press());
        }

        /// <summary>
        /// Add the press from the form to the repository, then redirect to index
        /// </summary>
        /// <param name="press">data from the form</param>
        /// <returns>redirect to index</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult CreatePress(Press formModel)
        {
            if (ModelState.IsValid)
            {
                var context = ModelFactory.GetUnitOfWork();
                var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
                try
                {
                    pRepo.Add(formModel);
                    context.Commit();

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressHaveBeenCreate;

                    return RedirectToAction(MVC.Admin.Activity.IndexPress());
                }
                catch (Exception ex)
                {
                    context.Complete();
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(formModel);
        }

        /// <summary>
        /// prepare the view to edit a press
        /// </summary>
        /// <param name="id">id of the press to edit</param>
        /// <returns>the form</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult EditPress(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
            var item = pRepo.Get(id);
            if (item == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressNotFound;
                return RedirectToAction(MVC.Admin.Activity.IndexPress());
            }

            return View(item);
        }

        /// <summary>
        /// Apply the modifications to a press
        /// </summary>
        /// <param name="press">data from the form</param>
        /// <returns>redirect to index</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult EditPress(int id, Press formModel)
        {
            if (ModelState.IsValid)
            {
                var context = ModelFactory.GetUnitOfWork();
                var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
                try
                {
                    var p = pRepo.Get(id);
                    UpdateModel(p);
                    context.Commit();
                }
                catch (Exception ex)
                {
                    context.Complete();
                    ModelState.AddModelError("", ex.Message);
                }

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressHaveBeenEdit;

                return RedirectToAction(MVC.Admin.Activity.IndexPress());
            }
            return View(formModel);
        }

        /// <summary>
        /// Prepares a web page to delete a press
        /// </summary>
        /// <param name="page">id of the Press</param>
        /// <returns>The action result.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("supprimer-press")]
        public virtual ActionResult DeletePress(int id, string returnUrl)
        {
            var context = ModelFactory.GetUnitOfWork();
            var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
            var press = pRepo.Get(id);
            if (press == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressNotFound;
                return RedirectToAction(MVC.Admin.Activity.IndexPress());
            }

            TempData["returnUrl"] = returnUrl;

            return View(MVC.Admin.Activity.Views.DeletePress);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("supprimer-press")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeletePress(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
            var article = pRepo.Get(id);
            if (article == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressNotFound;
                return RedirectToAction(MVC.Admin.Activity.IndexPress());
            }
            else
            {
                try
                {
                    pRepo.Delete(article.ID);
                    context.Commit();
                }
                catch (Exception ex)
                {
                    _Logger.Error("DeletePress", ex);
                    context.Complete();
                }

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressHaveBeenDel;

                return RedirectToAction(MVC.Admin.Activity.IndexPress());
            }
        }

        #endregion

        #region Blog

        public virtual ActionResult RefreshBlog()
        {
            try
            {
				DataCacheSingleton.Instance.Cache.Remove(BlogService.FrBlogCacheKey);
				DataCacheSingleton.Instance.Cache.Remove(BlogService.EnBlogCacheKey);
                DataCacheSingleton.Instance.Cache.Remove(BlogService.EsBlogCacheKey);
            }
            catch (Exception ex)
            {
                _Logger.Error("RefreshBlog", ex);
            }
            return RedirectToAction(MVC.Admin.Activity.IndexWelcomePeople());
        }

        #endregion
    }
}
