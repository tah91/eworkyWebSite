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

        public const int PageSize = 5;

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
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				var loc = lRepo.Get(id);
				Member.ValidateOwner(member, loc);

				var bookings = bRepo.GetMany(b => b.Offer.LocalisationId == id);
				var news = ModelHelper.GetNews(bookings, mb => { return Url.Action(MVC.Backoffice.Localisation.BookingDetail(mb.Id)); });
				news = news.OrderByDescending(n => n.Date).Take(10).ToList();

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
		/// <param name="id">id of offer</param>
		/// <returns>View containing offer data</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult ConfigureOffer(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

			try
			{
				var member = mRepo.Get(memberId);
				var offer = oRepo.Get(id);
				Member.Validate(member);
				Member.ValidateOwner(member, offer.Localisation);

				return View(new OfferFormViewModel { Offer = offer });
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
		public virtual ActionResult ConfigureOffer(int id, OfferFormViewModel formData)
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
					if (formData.Offer.IsBookable && string.IsNullOrEmpty(member.MemberMainData.PaymentAddress))
					{
						throw new Exception();
					}

					var o = oRepo.Get(id);
					UpdateModel(o, "Offer");
					context.Commit();
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Offer.OfferString.OfferEdited;
					return RedirectToAction(MVC.Backoffice.Localisation.OfferIndex(o.LocalisationId, o.Id));
				}
				catch (Exception ex)
				{
					_Logger.Error("Edit", ex);
					context.Complete();
					ModelState.AddModelError("", ex.Message);
				}
			}
            TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.NeedInfoPaypal;
            return RedirectToAction(MVC.Backoffice.Localisation.ConfigureOffer(id));
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

                var bookings = bRepo.GetMany(b => b.Offer.LocalisationId == id && b.StatusId == (int)MemberBooking.Status.Unknown);
				var model = new LocalisationBookingViewModel
				{
					Localisation = loc,
					Bookings = new PagingList<MemberBooking>
					{
                        List = bookings.OrderByDescending(mb => mb.CreationDate).Skip((p - 1) * PageSize).Take(PageSize).ToList(),
						PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = bookings.Count }
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
                        List = offer.MemberBookings.OrderByDescending(mb => mb.CreationDate).Skip((p - 1) * PageSize).Take(PageSize).ToList(),
						PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = offer.MemberBookings.Count }
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
				return View(booking);
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
		public virtual ActionResult ConfirmBooking(int id, MemberBooking memberBooking)
		{
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);

			if (ModelState.IsValid)
			{
				try
				{
					var booking = bRepo.Get(id);
					UpdateModel(booking);
					booking.StatusId = (int)MemberBooking.Status.Accepted;
					booking.MemberBookingLogs.Add(new MemberBookingLog
					{
						CreatedDate = DateTime.UtcNow,
						Event = "Booking Confirmed",
						EventType = (int)MemberBookingLog.BookingEvent.Approval
					});

					//send mail to owner
					dynamic ownerMail = new Email(MVC.Emails.Views.Email);
					ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
					ownerMail.To = booking.Owner.Email;
					ownerMail.Subject = Worki.Resources.Email.BookingString.ConfirmMailSubject;
					ownerMail.ToName = booking.Owner.MemberMainData.FirstName;
					ownerMail.Content = string.Format(Worki.Resources.Email.BookingString.AcceptBookingOwner,
														Localisation.GetOfferType(booking.Offer.Type),
														CultureHelpers.GetSpecificFormat(booking.FromDate, CultureHelpers.TimeFormat.General),
														CultureHelpers.GetSpecificFormat(booking.ToDate, CultureHelpers.TimeFormat.General),
														booking.Offer.Localisation.Name,
														booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City,
														booking.Price);
					ownerMail.Send();

                    //send mail to client
					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
					clientMail.To = booking.Client.Email;
					clientMail.Subject = Worki.Resources.Email.BookingString.ConfirmMailSubject;
					clientMail.ToName = booking.Client.MemberMainData.FirstName;
                    clientMail.Content = string.Format(Worki.Resources.Email.BookingString.AcceptBookingClient,
														Localisation.GetOfferType(booking.Offer.Type),
                                                        CultureHelpers.GetSpecificFormat(booking.FromDate, CultureHelpers.TimeFormat.General),
                                                        CultureHelpers.GetSpecificFormat(booking.ToDate, CultureHelpers.TimeFormat.General),
														booking.Offer.Localisation.Name,
														booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City,
														booking.Price);
					clientMail.Send();

					context.Commit();

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

				return View(booking);
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
		public virtual ActionResult RefuseBooking(int id, MemberBooking formModel)
		{
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);

			if (ModelState.IsValid)
			{
				try
				{
					var booking = bRepo.Get(id);
					UpdateModel(booking);
					booking.StatusId = (int)MemberBooking.Status.Refused;
					booking.MemberBookingLogs.Add(new MemberBookingLog
					{
						CreatedDate = DateTime.UtcNow,
						Event = "Booking Refused",
						EventType = (int)MemberBookingLog.BookingEvent.Refusal
					});

                    //send mail to owner
					dynamic ownerMail = new Email(MVC.Emails.Views.Email);
					ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
					ownerMail.To = booking.Owner.Email;
                    ownerMail.Subject = Worki.Resources.Email.BookingString.RefuseMailSubject;
					ownerMail.ToName = booking.Owner.MemberMainData.FirstName;
                    ownerMail.Content = string.Format(Worki.Resources.Email.BookingString.RefuseBookingOwner,
                                                     Localisation.GetOfferType(booking.Offer.Type),
                                                     CultureHelpers.GetSpecificFormat(booking.FromDate, CultureHelpers.TimeFormat.Date),
                                                     CultureHelpers.GetSpecificFormat(booking.ToDate, CultureHelpers.TimeFormat.Date),
                                                     booking.Offer.Localisation.Name,
                                                     booking.Offer.Localisation.Adress,
                                                     formModel.Response);
					ownerMail.Send();

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
                                                     formModel.Response);
					clientMail.Send();

					context.Commit();

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

				var quotations = qRepo.GetMany(b => b.Offer.LocalisationId == id);
				var model = new LocalisationQuotationViewModel
				{
					Localisation = loc,
					Quotations = new PagingList<MemberQuotation>
					{
						List = quotations.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
						PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = quotations.Count }
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
						List = offer.MemberQuotations.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
						PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = offer.MemberQuotations.Count }
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

		#endregion
    }
}
