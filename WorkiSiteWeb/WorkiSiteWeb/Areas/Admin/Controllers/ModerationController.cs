using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure.Repository;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;
using Worki.Data.Models;

namespace Worki.Web.Areas.Admin.Controllers
{
	public partial class ModerationController : AdminControllerBase
    {
        ILogger _Logger;

        public ModerationController(ILogger logger)
        {
            _Logger = logger;
        }

        #region Statistic

        public virtual ActionResult Stat()
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var all = lRepo.GetAll();

            var req = from p in all
                      group p by new { p.CountryId } into g
					  select new { Count = g.Count(), Country = g.Key.CountryId };

            IList<StateItem> list = new List<StateItem>();
			var countries = Localisation.GetCountries();

            foreach (var item in req)
            {
				StateItem item_stat = new StateItem(countries[item.Country], item.Count);
				item_stat.SpotWifi = all.Where(x => item_stat.Country_Name == x.CountryId && x.TypeValue == (int)LocalisationType.SpotWifi).Count();
				item_stat.CoffeeResto = all.Where(x => item_stat.Country_Name == x.CountryId && x.TypeValue == (int)LocalisationType.CoffeeResto).Count();
				item_stat.Biblio = all.Where(x => item_stat.Country_Name == x.CountryId && x.TypeValue == (int)LocalisationType.Biblio).Count();
				item_stat.PublicSpace = all.Where(x => item_stat.Country_Name == x.CountryId && x.TypeValue == (int)LocalisationType.PublicSpace).Count();
				item_stat.TravelerSpace = all.Where(x => item_stat.Country_Name == x.CountryId && x.TypeValue == (int)LocalisationType.TravelerSpace).Count();
				item_stat.Hotel = all.Where(x => item_stat.Country_Name == x.CountryId && x.TypeValue == (int)LocalisationType.Hotel).Count();
				item_stat.Telecentre = all.Where(x => item_stat.Country_Name == x.CountryId && x.TypeValue == (int)LocalisationType.Telecentre).Count();
				item_stat.BuisnessCenter = all.Where(x => item_stat.Country_Name == x.CountryId && x.TypeValue == (int)LocalisationType.BuisnessCenter).Count();
				item_stat.CoworkingSpace = all.Where(x => item_stat.Country_Name == x.CountryId && x.TypeValue == (int)LocalisationType.CoworkingSpace).Count();
				item_stat.WorkingHotel = all.Where(x => item_stat.Country_Name == x.CountryId && x.TypeValue == (int)LocalisationType.WorkingHotel).Count();
				item_stat.PrivateArea = all.Where(x => item_stat.Country_Name == x.CountryId && x.TypeValue == (int)LocalisationType.PrivateArea).Count();
				item_stat.SharedOffice = all.Where(x => item_stat.Country_Name == x.CountryId && x.TypeValue == (int)LocalisationType.SharedOffice).Count();
                list.Add(item_stat);
            }

            return View(MVC.Admin.Moderation.Views.Statistic, list.OrderBy(x => x.Country_Name).ToList());
        }

        public virtual ActionResult Last100Modif(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var pageValue = page ?? 1;
            var modifications = lRepo.GetLatestModifications(100, EditionType.Edition).Skip((pageValue - 1) * MiscHelpers.Constants.PageSize).Take(MiscHelpers.Constants.PageSize).ToList();
            var viewModel = new PagingList<MemberEdition>()
            {
                List = modifications,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = 100
                }
            };
            return View(MVC.Admin.Moderation.Views.LastModif, viewModel);
        }

        public virtual ActionResult LastCreate(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var pageValue = page ?? 1;
            var modifications = lRepo.GetLatestModifications(100, EditionType.Creation).Skip((pageValue - 1) * MiscHelpers.Constants.PageSize).Take(MiscHelpers.Constants.PageSize).ToList();
            var viewModel = new PagingList<MemberEdition>()
            {
                List = modifications,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = 100
                }
            };
            return View(MVC.Admin.Moderation.Views.LastCreate, viewModel);
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
            var bookings = bRepo.Get((pageValue - 1) * MiscHelpers.Constants.PageSize, MiscHelpers.Constants.PageSize, mb => mb.Id);
            var viewModel = new PagingList<MemberBooking>()
            {
                List = bookings,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = bRepo.GetCount()
                }
            };
            return View(viewModel);
        }

        #endregion

        #region Admin Quotation

        /// <summary>
        /// Prepares a web page containing a paginated list of localisations
        /// </summary>
        /// <param name="page">The page to display</param>
        /// <returns>The action result.</returns>
        public virtual ActionResult IndexQuotation(int? page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
            var pageValue = page ?? 1;
            var quotations = qRepo.Get((pageValue - 1) * MiscHelpers.Constants.PageSize, MiscHelpers.Constants.PageSize, mb => mb.Id);
            var viewModel = new PagingList<MemberQuotation>()
            {
                List = quotations,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = pageValue,
                    ItemsPerPage = MiscHelpers.Constants.PageSize,
                    TotalItems = qRepo.GetCount()
                }
            };
            return View(viewModel);
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

		///// <summary>
		///// Prepares a web page to upload a csv file
		///// </summary>
		///// <param name="page">The page to display</param>
		///// <returns>The action result.</returns>
		//[AcceptVerbs(HttpVerbs.Post)]
		//[ValidateAntiForgeryToken]
		//public virtual ActionResult IndexImport(FormCollection collection)
		//{
		//    char CSV_SEPARATOR = ';';
		//    string featureTrueIndicator = Worki.Resources.Views.Shared.SharedString.Yes; // by default, it's false. It's true only for the string
		//    int nbCol = 20;
		//    bool isHeaderLine = false;
		//    if (collection.Get("importCsvHeader") != null && collection.Get("importCsvHeader") == "on")
		//        isHeaderLine = true;
		//    int nbLocalisationsAdded = 0;
		//    string listLocalisationsAlreadyInDB = "";
		//    var context = ModelFactory.GetUnitOfWork();
		//    var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
		//    var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
		//    foreach (string name in Request.Files)
		//    {
		//        try
		//        {
		//            var postedFile = Request.Files[name];
		//            if (postedFile == null || string.IsNullOrEmpty(postedFile.FileName))
		//                continue;

		//            int fileLen;
		//            fileLen = postedFile.ContentLength;
		//            byte[] input = new byte[fileLen];

		//            StreamReader sr = new StreamReader(postedFile.InputStream, System.Text.Encoding.Default); // Using of encoding to don't loose french accents

		//            // Because we can have a csv line in multiple line in files (because in a value, we can have line-return), we have to check it
		//            string fullCSVLine = "";
		//            while (sr.Peek() >= 0) // Read of each line of CSV file
		//            {
		//                fullCSVLine += sr.ReadLine();
		//                string[] infosLocalisation = fullCSVLine.Split(CSV_SEPARATOR);

		//                // We have not the full CSV line
		//                if (infosLocalisation.Length < nbCol)
		//                    continue;

		//                if (!isHeaderLine)
		//                {
		//                    var localisationToAdd = new Localisation();
		//                    localisationToAdd.Name = infosLocalisation[1];
		//                    localisationToAdd.Adress = infosLocalisation[2];
		//                    localisationToAdd.PostalCode = infosLocalisation[3];
		//                    localisationToAdd.City = infosLocalisation[4];
		//                    localisationToAdd.Country = infosLocalisation[5];
		//                    localisationToAdd.PhoneNumber = infosLocalisation[6];
		//                    localisationToAdd.Fax = infosLocalisation[7];
		//                    localisationToAdd.Mail = infosLocalisation[8];
		//                    localisationToAdd.WebSite = infosLocalisation[9];
		//                    localisationToAdd.Description = infosLocalisation[10];
		//                    double latitude, longitude;
		//                    Double.TryParse(infosLocalisation[18].Replace(',', '.'), out latitude);
		//                    Double.TryParse(infosLocalisation[19].Replace(',', '.'), out longitude);
		//                    localisationToAdd.Latitude = latitude;
		//                    localisationToAdd.Longitude = longitude;

		//                    // Description in English : localisationToAdd.DescriptionEnglish = infosLocalisation[11]; ??
		//                    var member = mRepo.GetMember(infosLocalisation[0]);
		//                    localisationToAdd.SetOwner(member.MemberId);

		//                    if (infosLocalisation[12].Trim().ToLower() == featureTrueIndicator.ToLower())
		//                        localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.VisioRoom, OfferID = (int)FeatureType.VisioRoom });
		//                    if (infosLocalisation[13].Trim().ToLower() == featureTrueIndicator.ToLower())
		//                        localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.MeetingRoom, OfferID = (int)FeatureType.MeetingRoom });
		//                    if (infosLocalisation[14].Trim().ToLower() == featureTrueIndicator.ToLower())
		//                        localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.SeminarRoom, OfferID = (int)FeatureType.SeminarRoom });
		//                    if (infosLocalisation[15].Trim().ToLower() == featureTrueIndicator.ToLower())
		//                        localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.BuisnessLounge, OfferID = (int)FeatureType.WorkingPlace });
		//                    if (infosLocalisation[16].Trim().ToLower() == featureTrueIndicator.ToLower())
		//                        localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Workstation, OfferID = (int)FeatureType.WorkingPlace });
		//                    if (infosLocalisation[17].Trim().ToLower() == featureTrueIndicator.ToLower())
		//                        localisationToAdd.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Desktop, OfferID = (int)FeatureType.WorkingPlace });

		//                    localisationToAdd.TypeValue = (int)LocalisationType.BuisnessCenter;

		//                    var similarLoc = (from loc
		//                                         in lRepo.FindSimilarLocalisation((float)localisationToAdd.Latitude, (float)localisationToAdd.Longitude)
		//                                      where string.Compare(loc.Name, localisationToAdd.Name, StringComparison.InvariantCultureIgnoreCase) == 0
		//                                      select loc).ToList();
		//                    if (similarLoc.Count > 0)
		//                    {
		//                        //temp, add wifi
		//                        if (similarLoc.Count() == 1)
		//                        {
		//                            var loc = similarLoc[0];
		//                            if (!loc.HasFeature(Feature.Wifi_Free))
		//                            {
		//                                var l = lRepo.Get(loc.ID);
		//                                l.LocalisationFeatures.Add(new LocalisationFeature { FeatureID = (int)Feature.Wifi_Free, OfferID = (int)FeatureType.General });
		//                            }
		//                        }
		//                        listLocalisationsAlreadyInDB += "&bull; " + localisationToAdd.Name + "<br />";
		//                    }
		//                    else
		//                    {
		//                        lRepo.Add(localisationToAdd);
		//                        member.MemberEditions.Add(new MemberEdition { ModificationDate = DateTime.UtcNow, LocalisationId = localisationToAdd.ID, ModificationType = (int)EditionType.Creation });
		//                        nbLocalisationsAdded++;
		//                    }
		//                }
		//                else
		//                    isHeaderLine = false; // because we have skipped the first line

		//                fullCSVLine = ""; // Reinitialization because we have found the full CVS line

		//            }
		//            context.Commit();
		//        }
		//        catch (Exception ex)
		//        {
		//            _Logger.Error("Edit", ex);
		//            ModelState.AddModelError("", string.Format(Worki.Resources.Views.Shared.SharedString.AddLocError, nbLocalisationsAdded, ex.Message));
		//            context.Complete();
		//        }
		//    }

		//    AdminImportViewModel viewModel = new AdminImportViewModel();
		//    viewModel.resultMessage = string.Format(Worki.Resources.Views.Shared.SharedString.LocAdded, nbLocalisationsAdded);
		//    viewModel.localisationsAlreadyInDB = listLocalisationsAlreadyInDB;
		//    return View(viewModel);
		//}

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
			string featureTrueIndicator = Worki.Resources.Views.Shared.SharedString.Yes; // by default, it's false. It's true only for the string
			int nbCol = 5;
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
							var locName = infosLocalisation[1];
							var enDesc = infosLocalisation[3];

							var loc = lRepo.Get(l => string.Compare(l.Name, locName, StringComparison.InvariantCultureIgnoreCase) == 0);

							if (loc ==null)
							{
								listLocalisationsAlreadyInDB += "&bull; " + locName + "<br />";
							}
							else
							{
								loc.DescriptionEn = enDesc;
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
					ModelState.AddModelError("", string.Format(Worki.Resources.Views.Shared.SharedString.AddLocError, nbLocalisationsAdded, ex.Message));
					context.Complete();
				}
			}

			AdminImportViewModel viewModel = new AdminImportViewModel();
			viewModel.resultMessage = string.Format(Worki.Resources.Views.Shared.SharedString.LocAdded, nbLocalisationsAdded);
			viewModel.localisationsAlreadyInDB = listLocalisationsAlreadyInDB;
			return View(viewModel);
		}

        #endregion

		#region Migration

		public virtual ActionResult MigrateOfferPrices()
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);

			var message = "";
			try
			{
				var offersWithPrice = oRepo.GetMany(o => o.Price != 0);
				var count = 0;
				foreach (var item in offersWithPrice)
				{
					if (item.OfferPrices.Count != 0)
						continue;

					item.OfferPrices.Add(new OfferPrice
					{
						Price = item.Price,
						PriceType = item.Period
					});
					count++;
				}

				message = string.Format("migration of {0} offers", count);

				context.Commit();
			}
			catch (Exception ex)
			{
				_Logger.Error("MigrateOfferPrices", ex);
				context.Complete();
				Content(ex.Message);
			}
			return Content(message);
		}

		public virtual ActionResult MigrateClients()
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);

			var memberWithClients = mRepo.GetMany(m => m.MemberClients.Count != 0);

			var message = "";
			try
			{
				var count = 0;
				foreach (var item in memberWithClients)
				{
					var localisations = lRepo.GetMostBooked(item.MemberId, 1);
					if (localisations.Count == 0)
						continue;

					var loc = localisations[0];
					if (loc == null)
						continue;

					foreach (var client in item.MemberClients)
					{
						loc.LocalisationClients.Add(new LocalisationClient { ClientId = client.ClientId });
					}
					count++;
				}

				message = string.Format("addition of clients to {0} localisations", count);

				context.Commit();
			}
			catch (Exception ex)
			{
				_Logger.Error("MigrateClients", ex);
				context.Complete();
				Content(ex.Message);
			}
			return Content(message);
		}

		#endregion
	}
}
