using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Web.Helpers;
using Worki.Data.Repository;
using Worki.Infrastructure.Helpers;
using Worki.Data.Models;
using Worki.Memberships;
using Worki.Infrastructure;
using Postal;

namespace Worki.Web.Controllers
{
	[Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    public partial class AdminController : Controller
    {
        IMembershipService _MembershipService;
        ILogger _Logger;
        IEmailService _EmailService;

        public AdminController( IMembershipService memberShipservice, 
                                ILogger logger,
                                IEmailService emailService)
        {
            _MembershipService = memberShipservice;
            _Logger = logger;
            _EmailService = emailService;
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
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var pageValue = page ?? 1;
			var localisations = lRepo.Get((pageValue - 1) * PageSize, PageSize, l => l.ID);
            var viewModel = new AdminLocalisation(localisations.ToList())
            {
                Localisations = localisations,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = PageSize,
					TotalItems = lRepo.GetCount()
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
				var context = ModelFactory.GetUnitOfWork();
				var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
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
						var loc = lRepo.Get(locId);
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
								var member = mRepo.GetMember(item);
								if (member == null)
									throw new Exception();
                                loc.SetOwner(member.MemberId);
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
                            if (loc.MainLocalisation != null)
                                loc.MainLocalisation.Localisation = null;
						}
					}
					context.Commit();
				}
				catch (Exception e)
				{
					_Logger.Error(e.Message);
					context.Complete();
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
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			int pageValue = page ?? 1;
			var members = mRepo.Get((pageValue - 1) * PageSize, PageSize, m => m.MemberId);
			var viewModel = new UserListViewModel()
			{
				ListMemberShip = _MembershipService.GetAdminMapping(members),
				PagingInfo = new PagingInfo
				 {
					 CurrentPage = pageValue,
					 ItemsPerPage = PageSize,
					 TotalItems = mRepo.GetCount()
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
				var context = ModelFactory.GetUnitOfWork();
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                try
                {
                    var listCollection = collection.AllKeys;
                    foreach (var username in listCollection)
                    {
                        var roleCheck = collection[username].ToLower();
						var member = mRepo.GetMember(username);
                        if (member == null)
                            continue;
                        var userInRole = Roles.IsUserInRole(username, MiscHelpers.AdminConstants.AdminRole);
                        if (roleCheck.Contains("true"))
                        {
                            if (!userInRole)
                            {
                                Roles.AddUserToRole(username, MiscHelpers.AdminConstants.AdminRole);
                            }
                        }
                        else
                        {
                            if (userInRole)
                            {
                                Roles.RemoveUserFromRole(username, MiscHelpers.AdminConstants.AdminRole);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _Logger.Error(e.Message);
                }
            }
            
            TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.RoleHaveBeenSet;

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
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.UserNotFound;
                return RedirectToAction(MVC.Admin.IndexUser());
            }
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
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var vRepo = ModelFactory.GetRepository<IVisitorRepository>(context);
			var member = mRepo.GetMember(user.UserName);
            if (member == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.UserNotFound;
                return RedirectToAction(MVC.Admin.IndexUser());
            }
            else
            {
				try
				{
					mRepo.Delete(member.MemberId);
					vRepo.Delete(item => string.Compare(item.Email, user.UserName, StringComparison.InvariantCultureIgnoreCase) == 0);
					context.Commit();
				}
				catch (Exception ex)
				{
					_Logger.Error("", ex);
					context.Complete();
				}

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.UserHaveBeenDel;

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
			var context = ModelFactory.GetUnitOfWork();
			var vRepo = ModelFactory.GetRepository<IVisitorRepository>(context);
            int pageValue = page ?? 1;
            int itemTotal = 1;
            //IQueryable<Visitor> visitors = _VisitorRepository.GetVisitors((pageValue - 1) * PageSize, PageSize);
			var currentPage = vRepo.Get((pageValue - 1) * PageSize, PageSize, v => v.Id);
            var dict = currentPage.ToDictionary(v => v, v => _MembershipService.GetUserByMail(v.Email) != null);
			itemTotal = vRepo.GetCount();
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
			var context = ModelFactory.GetUnitOfWork();
			var vRepo = ModelFactory.GetRepository<IVisitorRepository>(context);
			var visitor = vRepo.Get(item => string.Compare(item.Email, email, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (visitor == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.VisitorNotFound;
                return RedirectToAction(MVC.Admin.IndexVisitor());
            }
            try
            {
                //this.SendVisitorMail(_EmailService, visitor);
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.Message);
                return RedirectToAction(MVC.Admin.IndexVisitor());
            }
            //validate all visitors with the email
			vRepo.ValidateVisitor(email);
			context.Commit();

            TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.MailHaveBeenSent;

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
			var context = ModelFactory.GetUnitOfWork();
			var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
            int pageValue = page ?? 1;
			var welcomPeople = wpRepo.Get((pageValue - 1) * PageSize, PageSize, wp => wp.Id);
            var viewModel = new WelcomePeopleListViewModel()
            {
                WelcomePeople = welcomPeople,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = PageSize,
					TotalItems = wpRepo.GetCount()
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
			var context = ModelFactory.GetUnitOfWork();
			var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
			var item = wpRepo.Get(id);
            if (item == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleNotFound;
                return RedirectToAction(MVC.Admin.IndexWelcomePeople());
            }
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
				var context = ModelFactory.GetUnitOfWork();
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
				var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
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
					var member = mRepo.GetMember(formModel.Email);
					formModel.WelcomePeople.MemberId = member.MemberId;
					//get localisation
					var loc = lRepo.Get(l => string.Compare(l.Name, formModel.LocalisationName, StringComparison.InvariantCultureIgnoreCase) == 0);
					formModel.WelcomePeople.LocalisationId = loc.ID;
					wpRepo.Add(formModel.WelcomePeople);
					context.Commit();

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleHaveBeenCreate;

					return RedirectToAction(MVC.Admin.IndexWelcomePeople());
				}
				catch (Exception ex)
				{
					context.Complete();
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
			var context = ModelFactory.GetUnitOfWork();
			var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
			var item = wpRepo.Get(id);
            if (item == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleNotFound;
                return RedirectToAction(MVC.Admin.IndexWelcomePeople());
            }
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
				var context = ModelFactory.GetUnitOfWork();
				var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
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

					var wp = wpRepo.Get(id);

					UpdateModel(wp, "WelcomePeople");
					//get member
					var member = mRepo.GetMember(formModel.Email);
					wp.MemberId = member.MemberId;
					//get localisation
					var loc = lRepo.Get(l => string.Compare(l.Name, formModel.LocalisationName, StringComparison.InvariantCultureIgnoreCase) == 0);
					wp.LocalisationId = loc.ID;

					if (!string.IsNullOrEmpty(formModel.WelcomePeople.LocalisationPicture))
						wp.LocalisationPicture = formModel.WelcomePeople.LocalisationPicture;

					context.Commit();
				}
				catch (Exception ex)
				{
					context.Complete();
					ModelState.AddModelError("", ex.Message);
				}

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleHaveBeenEdit;

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
        [ActionName("DeleteWelcomePeople")]
		public virtual ActionResult DeleteWelcomePeople(int id, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
			var welcomePeople = wpRepo.Get(id);
            if (welcomePeople == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleNotFound;
                return RedirectToAction(MVC.Admin.IndexWelcomePeople());
            }
			TempData["returnUrl"] = returnUrl;

			return View(welcomePeople);
		}

        // supprimer-welcomePeople

        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("DeleteWelcomePeople")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteWelcomePeople(int id)
        {
			var context = ModelFactory.GetUnitOfWork();
			var wpRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
			var profil = wpRepo.Get(id);
            if (profil == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleNotFound;
                return RedirectToAction(MVC.Admin.IndexWelcomePeople());
            }
            else
            {
				wpRepo.Delete(profil.Id);
                context.Commit();

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.WelcomePeopleHaveBeenDel;

                return RedirectToAction(MVC.Admin.IndexWelcomePeople());
            }
        }

        /// <summary>
        /// POST Action method to update admin roles
        /// and redirect to user admin home
        /// </summary>
        /// <param name="collection">form containg the list of ids to push to admin role</param>
        /// <returns>Redirect to return url</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult WelcomePeopleLine(FormCollection collection, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var context = ModelFactory.GetUnitOfWork();
                var wRepo = ModelFactory.GetRepository<IWelcomePeopleRepository>(context);
                try
                {
                    var listCollection = collection.AllKeys;
                    foreach (var username in listCollection)
                    {
                        var onlineCheck = collection[username].ToLower();
                        var welcomeppl = wRepo.GetWelcomePeople(username);
                        if (welcomeppl == null)
                            continue;
                        welcomeppl.Online = onlineCheck.Contains("true");
                    }
                    context.Commit();
                }
                catch (Exception e)
                {
                    _Logger.Error(e.Message);
                    context.Complete();
                }

            }

            TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.RoleHaveBeenSet;

            // Redirection
            return Redirect(returnUrl);
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
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
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
							var member = mRepo.GetMember(infosLocalisation[0]);
                            localisationToAdd.SetOwner(member.MemberId);

							if (infosLocalisation[12].Trim().ToLower() == featureTrueIndicator.ToLower())
								localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.VisioRoom, OfferID = (int)FeatureType.VisioRoom });
							if (infosLocalisation[13].Trim().ToLower() == featureTrueIndicator.ToLower())
								localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.MeetingRoom, OfferID = (int)FeatureType.MeetingRoom });
							if (infosLocalisation[14].Trim().ToLower() == featureTrueIndicator.ToLower())
								localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.SeminarRoom, OfferID = (int)FeatureType.SeminarRoom });
							if (infosLocalisation[15].Trim().ToLower() == featureTrueIndicator.ToLower())
								localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.BuisnessLounge, OfferID = (int)FeatureType.WorkingPlace });
							if (infosLocalisation[16].Trim().ToLower() == featureTrueIndicator.ToLower())
								localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Workstation, OfferID = (int)FeatureType.WorkingPlace });
							if (infosLocalisation[17].Trim().ToLower() == featureTrueIndicator.ToLower())
								localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Desktop, OfferID = (int)FeatureType.WorkingPlace });

							localisationToAdd.TypeValue = (int)LocalisationType.BuisnessCenter;

							var similarLoc = (from loc
												 in lRepo.FindSimilarLocalisation((float)localisationToAdd.Latitude, (float)localisationToAdd.Longitude)
											  where string.Compare(loc.Name, localisationToAdd.Name, StringComparison.InvariantCultureIgnoreCase) == 0
											  select loc).ToList();
							if (similarLoc.Count > 0)
							{
								//temp, add wifi
								if (similarLoc.Count() == 1)
								{
									var loc = similarLoc[0];
									if (!loc.HasFeature(Feature.Wifi_Free))
									{
										var l = lRepo.Get(loc.ID);
										l.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Wifi_Free, OfferID = (int)FeatureType.General });
									}
								}
								listLocalisationsAlreadyInDB += "&bull; " + localisationToAdd.Name + "<br />";
							}
							else
							{
								lRepo.Add(localisationToAdd);
								member.MemberEditions.Add(new MemberEdition { ModificationDate = DateTime.Now, LocalisationId = localisationToAdd.ID, ModificationType = (int)EditionType.Creation });
								nbLocalisationsAdded++;
							}
						}
						else
							isHeaderLine = false; // because we have skipped the first line

						fullCSVLine = ""; // Reinitialization because we have found the full CVS line

					}
					context.Commit();
				}
				catch (Exception ex)
				{
					_Logger.Error("Edit", ex);
					ModelState.AddModelError("", "Une erreur a eu lieu (" + nbLocalisationsAdded + " lieux de travail ont été ajoutés) => " + ex.Message);
					context.Complete();
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
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
			var pageValue = page ?? 1;
			var bookings = bRepo.Get((pageValue - 1) * PageSize, PageSize, mb => mb.Id);
			var viewModel = new MemberBookingListViewModel()
			{
				MemberBooking = bookings,
				PagingInfo = new PagingInfo
				{
					CurrentPage = pageValue,
					ItemsPerPage = PageSize,
					TotalItems = bRepo.GetCount()
				}
			};
			return View(viewModel);
		}

		#endregion

        #region Admin Press

        /// <summary>
        /// Prepares a web page containing a paginated list of the press on home page
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult IndexPress(int? page)
        {
			var context = ModelFactory.GetUnitOfWork();
			var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
            int pageValue = page ?? 1;
			var press = pRepo.Get((pageValue - 1) * PageSize, PageSize, p => p.ID);
            var viewModel = new PressListViewModel()
            {
                Press = press,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = PageSize,
					TotalItems = pRepo.GetCount()
                }
            };
            return View(viewModel);
        }

        /// <summary>
        /// Prepares a web page containing the details of a Press
        /// </summary>
        /// <param name="page">id of the Press</param>
        /// <returns>The action result.</returns>
        [Authorize]
        public virtual ActionResult DetailPress(int id)
        {
			var context = ModelFactory.GetUnitOfWork();
			var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
            var item = pRepo.Get(id);
            if (item == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressNotFound;
                return RedirectToAction(MVC.Admin.IndexPress());
            }

            return View(item);
        }

        /// <summary>
        /// Prepares a web page containing the form to create a new Press
        /// </summary>
        /// <returns>The action result.</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        public virtual ActionResult CreatePress()
        {
            return View(new Press());
        }

        /// <summary>
        /// Add the press from the form to the repository, then redirect to index
        /// </summary>
        /// <param name="press">data from the form</param>
        /// <returns>redirect to index</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
        [ValidateAntiForgeryToken]
        public virtual ActionResult CreatePress(Press formModel)
        {
            if (ModelState.IsValid)
            {
				var context = ModelFactory.GetUnitOfWork();
				var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
                try
                {
					pRepo.Add(formModel);
					context.Commit();

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressHaveBeenCreate;

                    return RedirectToAction(MVC.Admin.IndexPress());
                }
                catch (Exception ex)
                {
					context.Complete();
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(formModel);
        }

        /// <summary>
        /// prepare the view to edit a press
        /// </summary>
        /// <param name="id">id of the press to edit</param>
        /// <returns>the form</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        public virtual ActionResult EditPress(int id)
        {
			var context = ModelFactory.GetUnitOfWork();
			var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
			var item = pRepo.Get(id);
            if (item == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressNotFound;
                return RedirectToAction(MVC.Admin.IndexPress());
            }

            return View(item);
        }

        /// <summary>
        /// Apply the modifications to a press
        /// </summary>
        /// <param name="press">data from the form</param>
        /// <returns>redirect to index</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		[ValidateAntiForgeryToken]
		public virtual ActionResult EditPress(int id, Press formModel)
		{
			if (ModelState.IsValid)
			{
				var context = ModelFactory.GetUnitOfWork();
				var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
				try
				{
					var p = pRepo.Get(id);
					UpdateModel(p);
					context.Commit();
				}
				catch (Exception ex)
				{
					context.Complete();
					ModelState.AddModelError("", ex.Message);
				}

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressHaveBeenEdit;

				return RedirectToAction(MVC.Admin.IndexPress());
			}
			return View(formModel);
		}

        /// <summary>
        /// Prepares a web page to delete a press
        /// </summary>
        /// <param name="page">id of the Press</param>
        /// <returns>The action result.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("supprimer-press")]
        public virtual ActionResult DeletePress(int id, string returnUrl)
        {
			var context = ModelFactory.GetUnitOfWork();
			var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
			var press = pRepo.Get(id);
            if (press == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressNotFound;
                return RedirectToAction(MVC.Admin.IndexPress());
            }

            TempData["returnUrl"] = returnUrl;

            return View("DeletePress");
        }

		[AcceptVerbs(HttpVerbs.Post)]
		[ActionName("supprimer-press")]
		[ValidateAntiForgeryToken]
		public virtual ActionResult DeletePress(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var pRepo = ModelFactory.GetRepository<IPressRepository>(context);
			var article = pRepo.Get(id);
			if (article == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressNotFound;
                return RedirectToAction(MVC.Admin.IndexPress());
            }
			else
			{
				try
				{
					pRepo.Delete(article.ID);
					context.Commit();
				}
				catch (Exception ex)
				{
					_Logger.Error("DeletePress", ex);
					context.Complete();
				}

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Admin.AdminString.PressHaveBeenDel;

				return RedirectToAction(MVC.Admin.IndexPress());
			}
		}

        #endregion
        
        #region Admin Rental

        /// <summary>
        /// Prepares a web page containing a paginated list of localisations
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult IndexRental(int? page)
        {
			var context = ModelFactory.GetUnitOfWork();
			var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
            var pageValue = page ?? 1;
			var rentals = rRepo.Get((pageValue - 1) * PageSize, PageSize, r => r.Id);
            var viewModel = new RentalListViewModel()
            {
                Rentals = rentals,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = PageSize,
					TotalItems = rRepo.GetCount()
                }
            };
            return View(viewModel);
        }

        #endregion

        #region Migration

		//public virtual ActionResult MigrateToOffer()
		//{
		//    var strBuilder = new System.Text.StringBuilder();

		//    var context = ModelFactory.GetUnitOfWork();
		//    var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
		//    var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

		//    //already processed
		//    if (oRepo.GetCount() != 0)
		//        return Content(strBuilder.ToString());

		//    var all = lRepo.GetAll();
		//    var newofferSuffix = " 1";
		//    try
		//    {
		//        var count = 0;
		//        foreach (var item in all)
		//        {
		//            count++;

		//            //opennings
		//            if (item.LocalisationData == null)
		//                item.LocalisationData = new LocalisationData();

		//            if (item.MonOpen.HasValue)
		//                item.LocalisationData.MonOpenMorning = item.MonOpen;
		//            if (item.MonClose2.HasValue)
		//                item.LocalisationData.MonCloseMorning = item.MonClose2;
		//            if (item.MonOpen2.HasValue)
		//                item.LocalisationData.MonOpenAfter = item.MonOpen2;
		//            if (item.MonClose.HasValue)
		//                item.LocalisationData.MonCloseAfter = item.MonClose;

		//            if (item.TueOpen.HasValue)
		//                item.LocalisationData.TueOpenMorning = item.TueOpen;
		//            if (item.TueClose2.HasValue)
		//                item.LocalisationData.TueCloseMorning = item.TueClose2;
		//            if (item.TueOpen2.HasValue)
		//                item.LocalisationData.TueOpenAfter = item.TueOpen2;
		//            if (item.TueClose.HasValue)
		//                item.LocalisationData.TueCloseAfter = item.TueClose;

		//            if (item.WedOpen.HasValue)
		//                item.LocalisationData.WedOpenMorning = item.WedOpen;
		//            if (item.WedClose2.HasValue)
		//                item.LocalisationData.WedCloseMorning = item.WedClose2;
		//            if (item.WedOpen2.HasValue)
		//                item.LocalisationData.WedOpenAfter = item.WedOpen2;
		//            if (item.WedClose.HasValue)
		//                item.LocalisationData.WedCloseAfter = item.WedClose;

		//            if (item.ThuOpen.HasValue)
		//                item.LocalisationData.ThuOpenMorning = item.ThuOpen;
		//            if (item.ThuClose2.HasValue)
		//                item.LocalisationData.ThuCloseMorning = item.ThuClose2;
		//            if (item.ThuOpen2.HasValue)
		//                item.LocalisationData.ThuOpenAfter = item.ThuOpen2;
		//            if (item.ThuClose.HasValue)
		//                item.LocalisationData.ThuCloseAfter = item.ThuClose;

		//            if (item.FriOpen.HasValue)
		//                item.LocalisationData.FriOpenMorning = item.FriOpen;
		//            if (item.FriClose2.HasValue)
		//                item.LocalisationData.FriCloseMorning = item.FriClose2;
		//            if (item.FriOpen2.HasValue)
		//                item.LocalisationData.FriOpenAfter = item.FriOpen2;
		//            if (item.FriClose.HasValue)
		//                item.LocalisationData.FriCloseAfter = item.FriClose;

		//            if (item.SatOpen.HasValue)
		//                item.LocalisationData.SatOpenMorning = item.SatOpen;
		//            if (item.SatClose2.HasValue)
		//                item.LocalisationData.SatCloseMorning = item.SatClose2;
		//            if (item.SatOpen2.HasValue)
		//                item.LocalisationData.SatOpenAfter = item.SatOpen2;
		//            if (item.SatClose.HasValue)
		//                item.LocalisationData.SatCloseAfter = item.SatClose;

		//            if (item.SunOpen.HasValue)
		//                item.LocalisationData.SunOpenMorning = item.SunOpen;
		//            if (item.SunClose2.HasValue)
		//                item.LocalisationData.SunCloseMorning = item.SunClose2;
		//            if (item.SunOpen2.HasValue)
		//                item.LocalisationData.SunOpenAfter = item.SunOpen2;
		//            if (item.SunClose.HasValue)
		//                item.LocalisationData.SunCloseAfter = item.SunClose;


		//            //coffee price 
		//            if (item.LocalisationData != null && item.LocalisationData.CoffeePrice.HasValue && item.LocalisationData.CoffeePrice != 0)
		//            {

		//                item.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.CoffeePrice, DecimalValue = item.LocalisationData.CoffeePrice });
		//            }

		//            //case free loc
		//            if (item.IsFreeLocalisation())
		//                continue;

		//            if (item.HasFeature(Feature.BuisnessLounge))
		//            {
		//                var offer = new Offer();
		//                offer.Type = (int)LocalisationOffer.BuisnessLounge;
		//                offer.Name = Localisation.GetOfferType((int)LocalisationOffer.BuisnessLounge) + newofferSuffix;
		//                var features = item.GetFeaturesWithin(FeatureHelper.BuisnessLounge);
		//                foreach (var f in features)
		//                {
		//                    offer.OfferFeatures.Add(new OfferFeature { FeatureId = (int)f.Feature });
		//                }
		//                item.Offers.Add(offer);
		//                strBuilder.AppendLine("Localisation : " + item.Name + " ; Offer added : " + offer.Name);
		//            }

		//            if (item.HasFeature(Feature.Workstation))
		//            {
		//                var offer = new Offer();
		//                offer.Type = (int)LocalisationOffer.Workstation;
		//                offer.Name = Localisation.GetOfferType((int)LocalisationOffer.Workstation) + newofferSuffix;
		//                var features = item.GetFeaturesWithin(FeatureHelper.Workstation);
		//                foreach (var f in features)
		//                {
		//                    offer.OfferFeatures.Add(new OfferFeature { FeatureId = (int)f.Feature });
		//                }
		//                item.Offers.Add(offer);
		//                strBuilder.AppendLine("Localisation : " + item.Name + " ; Offer added : " + offer.Name);
		//            }

		//            if (item.HasFeature(Feature.Desktop))
		//            {
		//                var offer = new Offer();
		//                offer.Type = (int)LocalisationOffer.Desktop;
		//                offer.Name = Localisation.GetOfferType((int)LocalisationOffer.Desktop) + newofferSuffix;
		//                var features = item.GetFeaturesWithin(FeatureHelper.Desktop);
		//                foreach (var f in features)
		//                {
		//                    offer.OfferFeatures.Add(new OfferFeature { FeatureId = (int)f.Feature });
		//                }
		//                item.Offers.Add(offer);
		//                strBuilder.AppendLine("Localisation : " + item.Name + " ; Offer added : " + offer.Name);
		//            }

		//            if (item.HasFeature(Feature.MeetingRoom))
		//            {
		//                var offer = new Offer();
		//                offer.Type = (int)LocalisationOffer.MeetingRoom;
		//                offer.Name = Localisation.GetOfferType((int)LocalisationOffer.MeetingRoom) + newofferSuffix;
		//                var features = item.GetFeaturesWithin(FeatureHelper.MeetingRoom);
		//                foreach (var f in features)
		//                {
		//                    offer.OfferFeatures.Add(new OfferFeature { FeatureId = (int)f.Feature });
		//                }
		//                item.Offers.Add(offer);
		//                strBuilder.AppendLine("Localisation : " + item.Name + " ; Offer added : " + offer.Name);
		//            }

		//            if (item.HasFeature(Feature.SeminarRoom))
		//            {
		//                var offer = new Offer();
		//                offer.Type = (int)LocalisationOffer.SeminarRoom;
		//                offer.Name = Localisation.GetOfferType((int)LocalisationOffer.SeminarRoom) + newofferSuffix;
		//                var features = item.GetFeaturesWithin(FeatureHelper.SeminarRoom);
		//                foreach (var f in features)
		//                {
		//                    offer.OfferFeatures.Add(new OfferFeature { FeatureId = (int)f.Feature });
		//                }
		//                item.Offers.Add(offer);
		//                strBuilder.AppendLine("Localisation : " + item.Name + " ; Offer added : " + offer.Name);
		//            }

		//            if (item.HasFeature(Feature.VisioRoom))
		//            {
		//                var offer = new Offer();
		//                offer.Type = (int)LocalisationOffer.VisioRoom;
		//                offer.Name = Localisation.GetOfferType((int)LocalisationOffer.VisioRoom) + newofferSuffix;
		//                var features = item.GetFeaturesWithin(FeatureHelper.VisioRoom);
		//                foreach (var f in features)
		//                {
		//                    offer.OfferFeatures.Add(new OfferFeature { FeatureId = (int)f.Feature });
		//                }
		//                item.Offers.Add(offer);
		//                strBuilder.AppendLine("Localisation : " + item.Name + " ; Offer added : " + offer.Name);
		//            }
		//        }

		//        context.Commit();
		//    }
		//    catch (Exception ex)
		//    {
		//        strBuilder.AppendLine("error : " + ex.Message);
		//        context.Complete();
		//    }

		//    var content = MiscHelpers.Nl2Br(strBuilder.ToString());
		//    return Content(content);
		//}

        #endregion

        #region Statistic

        public virtual ActionResult Stat()
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var all = lRepo.GetAll().OrderBy(x => x.Country);
            List<StateItem> list = new List<StateItem>();

            foreach (var item in all)
            {
                var country_name = item.Country.Trim();
                var type = item.TypeValue;

                if (list.Count != 0)
                {
                    bool found = false;

                    // Find if one element exist in the list matching country_name
                    foreach (var state_item in list)
                    {
                        // if matched then increment is counter for the specific type and break from the loop
                        if (state_item.Country_Name == country_name)
                        {
                            state_item.incr_nb_type(type);
                            found = true;
                            break;
                        }
                    }
                    // if we get here means that we dont find any element matching "country_name" so create one and add it to the list
                    if (!found)
                    {
                        var new_state_item = new StateItem(country_name);
                        new_state_item.incr_nb_type(type);
                        list.Add(new_state_item);
                    }
                }
                else
                {
                    // 1st element added into the list of StateItem
                    var state_item = new StateItem(country_name);
                    state_item.incr_nb_type(type);
                    list.Add(state_item);
                }
            }
            list.OrderBy(x => x.Country_Name);
            // Add the last item which contains the total of each place type
            var last_item = new StateItem("Total");
            last_item.GetTotal(lRepo);
            list.Add(last_item);

            return View(MVC.Admin.Views.Statistic, list);
        }

        public virtual ActionResult Last100Modif()
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var all = lRepo.GetAll().OrderByDescending(x => x.MemberEditions.LastOrDefault().ModificationDate).Take(100);

            return View(MVC.Admin.Views.LastModif, all);
        }

        #endregion
    }
}
