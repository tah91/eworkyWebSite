using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.Linq;

namespace Worki.Data.Models
{
	public class MemberQuotationFormViewModel
	{
		public MemberQuotationFormViewModel()
		{
			MemberQuotation = new MemberQuotation();
		}

		public MemberQuotation MemberQuotation { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "PhoneNumber", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
		public string PhoneNumber { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "FirstName", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "LastName", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Email", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public string Email { get; set; }

        public bool NeedNewAccount { get; set; }
	}

	[MetadataType(typeof(MemberQuotation_Validation))]
	public partial class MemberQuotation
    {
        #region Quotation status

        public enum Status
        {
            Unknown,
            Accepted,
            Refused,
            Cancelled
        }

        /// <summary>
        /// Created but not handled by owner yet
        /// </summary>
        public bool Unknown
        {
            get { return StatusId == (int)Status.Unknown; }
        }


        /// <summary>
        /// Refused by owner
        /// </summary>
        public bool Refused
        {
            get { return StatusId == (int)Status.Refused; }
        }

        /// <summary>
        /// Cancelled by client
        /// </summary>
        public bool Cancelled
        {
            get { return StatusId == (int)Status.Cancelled; }
        }

        /// <summary>
        /// Accepted and paid by client
        /// </summary>
        public bool Paid
        {
            get { return StatusId == (int)Status.Accepted && MemberQuotationTransactions.Where(t => t.StatusId == (int)TransactionConstants.Status.Completed).Count() != 0; }
        }

        /// <summary>
        /// Payement date of paid quotation
        /// </summary>
        public DateTime PaidDate
        {
            get { return (from item in MemberQuotationTransactions where item.UpdatedDate.HasValue select item.UpdatedDate.Value).FirstOrDefault(); }
        }

        /// <summary>
        /// Creation date of the quotation by the client
        /// </summary>
        public DateTime CreationDate
        {
            get { return (from item in MemberQuotationLogs where item.EventType == (int)MemberQuotationLog.QuotationEvent.Creation select item.CreatedDate).FirstOrDefault(); }
        }

        /// <summary>
		/// Cancellation date of the quotation by the client
        /// </summary>
        public DateTime CancellationDate
        {
            get { return (from item in MemberQuotationLogs where item.EventType == (int)MemberQuotationLog.QuotationEvent.Cancellation select item.CreatedDate).FirstOrDefault(); }
        }

		/// <summary>
		/// Refusal date of the quotation by the client
		/// </summary>
		public DateTime RefusalDate
		{
			get { return (from item in MemberQuotationLogs where item.EventType == (int)MemberQuotationLog.QuotationEvent.Refusal select item.CreatedDate).FirstOrDefault(); }
		}

        /// <summary>
        /// Owner can pay
        /// </summary>
        public bool OwnerCanPay
        {
            get { return Unknown; }
        }

        /// <summary>
        /// Owner can refuse
        /// </summary>
        public bool OwnerCanRefuse
        {
            get { return Unknown; }
        }

        /// <summary>
        /// Client can cancel
        /// </summary>
        public bool ClientCanCancel
        {
            get { return Unknown; }
        }

        public void GetStatus(out string status, out string color, out DateTime? date)
        {
            status = "";
            color = "";
			date = null;
            if (Unknown)
            {
                color = "Yellow";
                status = "En attente de prise en charge";
            }
            else if (Refused)
            {
                status = "Demande de devis refusée le ";
				date = RefusalDate;
                color = "Red";
            }
            else if (Cancelled)
            {
				status = "Demande de devis annulé le ";
				date = CancellationDate;
                color = "Gray";
            }
            else if (Paid)
            {
				status = "Contact établi le ";
				date = PaidDate;
                color = "Green";
            }
        }

        #endregion

        #region Member

        public Member Client
        {
            get { return Member; }
        }

        public Member Owner
        {
            get { return Offer.Localisation.Member; }
        }

        #endregion
	}

	[Bind(Exclude = "Id,MemberId,LocalisationId,OfferId")]
	public class MemberQuotation_Validation
	{
		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Surface", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
		public decimal Surface { get; set; }

		[Display(Name = "Message", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
		public string Message { get; set; }

        [Display(Name = "VisitNeeded", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public bool VisitNeeded { get; set; }

        [Display(Name = "Response", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public bool Response { get; set; }
	}

    public partial class MemberQuotationLog
    {
        public enum QuotationEvent
        {
            General,
            Creation,
            Payment,
            Refusal,
            Cancellation
        }

        public string GetDisplay()
        {
            var type = (QuotationEvent)EventType;
            switch (type)
            {
                case QuotationEvent.Creation:
                    return string.Format("{0} a fait une demande de devis pour {1}", MemberQuotation.Client.GetFullDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case QuotationEvent.Payment:
                    return string.Format("{0} a payé la demande de devis pour {1}", MemberQuotation.Owner.GetFullDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case QuotationEvent.Refusal:
                    return string.Format("{0} a refusé la demande de devis pour {1}", MemberQuotation.Owner.GetFullDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case QuotationEvent.Cancellation:
                    return string.Format("{0} a annulé la demande de devis pour {1}", MemberQuotation.Client.GetFullDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case QuotationEvent.General:
                default:
                    return Event;
            }
        }
    }

    public class LocalisationQuotationViewModel
    {
        public PagingList<MemberQuotation> Quotations { get; set; }
        public Localisation Localisation { get; set; }
    }

    public class OfferQuotationViewModel
    {
        public PagingList<MemberQuotation> Quotations { get; set; }
        public Offer Offer { get; set; }
    }
}
