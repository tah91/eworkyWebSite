using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Worki.Infrastructure.Email;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Web.Helpers;
using Worki.Data.Repository;
using Worki.Infrastructure.Helpers;
using Worki.Data.Models;
using Worki.Memberships;
using Worki.Infrastructure;

namespace Worki.Web.Controllers
{
	[Authorize(Roles = MiscHelpers.AdminRole)]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    public partial class AdminController : Controller
    {
        ILocalisationRepository _LocalisationRepository;
        IMembershipService _MembershipService;
        IMemberRepository _MemberRepository;
        IVisitorRepository _VisitorRepository;
        IWelcomePeopleRepository _WelcomePeopleRepository;
		IRepository<MemberBooking> _BookingRepository;
        ILogger _Logger;
        IEmailService _EmailService;

        public AdminController( ILocalisationRepository localisationRepository, 
                                IMembershipService memberShipservice, 
                                IVisitorRepository visitorRepository,
                                ILogger logger,
                                IEmailService emailService,
                                IMemberRepository memberRepository,
                                IWelcomePeopleRepository welcomePeopleRepository,
								IRepository<MemberBooking> bookingRepository)
        {
            _LocalisationRepository = localisationRepository;
            _MembershipService = memberShipservice;
            _VisitorRepository = visitorRepository;
            _Logger = logger;
            _EmailService = emailService;
            _MemberRepository = memberRepository;
            _WelcomePeopleRepository = welcomePeopleRepository;
			_BookingRepository = bookingRepository;
        }

        public int PageSize = 25; // Will change this later

        #region Admin Localisation

        /// <summary>
        /// Prepares a web page containing a paginated list of localisations
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult Index(int? page)
        {
            var pageValue = page ?? 1;
			var localisations = _LocalisationRepository.Get((pageValue - 1) * PageSize, PageSize, l => l.ID);
            var viewModel = new AdminLocalisation(localisations.ToList())
            {
                Localisations = localisations,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = PageSize,
                    TotalItems = _LocalisationRepository.GetCount()
                }
            };
            return View(viewModel);
        }

        /// <summary>
        /// POST Action method to update "a la une" localisation list
        /// and redirect to localisation admin home
        /// </summary>
        /// <param name="collection">form containg the list of ids to push to "a la une"</param>
        /// <returns>Redirect to return url</returns>
        [HttpPost] 
        [ValidateAntiForgeryToken]
        public virtual ActionResult UpdateMainLocalisation(FormCollection collection, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // erase all values on the tables mainLocalisation inthe table
                    var listCollection = collection.AllKeys;
                    foreach (var key in listCollection)
                    {
                        var locId = 0;
                        try
                        {
                            locId = int.Parse(key);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        var value = collection[key].ToLower();
                        //var loc = _LocalisationRepository.GetLocalisation(locId);
                        _LocalisationRepository.Update(locId, loc =>
                            {
                                var values = value.Split(',');
                                var isMain = false;
                                var userName = string.Empty;
                                foreach (var item in values)
                                {
                                    if (string.IsNullOrEmpty(item))
                                        continue;
                                    if (string.Compare(item, "true", StringComparison.InvariantCultureIgnoreCase) == 0)
                                        isMain = true;
                                    //retrieve username
                                    if (item.Contains('@'))
                                    {
                                        var member = _MemberRepository.GetMember(item);
                                        if (member == null)
                                            throw new Exception();
                                        loc.OwnerID = member.MemberId;
                                    }
                                }
                                //case row is checked
                                if (isMain)
                                {
                                    if (loc.MainLocalisation == null)
                                        loc.MainLocalisation = new MainLocalisation { LocalisationID = locId };
                                }
                                else
                                {
                                    loc.MainLocalisation = null;
                                }
                            });
                    }
                }
                catch (Exception e)
                {
                    _Logger.Error(e.Message);
                    ModelState.AddModelError("", e.Message);
                }
            }
            // Redirection
            return Redirect(returnUrl);
        }

        #endregion 
        
        #region Admin User

        /// <summary>
        /// Prepares a web page containing a paginated list of members
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
		public virtual ActionResult IndexUser(int? page)
		{
			int pageValue = page ?? 1;
			var members = _MemberRepository.Get((pageValue - 1) * PageSize, PageSize, m => m.MemberId);
			var viewModel = new UserListViewModel()
			{
				ListMemberShip = _MembershipService.GetAdminMapping(members),
				PagingInfo = new PagingInfo
				 {
					 CurrentPage = pageValue,
					 ItemsPerPage = PageSize,
                     TotalItems = _MemberRepository.GetCount()
				 }
			};
			return View(viewModel);
		}

        /// <summary>
        /// POST Action method to update admin roles
        /// and redirect to user admin home
        /// </summary>
        /// <param name="collection">form containg the list of ids to push to admin role</param>
        /// <returns>Redirect to return url</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ChangeUserRole(FormCollection collection, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var listCollection = collection.AllKeys;
                    foreach (var username in listCollection)
                    {
                        var roleCheck = collection[username].ToLower();
                        var userInRole = Roles.IsUserInRole(username, MiscHelpers.AdminRole);
                        if (roleCheck.Contains("true"))
                        {
                            if (!userInRole)
                            {
                                Roles.AddUserToRole(username, MiscHelpers.AdminRole);
                            }
                        }
                        else
                        {
                            if (userInRole)
                            {
                                Roles.RemoveUserFromRole(username, MiscHelpers.AdminRole);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _Logger.Error(e.Message);
                }
            }
            // Redirection
            return Redirect(returnUrl);
        }


        /// <summary>
        /// Action method to delete a member
        /// </summary>
        /// <param name="username">user to delete</param>
        /// <returns>View to confirm the delete</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("supprimer-utilisateur")]
        public virtual ActionResult DeleteUser(string username, string returnUrl)
        {
            var user = _MembershipService.GetUser(username);
            if (user == null)
                return View(MVC.Admin.Views.utilisateur_absent);
            TempData["returnUrl"] = returnUrl;
            return View(new User { UserName = user.UserName });
        }

        /// <summary>
        /// POST Action method to delete a member
        /// and redirect to user admin home
        /// </summary>
        /// <param name="user">user to delete</param>
        /// <param name="returnUrl">url to redirect to</param>
        /// <returns>Redirect to return url</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("supprimer-utilisateur")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteUser(User user, string confirmButton,string returnUrl)
        {
            var member = _MemberRepository.GetMember(user.UserName);
			if (member == null)
                return View(MVC.Admin.Views.utilisateur_absent);
            else
            {
				_MemberRepository.Delete(member.MemberId);
				_VisitorRepository.Delete(item => string.Compare(item.Email, user.UserName, StringComparison.InvariantCultureIgnoreCase) == 0);
                return Redirect(returnUrl);
            }
        }

        #endregion

        #region Admin Visitor

        /// <summary>
        /// Prepares a web page containing a paginated list of visitors
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult IndexVisitor(int? page)
        {
            int pageValue = page ?? 1;
            int itemTotal = 1;
            //IQueryable<Visitor> visitors = _VisitorRepository.GetVisitors((pageValue - 1) * PageSize, PageSize);
			var currentPage = _VisitorRepository.Get((pageValue - 1) * PageSize, PageSize, v => v.Id);
            var dict = currentPage.ToDictionary(v => v, v => _MembershipService.GetUserByMail(v.Email) != null);
            itemTotal = _VisitorRepository.GetCount();
            var viewModel = new VisitorListViewModel()
            {
                ListVisitor = dict,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = PageSize,
                    TotalItems = itemTotal
                }
            };
            return View(viewModel);
        }

        /// <summary>
        /// Send an email to a visitor
        /// containing a link to create an account
        /// and redirect to visitor admin home
        /// </summary>
        /// <param name="email">The email of the visitor</param>
        /// <param name="returnUrl">url to redirect to</param>
        /// <returns>Redirect to return url</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult SendEmail(string email, string returnUrl)
        {
			var visitor = _VisitorRepository.Get(item => string.Compare(item.Email, email, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (visitor == null)
                return RedirectToAction(MVC.Admin.IndexVisitor());

            try
            {
                this.SendVisitorMail(_EmailService, visitor);
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.Message);
                return RedirectToAction(MVC.Admin.IndexVisitor());
            }
            //validate all visitors with the email
            _VisitorRepository.ValidateVisitor(email);
            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(MVC.Admin.IndexVisitor());
        }

        #endregion

        #region Admin WelcomePeople

        /// <summary>
        /// Prepares a web page containing a paginated list of the people on home page
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult IndexWelcomePeople(int? page)
        {
            int pageValue = page ?? 1;
			var welcomPeople = _WelcomePeopleRepository.Get((pageValue - 1) * PageSize, PageSize, wp => wp.Id);
            var viewModel = new WelcomePeopleListViewModel()
            {
                WelcomePeople = welcomPeople,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = PageSize,
                    TotalItems = _WelcomePeopleRepository.GetCount()
                }
            };
            return View(viewModel);
        }

        /// <summary>
        /// Prepares a web page containing the details of a WelcomePeople
        /// </summary>
        /// <param name="page">id of the WelcomePeople</param>
        /// <returns>The action result.</returns>
        [Authorize]
        public virtual ActionResult DetailWelcomePeople(int id)
        {
            var item = _WelcomePeopleRepository.Get(id);
            if (item == null)
                return RedirectToAction(MVC.Admin.IndexWelcomePeople());
            return View(item);
        }

        /// <summary>
        /// Prepares a web page containing the form to create a new WelcomePeople
        /// </summary>
        /// <returns>The action result.</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        public virtual ActionResult CreateWelcomePeople()
        {
			return View(new WelcomePeopleFormViewModel());
        }

        /// <summary>
        /// Add the welcomepeople from the form to the repository, then redirect to index
        /// </summary>
        /// <param name="welcomePeople">data from the form</param>
        /// <returns>redirect to index</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
        [ValidateAntiForgeryToken]
		public virtual ActionResult CreateWelcomePeople(WelcomePeopleFormViewModel formModel)
        {
            if (ModelState.IsValid)
            {
				try
				{
					//upload images and set image paths
					foreach (string name in Request.Files)
					{
						var postedFile = Request.Files[name];
						if (postedFile == null || string.IsNullOrEmpty(postedFile.FileName))
							continue;
						var uploadedFileName = this.UploadFile(postedFile);
						formModel.WelcomePeople.LocalisationPicture = uploadedFileName;
					}
					//get member
					var member = _MemberRepository.GetMember(formModel.Email);
					formModel.WelcomePeople.MemberId = member.MemberId;
					//get localisation
					var loc = _LocalisationRepository.Get(l => string.Compare(l.Name, formModel.LocalisationName, StringComparison.InvariantCultureIgnoreCase) == 0);
					formModel.WelcomePeople.LocalisationId = loc.ID;
					_WelcomePeopleRepository.Add(formModel.WelcomePeople);
					return RedirectToAction(MVC.Admin.IndexWelcomePeople());
				}
				catch (Exception ex)
				{
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
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        public virtual ActionResult EditWelcomePeople(int id)
        {
            var item = _WelcomePeopleRepository.Get(id);
            if (item == null)
                return RedirectToAction(MVC.Admin.IndexWelcomePeople());
			return View(new WelcomePeopleFormViewModel(item));
        }

        /// <summary>
        /// Apply the modifications to a welcomePeople
        /// </summary>
        /// <param name="welcomePeople">data from the form</param>
        /// <returns>redirect to index</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		[ValidateAntiForgeryToken]
		public virtual ActionResult EditWelcomePeople(int id, WelcomePeopleFormViewModel formModel)
		{
			if (ModelState.IsValid)
			{
				try
				{
					//upload images and set image paths
					foreach (string name in Request.Files)
					{

						var postedFile = Request.Files[name];
						if (postedFile == null || string.IsNullOrEmpty(postedFile.FileName))
							continue;
						var uploadedFileName = this.UploadFile(postedFile);
						formModel.WelcomePeople.LocalisationPicture = uploadedFileName;
					}

					_WelcomePeopleRepository.Update(id, wp =>
					{
						UpdateModel(wp, "WelcomePeople");
						//get member
						var member = _MemberRepository.GetMember(formModel.Email);
						wp.MemberId = member.MemberId;
						//get localisation
						var loc = _LocalisationRepository.Get(l => string.Compare(l.Name, formModel.LocalisationName, StringComparison.InvariantCultureIgnoreCase) == 0);
						wp.LocalisationId = loc.ID;

						if (!string.IsNullOrEmpty(formModel.WelcomePeople.LocalisationPicture))
							wp.LocalisationPicture = formModel.WelcomePeople.LocalisationPicture;
					});
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", ex.Message);
				}
				return RedirectToAction(MVC.Admin.IndexWelcomePeople());
			}
			return View(formModel);
		}

        /// <summary>
        /// Prepares a web page to delete a WelcomePeople
        /// </summary>
        /// <param name="page">id of the WelcomePeople</param>
        /// <returns>The action result.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("supprimer-welcomePeople")]
        public virtual ActionResult DeleteWelcomePeople(int id, string returnUrl)
        {
            var welcomePeople = _WelcomePeopleRepository.Get(id);
            if (welcomePeople == null)
                return View("utilisateur-absent");
            TempData["returnUrl"] = returnUrl;
             
            return View("DeleteWelcomePeople");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("supprimer-welcomePeople")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteWelcomePeople(WelcomePeople welcomePeople, string confirmButton)
        {
            var profil = _WelcomePeopleRepository.Get(welcomePeople.Id);
            if (profil == null)
                return View("utilisateur-absent");
            else
            {
                _WelcomePeopleRepository.Delete(profil.Id);
                return RedirectToAction(MVC.Admin.IndexWelcomePeople());
            }
        }

        #endregion

        #region Import csv

        /// <summary>
        /// Prepares a web page to upload a csv file
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult IndexImport()
        {
			AdminImportViewModel viewModel = new AdminImportViewModel();
			viewModel.resultMessage = "";
			return View(viewModel);
        }

		/// <summary>
		/// Prepares a web page to upload a csv file
		/// </summary>
		/// <param name="page">The page to display</param>
		/// <returns>The action result.</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult IndexImportValidate(string result)
		{
			AdminImportViewModel viewModel = new AdminImportViewModel();
			viewModel.resultMessage = result;
			return View(viewModel);
		}

        /// <summary>
        /// Prepares a web page to upload a csv file
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult IndexImport(FormCollection collection)
        {
			char CSV_SEPARATOR = ';';
			string featureTrueIndicator = "Oui"; // by default, it's false. It's true only for the string
			int nbCol = 20;
			bool isHeaderLine = false;
			if (collection.Get("importCsvHeader") != null && collection.Get("importCsvHeader") == "on")
				isHeaderLine = true;
			int nbLocalisationsAdded = 0;
			string listLocalisationsAlreadyInDB = "";
			foreach (string name in Request.Files)
			{
				try
				{
					var postedFile = Request.Files[name];
					if (postedFile == null || string.IsNullOrEmpty(postedFile.FileName))
						continue;

					int fileLen;
					fileLen = postedFile.ContentLength;
					byte[] input = new byte[fileLen];

					StreamReader sr = new StreamReader(postedFile.InputStream, System.Text.Encoding.Default); // Using of encoding to don't loose french accents

					// Because we can have a csv line in multiple line in files (because in a value, we can have line-return), we have to check it
					string fullCSVLine = "";
					while (sr.Peek() >= 0) // Read of each line of CSV file
					{
						fullCSVLine += sr.ReadLine();
						string[] infosLocalisation = fullCSVLine.Split(CSV_SEPARATOR);

						// We have not the full CSV line
						if (infosLocalisation.Length < nbCol)
							continue;

						if (!isHeaderLine)
						{
							var localisationToAdd = new Localisation();
							localisationToAdd.Name = infosLocalisation[1];
							localisationToAdd.Adress = infosLocalisation[2];
							localisationToAdd.PostalCode = infosLocalisation[3];
							localisationToAdd.City = infosLocalisation[4];
							localisationToAdd.Country = infosLocalisation[5];
							localisationToAdd.PhoneNumber = infosLocalisation[6];
							localisationToAdd.Fax = infosLocalisation[7];
							localisationToAdd.Mail = infosLocalisation[8];
							localisationToAdd.WebSite = infosLocalisation[9];
							localisationToAdd.Description = infosLocalisation[10];
							double latitude, longitude;
							Double.TryParse(infosLocalisation[18].Replace(',', '.'), out latitude);
							Double.TryParse(infosLocalisation[19].Replace(',', '.'), out longitude);
							localisationToAdd.Latitude = latitude;
							localisationToAdd.Longitude = longitude;

							// Description in English : localisationToAdd.DescriptionEnglish = infosLocalisation[11]; ??
							var member = _MemberRepository.GetMember(infosLocalisation[0]);
							localisationToAdd.OwnerID = member.MemberId;

							if (infosLocalisation[12].Trim().ToLower() == featureTrueIndicator.ToLower())
								localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.VisioRoom, OfferID = (int)FeatureType.VisioRoom });
							if (infosLocalisation[13].Trim().ToLower() == featureTrueIndicator.ToLower())
								localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.MeetingRoom, OfferID = (int)FeatureType.MeetingRoom });
							if (infosLocalisation[14].Trim().ToLower() == featureTrueIndicator.ToLower())
								localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.SeminarRoom, OfferID = (int)FeatureType.SeminarRoom });
							if (infosLocalisation[15].Trim().ToLower() == featureTrueIndicator.ToLower())
								localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.BuisnessRoom, OfferID = (int)FeatureType.WorkingPlace });
							if (infosLocalisation[16].Trim().ToLower() == featureTrueIndicator.ToLower())
								localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Workstation, OfferID = (int)FeatureType.WorkingPlace });
							if (infosLocalisation[17].Trim().ToLower() == featureTrueIndicator.ToLower())
								localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.SingleDesk, OfferID = (int)FeatureType.WorkingPlace });

							localisationToAdd.TypeValue = (int)LocalisationType.BuisnessCenter;

							var similarLoc = (from loc
												 in _LocalisationRepository.FindSimilarLocalisation((float)localisationToAdd.Latitude, (float)localisationToAdd.Longitude)
											  where string.Compare(loc.Name, localisationToAdd.Name, StringComparison.InvariantCultureIgnoreCase) == 0
											  select loc).ToList();
							if (similarLoc.Count > 0)
							{
								//temp, add wifi
								if (similarLoc.Count() == 1)
								{
									var loc = similarLoc[0];
									if(!loc.HasFeature(Feature.Wifi_Free,FeatureType.General))
									{
										_LocalisationRepository.Update(loc.ID, l =>
											{
												l.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Wifi_Free, OfferID = (int)FeatureType.General });
											});
									}
								}
								listLocalisationsAlreadyInDB += "&bull; " + localisationToAdd.Name + "<br />";
							}
							else
							{
								_LocalisationRepository.Add(localisationToAdd);
								_MemberRepository.Update(member.MemberId, m =>
								{
									m.MemberEditions.Add(new MemberEdition { ModificationDate = DateTime.Now, LocalisationId = localisationToAdd.ID, ModificationType = (int)EditionType.Creation });
								});
								nbLocalisationsAdded++;
							}
						}
						else
							isHeaderLine = false; // because we have skipped the first line

						fullCSVLine = ""; // Reinitialization because we have found the full CVS line

					}
				}
				catch (Exception ex)
				{
					_Logger.Error("Edit", ex);
					ModelState.AddModelError("", "Une erreur a eu lieu (" + nbLocalisationsAdded + " lieux de travail ont été ajoutés) => " + ex.Message);
				}
			}

			AdminImportViewModel viewModel = new AdminImportViewModel();
			viewModel.resultMessage = nbLocalisationsAdded.ToString() + " localisations added.";
			viewModel.localisationsAlreadyInDB = listLocalisationsAlreadyInDB;
			return View(viewModel);
        }

        #endregion

		#region Admin Booking

		/// <summary>
		/// Prepares a web page containing a paginated list of localisations
		/// </summary>
		/// <param name="page">The page to display</param>
		/// <returns>The action result.</returns>
		public virtual ActionResult IndexBooking(int? page)
		{
			var pageValue = page ?? 1;
			var bookings = _BookingRepository.Get((pageValue - 1) * PageSize, PageSize, mb => mb.Id);
			var viewModel = new MemberBookingListViewModel()
			{
				MemberBooking = bookings,
				PagingInfo = new PagingInfo
				{
					CurrentPage = pageValue,
					ItemsPerPage = PageSize,
					TotalItems = _BookingRepository.GetCount()
				}
			};
			return View(viewModel);
		}

		#endregion
	}
}
