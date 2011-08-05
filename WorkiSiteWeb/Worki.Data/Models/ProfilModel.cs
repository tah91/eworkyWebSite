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
    #region Profil Enums

    public enum CivilityType
    {
        Mr,
        Mme,
        Mlle
    }

    public enum ProfileType
    {
        LocalisationOwner,
        Nomad,
        Teleworker,
        Independant,
        Startup,
        Company,
        Student,
        PunctualUser,
        Other,
		NotSelected
    }

	public enum EditionType
	{
		Creation,
		Edition
	}

    #endregion

    #region Member

    [MetadataType(typeof(Member_Validation))]
    public partial class Member
    {
        public const char UserDataSeparator = '|';

        public static string GenerateKey()
        {
            Guid emailKey = Guid.NewGuid();
            return emailKey.ToString();
        }

        /// <summary>
        /// Get userdata of member to put in cookies
        /// </summary>
        /// <returns>the userdata</returns>
        public string GetUserData()
        {
            var displayName = MemberMainData == null ? Username : MemberMainData.FirstName;
            return displayName + UserDataSeparator + MemberId;
        }

        /// <summary>
        /// Get name from the userdata in cookies
        /// </summary>
        /// <param name="userData">userdata to parse</param>
        /// <returns>member name</returns>
        public static string GetNameFromUserData(string userData)
        {
            if (string.IsNullOrEmpty(userData))
                return string.Empty;
            var datas = userData.Split(UserDataSeparator);
            if (datas == null || datas.Length < 2)
                return string.Empty;
            return datas[0];
        }

        /// <summary>
        /// Get Id from the userdata in cookies
        /// </summary>
        /// <param name="userData">userdata to parse</param>
        /// <returns>member Id</returns>
        public static int GetIdFromUserData(string userData)
        {
            var toRet = 0;
            if (string.IsNullOrEmpty(userData))
                return toRet;
            var datas = userData.Split(UserDataSeparator);
            if (datas == null || datas.Length < 2)
                return toRet;
            try
            {
                return int.Parse(datas[1]);
            }
            catch (Exception)
            {
                return toRet;
            }
        }

        /// <summary>
        /// Get profil display name
        /// </summary>
        /// <returns>the display name</returns>
        public string GetDisplayName()
        {
            if (MemberMainData == null)
                return Username;
            return MemberMainData.FirstName;
        }

        /// <summary>
        /// Max edition in 24 hours
        /// </summary>
        public const int MaxEditionCount = 15;

        /// <summary>
        /// Time to wait when MaxEditionCount is reached
        /// </summary>
        public static TimeSpan EditionWaitInterval = new TimeSpan(24, 0, 0);

        /// <summary>
        /// Time to wait before editing when account created
        /// </summary>
        public static TimeSpan RegisterWaitInterval = new TimeSpan(0, 0, 0);

		/// <summary>
		/// Check if the user is valid
		/// </summary>
		/// <returns>true if valid</returns>
		public bool IsValidUser()
		{
			return MemberMainData != null;
		}

        /// <summary>
        /// Check if member satisfy all edition access rules 
        /// </summary>
		/// <param name="adminRole">if user has admin role</param>
        /// <returns>string containing the reason if can't edit, empty string else</returns>
        public string HasEditionAccess(bool adminRole)
        {
			if (!IsValidUser())
				return Worki.Resources.Validation.ValidationString.InvalidUser;

			if (adminRole)
				return string.Empty;

            var now = DateTime.Now;
            if (now - CreatedDate < RegisterWaitInterval)
                return "Vous devez attendre 48h avant de pouvoir éditer un lieu";
            var todayEdition = from item in MemberEditions where (now - item.ModificationDate).Ticks < TimeSpan.TicksPerDay select item;
            if (todayEdition != null && todayEdition.Count() > MaxEditionCount)
                return string.Format("Vous ne pouvez pas éditer plus de {0} en moins de 24h", MaxEditionCount);
            return string.Empty;
        }


        public enum MemberScore
        {
            Beginner,
            Silver,
            Gold,
            Platinium
        }

        /// <summary>
        /// return the score of the member, based on THE formula...
        /// </summary>
        /// <returns>the score</returns>
        public int ComputeScore()
        {
            var toRet=0;
            //registration
            if (MemberMainData != null)
                toRet += 10;
            //completed fields
            if (!string.IsNullOrEmpty(MemberMainData.CompanyName))
                toRet += 5;
            if (!string.IsNullOrEmpty(MemberMainData.City))
                toRet += 5;
            if (!string.IsNullOrEmpty(MemberMainData.PostalCode))
                toRet += 5;
            if (!string.IsNullOrEmpty(MemberMainData.Country))
                toRet += 5;
            if (!string.IsNullOrEmpty(MemberMainData.WorkCity))
                toRet += 5;
            if (!string.IsNullOrEmpty(MemberMainData.WorkPostalCode))
                toRet += 5;
            if (!string.IsNullOrEmpty(MemberMainData.WorkCountry))
                toRet += 5;
            if (MemberMainData.BirthDate.HasValue)
                toRet += 5;
            if (!string.IsNullOrEmpty(MemberMainData.Description))
                toRet += 5;
            if (!string.IsNullOrEmpty(MemberMainData.Facebook))
                toRet += 5;
            if (!string.IsNullOrEmpty(MemberMainData.Twitter))
                toRet += 5;
            if (!string.IsNullOrEmpty(MemberMainData.Linkedin))
                toRet += 5;
            if (!string.IsNullOrEmpty(MemberMainData.Viadeo))
                toRet += 5;
            //avatar
            if (!string.IsNullOrEmpty(MemberMainData.Avatar))
                toRet += 10;
            //favoritePlaces field
            if (!string.IsNullOrEmpty(MemberMainData.FavoritePlaces))
                toRet += 10;

            //comments
            var com = (from item in Comments where string.IsNullOrEmpty(item.Post) select 1).Count();
            toRet += com;

            //rating
            var rating = (from item in Comments where item.Rating != -1 || item.RatingDispo != -1 || item.RatingWifi != -1 || item.RatingWelcome != -1 || item.RatingWifi != -1 select 1).Count();
            toRet += rating;

            //favortites
            toRet += FavoriteLocalisations.Count;

            //editions
            toRet += MemberEditions.Count * 2;

            //added localisations
            toRet += Localisations.Count * 10;

            return toRet;
        }

		public int? GetAge()
		{
			if (!MemberMainData.BirthDate.HasValue)
				return null;
			DateTime now = DateTime.Today;
			int age = now.Year - MemberMainData.BirthDate.Value.Year;
			if (MemberMainData.BirthDate > now.AddYears(-age))
				age--;
			return age;
		}

        /// <summary>
        /// Return the status of the member 
        /// </summary>
        /// <param name="score">score to get the status</param>
        /// <returns>status of the member</returns>
        public string GetMemberScoreStatus(int score)
        {
            if (score > 500)
                return MemberScore.Platinium.ToString();
            else if (score > 250)
				return MemberScore.Gold.ToString();
            else if (score > 100)
				return MemberScore.Silver.ToString();
            else
				return string.Empty;
        }
    }

    [Bind(Exclude = "MemberId,Username")]
    public class Member_Validation
    {

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
