using System;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Data.Repository;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Web.Helpers;
using Worki.Service;
using Worki.Infrastructure;
using Postal;
using System.Linq;
using Worki.Memberships;
using System.Web.Security;
using System.Collections.Generic;

namespace Worki.Web.Controllers
{
	public partial class OfferController : ControllerBase
	{
		#region Private

        IMembershipService _MembershipService;

		#endregion

        public OfferController( ILogger logger, 
                                IObjectStore objectStore,
                                IMembershipService membershipService)
            : base(logger, objectStore)
		{
            _MembershipService = membershipService;
		}

		/// <summary>
		/// GET Action result to show offer form
		/// </summary>
		/// <param name="id">id of the offer localisation</param>
		/// <returns>View containing offer form</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		public virtual ActionResult Create(int id, int type, string returnUrl=null)
		{
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var loc = lRepo.Get(id);

			return View(new OfferFormViewModel(loc.IsSharedOffice()) { Offer = new Offer { LocalisationId = id, Type = type } });
		}

		/// <summary>
		/// Post Action result to add offer
		/// </summary>
		/// <returns>View containing localisation form</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Create(int id, string returnUrl, OfferFormViewModel offerFormViewModel)
        {
            _ObjectStore.Store<PictureDataContainer>(PictureData.GetKey(ProviderType.Offer), new PictureDataContainer(offerFormViewModel.Offer));

            if (ModelState.IsValid)
            {
                try
                {
                    var context = ModelFactory.GetUnitOfWork();
                    var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
					var loc = lRepo.Get(id);
                    try
                    {
						loc.Offers.Add(offerFormViewModel.Offer);
                        context.Commit();
                        _ObjectStore.Delete(PictureData.GetKey(ProviderType.Offer));
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex.Message);
                        context.Complete();
                        throw ex;
                    }
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Offer.OfferString.OfferCreated;
					if (!string.IsNullOrEmpty(returnUrl))
					{
						return Redirect(returnUrl);
					}
					else
					{
						return RedirectToAction(MVC.Localisation.Edit(offerFormViewModel.Offer.LocalisationId));
					}
                }
                catch (Exception ex)
                {
                    _Logger.Error("Create", ex);
                    ModelState.AddModelError("", ex.Message);
                }
            }
			return View(offerFormViewModel);
        }

		/// <summary>
		/// GET Action result to show offer data
		/// </summary>
		/// <param name="id">id of offer</param>
		/// <returns>View containing offer data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult Details(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var offer = oRepo.Get(id);
			return View(offer);
		}

		/// <summary>
		/// GET Action result to edit offer data
		/// </summary>
		/// <param name="id">id of offer</param>
		/// <returns>View containing offer data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		public virtual ActionResult Edit(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var offer = oRepo.Get(id);
			return View(MVC.Offer.Views.Create, new OfferFormViewModel(offer.Localisation.IsSharedOffice()) { Offer = offer });
		}

		/// <summary>
		/// Post Action result to edit offer data
		/// </summary>
		/// <param name="id">id of offer</param>
		/// <returns>View containing localisation data</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Edit(int id, OfferFormViewModel formData)
		{
            _ObjectStore.Store<PictureDataContainer>(PictureData.GetKey(ProviderType.Offer), new PictureDataContainer(formData.Offer));

			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			if (ModelState.IsValid)
			{
				try
				{
					var o = oRepo.Get(id);
					UpdateModel(o, "Offer");
					context.Commit();
                    _ObjectStore.Delete(PictureData.GetKey(ProviderType.Offer));
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Offer.OfferString.OfferEdited;
					return RedirectToAction(MVC.Localisation.Edit(o.LocalisationId));
				}
				catch (Exception ex)
				{
					_Logger.Error("Edit", ex);
					context.Complete();
					ModelState.AddModelError("", ex.Message);
				}
			}
			return View(MVC.Offer.Views.Create, formData);
		}

		/// <summary>
		/// GET Action result to delete an offer
		/// if the id is in db, ask for confirmation to delete the offer
		/// </summary>
		/// <param name="id">The id of the offer to delete</param>
		/// <returns>the confirmation view</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		//[ActionName("supprimer")]
		public virtual ActionResult Delete(int id, string returnUrl = null)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var offer = oRepo.Get(id);
			if (offer == null)
				return View(MVC.Shared.Views.Error);
			else
			{
				TempData["returnUrl"] = returnUrl;
				return View(offer);
			}
		}

		/// <summary>
		/// POST Action result to delete an offer
		/// remove offer from db
		/// <param name="id">The id of the offer to delete</param>
		/// </summary>
		/// <returns>the deletetion success view</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Delete(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			int localisationId = 0;
			try
			{
				var o = oRepo.Get(id);
				localisationId = o.LocalisationId;
				oRepo.Delete(id);
				context.Commit();
			}
			catch (Exception ex)
			{
				_Logger.Error("Delete", ex);
				context.Complete();
			}

			TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Offer.OfferString.OfferRemoved;
			return RedirectToAction(MVC.Localisation.Edit(localisationId));
		}

		/// <summary>
		/// Action result to return offerprice item for edition
		/// </summary>
		/// <returns>a partial view</returns>
        
        [AcceptVerbs(HttpVerbs.Get)]
		public virtual PartialViewResult AddOfferPrice()
		{
			return PartialView(MVC.Offer.Views._OfferPrice, new OfferPrice());
		}

		#region Ajax Offer

        Offer GetOffer(int offerId)
        {
            Offer offer = null;
            if (offerId > 0)
            {
                var context = ModelFactory.GetUnitOfWork();
                var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
                offer = oRepo.Get(offerId);
            }
            else
            {
                var offerList = _ObjectStore.Get<OfferFormListModel>("OfferList");
                offer = offerList.Offers.FirstOrDefault(o => o.Id == offerId);
            }

            return offer;
        }
        
		/// <summary>
		/// Action result to return offer creation form
		/// </summary>
		/// <returns>a partial view</returns>
		public virtual PartialViewResult AjaxAdd(int id, bool isShared)
		{
			return PartialView(MVC.Offer.Views._AjaxAdd, new OfferFormViewModel(isShared) { LocId = id });
		}

		/// <summary>
		/// POST Action result add an offer via ajax
		/// </summary>
		/// <param name="id">localisation id</param>
        /// <param name="offerFormViewModel">offer data</param>
		/// <returns>exception if error, partial view if added</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		//[ValidateAntiForgeryToken]
		[HandleModelStateException]
		public virtual PartialViewResult AjaxAdd(int id, OfferFormViewModel offerFormViewModel)
		{
            _ObjectStore.Store<PictureDataContainer>(PictureData.GetKey(ProviderType.Offer), new PictureDataContainer(offerFormViewModel.Offer));

            if (offerFormViewModel.Offer.OfferPrices.Count < 1)
            {
                ModelState.AddModelError("", string.Format(Worki.Resources.Validation.ValidationString.OfferPriceType, "Prix"));
            }

            else
            {
                OfferPrice firstOffer = offerFormViewModel.Offer.OfferPrices.ElementAt(0);
                decimal hourlyPrice = OfferPrice.GetHourlyPrice(firstOffer);
                Boolean comparison = true;

                foreach (OfferPrice p in offerFormViewModel.Offer.OfferPrices)
                {
                    comparison = OfferPrice.ComparePrice(hourlyPrice, p);
                    if ( comparison == false)
                    {
                        ModelState.AddModelError("", string.Format(Worki.Resources.Validation.ValidationString.OfferMismatch, new string[] { Offer.GetPaymentPeriodType(p.PriceType), Offer.GetPaymentPeriodType(firstOffer.PriceType) }));
                    }
                }
            }


			if (ModelState.IsValid)
			{
				try
				{
					//case loc exists
					if (id > 0)
					{
						var context = ModelFactory.GetUnitOfWork();
						var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
						var loc = lRepo.Get(id);
						try
						{
							loc.Offers.Add(offerFormViewModel.Offer);
							context.Commit();
						}
						catch (Exception ex)
						{
							_Logger.Error(ex.Message);
							context.Complete();
							throw ex;
						}
					}
					else
					//add to temp data, to be processed later
					{
                        var offerList = _ObjectStore.Get<OfferFormListModel>("OfferList");
						if (offerList == null)
                            offerList = new OfferFormListModel { IsSharedOffice = offerFormViewModel.IsSharedOffice };
                        //negative id to set the order
                        var minId = offerList.Offers.Min(o => (int?)o.Id) ?? -1;
                        offerFormViewModel.Offer.Id = minId;

						offerList.Offers.Add(offerFormViewModel.Offer);
                        _ObjectStore.Store<OfferFormListModel>("OfferList", offerList);
					}

                    _ObjectStore.Delete(PictureData.GetKey(ProviderType.Offer));
                    return PartialView(MVC.Offer.Views._OfferItem, new OfferFormListModelItem { Offer = offerFormViewModel.Offer, IsSharedOffice = offerFormViewModel.IsSharedOffice });
				}
				catch (Exception ex)
				{
                    _Logger.Error("AjaxAdd", ex);
					ModelState.AddModelError("", ex.Message);
                    throw new ModelStateException(ModelState);
				}
			}
			throw new ModelStateException(ModelState);
		}

        /// <summary>
        /// Action result to return offer edition form
        /// </summary>
        /// <param name="id">id of offer if any</param>
        /// <returns>a partial view</returns>
        public virtual PartialViewResult AjaxEdit(int id, bool isShared)
        {
            Offer offer = GetOffer(id);
            return PartialView(MVC.Offer.Views._AjaxAdd, new OfferFormViewModel(isShared) { Offer = offer });
        }

        /// <summary>
        /// POST Action result edit an offer via ajax
        /// </summary>
        /// <param name="id">offer id</param>
        /// <param name="offerFormViewModel">offer data</param>
        /// <returns>exception if error, partial view if added</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
        //[ValidateAntiForgeryToken]
        [HandleModelStateException]
        public virtual PartialViewResult AjaxEdit(int id, OfferFormViewModel offerFormViewModel)
        {
            _ObjectStore.Store<PictureDataContainer>(PictureData.GetKey(ProviderType.Offer), new PictureDataContainer(offerFormViewModel.Offer));
            if (ModelState.IsValid)
            {
                try
                {
                    var offers = new OfferFormListModel();
                    //case loc exists
                    if (id > 0)
                    {
                        var context = ModelFactory.GetUnitOfWork();
                        var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
                        var offer = oRepo.Get(id);
                        try
                        {
                            var locId = offer.LocalisationId;
                            UpdateModel(offer, "Offer");
                            context.Commit();
                            var newContext = ModelFactory.GetUnitOfWork();
                            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(newContext);
                            var loc = lRepo.Get(locId);
                            offers = new OfferFormListModel { Offers = loc.Offers.ToList(), IsSharedOffice = offerFormViewModel.IsSharedOffice };
                        }
                        catch (Exception ex)
                        {
                            _Logger.Error(ex.Message);
                            context.Complete();
                            throw ex;
                        }
                    }
                    else
                    //add to temp data, to be processed later
                    {
                        Offer offer = GetOffer(id);
                        UpdateModel(offer, "Offer");
                        offers = _ObjectStore.Get<OfferFormListModel>("OfferList");
                    }

                    _ObjectStore.Delete(PictureData.GetKey(ProviderType.Offer));
                    return PartialView(MVC.Offer.Views._OfferList, offers);
                }
                catch (Exception ex)
                {
                    _Logger.Error("AjaxEdit", ex);
                    ModelState.AddModelError("", ex.Message);
                    throw new ModelStateException(ModelState);
                }
            }
            throw new ModelStateException(ModelState);
        }

        /// <summary>
        /// Action result to delete offer
        /// </summary>
        /// <param name="id">id of offer if any</param>
        /// <returns>a partial view</returns>
		public virtual PartialViewResult AjaxDelete(int id)
		{
            try
            {
                var offers = new OfferFormListModel();
                //case loc exists
                if (id > 0)
                {
                    var context = ModelFactory.GetUnitOfWork();
                    var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
                    try
                    {
                        var offer = oRepo.Get(id);
                        var locId = offer.LocalisationId;
                        var isShared = offer.Localisation.IsSharedOffice();
                        oRepo.Delete(id);
                        context.Commit();
                        var newContext = ModelFactory.GetUnitOfWork();
                        var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(newContext);
                        var loc = lRepo.Get(locId);
                        offers = new OfferFormListModel { Offers = loc.Offers.ToList(), IsSharedOffice = isShared };
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error("AjaxDelete", ex);
                        context.Complete();
                        throw ex;
                    }
                }
                else
                //add to temp data, to be processed later
                {
                    var offerList = _ObjectStore.Get<OfferFormListModel>("OfferList");
                    offerList.Offers = offerList.Offers.Where(o => o.Id != id).ToList();
                    _ObjectStore.Store<OfferFormListModel>("OfferList", offerList); 
                    offers = offerList;
                }

                return PartialView(MVC.Offer.Views._OfferList, offers);
            }
            catch (Exception ex)
            {
                _Logger.Error("AjaxDelete", ex);
                ModelState.AddModelError("", ex.Message);
                throw new ModelStateException(ModelState);
            }
		}

		#endregion
	}
}
