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

namespace Worki.Web.Areas.Backoffice.Controllers
{
	public partial class LocalisationController : BackofficeControllerBase
    {
        ILogger _Logger;

        public LocalisationController(ILogger logger)
        {
            _Logger = logger;
        }

		#region Index

		/// <summary>
		/// Get action result to show recent activities of the owner localisation
		/// </summary>
		/// <param name="id">id of the localisation</param>
		/// <returns>View with recent activities</returns>
		public virtual ActionResult Index(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				var loc = lRepo.Get(id);
				Member.ValidateOwner(member, loc);

				var bookings = bRepo.GetMany(b => b.Offer.LocalisationId == id);
				var quotations = qRepo.GetMany(q => q.Offer.LocalisationId == id);

				var news = ModelHelper.GetNews(memberId, bookings, mb => { return Url.Action(MVC.Backoffice.Localisation.BookingDetail(mb.Id)); });
				news = news.Concat(ModelHelper.GetNews(memberId, quotations, q => { return Url.Action(MVC.Backoffice.Localisation.QuotationDetail(q.Id)); })).ToList();

				news = news.OrderByDescending(n => n.Date).Take(BackOfficeConstants.NewsCount).ToList();

				return View(new BackOfficeLocalisationHomeViewModel { Localisation = loc, News = news });
			}
			catch (Exception ex)
			{
				_Logger.Error("Index", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Get action result to show recent activities of the owner localisation
		/// </summary>
		/// <returns>View with recent activities</returns>
		public virtual ActionResult OfferIndex(int id, int offerid = 0)
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
				//case no offer selected, take the first one
				if (offerid == 0)
				{
					var loc = lRepo.Get(id);
					offer = loc.Offers.FirstOrDefault();
                    if (offer == null)
                    {
                        TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.PlaceDoNotHaveOffer;
                        return RedirectToAction(MVC.Backoffice.Localisation.Index(id));
                    }
				}
				else
				{
					offer = oRepo.Get(offerid);
				}
				Member.ValidateOwner(member, offer.Localisation);

				return View(offer);
			}
			catch (Exception ex)
			{
				_Logger.Error("Index", ex);
				return View(MVC.Shared.Views.Error);
			}
		}


		public virtual PartialViewResult AddOfferPrice()
		{
			return PartialView(MVC.Backoffice.Localisation.Views._OfferPrice, new OfferPrice());
		}

		/// <summary>
		/// GET Action result to configure offer
		/// </summary>
		/// <param name="id">id of localisation</param>
		/// <param name="offerId">offer id</param>
		/// <returns>View containing offer data</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult ConfigureOffer(int id, int offerId = 0)
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
				//case no offer selected, take the first one
				if (offerId == 0)
				{
					var loc = lRepo.Get(id);
					offer = loc.Offers.FirstOrDefault();
					if (offer == null)
					{
                        TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.PlaceDoNotHaveOffer;
						return RedirectToAction(MVC.Backoffice.Localisation.Index(id));
					}
				}
				else
				{
					offer = oRepo.Get(offerId);
				}
				Member.ValidateOwner(member, offer.Localisation);

				return View(new OfferModel<OfferFormViewModel> { InnerModel = new OfferFormViewModel { Offer = offer }, OfferModelId = offer.Id });
			}
			catch (Exception ex)
			{
				_Logger.Error("ConfigureOffer", ex);
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
		public virtual ActionResult ConfigureOffer(int id, OfferModel<OfferFormViewModel> formData)
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
					return RedirectToAction(MVC.Backoffice.Localisation.ConfigureOffer(o.LocalisationId, o.Id));
				}
				catch (Exception ex)
				{
					_Logger.Error("Edit", ex);
					context.Complete();
					ModelState.AddModelError("", ex.Message);
				}
			}
			return View(formData);
		}

        [ChildActionOnly]
        public virtual ActionResult OfferVerticalMenu(int id, int selected)
        {
            var context = ModelFactory.GetUnitOfWork();
            var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
            var offer = oRepo.Get(id);

            var model = new List<OfferMenuItem>();
            model.Add(new OfferMenuItem { Selected = (int)OfferMenuType.Config == selected, Text = Worki.Resources.Menu.Menu.Configure, Link = Url.Action(MVC.Backoffice.Localisation.ConfigureOffer(offer.LocalisationId)) });
            model.Add(new OfferMenuItem { Selected = (int)OfferMenuType.Booking == selected, Text = Worki.Resources.Menu.Menu.CurrentBookings, Link = Url.Action(MVC.Backoffice.Localisation.OfferBooking(offer.LocalisationId)) });
            model.Add(new OfferMenuItem { Selected = (int)OfferMenuType.Quotation == selected, Text = Worki.Resources.Menu.Menu.Quoations, Link = Url.Action(MVC.Backoffice.Localisation.OfferQuotation(offer.LocalisationId)) });
			model.Add(new OfferMenuItem { Selected = (int)OfferMenuType.Schedule == selected, Text = Worki.Resources.Menu.Menu.Schedule, Link = Url.Action(MVC.Backoffice.Localisation.OfferSchedule(offer.LocalisationId)) });

            return PartialView(MVC.Backoffice.Localisation.Views._OfferMenu, model);
        }

		[ChildActionOnly]
		public virtual ActionResult OfferHorizontalMenu(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var offer = oRepo.Get(id);

			return PartialView(MVC.Backoffice.Localisation.Views._LocalisationMenu, new LocalisationMenuIndex { MenuItem = LocalisationMenu.Offers, Id = offer.LocalisationId, Title = offer.Localisation.Name });
		}

		[ChildActionOnly]
		public virtual ActionResult OfferDropdown(int id, int selected)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var offer = oRepo.Get(id);

			var model = new OfferDropDownModel { Offer = offer  };
			var type = (OfferMenuType)selected;
			switch(type)
			{
				case OfferMenuType.Config:
					model.UrlMaker = o => Url.Action(MVC.Backoffice.Localisation.ConfigureOffer(o.LocalisationId, o.Id));
					model.Filter = OfferDropDownFilter.None;
					break;
				case OfferMenuType.Booking:
                    model.UrlMaker = o => Url.Action(MVC.Backoffice.Localisation.OfferBooking(offer.LocalisationId, o.Id));
					model.Filter = OfferDropDownFilter.Booking;
					break;
				case OfferMenuType.Quotation:
                    model.UrlMaker = o => Url.Action(MVC.Backoffice.Localisation.OfferQuotation(offer.LocalisationId, o.Id));
					model.Filter = OfferDropDownFilter.Quotation;
					break;
				default:
					break;
			}
			
			return PartialView(MVC.Backoffice.Localisation.Views._OfferDropDown, model);
		}

		#endregion

		#region Booking

		/// <summary>
		/// Get action method to show bookings of the owner, for a given localisation
		/// </summary>
		/// <param name="id">id of the localisation</param>
		/// <returns>View containing the bookings</returns>
		public virtual ActionResult Booking(int id, int page = 1)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var p = page;
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				var loc = lRepo.Get(id);
				Member.ValidateOwner(member, loc);

                var bookings = bRepo.GetMany(b => b.Offer.LocalisationId == id && b.StatusId == (int)MemberBooking.Status.Unknown).Where(b => !b.Expired).OrderByDescending(b => b.CreationDate);
				var model = new LocalisationBookingViewModel
				{
					Item = loc,
					List = new PagingList<MemberBooking>
					{
                        List = bookings.Skip((p - 1) * PagedListViewModel.PageSize).Take(PagedListViewModel.PageSize).ToList(),
						PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PagedListViewModel.PageSize, TotalItems = bookings.Count() }
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

        public virtual ActionResult Localisation_Schedule(int id)
        {
            var memberId = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
            var p = 1;
            try
            {
                var member = mRepo.Get(memberId);
                Member.Validate(member);
                var loc = lRepo.Get(id);
                Member.ValidateOwner(member, loc);

                var bookings = bRepo.GetMany(b => b.Offer.LocalisationId == id);
                var model = new LocalisationBookingViewModel
                {
                    Item = loc,
                    List = new PagingList<MemberBooking>
                    {
                        List = bookings.ToList(),
                        PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PagedListViewModel.PageSize, TotalItems = bookings.Count() }
                    }
                };
                return View(MVC.Backoffice.Localisation.Views._LocalisationCalendar, model);
            }
            catch (Exception ex)
            {
                _Logger.Error("Localisation_Schedule", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

		/// <summary>
		/// Get action method to show bookings of the owner, for a given localisation and offer
		/// </summary>
		/// <param name="offerid">id of the localisation</param>
        /// <param name="offerId">id of the offer</param>
		/// <returns>View containing the bookings</returns>
		public virtual ActionResult OfferBooking(int id,int offerId = 0, int page = 1)
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
                        return RedirectToAction(MVC.Backoffice.Localisation.ConfigureOffer(id));
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
				_Logger.Error("OfferBooking", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Get action method to show booking detail
		/// </summary>
		/// <returns>View containing the booking</returns>
		public virtual ActionResult BookingDetail(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			try
			{
                var member = mRepo.Get(memberId);
				Member.Validate(member);
				var booking = bRepo.Get(id);

				return View(booking);
			}
			catch (Exception ex)
			{
				_Logger.Error("BookingDetail", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// GET Action result to confirm booking
		/// </summary>
		/// <param name="id">id of booking to confirm</param>
		/// <returns>View to fill booking data</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult ConfirmBooking(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);

			try
			{
				var member = mRepo.Get(memberId);
				var booking = bRepo.Get(id);
				Member.Validate(member);
				Member.ValidateOwner(member, booking.Offer.Localisation);
				return View(new OfferModel<MemberBooking> { InnerModel = booking, OfferModelId = booking.OfferId });
			}
			catch (Exception ex)
			{
				_Logger.Error("ConfirmBooking", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// POST Action result to confirm booking
		/// </summary>
		/// <param name="id">id of booking to confirm</param>
		/// <returns>View to fill booking data</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		[ValidateAntiForgeryToken]
		[ValidateOnlyIncomingValues]
		public virtual ActionResult ConfirmBooking(int id, OfferModel<MemberBooking> memberBooking)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);

			if (ModelState.IsValid)
			{
				try
				{
					var booking = bRepo.Get(id);
					UpdateModel(booking, "InnerModel");
					booking.StatusId = (int)MemberBooking.Status.Accepted;
					booking.MemberBookingLogs.Add(new MemberBookingLog
					{
						CreatedDate = DateTime.UtcNow,
						Event = "Booking Confirmed",
						EventType = (int)MemberBookingLog.BookingEvent.Approval,
						LoggerId = memberId
					});

					//send mail to owner
					//useless

                    //send mail to client
                    var urlHelp = new UrlHelper(ControllerContext.RequestContext);
                    var userUrl = urlHelp.ActionAbsolute(MVC.Dashboard.Home.Booking());
                    TagBuilder userLink = new TagBuilder("a");
                    userLink.MergeAttribute("href", userUrl);
                    userLink.InnerHtml = Worki.Resources.Views.Shared.SharedString.SpaceUser;
					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
					clientMail.To = booking.Client.Email;
					clientMail.Subject = Worki.Resources.Email.BookingString.AcceptBookingClientSubject;
					clientMail.ToName = booking.Client.MemberMainData.FirstName;
                    clientMail.Content = string.Format(Worki.Resources.Email.BookingString.AcceptBookingClient,
														Localisation.GetOfferType(booking.Offer.Type),
														booking.GetStartDate(),
														booking.GetEndDate(),
														booking.Offer.Localisation.Name,
														booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City,
														booking.Price,
                                                        userLink);

					context.Commit();

                    clientMail.Send();

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Booking.BookingString.BookingAccepted;

					return RedirectToAction(MVC.Backoffice.Localisation.BookingDetail(booking.Id));
				}
				catch (Exception ex)
				{
					_Logger.Error("ConfirmBooking", ex);
					context.Complete();
				}
			}
			return View(memberBooking);
		}

		/// <summary>
		/// GET Action result to refuse booking
		/// </summary>
		/// <param name="id">id of booking to refuse</param>
		/// <returns>View to fill booking data</returns>
		[AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult RefuseBooking(int id, string returnUrl)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);

			try
			{
				var member = mRepo.Get(memberId);
				var booking = bRepo.Get(id);
				Member.Validate(member);
				Member.ValidateOwner(member, booking.Offer.Localisation);

                var model = new RefuseBookingModel { BookingId = id, ReturnUrl = returnUrl };
                return View(new OfferModel<RefuseBookingModel> { InnerModel = model, OfferModelId = booking.OfferId });
			}
			catch (Exception ex)
			{
				_Logger.Error("RefuseBooking", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// POST Action result to confirm booking
		/// </summary>
		/// <param name="id">id of booking to confirm</param>
		/// <returns>View to fill booking data</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		[ValidateAntiForgeryToken]
        public virtual ActionResult RefuseBooking(int id, OfferModel<RefuseBookingModel> formModel, string confirm)
		{
            if (string.IsNullOrEmpty(confirm))
            {
                return Redirect(formModel.InnerModel.ReturnUrl);
            }

			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var memberId = WebHelper.GetIdentityId(User.Identity);

			if (ModelState.IsValid)
			{
				try
				{
					var booking = bRepo.Get(id);
					UpdateModel(booking, "InnerModel");
					booking.StatusId = (int)MemberBooking.Status.Refused;
					booking.MemberBookingLogs.Add(new MemberBookingLog
					{
						CreatedDate = DateTime.UtcNow,
						Event = "Booking Refused",
						EventType = (int)MemberBookingLog.BookingEvent.Refusal,
						LoggerId = memberId
					});

                    dynamic clientMail = new Email(MVC.Emails.Views.Email);
                    clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
                    clientMail.To = booking.Client.Email;
                    clientMail.Subject = Worki.Resources.Email.BookingString.RefuseBookingClientSubject;
                    clientMail.ToName = booking.Client.MemberMainData.FirstName;
                    clientMail.Content = string.Format(Worki.Resources.Email.BookingString.RefuseBookingClient,
                                                        Localisation.GetOfferType(booking.Offer.Type),
														booking.GetStartDate(),
														booking.GetEndDate(),
                                                        booking.Offer.Localisation.Name,
                                                        booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City);

                    context.Commit();

                    clientMail.Send();

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Booking.BookingString.BookingRefused;

                    return RedirectToAction(MVC.Backoffice.Localisation.BookingDetail(booking.Id));
				}
				catch (Exception ex)
				{
					_Logger.Error("RefuseBooking", ex);
					context.Complete();
				}
			}
			return View(formModel);
		}

		#endregion

		#region Quotation

		/// <summary>
		/// Get action method to show quotations of the owner, for a given localisation
		/// </summary>
		/// <param name="id">id of the localisation</param>
		/// <returns>View containing the quotations</returns>
		public virtual ActionResult Quotation(int id, int page = 1)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
			var p = page;
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				var loc = lRepo.Get(id);
				Member.ValidateOwner(member, loc);

                var quotations = qRepo.GetMany(q => q.Offer.LocalisationId == id && q.StatusId == (int)MemberQuotation.Status.Unknown).OrderByDescending(q => q.CreationDate);
				var model = new LocalisationQuotationViewModel
				{
					Item = loc,
					List = new PagingList<MemberQuotation>
					{
						List = quotations.Skip((p - 1) * PagedListViewModel.PageSize).Take(PagedListViewModel.PageSize).ToList(),
						PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PagedListViewModel.PageSize, TotalItems = quotations.Count() }
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

        /// <summary>
        /// Get action method to show quotation detail
        /// </summary>
        /// <returns>View containing the quotation</returns>
        public virtual ActionResult QuotationDetail(int id)
        {
            var memberId = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
            try
            {
                var member = mRepo.Get(memberId);
                Member.Validate(member);
                var quotation = qRepo.Get(id);

                return View(quotation);
            }
            catch (Exception ex)
            {
                _Logger.Error("QuotationDetail", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

        /// <summary>
        /// Get action method to show quotation is paid
        /// </summary>
        /// <param name="id">member quotation id</param>
        /// <returns>View containing the quotation</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult QuotationAccepted(int id)
        {
            var memberId = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
            try
            {
                var quotation = qRepo.Get(id);
                return View(quotation);
            }
            catch (Exception ex)
            {
                _Logger.Error("QuotationAccepted", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

        /// <summary>
        /// Get action method to show quotation is canceled
        /// </summary>
        /// <param name="id">member quotation id</param>
        /// <returns>View containing the quotation</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult QuotationCancelled(int id)
        {
            var memberId = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
            try
            {
                var quotation = qRepo.Get(id);
                quotation.MemberQuotationLogs.Add(new MemberQuotationLog
                {
                    CreatedDate = DateTime.UtcNow,
                    Event = "Quotation Payment Cancelled"
                });

                context.Commit();
                return View(quotation);
            }
            catch (Exception ex)
            {
                context.Complete();
                _Logger.Error("QuotationCancelled", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

		/// <summary>
		/// Get action method to show quotation of the owner, for a given localisation and offer
		/// </summary>
		/// <param name="id">id of the localisation</param>
        /// <param name="offerId">id of the offer</param>
		/// <returns>View containing the quotations</returns>
        public virtual ActionResult OfferQuotation(int id, int offerId = 0, int page = 1)
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
                        return RedirectToAction(MVC.Backoffice.Localisation.ConfigureOffer(id));
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
				_Logger.Error("OfferQuotation", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

        /// <summary>
        /// GET Action result to refuse quotation
        /// </summary>
        /// <param name="id">id of quotation to refuse</param>
        /// <returns>View to fill quotation data</returns>
        [AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult RefuseQuotation(int id, string returnUrl)
        {
            var memberId = WebHelper.GetIdentityId(User.Identity);
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);

            try
            {
                var member = mRepo.Get(memberId);
                var quotation = qRepo.Get(id);
                Member.Validate(member);
                Member.ValidateOwner(member, quotation.Offer.Localisation);

				var model = new RefuseQuotationModel { QuotationId = id, ReturnUrl = returnUrl };
				return View(new OfferModel<RefuseQuotationModel> { InnerModel = model, OfferModelId = quotation.OfferId });
            }
            catch (Exception ex)
            {
                _Logger.Error("RefuseQuotation", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

        /// <summary>
        /// POST Action result to confirm quotation
        /// </summary>
        /// <param name="id">id of quotation to refuse</param>
        /// <returns>View to fill quotation data</returns>
        [AcceptVerbs(HttpVerbs.Post)]
		[ValidateAntiForgeryToken]
		public virtual ActionResult RefuseQuotation(int id, OfferModel<RefuseQuotationModel> formModel, string confirm)
        {
			if (string.IsNullOrEmpty(confirm))
			{
				return Redirect(formModel.InnerModel.ReturnUrl);
			}

            var context = ModelFactory.GetUnitOfWork();
            var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
			var memberId = WebHelper.GetIdentityId(User.Identity);

            if (ModelState.IsValid)
            {
                try
                {
					var quotation = qRepo.Get(id);
                    quotation.StatusId = (int)MemberQuotation.Status.Refused;
                    quotation.MemberQuotationLogs.Add(new MemberQuotationLog
                    {
                        CreatedDate = DateTime.UtcNow,
                        Event = "Quotation Refused",
                        EventType = (int)MemberQuotationLog.QuotationEvent.Refusal,
						LoggerId = memberId
                    });

                    //send mail to owner
                    //useless

                    context.Commit();

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Booking.BookingString.QuotationRefused;

                    return RedirectToAction(MVC.Backoffice.Localisation.QuotationDetail(quotation.Id));
                }
                catch (Exception ex)
                {
                    _Logger.Error("RefuseBooking", ex);
                    context.Complete();
                }
            }
            return View(formModel);
        }

		#endregion

		#region Schedule

		/// <summary>
		/// Get action method to show offer schedule
		/// </summary>
		/// <param name="id">localisation id</param>
		/// <param name="offerId">offer id</param>
		/// <returns>view with a calandar</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult OfferSchedule(int id, int offerId = 0)
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
				//case no offer selected, take the first one
				if (offerId == 0)
				{
					var loc = lRepo.Get(id);
					offer = loc.Offers.Where(o => o.CanHaveBooking).FirstOrDefault();
					if (offer == null)
					{
						TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.DoNotHaveOnlineBooking;
						return RedirectToAction(MVC.Backoffice.Localisation.ConfigureOffer(id));
					}
				}
				else
				{
					offer = oRepo.Get(offerId);
				}

				Member.ValidateOwner(member, offer.Localisation);

				return View(offer);
			}
			catch (Exception ex)
			{
				_Logger.Error("OfferBooking", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		#region Event feed

		/// <summary>
		/// provid a json array of all booking events
		/// </summary>
		/// <param name="id">id of the offer</param>
		/// <returns>json of booking events</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult BookingEvents(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

			var offer = oRepo.Get(id);

			var events = new List<CalandarJson>();
			foreach (var item in offer.MemberBookings)
			{
				events.Add(item.GetCalandarEvent(Url));
			}

			return Json(events);
		}

		#endregion

		#region Handlers

		/// <summary>
		/// Ajax action to handle dropevent
		/// </summary>
		/// <param name="id">id of the booking</param>
		/// <param name="dayDelta">day delta</param>
		/// <param name="minuteDelta">minute delta</param>
		[AcceptVerbs(HttpVerbs.Post)]
		[HandleModelStateException]
		public virtual ActionResult DropEvent(int id, int dayDelta, int minuteDelta)
		{
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var booking = bRepo.Get(id);

			if (booking != null)
            {
                var newFromDate = booking.FromDate.AddDays(dayDelta).AddMinutes(minuteDelta);
                if (!booking.CanModify(newFromDate))
                    throw new ModelStateException(ModelState);

				try
				{
					booking.FromDate = booking.FromDate.AddDays(dayDelta).AddMinutes(minuteDelta);
					booking.ToDate = booking.ToDate.AddDays(dayDelta).AddMinutes(minuteDelta);

					//send mail to booking client
					var urlHelper = new UrlHelper(ControllerContext.RequestContext);
					var dashboardUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Home.Index());
					TagBuilder dashboardLink = new TagBuilder("a");
					dashboardLink.MergeAttribute("href", dashboardUrl);
					dashboardLink.InnerHtml = dashboardUrl;

					var localisationUrl = booking.Offer.Localisation.GetDetailFullUrl(Url);
					TagBuilder localisationLink = new TagBuilder("a");
					localisationLink.MergeAttribute("href", localisationUrl);
					localisationLink.InnerHtml = localisationUrl;

					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
					clientMail.To = booking.Client.Email;
					clientMail.Subject = string.Format(Worki.Resources.Email.BookingString.CalandarBookingModificationSubject, booking.Offer.Localisation.Name);
					clientMail.ToName = booking.Client.MemberMainData.FirstName;
					clientMail.Content = string.Format(Worki.Resources.Email.BookingString.CalandarBookingModification,
														booking.Offer.Localisation.Name,
														booking.GetStartDate(),
														booking.GetEndDate(),
														dashboardLink,
														localisationLink);

					context.Commit();

					clientMail.Send();

					return Json("Drop success");
				}
				catch (Exception ex)
				{
					_Logger.Error("DropEvent", ex);
					context.Complete();
					throw new ModelStateException(ModelState);
				}
			}
			else
			{
				throw new ModelStateException(ModelState);
			}
		}

		/// <summary>
		/// Ajax action to handle Resizeevent
		/// </summary>
		/// <param name="id">id of the booking</param>
		/// <param name="dayDelta">day delta</param>
		/// <param name="minuteDelta">minute delta</param>
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleModelStateException]
        public virtual ActionResult ResizeEvent(int id, int dayDelta, int minuteDelta)
        {
            var context = ModelFactory.GetUnitOfWork();
            var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
            var booking = bRepo.Get(id);

            if (booking != null)
            {
                var newToDate = booking.ToDate.AddDays(dayDelta).AddMinutes(minuteDelta);
                if (!booking.CanModify(newToDate))
                    throw new ModelStateException(ModelState);

                try
                {
                    booking.ToDate = booking.ToDate.AddDays(dayDelta).AddMinutes(minuteDelta);

					//send mail to booking client
					var urlHelper = new UrlHelper(ControllerContext.RequestContext);
					var dashboardUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Home.Index());
					TagBuilder dashboardLink = new TagBuilder("a");
					dashboardLink.MergeAttribute("href", dashboardUrl);
					dashboardLink.InnerHtml = dashboardUrl;

					var localisationUrl = booking.Offer.Localisation.GetDetailFullUrl(Url);
					TagBuilder localisationLink = new TagBuilder("a");
					localisationLink.MergeAttribute("href", localisationUrl);
					localisationLink.InnerHtml = localisationUrl;

					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
					clientMail.To = booking.Client.Email;
					clientMail.Subject = string.Format(Worki.Resources.Email.BookingString.CalandarBookingModificationSubject, booking.Offer.Localisation.Name);
					clientMail.ToName = booking.Client.MemberMainData.FirstName;
					clientMail.Content = string.Format(	Worki.Resources.Email.BookingString.CalandarBookingModification,
														booking.Offer.Localisation.Name,
														booking.GetStartDate(),
														booking.GetEndDate(),
														dashboardLink,
														localisationLink);

                    context.Commit();

					clientMail.Send();

                    return Json("Resize success");
                }
                catch (Exception ex)
                {
                    _Logger.Error("ResizeEvent", ex);
                    context.Complete();
                    throw new ModelStateException(ModelState);
                }
            }
            else
            {
                throw new ModelStateException(ModelState);
            }
        }

		/// <summary>
		/// Action to create booking creation partial view for an offer
		/// </summary>
		/// <param name="id">id of the offer</param>
		/// <returns></returns>
		[ChildActionOnly]
		public virtual ActionResult CreateEvent(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			try
			{
				var context = ModelFactory.GetUnitOfWork();
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var member = mRepo.Get(memberId);
				Member.Validate(member);

                var clients = member.MemberClients.ToDictionary(mc => mc.ClientId, mc => mc.Client.GetFullDisplayName());
				var model = new CreateBookingModel { Booking = new MemberBooking { OfferId = id }, Clients = new SelectList(clients, "Key", "Value") };

				return PartialView(MVC.Backoffice.Localisation.Views._CreateBooking, model);
			}
			catch (Exception ex)
			{
				_Logger.Error("CreateEvent", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Action to show booking summary
		/// </summary>
		/// <param name="id">id of the booking</param>
		/// <returns></returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual PartialViewResult BookingSummary(int id)
		{
			try
			{
				var context = ModelFactory.GetUnitOfWork();
				var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
				var booking = bRepo.Get(id);

				return PartialView(MVC.Backoffice.Localisation.Views._BookingSummary, booking);
			}
			catch (Exception ex)
			{
				_Logger.Error("BookingSummary", ex);
				return null;
			}
		}

		/// <summary>
		/// Ajax action to handle Create event
		/// </summary>
		/// <param name="id">id of the booking</param>
		/// <param name="start">start date</param>
		/// <param name="end">end date</param>
		[AcceptVerbs(HttpVerbs.Post)]
		[HandleModelStateException]
		[ValidateOnlyIncomingValues]
		public virtual ActionResult CreateEvent(CreateBookingModel createBookingModel)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

			if (ModelState.IsValid)
			{
				try
				{
					var offer = oRepo.Get(createBookingModel.Booking.OfferId);

					MemberBookingLog log = new MemberBookingLog();
					log.CreatedDate = DateTime.UtcNow;
					log.EventType = (int)MemberBookingLog.BookingEvent.Creation;
					log.Event = "Booking Created From Calandar";
					createBookingModel.Booking.MemberBookingLogs.Add(log);

					offer.MemberBookings.Add(createBookingModel.Booking);

					context.Commit();

					var newContext = ModelFactory.GetUnitOfWork();
					var bRepo = ModelFactory.GetRepository<IBookingRepository>(newContext);
					var booking = bRepo.Get(createBookingModel.Booking.Id);

					//send mail to client
					var urlHelper = new UrlHelper(ControllerContext.RequestContext);
					var dashboardUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Home.Index());
					TagBuilder dashboardLink = new TagBuilder("a");
					dashboardLink.MergeAttribute("href", dashboardUrl);
					dashboardLink.InnerHtml = dashboardUrl;

					var localisationUrl = booking.Offer.Localisation.GetDetailFullUrl(Url);
					TagBuilder localisationLink = new TagBuilder("a");
					localisationLink.MergeAttribute("href", localisationUrl);
					localisationLink.InnerHtml = localisationUrl;

					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
					clientMail.To = booking.Client.Email;
					clientMail.Subject = string.Format(Worki.Resources.Email.BookingString.CalandarBookingCreationSubject, booking.Offer.Localisation.Name);
					clientMail.ToName = booking.Client.MemberMainData.FirstName;
					clientMail.Content = string.Format(Worki.Resources.Email.BookingString.CalandarBookingCreation,
														Localisation.GetOfferType(booking.Offer.Type),
														booking.Offer.Localisation.Name,
														booking.GetStartDate(),
														booking.GetEndDate(),
														dashboardLink,
														booking.Price,
														localisationLink);

					return Json(booking.GetCalandarEvent(Url));
				}
				catch (Exception ex)
				{
					_Logger.Error("CreateEvent", ex);
					context.Complete();
					throw new ModelStateException(ModelState);
				}
			}
			else
			{
				throw new ModelStateException(ModelState);
			}
		}

		#endregion

		#endregion		
    }
}
