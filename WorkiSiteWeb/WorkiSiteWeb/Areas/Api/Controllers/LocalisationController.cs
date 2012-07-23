using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure.Logging;
using Worki.Rest;
using Worki.Service;
using Worki.Web.Helpers;
using Worki.Memberships;
using Worki.Infrastructure.Repository;
using System.Web.Security;
using Worki.Infrastructure.Helpers;
using Postal;

namespace Worki.Web.Areas.Api.Controllers
{
    public partial class LocalisationController : Controller
    {
        ILocalisationRepository _LocalisationRepository;
        IMemberRepository _MemberRepository;
        ILogger _Logger;
        ISearchService _SearchService;
        IGeocodeService _GeocodeService;
        IMembershipService _MembershipService;

        public LocalisationController(ILocalisationRepository localisationRepository,
                                      IMemberRepository memberRepository,
                                      ILogger logger,
                                      ISearchService searchService,
                                      IGeocodeService geocodeService,
                                      IMembershipService membershipService)
        {
            _LocalisationRepository = localisationRepository;
            _MemberRepository = memberRepository;
            _Logger = logger;
            _SearchService = searchService;
            _GeocodeService = geocodeService;
            _MembershipService = membershipService;
        }

        static MiscHelpers.ImageSize _ImageSize = new MiscHelpers.ImageSize
        {
            Width = 250,
            Height = 250,
            TWidth = 80,
            THeight = 80
        };

        public virtual ActionResult Register(MemberApiModel formData)
        {
            if (ModelState.IsValid)
            {
                var context = ModelFactory.GetUnitOfWork();
                var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                var sendNewAccountMailPass = false; 
                var sendNewAccountMail = false;
				try
				{
                    int memberId;
                    var uploadedFileName = this.UploadFile(string.Format(MiscHelpers.FaceBookConstants.FacebookProfilePictureUrlPattern, formData.FacebookId), _ImageSize, Member.AvatarFolder);
					var memberData = new MemberMainData
					{
						FirstName = formData.FirstName,
						LastName = formData.LastName,
						PhoneNumber = formData.PhoneNumber,
                        BirthDate = formData.BirthDate,
                        Avatar = uploadedFileName,
                        Facebook = formData.FacebookLink
					};
                    if (string.IsNullOrEmpty(formData.Password))
                    {
                        sendNewAccountMailPass = _MembershipService.TryCreateAccount(formData.Email, memberData, out memberId);
                    }
                    else
                    {
                        sendNewAccountMail = _MembershipService.TryCreateAccount(formData.Email, formData.Password, memberData, out memberId);
                    }
					var member = mRepo.Get(memberId);

					try
					{
                        member.MemberMainData.PhoneNumber = formData.PhoneNumber;
						dynamic newMemberMail = null;
						if (sendNewAccountMailPass)
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

							newMemberMail.Subject = Worki.Resources.Email.BookingString.BookingNewMemberSubject;
                            /*
                            newMemberMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingNewMember,
                                                                    Localisation.GetOfferType(offer.Type),
                                                                    formData.MemberBooking.GetStartDate(),
                                                                    formData.MemberBooking.GetEndDate(),
                                                                    locName,
                                                                    offer.Localisation.Adress,
                                                                    formData.Email,
                                                                    _MembershipService.GetPassword(formData.Email, null),
                                                                    passwordLink,
                                                                    profilLink);
                             */
                            newMemberMail.Content = "Vous vous êtes bien inscrit via mobile.\nVotre mot de passe est : " +
                                _MembershipService.GetPassword(formData.Email, null) + " (" + passwordLink + ").";
						}
                        if (sendNewAccountMail)
                        {
                            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
                            var editprofilUrl = urlHelper.ActionAbsolute(MVC.Dashboard.Profil.Edit());
                            TagBuilder profilLink = new TagBuilder("a");
                            profilLink.MergeAttribute("href", editprofilUrl);
                            profilLink.InnerHtml = Worki.Resources.Views.Account.AccountString.EditMyProfile;

                            newMemberMail = new Email(MVC.Emails.Views.Email);
                            newMemberMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                            newMemberMail.To = formData.Email;
                            newMemberMail.ToName = formData.FirstName;

                            newMemberMail.Subject = Worki.Resources.Email.BookingString.BookingNewMemberSubject;
                            /*
                            newMemberMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingNewMember,
                                                                    Localisation.GetOfferType(offer.Type),
                                                                    formData.MemberBooking.GetStartDate(),
                                                                    formData.MemberBooking.GetEndDate(),
                                                                    locName,
                                                                    offer.Localisation.Adress,
                                                                    formData.Email,
                                                                    _MembershipService.GetPassword(formData.Email, null),
                                                                    passwordLink,
                                                                    profilLink);
                             */
                            newMemberMail.Content = "Vous vous êtes bien inscrit via mobile.\n";
                        }
						context.Commit();

						if (sendNewAccountMail)
						{
							newMemberMail.Send();
						}

                        var newContext = ModelFactory.GetUnitOfWork();
                        mRepo = ModelFactory.GetRepository<IMemberRepository>(newContext);
                        Member m = mRepo.GetMember(formData.Email);
                        AuthJson ret = new AuthJson
                        {
                            token = _MembershipService.GetToken(formData.Email),
                            email = m.Email,
                            name = m.MemberMainData.LastName,
                            firstname = m.MemberMainData.FirstName
                        };
                        return new ObjectResult<AuthJson>(ret);
					}
					catch (Exception ex)
					{
						_Logger.Error(ex.Message);
						context.Complete();
						throw ex;
					}
				}
				catch (Exception ex)
				{
					_Logger.Error("Create", ex);
					ModelState.AddModelError("", ex.Message);
                    return new ObjectResult<AuthJson>(null, 400, "Error in attempting to create.");
				}
            }
            else
            {
                return new ObjectResult<AuthJson>(null, 400, "More aguments needed.");
            }
        }

        public virtual ActionResult Connect(LogOnModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _MembershipService.ValidateUser(model.Login, model.Password);
                    var context = ModelFactory.GetUnitOfWork();
                    var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                    Member m = mRepo.GetMember(model.Login);
                    AuthJson ret = new AuthJson
                    {
                        token = _MembershipService.GetToken(model.Login),
                        email = m.Email,
                        name = m.MemberMainData.LastName,
                        firstname = m.MemberMainData.FirstName
                    };
                    return new ObjectResult<AuthJson>(ret);
                }
                catch (Exception ex)
                {
                    return new ObjectResult<AuthJson>(null, 400, ex.Message);
                }
            }
            else
            {
                return new ObjectResult<AuthJson>(null, 400, "Wrong login or password.");
            }
        }

        public virtual ActionResult Comment(int id, LogOnModel model, Comment com)
        {
            if (ModelState.IsValid && _MembershipService.ValidateUser(model.Login, model.Password))
            {
                var context = ModelFactory.GetUnitOfWork();
                var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
                var localisation = lRepo.Get(id);

                var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                var member = mRepo.GetMember(model.Login);
                var error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
                com.Localisation = localisation;
                com.PostUserID = member.MemberId;
                com.Date = System.DateTime.UtcNow;
                com.Validate(ref  error);

                localisation.Comments.Add(com);

                context.Commit();
                return new ObjectResult<LocalisationJson>(null, 200, "Ok");
            }
            else
            {
                return new ObjectResult<LocalisationJson>(null, 400, "Not found");
            }
        }

        /// <summary>
        /// Get action method to get localisation details
        /// </summary>
        /// <param name="id">localisation id</param>
        /// <returns>an object containing localisatiojn details, comments, and fans</returns>
        public virtual ActionResult Details(int id)
        {
            var localisation = _LocalisationRepository.Get(id);
            if (localisation == null)
                return new ObjectResult<LocalisationJson>(null, 400, "The id is not present in database");

            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
            var json = localisation.GetJson(this);
            return new ObjectResult<LocalisationJson>(json);
        }

        enum ApiFeatures
        {
            Wifi,
            Cofee,
            Resto,
            Parking,
            Handicap
        }

        static char[] _arrayTrim = { '[', ']' };

		void FillCriteria(ref SearchCriteria criteria, string types, string features)
		{
			//types
			if (!string.IsNullOrEmpty(types))
			{
				try
				{
					string[] typesArray = types.Trim(_arrayTrim).Split(',');
					foreach (var item in typesArray)
					{
						var intType = (LocalisationType)Int32.Parse(item);
						switch (intType)
						{
							case LocalisationType.SpotWifi:
								criteria.SpotWifi = true;
								break;
							case LocalisationType.CoffeeResto:
								criteria.CoffeeResto = true;
								break;
							case LocalisationType.Biblio:
								criteria.Biblio = true;
								break;
							case LocalisationType.PublicSpace:
								criteria.PublicSpace = true;
								break;
							case LocalisationType.TravelerSpace:
								criteria.TravelerSpace = true;
								break;
							case LocalisationType.Hotel:
								criteria.Hotel = true;
								break;
							case LocalisationType.Telecentre:
								criteria.Telecentre = true;
								break;
							case LocalisationType.BuisnessCenter:
								criteria.BuisnessCenter = true;
								break;
							case LocalisationType.CoworkingSpace:
								criteria.CoworkingSpace = true;
								break;
							case LocalisationType.WorkingHotel:
								criteria.WorkingHotel = true;
								break;
							case LocalisationType.PrivateArea:
								criteria.PrivateArea = true;
								break;
							case LocalisationType.SharedOffice:
								criteria.SharedOffice = true;
								break;
							default:
								break;
						}
					}
					criteria.Everything = false;
				}
				catch (Exception)
				{
					throw new Exception("The \"types\" parameter is not correctly filled");
				}
			}

			//features
			if (!string.IsNullOrEmpty(features))
			{
				try
				{
					//var offerId = Localisation.GetFeatureTypeFromOfferType(criteria.LocalisationOffer);
					string[] featuresArray = features.Trim(_arrayTrim).Split(',');
					foreach (var item in featuresArray)
					{
						var intType = (ApiFeatures)Int32.Parse(item);
						switch (intType)
						{
							case ApiFeatures.Wifi:
								criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Wifi_Free });
								break;
							case ApiFeatures.Cofee:
								criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Coffee });
								break;
							case ApiFeatures.Resto:
								criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Restauration });
								break;
							case ApiFeatures.Parking:
								criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Parking });
								break;
							case ApiFeatures.Handicap:
								criteria.LocalisationData.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Handicap });
								break;
							default:
								break;
						}
					}
				}
				catch (Exception)
				{
					throw new Exception("The \"features\" parameter is not correctly filled");
				}
			}

			criteria.OfferData.Type = -1;
		}

        /// <summary>
        /// get action result to search for localisation, for given criteria
        /// </summary>
        /// <param name="place">an address near where to find localisations</param>
        /// <param name="offerType"></param>
        /// <param name="types"></param>
        /// <param name="features"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public virtual ActionResult Search(string place,
                                            string name,
                                            float latitude = 0,
                                            float longitude = 0,
                                            float boundary = 50,
                                            string offerType = null,
                                            int orderBy = 1,
                                            string types = null,
                                            string features = null,
                                            int maxCount = 30)
        {
            //validate
            if (string.IsNullOrEmpty(place) && (latitude == 0 || longitude == 0) && string.IsNullOrEmpty(name))
                return new ObjectResult<List<LocalisationJson>>(null, 400, "The \"place or name or latitude/longitude\" parameters must be filled");

            //fill from parameter
            eSearchType searchType = string.IsNullOrEmpty(name) ? eSearchType.ePerOffer : eSearchType.ePerName;
            eOrderBy order = string.IsNullOrEmpty(name) ? (eOrderBy)orderBy : eOrderBy.Rating;
            var criteria = new SearchCriteria { SearchType = searchType, OrderBy = order };

            criteria.Boundary = boundary;

            try
            {
                FillCriteria(ref  criteria, types, features);
            }
            catch (Exception ex)
            {
                return new ObjectResult<List<LocalisationJson>>(null, 400, ex.Message);
            }

            //place
            if (!string.IsNullOrEmpty(place))
            {
                criteria.Place = place;
                _GeocodeService.GeoCode(place, out latitude, out longitude);
                if (latitude == 0 || longitude == 0)
                    return new ObjectResult<List<LocalisationJson>>(null, 404, "The \"place\" can not be geocoded");
            }
            criteria.LocalisationData.Latitude = latitude;
            criteria.LocalisationData.Longitude = longitude;

            if (!string.IsNullOrEmpty(name))
                criteria.LocalisationData.Name = name;

            //search for matching localisations
            var results = _SearchService.FillSearchResults(criteria);

            //get the wanted offer types

            //take the json
            var list = results.List;
            if (offerType != null)
            {
                string[] offerTypeArray = offerType.Trim(_arrayTrim).Split(',');
                list = list.Where(p => p.GetOfferTypes().ToList().Any(x => offerTypeArray.Contains(((int) x).ToString()))).ToList();
            }

            List<LocalisationJson> neededLocs;
            if (string.IsNullOrEmpty(place)) // compute the distance, or not
                neededLocs = (from item in list.Take(maxCount) select item.GetJson(this)).ToList();
            else
                neededLocs = (from item in list.Take(maxCount) select item.GetJson(this, criteria)).ToList();

            return new ObjectResult<List<LocalisationJson>>(neededLocs);
        }
    }
}
