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
	public partial class LocalisationController : BackofficeControllerBase
    {
        public LocalisationController(ILogger logger, IObjectStore objectStore)
            : base(logger, objectStore)
        {
            
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

				var bookings = bRepo.GetLocalisationProducts(id);
                var quotations = qRepo.GetLocalisationProducts(id);

				var news = ModelHelper.GetNews(memberId, bookings, mbl => { return Url.Action(MVC.Backoffice.Localisation.ReadBookingLog(mbl.Id)); });
				news = news.Concat(ModelHelper.GetNews(memberId, quotations, ql => { return Url.Action(MVC.Backoffice.Localisation.ReadQuotationLog(ql.Id)); })).ToList();

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
		/// GET action to adit an existing localisation
		/// </summary>
		/// <returns>The form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult Edit(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				var loc = lRepo.Get(id);
				Member.ValidateOwner(member, loc);

				return View(new LocalisationFormViewModel(loc));
			}
			catch (Exception ex)
			{
				_Logger.Error("Edit", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		const string LocalisationPrefix = "Localisation";

		/// <summary>
		/// POST action to edit a localisation from bo
		/// </summary>
		/// <param name="localisation">The localisation data from the form (provided from custom model binder)</param>
		/// <param name="id">The id of the edited localisation</param>
		/// <returns>the detail view of localistion if ok, the form with errors else</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Edit(LocalisationFormViewModel localisationForm, int id, string addOffer)
		{
			var error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
			var field = string.Empty;
			//to keep files state in case of error
            _ObjectStore.Store<PictureDataContainer>(PictureData.GetKey(ProviderType.Localisation), new PictureDataContainer(localisationForm.Localisation));
			
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			try
			{
				var member = mRepo.GetMember(User.Identity.Name);
				Member.Validate(member);
				if (ModelState.IsValid)
				{
					var loc = lRepo.Get(id);
					UpdateModel(loc, LocalisationPrefix);
					loc.MemberEditions.Add(new MemberEdition { ModificationDate = DateTime.UtcNow, MemberId = member.MemberId, ModificationType = (int)EditionType.Edition });
					var offerCount = loc.Offers.Count;

					if (string.IsNullOrEmpty(addOffer) && !localisationForm.IsFreeLocalisation && offerCount == 0)
					{
						error = Worki.Resources.Views.Localisation.LocalisationString.MustAddOffer;
						field = "NewOfferType";
						throw new Exception(error);
					}
					context.Commit();
                    _ObjectStore.Delete(PictureData.GetKey(ProviderType.Localisation));

					if (!string.IsNullOrEmpty(addOffer))
					{
						return RedirectToAction(MVC.Offer.Create(id, localisationForm.NewOfferType,Url.Action(MVC.Backoffice.Localisation.Index(id))));
					}
					else
					{
						TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.LocHaveBeenEdit;
						return RedirectToAction(MVC.Backoffice.Localisation.Index(id));
					}
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("Edit", ex);
				context.Complete();
				ModelState.AddModelError(field, error);
			}
			return View(new LocalisationFormViewModel(localisationForm.Localisation));
		}

		[ChildActionOnly]
		public virtual ActionResult HorizontalMenu(int id, LocalisationMainMenu selected)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var loc = lRepo.Get(id);

			var model = new LocalisationMenuIndex { MenuItem = selected, Id = loc.ID, Title = loc.Name };

			return PartialView(MVC.Backoffice.Localisation.Views._LocalisationMenu, model);
		}

		[ChildActionOnly]
		public virtual ActionResult VerticalMenu(int id, int selected)
		{
			var model = new List<LinkMenuItem>();
			model.Add(new LinkMenuItem { Selected = (int)LocalisationMenu.Home == selected, Text = Worki.Resources.Menu.Menu.BONews, Link = Url.Action(MVC.Backoffice.Localisation.Index(id)) });
			model.Add(new LinkMenuItem { Selected = (int)LocalisationMenu.Bookings == selected, Text = Worki.Resources.Menu.Menu.CurrentBookings, Link = Url.Action(MVC.Backoffice.Localisation.Booking(id)) });
			model.Add(new LinkMenuItem { Selected = (int)LocalisationMenu.Quotations == selected, Text = Worki.Resources.Menu.Menu.WorkingQuotation, Link = Url.Action(MVC.Backoffice.Localisation.Quotation(id)) });
			model.Add(new LinkMenuItem { Selected = (int)LocalisationMenu.Edit == selected, Text = Worki.Resources.Menu.Menu.EditPlace, Link = Url.Action(MVC.Backoffice.Localisation.Edit(id)) });

			return PartialView(MVC.Backoffice.Shared.Views._LinkVerticalMenu, model);
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

                var bookings = bRepo.GetLocalisationProducts(id).OrderByDescending(b => b.CreationDate);
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
		/// Get action method to read booking log
		/// </summary>
		/// <returns>redirect to booking detail</returns>
		public virtual ActionResult ReadBookingLog(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var blRepo = ModelFactory.GetRepository<IBookingLogRepository>(context);
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				var bookingLog = blRepo.Get(id);
				if (!bookingLog.Read)
				{
					bookingLog.Read = true;
					context.Commit();
				}

				return RedirectToAction(MVC.Backoffice.Localisation.BookingDetail(bookingLog.MemberBookingId));
			}
			catch (Exception ex)
			{
				context.Complete();
				_Logger.Error("ReadBookingLog", ex);
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
				return View(new LocalisationModel<MemberBooking> { InnerModel = booking, LocalisationModelId = booking.Offer.LocalisationId });
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
		public virtual ActionResult ConfirmBooking(int id, LocalisationModel<MemberBooking> memberBooking)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);

			if (ModelState.IsValid)
			{
				try
				{
					var booking = bRepo.Get(id);
					TryUpdateModel(booking, "InnerModel");
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
                return View(new LocalisationModel<RefuseBookingModel> { InnerModel = model, LocalisationModelId = booking.Offer.LocalisationId });
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
		public virtual ActionResult RefuseBooking(int id, LocalisationModel<RefuseBookingModel> formModel, string confirm)
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

                var quotations = qRepo.GetLocalisationProducts(id).OrderByDescending(q => q.CreationDate);
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
        public virtual ActionResult QuotationDetail(int id, bool paypal = false)
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

                if (paypal)
                {
                    TempData[MiscHelpers.TempDataConstants.Info] = string.Format(Worki.Resources.Views.Booking.BookingString.ContactAvailable, quotation.Offer.Name);
                }

                return View(quotation);
            }
            catch (Exception ex)
            {
                _Logger.Error("QuotationDetail", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

		/// <summary>
		/// Get action method to read quotation log
		/// </summary>
		/// <returns>redirect to quotation detail</returns>
		public virtual ActionResult ReadQuotationLog(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var qlRepo = ModelFactory.GetRepository<IQuotationLogRepository>(context);
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				var quotationLog = qlRepo.Get(id);
				if (!quotationLog.Read)
				{
					quotationLog.Read = true;
					context.Commit();
				}

				return RedirectToAction(MVC.Backoffice.Localisation.QuotationDetail(quotationLog.MemberQuotationId));
			}
			catch (Exception ex)
			{
				context.Complete();
				_Logger.Error("ReadQuotationLog", ex);
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
				return View(new LocalisationModel<RefuseQuotationModel> { InnerModel = model, LocalisationModelId = quotation.Offer.LocalisationId });
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
		public virtual ActionResult RefuseQuotation(int id, LocalisationModel<RefuseQuotationModel> formModel, string confirm)
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
	}
}
