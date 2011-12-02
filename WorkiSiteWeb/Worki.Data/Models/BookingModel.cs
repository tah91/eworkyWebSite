using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.Linq;
using System.ComponentModel;

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
	public partial class MemberBooking : IDataErrorInfo
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
			Refused,
            Cancelled
		}

		#region IDataErrorInfo

		public string Error
		{
			get { return string.Empty; }
		}

		public string this[string columnName]
		{
			get
			{
				var oneDay = new TimeSpan(1,0,0,0);
				switch (columnName)
				{
					case "FromDate":
						{
							if ((FromDate - DateTime.UtcNow).Days < 0)
							{
								return "Choisir une date de début à partir d'aujourd'hui";
							}
							else if (FromDate > ToDate)
								return "Date de début avant date de fin";
							else
								return string.Empty;
						}
					case "ToDate":
						{
							if ((ToDate - DateTime.UtcNow).Days < 0)
							{
								return "Choisir une date de fin à partir d'aujourd'hui";
							}
							else if (FromDate > ToDate)
								return "Date de début avant date de fin";
							else
								return string.Empty;
						}
					default:
						return string.Empty;
				}
			}
		}

		#endregion

		#region Booking status

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
		/// Accepted by owner but not paid yet by client
		/// </summary>
		public bool Waiting
		{
			get { return StatusId == (int)Status.Accepted && Transactions.Where(t => t.StatusId == (int)TransactionConstants.Status.Completed).Count() == 0; }
		}


		/// <summary>
		/// Accepted and paid by client
		/// </summary>
		public bool Paid
		{
			get { return StatusId == (int)Status.Accepted && Transactions.Where(t => t.StatusId == (int)TransactionConstants.Status.Completed).Count() != 0; }
		}

		/// <summary>
		/// Expired
		/// </summary>
		public bool Expired
		{
			get { return ToDate < DateTime.UtcNow; }
		}

		/// <summary>
		/// Payement date of paid booking
		/// </summary>
		public DateTime PaidDate
		{
			get { return (from item in Transactions where item.UpdatedDate.HasValue select item.UpdatedDate.Value).FirstOrDefault(); }
		}

		/// <summary>
		/// Creation date of the booking by the client
		/// </summary>
		public DateTime CreationDate
		{
			get { return (from item in MemberBookingLogs where item.EventType == (int)MemberBookingLog.BookingEvent.Creation select item.CreatedDate).FirstOrDefault(); }
		}

        /// <summary>
        /// Cancellation date of the booking by the client
        /// </summary>
        public DateTime CancellationDate
        {
            get { return (from item in MemberBookingLogs where item.EventType == (int)MemberBookingLog.BookingEvent.Cancellation select item.CreatedDate).FirstOrDefault(); }
        }

		/// <summary>
		/// Owner can accept / refuse
		/// </summary>
        public bool OwnerCanAccept
        {
            get { return !Expired && Unknown; }
        }

		/// <summary>
		/// Client can pay
		/// </summary>
        public bool ClientCanPay
        {
            get { return !Expired && Waiting; }
        }

        /// <summary>
        /// Client can cancel
        /// </summary>
        public bool ClientCanCancel
        {
            get { return !Expired && Unknown; }
        }

        public void GetStatus(out string status, out string color)
        {
            status = "";
            color = "";
            if (Expired)
            {
                status = "Achevée le " + ToDate;
                color = "Gray";
            }
            else if (Unknown)
            {
                status = "En attente de confirmation";
                color = "Yellow";
            }
            else if (Refused)
            {
                status = "Refusée";
                color = "Red";
            }
            else if (Cancelled)
            {
                status = "Annulée le " + CancellationDate;
                color = "Gray";
            }
            else if (Waiting)
            {
                status = "En attente de réglement";
                color = "Orange";
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

        [Display(Name = "Response", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public string Response { get; set; }
	}

	public partial class MemberBookingLog
	{
		public enum BookingEvent
		{
			General,
			Creation,
			Approval,
			Refusal,
			Payment,
            Cancellation
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
