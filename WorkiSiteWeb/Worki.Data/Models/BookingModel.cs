using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.Linq;

namespace Worki.Data.Models
{
	public class MemberBookingFormViewModel
	{
		public MemberBookingFormViewModel()
		{
			MemberBooking = new MemberBooking();
		}

		public MemberBooking MemberBooking { get; set; }

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

	[MetadataType(typeof(MemberBooking_Validation))]
	public partial class MemberBooking
	{
		partial void OnInitialized()
		{
            System.DateTime now = DateTime.UtcNow;
            FromDate = now.Subtract(new TimeSpan(now.Hour, now.Minute, now.Second)).AddHours(8).AddDays(1);
            ToDate = FromDate;
		}

		public enum Status
		{
			Unknown,
			Accepted,
			Refused
		}

		#region Booking status

		public bool Unknown
		{
			get { return StatusId == (int)Status.Unknown; }
		}

		public bool Refused
		{
			get { return StatusId == (int)Status.Refused; }
		}

		public bool Waiting
		{
			get { return StatusId == (int)Status.Accepted && Transactions.Where(t => t.StatusId == (int)Transaction.Status.Completed).Count() == 0; }
		}

		public bool Paid
		{
			get { return StatusId == (int)Status.Accepted && Transactions.Where(t => t.StatusId == (int)Transaction.Status.Completed).Count() != 0; }
		}

		public bool Expired
		{
			get { return ToDate < DateTime.UtcNow; }
		}

		public DateTime PaidDate
		{
			get { return (from item in Transactions where item.UpdatedDate.HasValue select item.UpdatedDate.Value).FirstOrDefault(); }
		}

		public DateTime CreationDate
		{
			get { return (from item in MemberBookingLogs where item.EventType == (int)MemberBookingLog.BookingEvent.Creation select item.CreatedDate).FirstOrDefault(); }
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
	public class MemberBooking_Validation
	{
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "FromDate", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
		public DateTime FromDate { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "ToDate", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
		public DateTime ToDate { get; set; }

		[Display(Name = "Message", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
		public string Message { get; set; }
	}

	public partial class MemberBookingLog
	{
		public enum BookingEvent
		{
			General,
			Creation,
			Approval,
			Refusal,
			Payment
		}

		public string GetDisplay()
		{
			var type = (BookingEvent)EventType;
			switch(type)
			{
				case BookingEvent.Creation:
					return string.Format("{0} a fait une demande de réservation pour le lieu {1}", MemberBooking.Client.GetFullDisplayName(), MemberBooking.Offer.Localisation.Name);
				case BookingEvent.Approval:
					return string.Format("{0} a accepté la demande de réservation pour le lieu {1}", MemberBooking.Owner.GetFullDisplayName(), MemberBooking.Offer.Localisation.Name);
				case BookingEvent.Refusal:
					return string.Format("{0} a refusé la demande de réservation pour le lieu {1}", MemberBooking.Owner.GetFullDisplayName(), MemberBooking.Offer.Localisation.Name);
				case BookingEvent.Payment:
					return string.Format("{0} a payé la demande de réservation pour le lieu {1}", MemberBooking.Client.GetFullDisplayName(), MemberBooking.Offer.Localisation.Name);
				case BookingEvent.General:
				default:
					return Event;
			}
		}
	}

    public class LocalisationMenuIndex
    {
        public LocalisationMenu MenuItem { get; set; }
        public int Id { get; set; }
    }

    public class LocalisationNavigation
    {
        public Localisation Localisation { get; set; }
        public Offer Offer { get; set; }
    }

    public class LocalisationBookingViewModel
    {
        public PagingList<MemberBooking> Bookings { get; set; }
        public Localisation Localisation { get; set; }
    }

    public class OfferBookingViewModel
    {
        public PagingList<MemberBooking> Bookings { get; set; }
        public Offer Offer { get; set; }
    }
}
