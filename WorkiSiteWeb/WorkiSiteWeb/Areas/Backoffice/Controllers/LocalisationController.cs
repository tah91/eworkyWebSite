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

		/// <summary>
		/// Get action result to show recent activities of the owner localisation
		/// </summary>
		/// <returns>View with recent activities</returns>
		public virtual ActionResult Index(int id)
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
				if (loc.OwnerID != memberId)
					throw new Exception(Worki.Resources.Validation.ValidationString.InvalidUser);

				return View(loc);
			}
			catch (Exception ex)
			{
				_Logger.Error("Index", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

        /// <summary>
        /// Get action method to show bookings of the owner, for a given localisation
        /// </summary>
        /// <param name="id">id of the localisation</param>
        /// <returns>View containing the bookings</returns>
        public virtual ActionResult Booking(int id, int page=1)
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
                if (loc.OwnerID != memberId)
                    throw new Exception(Worki.Resources.Validation.ValidationString.InvalidUser);

				var bookings = bRepo.GetMany(b => b.Offer.LocalisationId == id);
                var model = new LocalisationBookingViewModel
                {
                    Localisation = loc,
                    Bookings = new PagingList<MemberBooking>
                    {
						List = bookings.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
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
        /// Get action method to show quotations of the owner, for a given localisation
        /// </summary>
        /// <param name="id">id of the localisation</param>
        /// <returns>View containing the quotations</returns>
        public virtual ActionResult Quotation(int id, int page=1)
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
                if (loc.OwnerID != memberId)
                    throw new Exception(Worki.Resources.Validation.ValidationString.InvalidUser);

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
                
                if (offer.Localisation.OwnerID != memberId)
					throw new Exception(Worki.Resources.Validation.ValidationString.InvalidUser);

                return View(offer);
			}
			catch (Exception ex)
			{
				_Logger.Error("Index", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

        /// <summary>
        /// Get action method to show bookings of the owner, for a given localisation and offer
        /// </summary>
        /// <param name="offerid">id of the offer</param>
        /// <returns>View containing the bookings</returns>
        public virtual ActionResult OfferBooking(int id, int page=1)
        {
            var memberId = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
            var p = page ;
            try
            {
                var member = mRepo.Get(memberId);
                Member.Validate(member);
                var offer = oRepo.Get(id);
                if (offer.Localisation.OwnerID != memberId)
                    throw new Exception(Worki.Resources.Validation.ValidationString.InvalidUser);

                var model = new OfferBookingViewModel
                {
                    Offer = offer,
                    Bookings = new PagingList<MemberBooking>
                    {
                        List = offer.MemberBookings.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
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
                Member.Validate(member);
                var offer = oRepo.Get(id);
                if (offer.Localisation.OwnerID != memberId)
                    throw new Exception(Worki.Resources.Validation.ValidationString.InvalidUser);

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

        /// <summary>
        /// GET Action result to configure offer
        /// </summary>
        /// <param name="id">id of offer</param>
        /// <returns>View containing offer data</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult ConfigureOffer(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
            var offer = oRepo.Get(id);
            return View(new OfferFormViewModel { Offer = offer });
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
            var context = ModelFactory.GetUnitOfWork();
            var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
            if (ModelState.IsValid)
            {
                try
                {
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
            return View(formData);
        }

		/// <summary>
		/// GET Action result to confirm booking
		/// </summary>
		/// <param name="id">id of booking to confirm</param>
		/// <returns>View to fill booking data</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult ConfirmBooking(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var booking = bRepo.Get(id);
			return View(booking);
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

					//send mail to owner
					var owner = booking.Offer.Localisation.Member;
					dynamic ownerMail = new Email(MVC.Emails.Views.Email);
					ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
					ownerMail.To = owner.Email;
					ownerMail.Subject = Worki.Resources.Email.BookingString.ConfirmMailSubject;
					ownerMail.ToName = owner.MemberMainData.FirstName;
					ownerMail.Content = string.Format(Worki.Resources.Email.BookingString.ConfirmMailBody,
														Localisation.GetOfferType(booking.Offer.Type),
														string.Format("{0:dd/MM/yyyy HH:MM}", booking.FromDate),
														string.Format("{0:dd/MM/yyyy HH:MM}", booking.ToDate),
														booking.Offer.Localisation.Name,
														booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City,
														booking.Price);
					ownerMail.Send();

					//send mail to client
					var client = booking.Member;
					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
					clientMail.To = client.Email;
					clientMail.Subject = Worki.Resources.Email.BookingString.ConfirmMailSubject;
					clientMail.ToName = client.MemberMainData.FirstName;
					clientMail.Content = string.Format(Worki.Resources.Email.BookingString.ConfirmMailBody,
														Localisation.GetOfferType(booking.Offer.Type),
														string.Format("{0:dd/MM/yyyy HH:MM}", booking.FromDate),
														string.Format("{0:dd/MM/yyyy HH:MM}", booking.ToDate),
														booking.Offer.Localisation.Name,
														booking.Offer.Localisation.Adress + ", " + booking.Offer.Localisation.PostalCode + " " + booking.Offer.Localisation.City,
														booking.Price);
					clientMail.Send();

					context.Commit();

					return RedirectToAction(MVC.Backoffice.Localisation.OfferBooking(booking.OfferId));
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
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var booking = bRepo.Get(id);
			return View(new RefuseBookingFormViewModel { MemberBooking = booking });
		}

		/// <summary>
		/// POST Action result to confirm booking
		/// </summary>
		/// <param name="id">id of booking to confirm</param>
		/// <returns>View to fill booking data</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult RefuseBooking(int id, RefuseBookingFormViewModel formModel)
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

					//send mail to owner
					var owner = booking.Offer.Localisation.Member;
					dynamic ownerMail = new Email(MVC.Emails.Views.Email);
					ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
					ownerMail.To = owner.Email;
					ownerMail.Subject = Worki.Resources.Email.BookingString.ConfirmMailSubject;
					ownerMail.ToName = owner.MemberMainData.FirstName;
					ownerMail.Content = formModel.Message;
					ownerMail.Send();

					//send mail to client
					var client = booking.Member;
					dynamic clientMail = new Email(MVC.Emails.Views.Email);
					clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.BookingMail + ">";
					clientMail.To = client.Email;
					clientMail.Subject = Worki.Resources.Email.BookingString.ConfirmMailSubject;
					clientMail.ToName = client.MemberMainData.FirstName;
					clientMail.Content = formModel.Message;
					clientMail.Send();

					context.Commit();

					return RedirectToAction(MVC.Backoffice.Localisation.OfferBooking(booking.OfferId));
				}
				catch (Exception ex)
				{
					_Logger.Error("RefuseBooking", ex);
					context.Complete();
				}
			}
			return View(formModel);
		}
    }
}
