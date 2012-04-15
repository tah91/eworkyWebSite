using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.Linq;
using System.ComponentModel;
using Worki.Infrastructure.Helpers;

namespace Worki.Data.Models
{
    [MetadataType(typeof(Invoice_Validation))]
    public partial class Invoice
    {
        #region Ctor

        public void Init(Localisation localisation)
        {
            Localisation = localisation;
            LocalisationId = localisation.ID;
            Title = localisation.Name;
            TaxRate = localisation.Member.MemberMainData.TaxRate;
        }

        public Invoice(MemberBooking booking)
        {
            MemberId = booking.Client.MemberId;
            LocalisationId = booking.Offer.Localisation.ID;
            Member = booking.Client;
            Localisation = booking.Offer.Localisation;
            InvoiceItems = new List<InvoiceItem> { new InvoiceItem { Description = booking.Offer.Name, Quantity = 1, Price = booking.Price, Invoice = this } };
            Id = booking.Id;
			Title = booking.Offer.Name;
            Currency = booking.Offer.Currency;
            CreationDate = booking.CreationDate;
            TaxRate = booking.Owner.MemberMainData.TaxRate;
            IsFromBooking = true;
        }

        #endregion

        public bool IsFromBooking { get; set; }

        #region Utils

		public string GetFileName()
		{
			return CultureHelpers.GetSpecificFormat(CreationDate, CultureHelpers.TimeFormat.Date) + " - " + Localisation.Name + " - " + Member.GetFullDisplayName();
		}

        public decimal GetTotalWithoutTax()
        {
			return InvoiceItems.Sum(i => i.Price * i.Quantity) * (1 - TaxRate / 100);
        }

        public decimal GetTotalTax()
        {
            return InvoiceItems.Sum(i => i.Price * i.Quantity) * TaxRate / 100;
        }

        public decimal GetTotal()
        {
            return InvoiceItems.Sum(i => i.Price * i.Quantity);
        }

        public string GetTotalDisplay()
        {
            return GetTotal().GetPriceDisplay((Offer.CurrencyEnum)Currency);
        }

        #endregion
    }

    [Bind(Exclude = "Id")]
    public class Invoice_Validation
    {
        [Display(Name = "ClientId", ResourceType = typeof(Worki.Resources.Models.Booking.Invoice))]
        public int MemberId { get; set; }

        [Display(Name = "PaymentType", ResourceType = typeof(Worki.Resources.Models.Booking.Invoice))]
        public int PaymentType { get; set; }

        [Display(Name = "Currency", ResourceType = typeof(Worki.Resources.Models.Booking.Invoice))]
        public int Currency { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Title", ResourceType = typeof(Worki.Resources.Models.Booking.Invoice))]
        public string Title { get; set; }

        [Display(Name = "TaxRate", ResourceType = typeof(Worki.Resources.Models.Booking.Invoice))]
        public decimal TaxRate { get; set; }
    }

    [MetadataType(typeof(InvoiceItem_Validation))]
    public partial class InvoiceItem
    {
		public decimal GetWithoutTax()
		{
			return Price * (1 - Invoice.TaxRate / 100);
		}
    }

    public class InvoiceItem_Validation
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Description", ResourceType = typeof(Worki.Resources.Models.Booking.Invoice))]
        public string Description { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Price", ResourceType = typeof(Worki.Resources.Models.Booking.Invoice))]
        public decimal Price { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Quantity", ResourceType = typeof(Worki.Resources.Models.Booking.Invoice))]
        public int Quantity { get; set; }
    }

    public class InvoiceListViewModel
    {
        public MonthYearList<Invoice> Invoices { get; set; }
        public Localisation Localisation { get; set; }
    }

    public class InvoiceFormViewModel
    {
        #region Ctor

        public InvoiceFormViewModel()
        {
            Invoice = new Invoice();
        }

        public InvoiceFormViewModel(Localisation localisation, Invoice model = null)
        {
            Invoice = model ?? new Invoice();
            Invoice.Init(localisation);
            var clients = localisation.LocalisationClients.ToDictionary(mc => mc.ClientId, mc => mc.Member.GetFullDisplayName());
            Clients = new SelectList(clients, "Key", "Value");
            PaymentTypes = new SelectList(Offer.GetPaymentTypeEnumTypes(), "Key", "Value", Offer.PaymentTypeEnum.Paypal);
            Currencies = new SelectList(Offer.GetCurrencyEnumTypes(), "Key", "Value", Offer.CurrencyEnum.EUR);
        }

        #endregion

        #region Properties

        public SelectList Clients { get; set; }
        public SelectList PaymentTypes { get; set; }
        public SelectList Currencies { get; set; }
        public Invoice Invoice { get; set; }

        #endregion

        public Invoice GetInvoiceModel(Localisation localisation)
        {
            Invoice.Localisation = localisation;
            return Invoice;
        }
    }

	public class InvoiceSummary
	{
		#region Properties

		public string LastName { get; set; }

		public string FirstName { get; set; }

		public string InvoiceNumber { get; set; }

		public string Paid { get; set; }

		public decimal Amount { get; set; }

		public decimal Tax { get; set; }

		public string Description { get; set; }

		public string Date { get; set; }

		public string PaymentType { get; set; }

		public string Company { get; set; }

		public string Address { get; set; }

		public string TaxNumber { get; set; }

		#endregion

        public InvoiceSummary(Invoice invoice)
		{
            LastName = invoice.Member.MemberMainData.LastName;
            FirstName = invoice.Member.MemberMainData.FirstName;
            Address = invoice.Member.MemberMainData.Address + " " + invoice.Member.MemberMainData.City;
            TaxNumber = invoice.Member.MemberMainData.TaxNumber;
            Date = CultureHelpers.GetSpecificFormat(invoice.CreationDate, CultureHelpers.TimeFormat.Date);
            PaymentType = Offer.GetPaymentTypeEnumType(invoice.PaymentType);
            InvoiceNumber = invoice.Id.ToString();
            Description = invoice.Title;
            Amount = invoice.GetTotal();
            Tax = Amount * invoice.TaxRate / 100;
            //Paid = true ? Worki.Resources.Views.Shared.SharedString.Yes : Worki.Resources.Views.Shared.SharedString.No;
            Paid = Worki.Resources.Views.Shared.SharedString.Yes;
		}
	}
}
