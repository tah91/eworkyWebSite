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
            Refused
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
        /// Need action of the owner
        /// </summary>
        public bool NeedOwnerAction
        {
            get { return Unknown; }
        }

        /// <summary>
        /// Need action of the client
        /// </summary>
        public bool NeedClientAction
        {
            get { return false; }
        }

        public void GetStatusForOwner(out string status, out string color)
        {
            status = "";
            color = "";
            if (Unknown)
            {
                color = "Yellow";
                status = "";
            }
            else if (Refused)
            {
                status = "Refusée";
                color = "Red";
            }
            else if (Paid)
            {
                status = "Payé le " + PaidDate;
                color = "Green";
            }
        }

        public void GetStatusForClient(out string status, out string color)
        {
            status = "";
            color = "";
            if (Unknown)
            {
                status = "En attente de confirmation";
                color = "Yellow";
            }
            else if (Refused)
            {
                status = "Refusée";
                color = "Red";
            }
            else if (Paid)
            {
                status = "Payé le " + PaidDate;
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
	}

    public partial class MemberQuotationLog
    {
        public enum QuotationEvent
        {
            General,
            Creation,
            Payment
        }

        public string GetDisplay()
        {
            var type = (QuotationEvent)EventType;
            switch (type)
            {
                case QuotationEvent.Creation:
                    return string.Format("{0} a fait une demande de devis pour le lieu {1}", MemberQuotation.Client.GetFullDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case QuotationEvent.Payment:
                    return string.Format("{0} a payé la demande de devis pour le lieu {1}", MemberQuotation.Owner.GetFullDisplayName(), MemberQuotation.Offer.Localisation.Name);
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
