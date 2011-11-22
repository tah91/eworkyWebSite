﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;

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
		public MemberBooking()
		{
            System.DateTime now = DateTime.Now;
            FromDate = now.Subtract(new TimeSpan(now.Hour, now.Minute, now.Second)).AddHours(8).AddDays(1);
            ToDate = FromDate;
		}
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
