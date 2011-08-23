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

	/// <summary>
	/// Describe all type of rentals
	/// correspond to field Type
	/// </summary>
    public enum RentalType
    {
		NotDefined = -1,
        Desk,
        Leasehold,
        Commercial,
        Farm,
        Ground,
        Franchise
    }

	/// <summary>
	/// Describe all type of lease
	/// correspond to field LeaseType
	/// </summary>
    public enum Lease
    {
		NotDefined = -1,
        Type_24,
        Type_369
    }

	/// <summary>
	/// Diagnostic rate letters
	/// Correspond to field Energy or GreenHouse
	/// </summary>
	public enum DiagnosticRate
	{
		NotDefined = -1,
		A,
		B,
        C,
        D,
        E,
        F,
        G
	}


	/// <summary>
	/// Correspond to field HeatingType
	/// </summary>
    public enum Heating
    {
		NotDefined = -1,
        Gas,
        Electric
    }

	/// <summary>
	/// Correspond to RentalAccess Type field
	/// </summary>
    public enum Access
    {
		NotDefined = -1,
        Metro,
        Train,
        Tram,
        Bus
    }

	/// <summary>
	/// Describe rental features
	/// correspond to RentalFeature FeatureId
	/// </summary>
	[LocalizedEnum(ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
	public enum RentalFeatureType
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

	#region Equality Comparer

	public class RentalFeatureEqualityComparer : IEqualityComparer<RentalFeature>
	{
		#region IEqualityComparer<RentalFeature> Members

		public bool Equals(RentalFeature x, RentalFeature y)
		{
			return x.FeatureId == y.FeatureId;
		}

		public int GetHashCode(RentalFeature obj)
		{
			return base.GetHashCode();
		}

		#endregion
	}

	#endregion

    [MetadataType(typeof(Rental_Validation))]
    public partial class Rental : IPictureDataProvider
    {
		#region Static Fields

		public static Dictionary<int, string> RentalTypes = new Dictionary<int, string>()
        {
			{ (int)RentalType.NotDefined,Worki.Resources.Models.Rental.Rental.NotDefined},
            { (int)RentalType.Desk,Worki.Resources.Models.Rental.Rental.Desk},
			{ (int)RentalType.Leasehold,Worki.Resources.Models.Rental.Rental.Leasehold},
			{ (int)RentalType.Commercial,Worki.Resources.Models.Rental.Rental.Commercial},
			{ (int)RentalType.Farm,Worki.Resources.Models.Rental.Rental.Farm},
			{ (int)RentalType.Ground,Worki.Resources.Models.Rental.Rental.Ground},
			{ (int)RentalType.Franchise,Worki.Resources.Models.Rental.Rental.Franchise}			
        };

		public static Dictionary<int, string> LeaseTypes = new Dictionary<int, string>()
        {
			{ (int)Lease.NotDefined,Worki.Resources.Models.Rental.Rental.NotDefined},
            { (int)Lease.Type_24,Worki.Resources.Models.Rental.Rental.Type_24},
			{ (int)Lease.Type_369,Worki.Resources.Models.Rental.Rental.Type_369}
        };

		public static Dictionary<int, string> DiagnosticRates = new Dictionary<int, string>()
        {
			{ (int)DiagnosticRate.NotDefined,Worki.Resources.Models.Rental.Rental.NotDefined},
            { (int)DiagnosticRate.A,DiagnosticRate.A.ToString()},
			{ (int)DiagnosticRate.B,DiagnosticRate.B.ToString()},
			{ (int)DiagnosticRate.C,DiagnosticRate.C.ToString()},
			{ (int)DiagnosticRate.D,DiagnosticRate.D.ToString()},
			{ (int)DiagnosticRate.E,DiagnosticRate.E.ToString()},
			{ (int)DiagnosticRate.F,DiagnosticRate.F.ToString()},
			{ (int)DiagnosticRate.G,DiagnosticRate.G.ToString()}
        };

		public static Dictionary<int, string> HeatingTypes = new Dictionary<int, string>()
        {
			{ (int)RentalType.NotDefined,Worki.Resources.Models.Rental.Rental.NotDefined},
            { (int)Heating.Electric,Worki.Resources.Models.Rental.Rental.Electric},
			{ (int)Heating.Gas,Worki.Resources.Models.Rental.Rental.Gas}
        };

		public static Dictionary<int, string> AccessTypes = new Dictionary<int, string>()
        {
			{ (int)Access.NotDefined,Worki.Resources.Models.Rental.Rental.NotDefined},
            { (int)Access.Metro,Access.Metro.ToString()},
			{ (int)Access.Train,Access.Train.ToString()},
			{ (int)Access.Tram,Access.Tram.ToString()},
			{ (int)Access.Bus,Access.Bus.ToString()}
        };

		public static Dictionary<int, string> RentalFeatureDict = MiscHelpers.GetEnumDescriptors(typeof(RentalFeatureType));

		#endregion

        partial void OnInitialized()
        {
            Type = MiscHelpers.UnselectedItem;
            LeaseType = MiscHelpers.UnselectedItem;
            HeatingType = MiscHelpers.UnselectedItem;
            TimeStamp = DateTime.Now;
        }

        public string SurfaceString
        {
            get
            {
                return Surface.ToString() + " m²";
            }
        }

        public string RateString
        {
            get
            {
                return (Rate+Charges).ToString() + " € cc";
            }
        }

        #region IPictureDataProvider

        public List<PictureData> GetPictureData()
        {
            if (RentalFiles != null)
                return (from item in RentalFiles select new PictureData { FileName = item.FileName, IsDefault = item.IsDefault }).ToList();
            return new List<PictureData>();
        }

        public string GetMainPic()
        {
            var main = (from item in RentalFiles where item.IsDefault orderby item.Id select item.FileName).FirstOrDefault();
            return main;
        }

        public string GetPic(int index)
        {
            var list = (from item in RentalFiles where !item.IsDefault orderby item.Id select item.FileName).ToList();
            var count = list.Count();
            if (count == 0 || index < 0 || index >= count)
                return string.Empty;
            return list[index];
        }

        public string GetLogoPic()
        {
            throw new NotImplementedException();
        }

        public string GetDisplayName()
        {
            return RentalTypes[Type] + " - " + City;
        }

        public string GetDescription()
        {
            return Description;
        }

        #endregion

		#region RentalFeatures

		public bool HasFeature(RentalFeatureType feature)
		{
			return RentalFeatures.Contains(new RentalFeature { FeatureId = (int)feature }, new RentalFeatureEqualityComparer());
		}

        public List<RentalFeatureType> GetFeatures()
        {
            var toRet = new List<RentalFeatureType>();
            return (from item in RentalFeatures select (RentalFeatureType)item.FeatureId).ToList();
        }

        public string AvailableString
        {
            get
            {
                if (AvailableNow)
                    return string.Format("{0} : {1}", Worki.Resources.Models.Rental.Rental.Availability, Worki.Resources.Models.Rental.Rental.AvailableNow);
                else if (AvailableDate.HasValue)
                    return string.Format("{0} : {1:dd/MM/yyyy}", Worki.Resources.Models.Rental.Rental.Availability, AvailableDate.Value);
                else
                    return string.Empty;
            }
        }

        public string EnergyString
        {
            get
            {
                if (Energy != MiscHelpers.UnselectedItem)
                    return Worki.Resources.Models.Rental.Rental.Energy + " : " + ((DiagnosticRate)Energy).ToString();
                else
                    return string.Empty;
            }
        }

        public string GreenHouseString
        {
            get
            {
                if (GreenHouse != MiscHelpers.UnselectedItem)
                    return Worki.Resources.Models.Rental.Rental.GreenHouse + " : " + ((DiagnosticRate)GreenHouse).ToString();
                else
                    return string.Empty;
            }
        }

		#endregion

        #region IMapModelProvider

        public MapModel GetMapModel()
        {
            return new MapModel
            {
                Latitude = Latitude,
                Longitude = Longitude,
                Name = GetDisplayName()
            };
        }

        #endregion
	}

    [Bind(Exclude = "Id,MemberId")]
    public class Rental_Validation
    {
        const int MinRange = 1;
        const int MaxRange = 10000;

		[Display(Name = "Reference", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Reference { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [SelectValidation(ErrorMessageResourceName = "SelectOne", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
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
        [SelectValidation(ErrorMessageResourceName = "SelectOne", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public int LeaseType { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Range(MinRange, MaxRange, ErrorMessageResourceName = "Range", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Rate", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public int Rate { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Range(MinRange, MaxRange, ErrorMessageResourceName = "Range", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Charges", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public int Charges { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Range(MinRange, MaxRange, ErrorMessageResourceName = "Range", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
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

	#region RentalFormViewModel

	public class RentalFormViewModel
	{
		#region Properties

		public Rental Rental { get; set; }
		public SelectList RentalTypeSelect { get; private set; }
		public SelectList LeaseTypeSelect { get; private set; }
		public SelectList DiagnosticRateSelect { get; private set; }
		public SelectList HeatingTypeSelect { get; private set; }
		public SelectList AccessTypeSelect { get; private set; }

		#endregion

		#region Ctor

		public RentalFormViewModel()
		{
			Init();
			Rental = new Rental();
		}

		public RentalFormViewModel(Rental rental)
		{
			Init();
			Rental = rental;
		}

		void Init()
		{
			RentalTypeSelect = new SelectList(Rental.RentalTypes, "Key", "Value", RentalType.NotDefined);
			LeaseTypeSelect = new SelectList(Rental.LeaseTypes, "Key", "Value", Lease.NotDefined);
			DiagnosticRateSelect = new SelectList(Rental.DiagnosticRates, "Key", "Value", DiagnosticRate.NotDefined);
			HeatingTypeSelect = new SelectList(Rental.HeatingTypes, "Key", "Value", Heating.NotDefined);
			AccessTypeSelect = new SelectList(Rental.AccessTypes, "Key", "Value", Access.NotDefined);
		}

		#endregion
	}

	#endregion

    #region RentalAccess

    [MetadataType(typeof(RentalAccess_Validation))]
    public partial class RentalAccess
    {
        public RentalAccess()
        {
            Type = MiscHelpers.UnselectedItem;
        }

        public string DisplayName
        {
            get
            {
                return string.Format("{0} ({1} {2})", Station, ((Access)Type).ToString(), Line);
            }
        }
	}

    [Bind(Exclude = "Id,RentalId")]
    public class RentalAccess_Validation
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [SelectValidation(ErrorMessageResourceName = "SelectOne", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Type", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
		public int Type { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Line", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Line { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Station", ResourceType = typeof(Worki.Resources.Models.Rental.Rental))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Station { get; set; }
	}

    #endregion
}
