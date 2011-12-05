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
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    [Authorize]
    public partial class LocalisationController : Controller
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
                        TempData[MiscHelpers.TempDataConstants.Info] = "Ce lieu n'a pas encore d'offre";
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
						TempData[MiscHelpers.TempDataConstants.Info] = "Ce lieu n'a pas encore d'offre";
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
            model.Add(new OfferMenuItem { Selected = (int)OfferMenuType.Config == selected, Text = Worki.Resources.Menu.Menu.Configure, Link = Url.Action(MVC.Backoffice.Localisation.ConfigureOffer(offer.LocalisationId, offer.Id)) });
            if (offer.CanHaveBooking)
                model.Add(new OfferMenuItem { Selected = (int)OfferMenuType.Booking == selected, Text = Worki.Resources.Menu.Menu.CurrentBookings, Link = Url.Action(MVC.Backoffice.Localisation.OfferBooking(offer.Id)) });
            if (offer.CanHaveQuotation)
                model.Add(new OfferMenuItem { Selected = (int)OfferMenuType.Quotation == selected, Text = Worki.Resources.Menu.Menu.Quoations, Link = Url.Action(MVC.Backoffice.Localisation.OfferQuotation(offer.Id)) });

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

			var model = new OfferDropDownModel { Offer = offer };
			var type = (OfferMenuType)selected;
			switch(type)
			{
				case OfferMenuType.Config:
					model.UrlMaker = o => Url.Action(MVC.Backoffice.Localisation.ConfigureOffer(o.LocalisationId, o.Id));
					break;
				case OfferMenuType.Booking:
					model.UrlMaker = o => Url.Action(MVC.Backoffice.Localisation.OfferBooking(o.Id));
					break;
				case OfferMenuType.Quotation:
					model.UrlMaker = o => Url.Action(MVC.Backoffice.Localisation.OfferQuotation(o.Id));
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

                var bookings = bRepo.GetMany(b => b.Offer.LocalisationId == id && b.StatusId == (int)MemberBooking.Status.Unknown).OrderByDescending(b => b.CreationDate);
				var model = new LocalisationBookingViewModel
				{
					Localisation = loc,
					Bookings = new PagingList<MemberBooking>
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

		/// <summary>
		/// Get action method to show bookings of the owner, for a given localisation and offer
		/// </summary>
		/// <param name="offerid">id of the offer</param>
		/// <returns>View containing the bookings</returns>
		public virtual ActionResult OfferBooking(int id, int page = 1)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var p = page;
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				var offer = oRepo.Get(id);
				Member.ValidateOwner(member, offer.Localisation);

				var model = new OfferBookingViewModel
				{
					Offer = offer,
					Bookings = new PagingList<MemberBooking>
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
                    userLink.InnerHtml = "espace utilisateur";
					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
					clientMail.To = booking.Client.Email;
					clientMail.Subject = Worki.Resources.Email.BookingString.AcceptBookingClientSubject;
					clientMail.ToName = booking.Client.MemberMainData.FirstName;
                    clientMail.Content = string.Format(Worki.Resources.Email.BookingString.AcceptBookingClient,
														Localisation.GetOfferType(booking.Offer.Type),
                                                        CultureHelpers.GetSpecificFormat(booking.FromDate, CultureHelpers.TimeFormat.General),
                                                        CultureHelpers.GetSpecificFormat(booking.ToDate, CultureHelpers.TimeFormat.General),
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
		public virtual ActionResult RefuseBooking(int id)
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
		public virtual ActionResult RefuseBooking(int id, OfferModel<MemberBooking> formModel)
		{
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

                    //send mail to owner
					//useless

                    //send mail to client
					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
					clientMail.To = booking.Client.Email;
                    clientMail.Subject = Worki.Resources.Email.BookingString.RefuseMailSubject;
					clientMail.ToName = booking.Client.MemberMainData.FirstName;
                    clientMail.Content = string.Format(Worki.Resources.Email.BookingString.RefuseBookingClient,
                                                     Localisation.GetOfferType(booking.Offer.Type),
                                                     CultureHelpers.GetSpecificFormat(booking.FromDate, CultureHelpers.TimeFormat.Date),
                                                     CultureHelpers.GetSpecificFormat(booking.ToDate, CultureHelpers.TimeFormat.Date),
                                                     booking.Offer.Localisation.Name,
                                                     booking.Offer.Localisation.Adress,
                                                     formModel.InnerModel.Response);
					

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
					Localisation = loc,
					Quotations = new PagingList<MemberQuotation>
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
		/// <param name="id">id of the offer</param>
		/// <returns>View containing the quotations</returns>
		public virtual ActionResult OfferQuotation(int id, int page = 1)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var p = page;
			try
			{
				var member = mRepo.Get(memberId);
				var offer = oRepo.Get(id);
				Member.Validate(member);
				Member.ValidateOwner(member, offer.Localisation);

				var model = new OfferQuotationViewModel
				{
					Offer = offer,
					Quotations = new PagingList<MemberQuotation>
					{
						List = offer.MemberQuotations.Skip((p - 1) * PagedListViewModel.PageSize).Take(PagedListViewModel.PageSize).ToList(),
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
        public virtual ActionResult RefuseQuotation(int id)
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

				return View(new OfferModel<MemberQuotation> { InnerModel = quotation, OfferModelId = quotation.OfferId });
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
		public virtual ActionResult RefuseQuotation(int id, OfferModel<MemberQuotation> formModel)
        {
            var context = ModelFactory.GetUnitOfWork();
            var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
			var memberId = WebHelper.GetIdentityId(User.Identity);

            if (ModelState.IsValid)
            {
                try
                {
					var quotation = qRepo.Get(id);
					UpdateModel(quotation, "InnerModel");
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

                    //send mail to client
                    dynamic clientMail = new Email(MVC.Emails.Views.Email);
                    clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
                    clientMail.To = quotation.Client.Email;
                    clientMail.Subject = Worki.Resources.Email.BookingString.RefuseMailSubject;
                    clientMail.ToName = quotation.Client.MemberMainData.FirstName;
                    clientMail.Content = string.Format(Worki.Resources.Email.BookingString.RefuseQuotationClient,
                                                     Localisation.GetOfferType(quotation.Offer.Type),
                                                     quotation.Offer.Localisation.Name,
                                                     quotation.Offer.Localisation.Adress,
                                                     formModel.InnerModel.Response);

                    context.Commit();

                    clientMail.Send();

                    TempData[MiscHelpers.TempDataConstants.Info] = "La demande de devis a été refusée";

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
    }
}
