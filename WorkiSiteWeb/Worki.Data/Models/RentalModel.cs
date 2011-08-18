using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using System.Linq;
using Worki.Infrastructure.Helpers;

namespace Worki.Data.Models
{
    #region Rental Enums

    public enum RentalType
    {
        Desk,
        Leasehold,
        Commercial,
        Farm,
        Ground,
        Franchise
    }

    public enum LeaseType
    {
        Type_24,
        Type_369
    }

	public enum DiagnosticRate
	{
		A,
		B,
        C,
        D,
        E,
        F,
        G
	}

    public enum HeatingType
    {
        Gas,
        Electric
    }

    public enum AccessType
    {
        Metro,
        Train,
        Tram,
        Bus
    }

	public enum RentalFeature
	{
		StartUpFriendly,
		Kitchen,
		Toilets,
		New,
		MeetingRoom,
		LocalNetwork,
		Parking,
		Quiet
	}

    #endregion

    #region Rental

    [MetadataType(typeof(Rental_Validation))]
    public partial class Rental
    {
        
    }

    [Bind(Exclude = "Id,MemberId")]
    public class Rental_Validation
    {
		[Display(Name = "Reference", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Reference { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Type", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public int Type { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Address", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Address { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "City", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string City { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "PostalCode", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
        [StringLength(10, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string PostalCode { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Country", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Country { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Description", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public string Description { get; set; }

		[Display(Name = "AvailableDate", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public Nullable<DateTime> AvailableDate { get; set; }

		[Display(Name = "AvailableNow", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public bool AvailableNow { get; set; }

		[Display(Name = "LeaseType", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public int LeaseType { get; set; }

		[Display(Name = "Rate", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public int Rate { get; set; }

		[Display(Name = "Charges", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public int Charges { get; set; }

		[Display(Name = "Surface", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public int Surface { get; set; }

		[Display(Name = "Energy", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public int Energy { get; set; }

		[Display(Name = "GreenHouse", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public int GreenHouse { get; set; }

		[Display(Name = "HeatingType", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public int HeatingType { get; set; }

		[Display(Name = "TimeStamp", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public DateTime TimeStamp { get; set; }
    }

    #endregion
}
