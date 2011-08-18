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

    #endregion

    #region Member

    [MetadataType(typeof(Rental_Validation))]
    public partial class Rental
    {
        
    }

    [Bind(Exclude = "Id,MemberId")]
    public class Rental_Validation
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Name", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "FirstName", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string FirstName { get; set; }

        [Display(Name = "CompanyName", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string CompanyName { get; set; }

        [Display(Name = "City", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string City { get; set; }

        [Display(Name = "PostalCode", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(10, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string PostalCode { get; set; }

        [Display(Name = "Country", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Country { get; set; }

        [Display(Name = "WorkCity", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string WorkCity { get; set; }

        [Display(Name = "WorkPostalCode", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(10, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string WorkPostalCode { get; set; }

        [Display(Name = "WorkCountry", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string WorkCountry { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "ProfileString", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        public int Profile { get; set; }

        [Display(Name = "BirthDate", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "PhoneNumber", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(50, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string PhoneNumber { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Description { get; set; }

        [Display(Name = "FavoritePlaces", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string FavoritePlaces { get; set; }

        [Display(Name = "Facebook", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Facebook { get; set; }

        [Display(Name = "Twitter", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Twitter { get; set; }

        [Display(Name = "Linkedin", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Linkedin { get; set; }

        [Display(Name = "Viadeo", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Viadeo { get; set; }
    }

    #endregion

    #region MemberMainData

    [MetadataType(typeof(MemberMainData_Validation))]
    public partial class MemberMainData : IDataErrorInfo
    {
		public MemberMainData()
        {
			Profile = (int)ProfileType.NotSelected;
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
				switch (columnName)
				{
					case "Profile":
						{
							if (Profile == (int)ProfileType.NotSelected)
							{
								return Worki.Resources.Validation.ValidationString.PleaseSelectProfile;
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

        public static Dictionary<int, string> CivilityTypes = new Dictionary<int, string>()
        {
            { (int)CivilityType.Mr,Worki.Resources.Models.Localisation.Localisation.Mr},
            { (int)CivilityType.Mme,Worki.Resources.Models.Localisation.Localisation.Mme},
            { (int)CivilityType.Mlle,Worki.Resources.Models.Localisation.Localisation. Mlle},
        };

        public static Dictionary<int, string> ProfileTypes = new Dictionary<int, string>()
        {
            { (int)ProfileType.LocalisationOwner,Worki.Resources.Models.Profile.Profile.LocalisationOwner},
            { (int)ProfileType.Nomad ,Worki.Resources.Models.Profile.Profile.Nomad},            
            { (int)ProfileType.Teleworker,Worki.Resources.Models.Profile.Profile.Teleworker},
            { (int)ProfileType.Independant,Worki.Resources.Models.Profile.Profile.Independant},
            { (int)ProfileType.Startup,Worki.Resources.Models.Profile.Profile.Startup },            
            { (int)ProfileType.Company,Worki.Resources.Models.Profile.Profile.Company },
            { (int)ProfileType.Student,Worki.Resources.Models.Profile.Profile.Student},
            { (int)ProfileType.PunctualUser,Worki.Resources.Models.Profile.Profile.PunctualUser},
            { (int)ProfileType.Other,Worki.Resources.Models.Profile.Profile.Other },
			{ (int)ProfileType.NotSelected,Worki.Resources.Models.Profile.Profile.NotSelected }
        };
    }

    [Bind(Exclude = "RelationId,MemberId,Avatar")]
    public class MemberMainData_Validation
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Civility", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        public int Civility { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "LastName", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "FirstName", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string FirstName { get; set; }

        [Display(Name = "CompanyName", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string CompanyName { get; set; }

        [Display(Name = "City", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string City { get; set; }

        [Display(Name = "PostalCode", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(10, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string PostalCode { get; set; }

        [Display(Name = "Country", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Country { get; set; }

        [Display(Name = "WorkCity", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string WorkCity { get; set; }

        [Display(Name = "WorkPostalCode", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(10, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string WorkPostalCode { get; set; }

        [Display(Name = "WorkCountry", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string WorkCountry { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "ProfileString", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        public int Profile { get; set; }

        [Display(Name = "BirthDate", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "PhoneNumber", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(50, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string PhoneNumber { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Description { get; set; }

        [Display(Name = "FavoritePlaces", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string FavoritePlaces { get; set; }

        [Display(Name = "Facebook", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Facebook { get; set; }

        [Display(Name = "Twitter", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Twitter { get; set; }

        [Display(Name = "Linkedin", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Linkedin { get; set; }

        [Display(Name = "Viadeo", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Viadeo { get; set; }
    }

    #endregion

    #region ProfilFormViewModel

    public class ProfilFormViewModel
    {
        #region Properties

        public Member Member { get; set; }
        public SelectList CivilitySelectTypes { get; private set; }
        public SelectList ProfileSelectTypes { get; private set; }

        #endregion

        #region Ctor

        public ProfilFormViewModel()
        {
            CivilitySelectTypes = new SelectList(MemberMainData.CivilityTypes, "Key", "Value", CivilityType.Mr);
            ProfileSelectTypes = new SelectList(MemberMainData.ProfileTypes, "Key", "Value", ProfileType.LocalisationOwner);
            Member = new Member();
        }

        #endregion
    }

    #endregion

    #region LocalisationSummary

    public class LocalisationSummary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }

    #endregion

    #region ProfilDashboardModel

    public class ProfilDashboardModel
    {
		public const string TabId = "TabId";

		public enum DashboardTab
		{
			FavLoc,
			AddedLoc,
			PostedCom,
			RelCom
		}

		public const string AddToFavorite = "AddToFavorite";
		public const string DelFavorite = "DelFavorite";

        #region Properties

        public Member Member { get; set; }
        public List<Localisation> FavoriteLocalisations { get; set; }
        public List<Localisation> AddedLocalisations { get; set; }
        public List<Comment> PostedComments { get; set; }
        public List<Comment> RelatedComments { get; set; }
        public PagingInfo FavoriteLocalisationsPI { get; set; }
        public PagingInfo AddedLocalisationsPI { get; set; }
        public PagingInfo PostedCommentsPI { get; set; }
        public PagingInfo RelatedCommentsPI { get; set; }
		public bool IsPrivate { get; set; }

        #endregion

        #region Ctor

        public ProfilDashboardModel()
        {

        }

        #endregion
    }

    #endregion
}
