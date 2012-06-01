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
		public Offer QuotationOffer { get; set; }

		public MemberQuotationFormViewModel(Member member, Offer offer)
        {
			MemberQuotation = new MemberQuotation();
			QuotationOffer = offer;
            var membetExists = member != null;
            PhoneNumber = membetExists ? member.MemberMainData.PhoneNumber : string.Empty;
            NeedNewAccount = !membetExists;
            FirstName = membetExists ? member.MemberMainData.FirstName : string.Empty;
            LastName = membetExists ? member.MemberMainData.LastName : string.Empty;
            Email = membetExists ? member.Email : string.Empty;
        }

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

    public class PartyRegisterFormViewModel : MemberQuotationFormViewModel
    {
        public PartyRegisterFormViewModel() : base() { }

        public PartyRegisterFormViewModel(Member member, Offer offer)
            : base(member, offer)
        {

        }

        public SelectList FavoritePlaces { get; set; }

        public string FavoritePlaceNames { get; set; }

        [Display(Name = "FavoritePlaceId", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public int FavoritePlaceId { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public string Description { get; set; }
    }

	[MetadataType(typeof(MemberQuotation_Validation))]
	public partial class MemberQuotation
    {
        #region Quotation status

        public enum Status
        {
            Pending,
            Unknown,
            Accepted,
            Refused,
            Cancelled,
			Paid
        }

        public static string GetStatusType(Status type)
        {
            switch (type)
            {
                case Status.Pending:
                    return Worki.Resources.Views.Booking.BookingString.StatusPending;
                case Status.Unknown:
                    return Worki.Resources.Views.Booking.BookingString.StatusUnknown;
                case Status.Accepted:
                    return Worki.Resources.Views.Booking.BookingString.StatusAccepted;
                case Status.Refused:
                    return Worki.Resources.Views.Booking.BookingString.StatusRefused;
                case Status.Cancelled:
                    return Worki.Resources.Views.Booking.BookingString.StatusCancelled;
                case Status.Paid:
                    return Worki.Resources.Views.Booking.BookingString.StatusPaid;
                default:
                    return string.Empty;
            }
        }

        public string GetStatus()
        {
            return GetStatusType((Status)StatusId);
        }

        /// <summary>
        /// Created but not validated by moderation
        /// </summary>
        public bool Pending
        {
            get { return StatusId == (int)Status.Pending; }
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
			get { return StatusId == (int)Status.Paid /*|| (Offer != null && Offer.Localisation != null && !Offer.Localisation.ShouldPayQuotation())*/; }
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
			get { return Unknown /*&& (Offer != null && Offer.Localisation != null && Offer.Localisation.ShouldPayQuotation())*/; }
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
					return string.Format(Worki.Resources.Views.Booking.BookingString.HasAskQuotation, MemberQuotation.Client.GetAnonymousDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case QuotationEvent.Payment:
					return string.Format(Worki.Resources.Views.Booking.BookingString.HasPaidQuotation, MemberQuotation.Owner.GetAnonymousDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case QuotationEvent.Refusal:
					return string.Format(Worki.Resources.Views.Booking.BookingString.HasRefuseQuotation, MemberQuotation.Owner.GetAnonymousDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case QuotationEvent.Cancellation:
					return string.Format(Worki.Resources.Views.Booking.BookingString.HasCancelQuotation, MemberQuotation.Client.GetAnonymousDisplayName(), MemberQuotation.Offer.Localisation.Name);
                case QuotationEvent.General:
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
