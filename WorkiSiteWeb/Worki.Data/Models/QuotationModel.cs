using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.Linq;

namespace Worki.Data.Models
{
    public class MemberQuotationFormViewModel : MasterOfferFormViewModel<MemberQuotation>
	{
		public MemberQuotationFormViewModel()
		{
            MemberOffer = new MemberQuotation();
		}
	}

    [MetadataType(typeof(MemberQuotation_Validation))]
	public partial class MemberQuotation : MasterMemberOffer
    {
        #region Quotation status

        /// <summary>
        /// Created but not handled by owner yet
        /// </summary>
        public override bool Unknown
        {
            get { return StatusId == (int)Status.Unknown; }
        }


        /// <summary>
        /// Refused by owner
        /// </summary>
        public override bool Refused
        {
            get { return StatusId == (int)Status.Refused; }
        }

        /// <summary>
        /// Cancelled by client
        /// </summary>
        public override bool Cancelled
        {
            get { return StatusId == (int)Status.Cancelled; }
        }

        /// <summary>
        /// Accepted and paid by client
        /// </summary>
        public bool Paid
        {
            get { return StatusId == (int)Status.Paid; }
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
        public override DateTime CreationDate
        {
            get { return (from item in MemberQuotationLogs where item.EventType == (int)MemberQuotationLog.OfferEvent.Creation select item.CreatedDate).FirstOrDefault(); }
        }

        /// <summary>
		/// Cancellation date of the quotation by the client
        /// </summary>
        public override DateTime CancellationDate
        {
            get { return (from item in MemberQuotationLogs where item.EventType == (int)MemberQuotationLog.OfferEvent.Cancellation select item.CreatedDate).FirstOrDefault(); }
        }

		/// <summary>
		/// Refusal date of the quotation by the client
		/// </summary>
        public override DateTime RefusalDate
		{
            get { return (from item in MemberQuotationLogs where item.EventType == (int)MemberQuotationLog.OfferEvent.Refusal select item.CreatedDate).FirstOrDefault(); }
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
        public override bool ClientCanCancel
        {
            get { return Unknown; }
        }

        public override void GetStatus(out string status, out string color, out DateTime? date)
        {
            status = "";
            color = "";
			date = null;
            if (Unknown)
            {
                color = "Yellow";
                status = Worki.Resources.Views.Booking.BookingString.WaitingStatus;
            }
            else if (Refused)
            {
                status = Worki.Resources.Views.Booking.BookingString.QuotationRefuse;
				date = RefusalDate;
                color = "Red";
            }
            else if (Cancelled)
            {
                status = Worki.Resources.Views.Booking.BookingString.QuotationCancel;
				date = CancellationDate;
                color = "Gray";
            }
            else if (Paid)
            {
                status = Worki.Resources.Views.Booking.BookingString.ContactStatus;
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

    public partial class MemberQuotationLog : MasterMemberLog
    {
        public override string GetDisplay()
        {
            var type = (OfferEvent)EventType;
            switch (type)
            {
                case OfferEvent.Creation:
					return string.Format(Worki.Resources.Views.Booking.BookingString.HasAskQuotation, MemberQuotation.Client.GetAnonymousDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case OfferEvent.Payment:
					return string.Format(Worki.Resources.Views.Booking.BookingString.HasPaidQuotation, MemberQuotation.Owner.GetAnonymousDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case OfferEvent.Refusal:
					return string.Format(Worki.Resources.Views.Booking.BookingString.HasRefuseQuotation, MemberQuotation.Owner.GetAnonymousDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case OfferEvent.Cancellation:
					return string.Format(Worki.Resources.Views.Booking.BookingString.HasCancelQuotation, MemberQuotation.Client.GetAnonymousDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case OfferEvent.General:
                default:
                    return Event;
            }
        }
    }

    public class LocalisationQuotationViewModel : MasterViewModel<MemberQuotation, Localisation>
    {
    }

    public class OfferQuotationViewModel : MasterViewModel<MemberQuotation, Offer>
    {
    }
}
