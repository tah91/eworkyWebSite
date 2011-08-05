using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Worki.Data.Models
{
	public class MemberBookingFormViewModel
	{
		public MemberBookingFormViewModel()
		{
			var offers = Localisation.GetOfferTypeDict(new List<LocalisationOffer> { LocalisationOffer.FreeArea });
			Offers = new SelectList(offers, "Key", "Value", LocalisationOffer.BuisnessRoom);
			MemberBooking = new MemberBooking();
		}

		public MemberBooking MemberBooking { get; set; }
		public SelectList Offers { get; set; }
		public string ReturnUrl { get; set; }

		[Display(Name = "PhoneNumber", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
		public string PhoneNumber { get; set; }
	}

	[MetadataType(typeof(MemberBooking_Validation))]
	public partial class MemberBooking
	{
		public MemberBooking()
		{
			FromDate = DateTime.Now;
			ToDate = DateTime.Now;
		}
	}

	[Bind(Exclude = "Id,MemberId,LocalisationId")]
	public class MemberBooking_Validation
	{
		[Display(Name = "Offer", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
		public int Offer { get; set; }

		[Display(Name = "FromDate", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
		public DateTime FromDate { get; set; }

		[Display(Name = "ToDate", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
		public DateTime ToDate { get; set; }

		[Display(Name = "Message", ResourceType = typeof(Worki.Resources.Models.Booking.Booking))]
		public string Message { get; set; }
	}
}
