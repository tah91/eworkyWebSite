using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Web.Helpers;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;
using Worki.Infrastructure.Helpers;
using Postal;
using Worki.Web.Model;
using System.Web.Security;
using System.IO;
using Worki.Service;

namespace Worki.Web.Areas.Backoffice.Controllers
{
	public partial class OfferController : BackofficeControllerBase
    {
        public OfferController(ILogger logger, IObjectStore objectStore)
            : base(logger, objectStore)
        {
            
        }

		#region Index

		public static void GetOffer(int id, int offerId, out Offer offer, ILocalisationRepository lRepo, IOfferRepository oRepo, Func<ActionResult> caseError)
		{
			//case no offer selected, take the first one
			if (offerId == 0)
			{
				var loc = lRepo.Get(id);
				offer = loc.Offers.FirstOrDefault();
				if (offer == null)
				{
					caseError.Invoke();
				}
			}
			else
			{
				offer = oRepo.Get(offerId);
			}
		}

		/// <summary>
		/// Get action result to show recent activities of the owner localisation
		/// </summary>
		/// <returns>View with recent activities</returns>
		public virtual ActionResult Index(int id, int offerId = 0)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				Offer offer;
				GetOffer(id, offerId, out offer, lRepo, oRepo, () => 
				{
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.PlaceDoNotHaveOffer;
					return RedirectToAction(MVC.Backoffice.Localisation.Index(id));
				});
				
				Member.ValidateOwner(member, offer.Localisation);

				return View(offer);
			}
			catch (Exception ex)
			{
				_Logger.Error("Index", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// GET Action result to edit offer data
		/// </summary>
		/// <param name="id">id of localisation</param>
		/// <param name="offerId">id of offer</param>
		/// <returns>View containing offer data</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult Edit(int id, int offerId = 0)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);

				Offer offer;
				GetOffer(id, offerId, out offer, lRepo, oRepo, () =>
				{
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.PlaceDoNotHaveOffer;
					return RedirectToAction(MVC.Backoffice.Offer.Configure(id));
				});

				Member.ValidateOwner(member, offer.Localisation);

				return View(new OfferFormViewModel(offer.Localisation.IsSharedOffice()) { Offer = offer });
			}
			catch (Exception ex)
			{
				_Logger.Error("Edit", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Post Action result to edit offer data
		/// </summary>
		/// <param name="id">id of offer</param>
		/// <returns>View containing localisation data</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Edit(int id, int offerId, OfferFormViewModel formData)
		{
            _ObjectStore.Store<PictureDataContainer>(PictureData.GetKey(ProviderType.Offer), new PictureDataContainer(formData.Offer));

			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			Offer offer;
			GetOffer(id, offerId, out offer, lRepo, oRepo, () =>
			{
				TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.PlaceDoNotHaveOffer;
				return RedirectToAction(MVC.Backoffice.Localisation.Index(id));
			});

			if (ModelState.IsValid)
			{
				try
				{
					UpdateModel(offer, "Offer");
					context.Commit();
                    _ObjectStore.Delete(PictureData.GetKey(ProviderType.Offer));
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Offer.OfferString.OfferEdited;
					return RedirectToAction(MVC.Backoffice.Offer.Edit(id, offer.Id));
				}
				catch (Exception ex)
				{
					_Logger.Error("Edit", ex);
					context.Complete();
					ModelState.AddModelError("", ex.Message);
				}
			}
			formData.Offer = offer;
			return View(formData);
		}

		/// <summary>
		/// GET Action result to configure offer
		/// </summary>
		/// <param name="id">id of localisation</param>
		/// <param name="offerId">offer id</param>
		/// <returns>View containing offer data</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult Configure(int id, int offerId = 0)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);

				Offer offer;
				GetOffer(id, offerId, out offer, lRepo, oRepo, () =>
				{
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.PlaceDoNotHaveOffer;
					return RedirectToAction(MVC.Backoffice.Localisation.Index(id));
				});

				Member.ValidateOwner(member, offer.Localisation);

				return View(new OfferModel<OfferFormViewModel> { InnerModel = new OfferFormViewModel { Offer = offer }, OfferModelId = offer.Id, LocalisationModelId = id });
			}
			catch (Exception ex)
			{
				_Logger.Error("Configure", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Post Action result to configure offer
		/// </summary>
		/// <param name="id">id of offer</param>
		/// <returns>View containing localisation data</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Configure(int id, OfferModel<OfferFormViewModel> formData)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			if (ModelState.IsValid)
			{
				try
				{
					var member = mRepo.Get(memberId);
					if (formData.InnerModel.Offer.HasProduct && string.IsNullOrEmpty(member.MemberMainData.PaymentAddress))
					{
						throw new Exception(Worki.Resources.Views.BackOffice.BackOfficeString.NeedInfoPaypal);
					}

					var o = oRepo.Get(formData.OfferModelId);
					UpdateModel(o, "InnerModel.Offer");
					o.Validate();
					context.Commit();
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Offer.OfferString.OfferEdited;
					return RedirectToAction(MVC.Backoffice.Offer.Configure(o.LocalisationId, o.Id));
				}
				catch (Exception ex)
				{
					_Logger.Error("Configure", ex);
					context.Complete();
					ModelState.AddModelError("", ex.Message);
				}
			}
			return View(formData);
		}

		/// <summary>
		/// GET Action result to  offer prices
		/// </summary>
		/// <param name="id">id of localisation</param>
		/// <param name="offerId">offer id</param>
		/// <returns>View containing offer prices</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult Prices(int id, int offerId = 0)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);

				Offer offer;
				GetOffer(id, offerId, out offer, lRepo, oRepo, () =>
				{
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.PlaceDoNotHaveOffer;
					return RedirectToAction(MVC.Backoffice.Localisation.Index(id));
				});

				Member.ValidateOwner(member, offer.Localisation);

				return View(new OfferModel<OfferFormViewModel>
				{
					InnerModel = new OfferFormViewModel(offer.Localisation.IsSharedOffice()) { Offer = offer },
					OfferModelId = offer.Id,
					LocalisationModelId = id
				});
			}
			catch (Exception ex)
			{
				_Logger.Error("Prices", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Post Action result to edit offer prices
		/// </summary>
		/// <param name="id">id of localisation</param>
		/// <param name="offerId">offer id</param>
		/// <returns>View containing offer prices</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		[ValidateOnlyIncomingValues]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Prices(int id, int offerId, OfferModel<OfferFormViewModel> formData)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);

			if (ModelState.IsValid)
			{
				try
				{
					var offer = oRepo.Get(formData.OfferModelId);
					UpdateModel(offer, "InnerModel.Offer");
					context.Commit();
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Offer.OfferString.OfferEdited;
					return RedirectToAction(MVC.Backoffice.Offer.Prices(id, offer.Id));
				}
				catch (Exception ex)
				{
					_Logger.Error("Prices", ex);
					context.Complete();
					ModelState.AddModelError("", ex.Message);
				}
			}
			
			return View(formData);
		}

        [ChildActionOnly]
        public virtual ActionResult VerticalMenu(int id, int selected)
        {
            var context = ModelFactory.GetUnitOfWork();
            var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
            var offer = oRepo.Get(id);

            var model = new List<LinkMenuItem>();
			model.Add(new LinkMenuItem { Selected = (int)OfferMenu.Prices == selected, Text = Worki.Resources.Menu.Menu.Prices, Link = Url.Action(MVC.Backoffice.Offer.Prices(offer.LocalisationId)) });
			model.Add(new LinkMenuItem { Selected = (int)OfferMenu.Config == selected, Text = Worki.Resources.Menu.Menu.Configure, Link = Url.Action(MVC.Backoffice.Offer.Configure(offer.LocalisationId)) });
			model.Add(new LinkMenuItem { Selected = (int)OfferMenu.Edit == selected, Text = Worki.Resources.Menu.Menu.EditOffer, Link = Url.Action(MVC.Backoffice.Offer.Edit(offer.LocalisationId)) });
			//model.Add(new LinkMenuItem { Selected = (int)OfferMenu.Booking == selected, Text = Worki.Resources.Menu.Menu.CurrentBookings, Link = Url.Action(MVC.Backoffice.Offer.Booking(offer.LocalisationId)) });
			//model.Add(new LinkMenuItem { Selected = (int)OfferMenu.Quotation == selected, Text = Worki.Resources.Menu.Menu.Quoations, Link = Url.Action(MVC.Backoffice.Offer.Quotation(offer.LocalisationId)) });

			return PartialView(MVC.Backoffice.Shared.Views._LinkVerticalMenu, model);
        }

		[ChildActionOnly]
		public virtual ActionResult OfferDropdown(int id, int selected)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var offer = oRepo.Get(id);

			var model = new OfferDropDownModel { Offer = offer  };
			var type = (OfferMenu)selected;
			switch(type)
			{
				case OfferMenu.Config:
					model.UrlMaker = o => Url.Action(MVC.Backoffice.Offer.Configure(o.LocalisationId, o.Id));
					model.Filter = OfferDropDownFilter.None;
					break;
				case OfferMenu.Edit:
					model.UrlMaker = o => Url.Action(MVC.Backoffice.Offer.Edit(o.LocalisationId, o.Id));
					model.Filter = OfferDropDownFilter.None;
					break;
				case OfferMenu.Booking:
					model.UrlMaker = o => Url.Action(MVC.Backoffice.Offer.Booking(offer.LocalisationId, o.Id));
					model.Filter = OfferDropDownFilter.Booking;
					break;
				case OfferMenu.Quotation:
					model.UrlMaker = o => Url.Action(MVC.Backoffice.Offer.Quotation(offer.LocalisationId, o.Id));
					model.Filter = OfferDropDownFilter.Quotation;
					break;
				case OfferMenu.Schedule:
					model.UrlMaker = o => Url.Action(MVC.Backoffice.Schedule.OfferSchedule(offer.LocalisationId, o.Id));
					model.Filter = OfferDropDownFilter.None;
					break;
				case OfferMenu.Prices:
					model.UrlMaker = o => Url.Action(MVC.Backoffice.Offer.Prices(offer.LocalisationId, o.Id));
					model.Filter = OfferDropDownFilter.None;
					break;
				default:
					break;
			}

			return PartialView(MVC.Backoffice.Offer.Views._OfferDropDown, model);
		}

		#endregion

		/// <summary>
		/// Get action method to show bookings of the owner, for a given localisation and offer
		/// </summary>
		/// <param name="offerid">id of the localisation</param>
		/// <param name="offerId">id of the offer</param>
		/// <returns>View containing the bookings</returns>
		public virtual ActionResult Booking(int id, int offerId = 0, int page = 1)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var p = page;
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				Offer offer;
				//case no offer selected, take the first one
				if (offerId == 0)
				{
					var loc = lRepo.Get(id);
					offer = loc.Offers.Where(o => o.CanHaveBooking).FirstOrDefault();
					if (offer == null)
					{
						TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.DoNotHaveOnlineBooking;
						return RedirectToAction(MVC.Backoffice.Offer.Configure(id));
					}
				}
				else
				{
					offer = oRepo.Get(offerId);
				}

				Member.ValidateOwner(member, offer.Localisation);

				var model = new OfferBookingViewModel
				{
					Item = offer,
					List = new PagingList<MemberBooking>
					{
						List = offer.MemberBookings.OrderByDescending(mb => mb.CreationDate).Skip((p - 1) * PagedListViewModel.PageSize).Take(PagedListViewModel.PageSize).ToList(),
						PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PagedListViewModel.PageSize, TotalItems = offer.MemberBookings.Count }
					}
				};
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("Booking", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Get action method to show quotation of the owner, for a given localisation and offer
		/// </summary>
		/// <param name="id">id of the localisation</param>
		/// <param name="offerId">id of the offer</param>
		/// <returns>View containing the quotations</returns>
		public virtual ActionResult Quotation(int id, int offerId = 0, int page = 1)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var p = page;
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				Offer offer;
				//case no offer selected, take the first one
				if (offerId == 0)
				{
					var loc = lRepo.Get(id);
					offer = loc.Offers.Where(o => o.CanHaveQuotation).FirstOrDefault();
					if (offer == null)
					{
						TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.DoNotHaveOnlineQuotation;
						return RedirectToAction(MVC.Backoffice.Offer.Configure(id));
					}
				}
				else
				{
					offer = oRepo.Get(offerId);
				}
				Member.ValidateOwner(member, offer.Localisation);

				var model = new OfferQuotationViewModel
				{
					Item = offer,
					List = new PagingList<MemberQuotation>
					{
						List = offer.MemberQuotations.OrderByDescending(mq => mq.CreationDate).Skip((p - 1) * PagedListViewModel.PageSize).Take(PagedListViewModel.PageSize).ToList(),
						PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PagedListViewModel.PageSize, TotalItems = offer.MemberQuotations.Count }
					}
				};
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("Quotation", ex);
				return View(MVC.Shared.Views.Error);
			}
		}
	}
}
