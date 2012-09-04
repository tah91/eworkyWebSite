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
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Collections;
using System.Text;
using System.Net;
using Worki.Web.Helpers;
using Worki.Infrastructure.Email;
using System.Web.Security;
using System.Data;
using System.Net.Mail;

namespace Worki.Web.Areas.Admin.Controllers
{
	public partial class ModerationController : AdminControllerBase
    {
        public ModerationController(ILogger logger, IEmailService emailService)
            : base(logger, emailService)
        {
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
				item_stat.SpotWifi = all.Where(x => item.Country == x.CountryId && x.TypeValue == (int)LocalisationType.SpotWifi).Count();
				item_stat.CoffeeResto = all.Where(x => item.Country == x.CountryId && x.TypeValue == (int)LocalisationType.CoffeeResto).Count();
				item_stat.Biblio = all.Where(x => item.Country == x.CountryId && x.TypeValue == (int)LocalisationType.Biblio).Count();
				item_stat.PublicSpace = all.Where(x => item.Country == x.CountryId && x.TypeValue == (int)LocalisationType.PublicSpace).Count();
				item_stat.TravelerSpace = all.Where(x => item.Country == x.CountryId && x.TypeValue == (int)LocalisationType.TravelerSpace).Count();
				item_stat.Hotel = all.Where(x => item.Country == x.CountryId && x.TypeValue == (int)LocalisationType.Hotel).Count();
				item_stat.Telecentre = all.Where(x => item.Country == x.CountryId && x.TypeValue == (int)LocalisationType.Telecentre).Count();
				item_stat.BuisnessCenter = all.Where(x => item.Country == x.CountryId && x.TypeValue == (int)LocalisationType.BuisnessCenter).Count();
				item_stat.CoworkingSpace = all.Where(x => item.Country == x.CountryId && x.TypeValue == (int)LocalisationType.CoworkingSpace).Count();
				item_stat.WorkingHotel = all.Where(x => item.Country == x.CountryId && x.TypeValue == (int)LocalisationType.WorkingHotel).Count();
				item_stat.PrivateArea = all.Where(x => item.Country == x.CountryId && x.TypeValue == (int)LocalisationType.PrivateArea).Count();
				item_stat.SharedOffice = all.Where(x => item.Country == x.CountryId && x.TypeValue == (int)LocalisationType.SharedOffice).Count();
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

        /// <summary>
        /// GET Action result to Transfer quotation to owner
        /// </summary>
        /// <param name="id">id of Quotation</param>
        /// <returns>View containing Quotation data</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult TransferQuotation(int id, int page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);

            try
            {
                var quotation  = qRepo.Get(id);
                quotation.StatusId = (int)MemberQuotation.Status.Unknown;
                TempData[MiscHelpers.TempDataConstants.Info] = "La demande de devis a bien été transférée";

                //send mail to quotation owner

                var offer = quotation.Offer;
                var localisation = offer.Localisation;
                object ownerMail = null;

                if (Roles.IsUserInRole(localisation.Member.Email, MiscHelpers.BackOfficeConstants.BackOfficeRole))
                {
                    var urlHelp = new UrlHelper(ControllerContext.RequestContext);
                    var ownerUrl = urlHelp.ActionAbsolute(MVC.Backoffice.Localisation.QuotationDetail(quotation.Id));
                    TagBuilder ownerLink = new TagBuilder("a");
                    ownerLink.MergeAttribute("href", ownerUrl);
                    ownerLink.InnerHtml = Worki.Resources.Views.Account.AccountString.OwnerSpace;

                    var ownerMailContent = string.Format(Worki.Resources.Email.BookingString.CreateQuotationOwner,
                                                    Localisation.GetOfferType(offer.Type),
                                                    localisation.Name,
                                                    localisation.Adress,
                                                    ownerLink);

                    ownerMail = _EmailService.PrepareMessageFromDefault(new MailAddress(localisation.Member.Email, localisation.Member.MemberMainData.FirstName),
                            string.Format(Worki.Resources.Email.BookingString.CreateQuotationOwnerSubject, localisation.Name),
                            WebHelper.RenderEmailToString(localisation.Member.MemberMainData.FirstName, ownerMailContent));
                }
                else
                {
                    //add backoffice
                    Roles.AddUserToRole(localisation.Member.Email, MiscHelpers.BackOfficeConstants.BackOfficeRole);

                    //send specific mail
                    var urlHelp = new UrlHelper(ControllerContext.RequestContext);

                    var ownerUrl = urlHelp.ActionAbsolute(MVC.Backoffice.Localisation.QuotationDetail(quotation.Id));
                    TagBuilder ownerLink = new TagBuilder("a");
                    ownerLink.MergeAttribute("href", ownerUrl);
                    ownerLink.InnerHtml = Worki.Resources.Views.Account.AccountString.OwnerSpace;

                    var ownerMailContent =  string.Format(Worki.Resources.Email.BookingString.CreateQuotationAndBOOwner,
                                                    Localisation.GetOfferType(offer.Type),
                                                    localisation.Name,
                                                    localisation.Adress,
                                                    ownerUrl);

                    ownerMail = _EmailService.PrepareMessageFromDefault(new MailAddress(localisation.Member.Email, localisation.Member.MemberMainData.FirstName),
                            string.Format(Worki.Resources.Email.BookingString.CreateQuotationOwnerSubject, localisation.Name),
                            WebHelper.RenderEmailToString(localisation.Member.MemberMainData.FirstName, ownerMailContent));
                }

                context.Commit();

                _EmailService.Deliver(ownerMail);
            }
            catch (Exception ex)
            {
                _Logger.Error("TransferQuotation", ex);
                context.Complete();
            }

            return RedirectToAction(MVC.Admin.Moderation.IndexQuotation(page));
        }

        /// <summary>
        /// GET Action result to Delete quotation
        /// </summary>
        /// <param name="id">id of Quotation</param>
        /// <returns>View containing Quotation data</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult DeleteQuotation(int id, int page)
        {
            var context = ModelFactory.GetUnitOfWork();
            var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);

            try
            {
                qRepo.Delete(id);
                context.Commit();
                TempData[MiscHelpers.TempDataConstants.Info] = "La demande de devis a bien été effacée";
            }
            catch (Exception ex)
            {
                _Logger.Error("DeleteQuotation", ex);
                context.Complete();
            }

            return RedirectToAction(MVC.Admin.Moderation.IndexQuotation(page));
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

        #region Resources

        public class ResourceValue
		{
			public string FrValue { get; set; }
			public string EnValue { get; set; }
			public string EsValue { get; set; }
            public string NlValue { get; set; }
            public string DeValue { get; set; }

			public void SetValue(string culture, string value)
			{
				switch (culture)
				{
					case "fr":
						FrValue = value;
						break;
					case "en":
						EnValue = value;
						break;
					case "es":
						EsValue = value;
						break;
                    case "de":
                        DeValue = value;
                        break;
                    case "nl":
                        NlValue = value;
                        break;
				}
			}

			public bool CompareValue(string culture)
			{
				switch (culture)
				{
					case "en":
						return string.Compare(FrValue, EnValue, false) == 0;
					case "es":
						return string.Compare(FrValue, EsValue, false) == 0;
                    case "de":
                        return string.Compare(FrValue, DeValue, false) == 0;
                    case "nl":
                        return string.Compare(NlValue, DeValue, false) == 0;
					default:
						return false;
				}
			}

            public bool ValueNotFound(string culture)
            {
                switch (culture)
                {
                    case "en":
                        return string.IsNullOrEmpty(EnValue);
                    case "es":
                        return string.IsNullOrEmpty(EsValue);
                    case "de":                             
                        return string.IsNullOrEmpty(DeValue);
                    case "nl":
                        return string.IsNullOrEmpty(NlValue);
                    default:
                        return false;
                }
            }
		}

		void FillOutput(ref Dictionary<string, ResourceValue> output, Type t, string culture)
		{
			var r = new ResourceManager(t);
			var set = r.GetResourceSet(CultureInfo.CreateSpecificCulture(culture), true, true);
			foreach (DictionaryEntry s in set)
			{
				var key = t.Namespace + "." + s.Key.ToString();
				if (output.ContainsKey(key))
				{
					var res = output[key];
					res.SetValue(culture, s.Value.ToString());
				}
				else
				{
					var res = new ResourceValue();
					res.SetValue(culture, s.Value.ToString());
					output.Add(key, res);
				}
			}
		}

		void WriteDuplicates(Dictionary<string, ResourceValue> output, StringBuilder builder, string culture)
		{
			foreach (var item in output)
			{
				if (item.Value.CompareValue(culture))
				{
					builder.AppendLine(item.Key + " textes identiques : " + item.Value.FrValue);
				}
                if (item.Value.ValueNotFound(culture))
                {
                    builder.AppendLine(item.Key + " texte non trouvé  : " + item.Value.FrValue);
                }
			}
		}

		public virtual ActionResult CheckResources()
		{
			var assembly = Assembly.GetAssembly(typeof(Worki.Resources.Email.Activation));
			var types = assembly.GetTypes();
			var output = new Dictionary<string, ResourceValue>();
			foreach (var t in types)
			{
				FillOutput(ref output, t, "fr");
				FillOutput(ref output, t, "en");
				FillOutput(ref output, t, "es");
                FillOutput(ref output, t, "de");
                FillOutput(ref output, t, "nl");
			}

			var builder = new StringBuilder();

			builder.AppendLine("Anglais : ");
			builder.AppendLine();
			WriteDuplicates(output, builder, "en");

			builder.AppendLine();
			builder.AppendLine();
			builder.AppendLine();

			builder.AppendLine("Espagnol : ");
			builder.AppendLine();
			WriteDuplicates(output, builder, "es");

            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine();

            builder.AppendLine("Allemand : ");
            builder.AppendLine();
            WriteDuplicates(output, builder, "de");

            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine();

            builder.AppendLine("Néerlandais : ");
            builder.AppendLine();
            WriteDuplicates(output, builder, "nl");

			var content = MiscHelpers.Nl2Br(builder.ToString());
			return Content(content);
		}

        #endregion

        public virtual ActionResult SendMailToOldOffice()
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);

            var message = "";
            var tenDaysAgo =  DateTime.UtcNow.AddDays(-10);
            var surveyLink = "http://freeonlinesurveys.com/app/rendersurvey.asp?sid=k6ayan6kqlq5jzg103522";
            try
            {
                var oldSharedOffices = lRepo.GetMany(l => l.TypeValue == (int)LocalisationType.SharedOffice &&
                    l.CountryId == "FR" &&
                    l.MemberEditions.FirstOrDefault(me => me.ModificationType == (int)EditionType.Creation).ModificationDate < tenDaysAgo &&
                    l.MainLocalisation.IsOffline == false);

                int count = 1;
                foreach (var item in oldSharedOffices)
                {
                    if (!item.HasOwner())
                        continue;

                    var urlHelp = new UrlHelper(ControllerContext.RequestContext);

                    var offlineAndRedirectUrl = urlHelp.ActionAbsolute(MVC.Localisation.PutOfflineAndRedirect(item.ID, surveyLink));
                    offlineAndRedirectUrl = offlineAndRedirectUrl.Replace("localhost:15157", "www.eworky.fr");
                    TagBuilder offlineAndRedirectLink = new TagBuilder("a");
                    offlineAndRedirectLink.MergeAttribute("href", offlineAndRedirectUrl);
                    offlineAndRedirectLink.InnerHtml = "cliquer sur ce lien pour la mettre hors ligne";

                    var editUrl = urlHelp.ActionAbsolute(MVC.Localisation.Edit(item.ID));
                    editUrl = editUrl.Replace("localhost:15157", "www.eworky.fr");
                    TagBuilder editLink = new TagBuilder("a");
                    editLink.MergeAttribute("href", editUrl);
                    editLink.InnerHtml = "compléter au mieux votre fiche avec les prix des différentes offres et des photos";

                    var content = @"vous avez ajouté une annonce de bureau à partager sur eWorky.

Si vous avez loué votre bureau et que votre annonce n'est plus d'actualité, merci de {0}.

Si vos bureaux sont toujours disponibles ou que vous les louez régulièrement sur du court terme, nous vous invitons à {1}. Les annonces les mieux complétées, avec de belles photos et un prix attractif pour la localisation sont celles qui reçoivent le plus de demandes.
";

                    var oldOfficeMailContent = string.Format(content, offlineAndRedirectLink, editLink);

                    var oldOfficeMail = _EmailService.PrepareMessageFromDefault(new MailAddress(item.Member.Email,item.Member.MemberMainData.FirstName),
                            string.Format("Votre annonce {0} sur eWorky", item.Name),
                            WebHelper.RenderEmailToString(item.Member.MemberMainData.FirstName, oldOfficeMailContent));

                    _EmailService.Deliver(oldOfficeMail);

                    count++;

                    message += item.ID.ToString();
                    message += "\n";
                    if (WebHelper.IsDebug())
                        break;
                }

                message += string.Format("\nsending of {0} mails", count);
            }
            catch (Exception ex)
            {
                _Logger.Error("SendMailToOldOffice", ex);
                return Content(ex.Message);
            }
            return Content(MiscHelpers.Nl2Br(message));
        }
	}
}
