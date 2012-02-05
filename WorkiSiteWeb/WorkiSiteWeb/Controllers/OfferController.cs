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

namespace Worki.Web.Controllers
{
	public partial class OfferController : ControllerBase
	{
		#region Private

		ILogger _Logger;
        IMembershipService _MembershipService;

		#endregion

		public OfferController( ILogger logger,
                                IMembershipService membershipService)
		{
			_Logger = logger;
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

			return View(new OfferFormViewModel(loc.IsSharedOffice(), Roles.IsUserInRole(MiscHelpers.AdminConstants.AdminRole)) { Offer = new Offer { LocalisationId = id, Type = type } });
		}

		/// <summary>
		/// Post Action result to add offer
		/// </summary>
		/// <returns>View containing localisation form</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Create(int id, string returnUrl, OfferFormViewModel offerFormViewModel)
        {
            TempData[PictureData.PictureDataString] = new PictureDataContainer(offerFormViewModel.Offer);
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
						TempData.Remove(PictureData.PictureDataString);
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
			return View(MVC.Offer.Views.Create, new OfferFormViewModel(offer.Localisation.IsSharedOffice(), Roles.IsUserInRole(MiscHelpers.AdminConstants.AdminRole)) { Offer = offer });
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
			var context = ModelFactory.GetUnitOfWork();
            TempData[PictureData.PictureDataString] = new PictureDataContainer(formData.Offer);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			if (ModelState.IsValid)
			{
				try
				{
					var o = oRepo.Get(id);
					UpdateModel(o, "Offer");
					context.Commit();
					TempData.Remove(PictureData.PictureDataString);
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
		public virtual PartialViewResult AddOfferPrice()
		{
			return PartialView(MVC.Offer.Views._OfferPrice, new OfferPrice());
		}

        void SetFavoritePlaces(PartyRegisterFormViewModel model)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var admin = mRepo.Get(mRepo.GetAdminId());
            var locDict = admin.FavoriteLocalisations.ToDictionary(fl => fl.LocalisationId, fl => fl.Localisation.Name);

            model.FavoritePlaceNames = string.Join(", ", locDict.Values);

            locDict.Add(-1, "eWorker du dimanche");

            model.FavoritePlaces= new SelectList(locDict, "Key", "Value");
        }

        /// <summary>
        /// GET Action result to show join party form
        /// </summary>
        /// <param name="id">id of offer to join</param>
        /// <returns>View containing join party form</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult JoinParty(int id)
        {
            var memberId = WebHelper.GetIdentityId(User.Identity);
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
            var offer = oRepo.Get(id);
            var member = mRepo.Get(memberId);

            var formModel = new PartyRegisterFormViewModel(member, offer);
            SetFavoritePlaces(formModel);

            return View(formModel);
        }

        /// <summary>
        /// Post Action result to add join party request
        /// </summary>
        /// <returns>View containing join party form</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult JoinParty(int id, PartyRegisterFormViewModel formData)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
            var memberId = WebHelper.GetIdentityId(User.Identity);
            var member = mRepo.Get(memberId);
            var offer = oRepo.Get(id);

            if (ModelState.IsValid)
            {
                var sendNewAccountMail = false;
                try
                {
                    var memberData = new MemberMainData
                    {
                        FirstName = formData.FirstName,
                        LastName = formData.LastName,
                        PhoneNumber = formData.PhoneNumber,
                    };
                    sendNewAccountMail = _MembershipService.TryCreateAccount(formData.Email, memberData, out memberId);
                    member = mRepo.Get(memberId);

                    var locName = offer.Localisation.Name;
                    try
                    {
                        formData.MemberQuotation.MemberId = memberId;
                        formData.MemberQuotation.OfferId = id;

                        //set member data
                        member.MemberMainData.PhoneNumber = formData.PhoneNumber;
                        member.MemberMainData.Description = formData.Description;
                        var favLoc = lRepo.Get(formData.FavoritePlaceId);
                        if (favLoc != null && member.FavoriteLocalisations.Count(fl => fl.LocalisationId == favLoc.ID) == 0)
                        {
                            member.FavoriteLocalisations.Add(new FavoriteLocalisation { LocalisationId = favLoc.ID });
                        }
                        member.MemberQuotations.Add(formData.MemberQuotation);

                        dynamic newMemberMail = null;
                        if (sendNewAccountMail)
                        {
                            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
                            var editprofilUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit());
                            TagBuilder profilLink = new TagBuilder("a");
                            profilLink.MergeAttribute("href", editprofilUrl);
                            profilLink.InnerHtml = Worki.Resources.Views.Account.AccountString.EditMyProfile;

                            var editpasswordUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit());
                            TagBuilder passwordLink = new TagBuilder("a");
                            passwordLink.MergeAttribute("href", editpasswordUrl);
                            passwordLink.InnerHtml = Worki.Resources.Views.Account.AccountString.ChangeMyPassword;

                            newMemberMail = new Email(MVC.Emails.Views.Email);
                            newMemberMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                            newMemberMail.To = formData.Email;
                            newMemberMail.ToName = formData.FirstName;

                            newMemberMail.Subject = Worki.Resources.Email.BookingString.PartyCreateAccountSubject;
							newMemberMail.Content = string.Format(Worki.Resources.Email.BookingString.PartyCreateAccount,
																	formData.Email,
																	_MembershipService.GetPassword(formData.Email, null),
																	passwordLink,
																	profilLink);
                        }

                        //send mail to team
                        dynamic teamMail = new Email(MVC.Emails.Views.Email);
                        teamMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                        teamMail.To = MiscHelpers.EmailConstants.BookingMail;
                        teamMail.Subject = Worki.Resources.Email.BookingString.PartyRegisterTeamSubject;
                        teamMail.ToName = MiscHelpers.EmailConstants.ContactDisplayName;
						teamMail.Content = string.Format(Worki.Resources.Email.BookingString.PartyRegisterTeam,
														 string.Format("{0} {1}", member.MemberMainData.FirstName, member.MemberMainData.LastName),
														 formData.PhoneNumber,
														 member.Email,
														 locName);

                        //send mail to booking member
                        dynamic clientMail = new Email(MVC.Emails.Views.Email);
                        clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                        clientMail.To = member.Email;
                        clientMail.Subject = Worki.Resources.Email.BookingString.PartyRegisterSubject;
                        clientMail.ToName = member.MemberMainData.FirstName;
						clientMail.Content = string.Format(Worki.Resources.Email.BookingString.PartyRegister,
															locName,
															offer.Localisation.Adress);

                        context.Commit();

                        if (sendNewAccountMail)
                        {
                            newMemberMail.Send();
                        }
                        clientMail.Send();
                        teamMail.Send();
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex.Message);
                        context.Complete();
                        throw ex;
                    }

					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Booking.BookingString.PartyRegisterConfirmation;
                    return Redirect(offer.Localisation.GetDetailFullUrl(Url));
                }
                catch (Exception ex)
                {
                    _Logger.Error("Create", ex);
                    ModelState.AddModelError("", ex.Message);
                }
            }
            SetFavoritePlaces(formData);
            formData.QuotationOffer = offer;
            return View(formData);
        }
    }
}
