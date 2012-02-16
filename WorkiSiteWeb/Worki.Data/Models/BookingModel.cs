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
	public class MemberBookingFormViewModel
	{
        public enum eHalfDay
        {
            Morning,
            Afternoon
        }

		public MemberBookingFormViewModel()
		{
			MemberBooking = new MemberBooking();
			Periods = new SelectList(Offer.GetPaymentPeriodTypes(), "Key", "Value", Offer.PaymentPeriod.Hour);
		}

        public MemberBookingFormViewModel(Member member, Offer offer)
        {
            MemberBooking = new MemberBooking();
			BookingOffer = offer;
            var membetExists = member != null;
            PhoneNumber = membetExists ? member.MemberMainData.PhoneNumber : string.Empty;
            NeedNewAccount = !membetExists;
            FirstName = membetExists ? member.MemberMainData.FirstName : string.Empty;
            LastName = membetExists ? member.MemberMainData.LastName : string.Empty;
            Email = membetExists ? member.Email : string.Empty;
            Periods = new SelectList(Offer.GetPaymentPeriodTypes(offer.GetPricePeriods()), "Key", "Value");
        }

		public MemberBooking MemberBooking { get; set; }
		public Offer BookingOffer { get; set; }
		public SelectList Periods { get; set; }
        public eHalfDay HalfDay { get; set; }
		public bool NeedNewAccount { get; set; }

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

        /// <summary>
        /// Get the start / end date given form parameters
        /// </summary>
        public void AjustBookingPeriod()
        {
            switch ((Offer.PaymentPeriod)MemberBooking.TimeType)
            {
                case Offer.PaymentPeriod.Hour:
                    MemberBooking.ToDate = MemberBooking.FromDate.AddHours(MemberBooking.TimeUnits);
                    break;
                case Offer.PaymentPeriod.HalfDay:
                    {
                        switch (HalfDay)
                        {
                            case eHalfDay.Morning:
                                //add half days
                                MemberBooking.FromDate = new DateTime(MemberBooking.FromDate.Year, MemberBooking.FromDate.Month, MemberBooking.FromDate.Day);
                                MemberBooking.ToDate = MemberBooking.FromDate.AddDays(MemberBooking.TimeUnits * 0.5);
                                //start at 8am
                                MemberBooking.FromDate = MemberBooking.FromDate.AddHours(8);
                                break;
                            case eHalfDay.Afternoon:
                                //start from 12am and add half days
                                MemberBooking.FromDate = new DateTime(MemberBooking.FromDate.Year, MemberBooking.FromDate.Month, MemberBooking.FromDate.Day).AddDays(0.5);
                                MemberBooking.ToDate = MemberBooking.FromDate.AddDays(MemberBooking.TimeUnits * 0.5);
                                //start at 2pm
                                MemberBooking.FromDate = MemberBooking.FromDate.AddHours(2);
                                break;
                            default:
                                break;
                        }
                        //end at 12am or 6pm
                        MemberBooking.ToDate = MemberBooking.ToDate.Hour == 0 ? MemberBooking.ToDate.AddHours(-6) : MemberBooking.ToDate;
                    }
                    break;
                case Offer.PaymentPeriod.Day:
                    {
						switch ((MemberBooking.ePeriodType)MemberBooking.PeriodType)
                        {
							case MemberBooking.ePeriodType.SpendUnit:
                                MemberBooking.ToDate = MemberBooking.FromDate.AddDays(MemberBooking.TimeUnits);
                                break;
							case MemberBooking.ePeriodType.EndDate:
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case Offer.PaymentPeriod.Week:
                    MemberBooking.ToDate = MemberBooking.FromDate.AddDays(MemberBooking.TimeUnits * 7);
                    break;
                case Offer.PaymentPeriod.Month:
                    MemberBooking.ToDate = MemberBooking.FromDate.AddDays(MemberBooking.TimeUnits * 30);
                    break;
                case Offer.PaymentPeriod.Year:
                    MemberBooking.ToDate = MemberBooking.FromDate.AddDays(MemberBooking.TimeUnits * 365);
                    break;
                default:
                    break;
            }
        }
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

		public enum ePeriodType
		{
			SpendUnit,
			EndDate
		}

		public enum Status
		{
			Unknown,
			Accepted,
			Refused,
            Cancelled,
			Paid
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
				var oneDay = new TimeSpan(1, 0, 0, 0);
				switch (columnName)
				{
					case "FromDate":
						{
							if ((FromDate - DateTime.UtcNow).Days < -1)
							{
								return Worki.Resources.Views.Booking.BookingString.BookingBeforeToday;
							}
							else if (PeriodType == (int)ePeriodType.EndDate && FromDate >= ToDate)
								return Worki.Resources.Views.Booking.BookingString.EndBookingBeforeStart;
							else
								return string.Empty;
						}
					case "ToDate":
						{
							if (PeriodType == (int)ePeriodType.EndDate && (ToDate - DateTime.UtcNow).Days < -1)
							{
								return Worki.Resources.Views.Booking.BookingString.BookingBeforeToday;
							}
							else
								return string.Empty;
						}
					case "TimeUnits":
						{
							if (PeriodType == (int)ePeriodType.SpendUnit && TimeUnits <= 0)
							{
								return Worki.Resources.Views.Booking.BookingString.NotNullQuantity;
							}
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

		public string BookingPeriod
		{
			get { return string.Format("du {0} au {1}", CultureHelpers.GetSpecificFormat(FromDate, CultureHelpers.TimeFormat.General), CultureHelpers.GetSpecificFormat(ToDate, CultureHelpers.TimeFormat.General)); }
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
		/// Accepted by owner but not paid yet by client
		/// </summary>
		public bool Waiting
		{
			get { return StatusId == (int)Status.Accepted; }
		}


		/// <summary>
		/// Accepted and paid by client
		/// </summary>
		public bool Paid
		{
			get { return StatusId == (int)Status.Paid; }
		}

		/// <summary>
		/// Expired
		/// </summary>
		public bool Expired
		{
            get { return (FromDate - DateTime.UtcNow).Days < -1; }
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
		/// Refusal date of the booking by the owner
		/// </summary>
		public DateTime RefusalDate
		{
			get { return (from item in MemberBookingLogs where item.EventType == (int)MemberBookingLog.BookingEvent.Refusal select item.CreatedDate).FirstOrDefault(); }
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
			get { return !Expired && !Cancelled && !Paid && !Refused; }
        }

        public void GetStatus(out string status, out string color, out DateTime? date)
        {
            status = "";
            color = "";
			date = null;
            if (Expired)
            {
                status = Worki.Resources.Views.Booking.BookingString.Achieved;
				date = ToDate;
                color = "Gray";
            }
            else if (Unknown)
            {
                status = Worki.Resources.Views.Booking.BookingString.WaitingConfirm;
                color = "Yellow";
            }
            else if (Refused)
            {
                status = Worki.Resources.Views.Booking.BookingString.RefuseStatus;
				date = RefusalDate;
                color = "Red";
            }
            else if (Cancelled)
            {
                status = Worki.Resources.Views.Booking.BookingString.CancelStatus;
				date = CancellationDate;
                color = "Gray";
            }
            else if (Waiting)
            {
                status = Worki.Resources.Views.Booking.BookingString.WaitingPayment;
                color = "Orange";
            }
            else if (Paid)
            {
                status = Worki.Resources.Views.Booking.BookingString.PaidStatus;
				date = PaidDate;
                color = "Green";
            }
        }

		public string GetPriceDisplay()
		{
			return Price + " €";
		}

		#endregion

        #region Booking Modification

        public bool CanModify(DateTime newDate)
        {
            return Unknown && (newDate > DateTime.UtcNow);
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

		#region Email Strings

		public string GetStartDate()
		{
			var format = FromDate.Hour == 0 ? CultureHelpers.TimeFormat.Date : CultureHelpers.TimeFormat.General;
			return CultureHelpers.GetSpecificFormat(FromDate, format);
		}

		public string GetEndDate()
		{
			var format = ToDate.Hour == 0 ? CultureHelpers.TimeFormat.Date : CultureHelpers.TimeFormat.General;
			var toRet = CultureHelpers.GetSpecificFormat(ToDate, format);
			if (PeriodType == (int)ePeriodType.SpendUnit && TimeUnits != 0)
				toRet += string.Format(" ({0} {1})", TimeUnits, Offer.GetPricingPeriod((Offer.PaymentPeriod)TimeType));

			return toRet;
		}

		#endregion
	}

	public class CreateBookingModel
	{
		public MemberBooking Booking { get; set; }
		public SelectList Clients { get; set; }
	}

	[Bind(Exclude = "Id")]
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

        [Display(Name = "Price", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public decimal Price { get; set; }

        [Display(Name = "TimeUnits", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public int TimeUnits { get; set; }

        [Display(Name = "TimeType", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
        public int TimeType { get; set; }
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
                    return string.Format(Worki.Resources.Views.Booking.BookingString.HasAskBooking, MemberBooking.Client.GetAnonymousDisplayName(), MemberBooking.Offer.Localisation.Name);
				case BookingEvent.Approval:
                    return string.Format(Worki.Resources.Views.Booking.BookingString.HasAcceptBooking, MemberBooking.Owner.GetAnonymousDisplayName(), MemberBooking.Offer.Localisation.Name);
				case BookingEvent.Refusal:
                    return string.Format(Worki.Resources.Views.Booking.BookingString.HasRefuseBooking, MemberBooking.Owner.GetAnonymousDisplayName(), MemberBooking.Offer.Localisation.Name);
				case BookingEvent.Payment:
                    return string.Format(Worki.Resources.Views.Booking.BookingString.HasPaidBooking, MemberBooking.Client.GetAnonymousDisplayName(), MemberBooking.Offer.Localisation.Name);
                case BookingEvent.Cancellation:
                    return string.Format(Worki.Resources.Views.Booking.BookingString.HasCancelBooking, MemberBooking.Client.GetAnonymousDisplayName(), MemberBooking.Offer.Localisation.Name);
				case BookingEvent.General:
				default:
					return Event;
			}
		}
	}

    public class LocalisationMenuIndex
    {
        public LocalisationMenu MenuItem { get; set; }
		public string Title { get; set; }
        public int Id { get; set; }
    }

    public class LocalisationNavigation
    {
        public Localisation Localisation { get; set; }
        public Offer Offer { get; set; }
    }

    public class LocalisationBookingViewModel : MasterViewModel<MemberBooking, Localisation>
    {
    }

    public class OfferBookingViewModel : MasterViewModel<MemberBooking, Offer>
    {
    }

	public class InvoiceListViewModel
	{
		public MonthYearList<MemberBooking> Bookings { get; set; }
		public Localisation Localisation { get; set; }
	}

	public class InvoiceItem
	{
		public InvoiceItem()
		{
			Price = 0;
			Quantity = 1;
		}

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Description { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public decimal Price { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public int Quantity { get; set; }
	}

	public class InvoiceFormViewModel
	{
		public InvoiceFormViewModel()
		{
			Items = new List<InvoiceItem>();
		}

		public InvoiceFormViewModel(MemberBooking booking)
		{
			ClientId = booking.Client.MemberId;
			Localisation = booking.Offer.Localisation;
			Items = new List<InvoiceItem> { new InvoiceItem { Description = booking.Offer.Name, Quantity = 1, Price = booking.Price } };
			InvoiceId = booking.Id;
		}

		public InvoiceFormViewModel(Localisation localisation, IEnumerable<InvoiceItem> items = null)
		{
			Localisation = localisation;
			var clients = Localisation.Member.MemberClients.ToDictionary(mc => mc.ClientId, mc => mc.Client.GetFullDisplayName());
			Clients = new SelectList(clients, "Key", "Value");
			Items = items ?? new List<InvoiceItem>();
		}

		public SelectList Clients { get; set; }
		public int ClientId { get; set; }
		public IEnumerable<InvoiceItem> Items { get; set; }
		public Localisation Localisation { get; set; }
		public int InvoiceId { get; set; }
	}
}
