using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;

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
}
