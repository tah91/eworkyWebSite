using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;
using Worki.Web.Helpers;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;
using Worki.Memberships;
using Worki.Infrastructure.Email;
using System.IO;
using Worki.Service;
using Worki.Web.Model;
using System.Web.UI;

namespace Worki.Web.Areas.Backoffice.Controllers
{
	public partial class ClientController : BackofficeControllerBase
    {
        IMembershipService _MembershipService;
		IInvoiceService _InvoiceService;

        public ClientController(ILogger logger,
                                IObjectStore objectStore,
                                IMembershipService membershipService,
                                IInvoiceService invoiceService)
            : base(logger, objectStore)
        {
            _MembershipService = membershipService;
			_InvoiceService  = invoiceService;
        }

        public const int PageSize = 5;

        /// <summary>
        /// Get action method to show clients of the owner
        /// </summary>
        /// <returns>View containing the clients</returns>
        public virtual ActionResult List(int? page)
        {
            var id = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var p = page ?? 1;
            try
            {
                var member = mRepo.Get(id);
                Member.Validate(member);

				var clients = new List<Member>();
				foreach (var loc in member.Localisations)
				{
					clients = clients.Concat(loc.LocalisationClients.Select(mc => mc.Member)).ToList();
				}
                var model = new PagingList<Member>
                {
                    List = clients.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
                    PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = clients.Count() }
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _Logger.Error("List", ex);
                return View(MVC.Shared.Views.Error);
            }
        }

		/// <summary>
		/// Get action method to show clients of a localisation
		/// </summary>
		/// <returns>View containing the clients</returns>
		public virtual ActionResult LocalisationList(int id, int? page)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var p = page ?? 1;
			try
			{
				var localisation = lRepo.Get(id);

				var clients = localisation.LocalisationClients.Select(mc => mc.Member);
				var model = new PagingList<Member>
				{
					List = clients.Skip((p - 1) * PageSize).Take(PageSize).ToList(),
					PagingInfo = new PagingInfo { CurrentPage = p, ItemsPerPage = PageSize, TotalItems = clients.Count() }
				};
				return View(new LocalisationModel<PagingList<Member>> { InnerModel = model, LocalisationModelId = id });
			}
			catch (Exception ex)
			{
				_Logger.Error("LocalisationList", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

        /// <summary>
        /// Get action method to add a client to a localisation
        /// </summary>
		/// <param name="id">localisation id</param>
        /// <returns>View containing the client data</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult Add(int id)
        {
			return View(new LocalisationModel<ProfilFormViewModel> { InnerModel = new ProfilFormViewModel(), LocalisationModelId = id });
        }

        /// <summary>
        /// Post action method to add a client
        /// </summary>
        /// <returns>Redirect to client list if ok</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateOnlyIncomingValues]
		public virtual ActionResult Add(int id, LocalisationModel<ProfilFormViewModel> formData)
        {
            //the other page that use the same models don't require the phonenumber, so we check only in this method if it's required
            if (String.IsNullOrEmpty(formData.InnerModel.Member.MemberMainData.PhoneNumber))
            {
                ModelState.AddModelError("InnerModel.Member.MemberMainData.PhoneNumber", string.Format(Worki.Resources.Validation.ValidationString.Required, Worki.Resources.Models.Profile.Profile.PhoneNumber));
            }

            if (ModelState.IsValid)
            {
                var memberId = WebHelper.GetIdentityId(User.Identity);
                var sendNewAccountMail = false;
                try
                {
                    var clientId = 0;
                    sendNewAccountMail = _MembershipService.TryCreateAccount(formData.InnerModel.Member.Email, formData.InnerModel.Member.MemberMainData, out clientId);

                    var context = ModelFactory.GetUnitOfWork();
                    var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
					var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
					var localisation = lRepo.Get(id); 
                    var client = mRepo.Get(clientId);
					if (localisation.HasClient(clientId))
                    {
                        throw new Exception(Worki.Resources.Views.BackOffice.BackOfficeString.ClientAlreadyExists);
                    }
                    try
                    {
						localisation.LocalisationClients.Add(new LocalisationClient { ClientId = clientId });

						TryUpdateModel(client, "InnerModel.Member");

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
                            newMemberMail.To = client.Email;
                            newMemberMail.ToName = client.MemberMainData.FirstName;

							newMemberMail.Subject = Worki.Resources.Email.BookingString.ClientCreationAccountSubject;
							newMemberMail.Content = string.Format(Worki.Resources.Email.BookingString.ClientCreationAccount,
																	client.Email,
																	_MembershipService.GetPassword(client.Email, null),
																	passwordLink,
																	profilLink);
                        }

                        context.Commit();

                        if (sendNewAccountMail)
                        {
                            newMemberMail.Send();
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex.Message);
                        context.Complete();
                        throw ex;
                    }

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.ClientAdded;
					return RedirectToAction(MVC.Backoffice.Client.LocalisationList(id, null));
                }
                catch (Exception ex)
                {
                    _Logger.Error("Add", ex);
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(formData);
        }

		/// <summary>
		/// Get action method to edit a client
		/// </summary>
		/// <param name="id">localisation id</param>
		/// <param name="clientId">client id</param>
		/// <returns>View containing the client data</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult Edit(int id, int clientId)
		{
			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var client = mRepo.Get(clientId);
			if (client == null)
			{
				return View(MVC.Shared.Views.Error);
			}
			var innerModel = new ProfilFormViewModel { Member = client };
			return View(MVC.Backoffice.Client.Views.Add, new LocalisationModel<ProfilFormViewModel> { InnerModel = innerModel, LocalisationModelId = id });
		}

		/// <summary>
		/// Post action method to edit a client
		/// </summary>
		/// <returns>Redirect to client list if ok</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		[ValidateOnlyIncomingValues]
		public virtual ActionResult Edit(int id, int clientId, LocalisationModel<ProfilFormViewModel> formData)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var context = ModelFactory.GetUnitOfWork();
					var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
					var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
					var localisation = lRepo.Get(id);
					var client = mRepo.Get(clientId);
					try
					{
                        TryUpdateModel(client, "InnerModel.Member");
						context.Commit();
					}
					catch (Exception ex)
					{
						_Logger.Error(ex.Message);
						context.Complete();
						throw ex;
					}

					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.BackOffice.BackOfficeString.ClientAdded;
					return RedirectToAction(MVC.Backoffice.Client.LocalisationList(id, null));
				}
				catch (Exception ex)
				{
					_Logger.Error("Edit", ex);
					ModelState.AddModelError("", ex.Message);
				}
			}
			return View(MVC.Backoffice.Client.Views.Add, formData);
		}

		#region Invoices

		/// <summary>
		/// Get action method to show invoices of the owner
		/// </summary>
		/// <returns>View containing the invoices</returns>
		public virtual ActionResult Invoices(int id, string date = "")
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var iRepo = ModelFactory.GetRepository<IInvoiceRepository>(context);
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				MonthYear monthYear;
				if (string.IsNullOrEmpty(date))
				{
					monthYear = MonthYear.GetCurrent();
				}
				else
				{
					monthYear = MonthYear.Parse(date);
				}

                var localisation = lRepo.Get(id);

                DateTime initial;
                var invoices = iRepo.GetInvoices(id, monthYear, out initial).ToList();

				var model = new InvoiceListViewModel
				{
					Invoices = new MonthYearList<Invoice>
					{
                        List = invoices,
						Current = monthYear,
						Initial = MonthYear.FromDateTime(initial)
					},
					Localisation = localisation
				};
				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("Invoices", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// Get action method to export invoices summary
		/// </summary>
		/// <returns>Excel file containing summary</returns>
		public virtual ActionResult GetMonthSummary(int id, string date = "")
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var iRepo = ModelFactory.GetRepository<IInvoiceRepository>(context);
			try
			{
				var member = mRepo.Get(memberId);
				Member.Validate(member);
				MonthYear monthYear;
				if (string.IsNullOrEmpty(date))
				{
					monthYear = MonthYear.GetCurrent();
				}
				else
				{
					monthYear = MonthYear.Parse(date);
				}

                DateTime initial;
                var invoices = iRepo.GetInvoices(id, monthYear, out initial).ToList();

				StringWriter sw = new StringWriter();

				//First line for column names
                var columns = "\""+Worki.Resources.Models.Contact.Contact.LastName+"\";";
                columns+=  "\""+Worki.Resources.Models.Contact.Contact.FirstName+"\";";
                columns += "\"" + Worki.Resources.Models.Booking.Invoice.InvoiceId + "\";";
                columns += "\"" + Worki.Resources.Models.Booking.Invoice.Paid + "\";";
                columns += "\"" + Worki.Resources.Models.Booking.Invoice.Price + "\";";
                columns += "\"" + Worki.Resources.Models.Booking.Invoice.TaxRate + "\";";
                columns += "\"" + Worki.Resources.Models.Booking.Invoice.Description + "\";";
                columns += "\"" + Worki.Resources.Models.Booking.Invoice.DateColumn + "\";";
                columns += "\"" + Worki.Resources.Models.Booking.Invoice.PaymentType + "\";";
                columns += "\"" + Worki.Resources.Models.Profile.Profile.CompanyName + "\";";
                columns += "\"" + Worki.Resources.Models.Profile.Profile.Address + "\";";
                columns += "\"" + Worki.Resources.Models.Profile.Profile.TaxNumber + "\";";

                sw.WriteLine(columns);

                foreach (var item in invoices.Select(i => new InvoiceSummary(i)))
				{
                    sw.WriteLine(string.Format("\"{0}\";\"{1}\";\"{2}\";\"{3}\";\"{4}\";\"{5}\";\"{6}\";\"{7}\";\"{8}\";\"{9}\";\"{10}\";\"{11}\";",
											   item.LastName,
                                               item.FirstName,
                                               item.InvoiceNumber,
                                               item.Paid,
                                               item.Amount.ToString("0.00"),
                                               item.Tax.ToString("0.00"),
                                               item.Description,
                                               item.Date,
                                               item.PaymentType,
                                               item.Company,
                                               item.Address,
                                               item.TaxNumber));
				}

                sw.Write('\r');
                sw.Write('\n');

                var name = string.Format(Worki.Resources.Models.Booking.Invoice.Archive, date);
                Response.AddHeader("Content-Disposition", "attachment; filename=" + name + ".csv");
				Response.ContentType = "text/csv";
				Response.ContentEncoding = System.Text.Encoding.Unicode;
				Response.Write(sw);
				Response.End(); 

				return Content("GetMonthSummary");
			}
			catch (Exception ex)
			{
				_Logger.Error("GetMonthSummary", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

        /// <summary>
        /// Get action method to get invoice pdf
        /// </summary>
        /// <returns>Pdf file containing summary</returns>
		public virtual ActionResult GetInvoice(int id, bool fromBooking)
		{
			var context = ModelFactory.GetUnitOfWork();
			var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
            var iRepo = ModelFactory.GetRepository<IInvoiceRepository>(context);
			try
			{
                Invoice invoiceData = null;
                if (fromBooking)
                {
                    var booking = bRepo.Get(id);
                    invoiceData = new Invoice(booking);
                }
                else
                {
                    invoiceData = iRepo.Get(id);
                }
				
				using (var stream = new MemoryStream())
				{
					_InvoiceService.GenerateInvoice(stream, invoiceData);
					return File(stream.ToArray(), "application/pdf", invoiceData.GetFileName() + ".pdf");
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("GetInvoice", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// GET Action method to create invoice
		/// </summary>
		/// <returns>the form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult CreateInvoice(int id)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);
			if (memberId == 0)
				return View(MVC.Shared.Views.Error);

			try
			{
				var context = ModelFactory.GetUnitOfWork();
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
				var member = mRepo.Get(memberId);
				var localisation = lRepo.Get(id);
				Member.Validate(member);

				var model = new InvoiceFormViewModel(localisation);

				return View(model);
			}
			catch (Exception ex)
			{
				_Logger.Error("CreateInvoice", ex);
				return View(MVC.Shared.Views.Error);
			}
		}

		/// <summary>
		/// POST Action method to create invoice
		/// </summary>
		/// <param name="model">The invoice data from the form</param>
		/// <returns>Back office home page if ok, the form with error if not</returns>
		[AcceptVerbs(HttpVerbs.Post)]
		[ValidateAntiForgeryToken]
		public virtual ActionResult CreateInvoice(int id, InvoiceFormViewModel model)
		{
			var memberId = WebHelper.GetIdentityId(User.Identity);
			if (memberId == 0)
				return View(MVC.Shared.Views.Error);

			var context = ModelFactory.GetUnitOfWork();
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var iRepo = ModelFactory.GetRepository<IInvoiceRepository>(context);

			var member = mRepo.Get(memberId);
			var localisation = lRepo.Get(id);
			if (member == null)
				return View(MVC.Shared.Views.Error);

			if (ModelState.IsValid)
			{
				try
				{
                    model.Invoice.CreationDate = DateTime.UtcNow;
					model.Invoice.InvoiceNumber = new InvoiceNumber();
                    iRepo.Add(model.Invoice);
					context.Commit();

					using (var stream = new MemoryStream())
					{
						context = ModelFactory.GetUnitOfWork();
						iRepo = ModelFactory.GetRepository<IInvoiceRepository>(context);
						var invoiceData = iRepo.Get(model.Invoice.Id);

						_InvoiceService.GenerateInvoice(stream, invoiceData);
						return File(stream.ToArray(), "application/pdf", invoiceData.GetFileName() + ".pdf");
					}
				}
				catch (Exception ex)
				{
                    context.Complete();
					_Logger.Error("CreateInvoice", ex);
					ModelState.AddModelError("", ex.Message);
				}
			}

			context = ModelFactory.GetUnitOfWork();
			lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			localisation = lRepo.Get(id);
			return View(new InvoiceFormViewModel(localisation, model.Invoice));
		}

		/// <summary>
		/// Action result to return invoice item
		/// </summary>
		/// <returns>a partial view</returns>
		public virtual PartialViewResult AddInvoiceItem()
		{
			return PartialView(MVC.Backoffice.Client.Views._InvoiceItem, new InvoiceItem());
		}

		#endregion
    }
}
