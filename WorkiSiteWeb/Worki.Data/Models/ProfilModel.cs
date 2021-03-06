﻿using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Worki.Infrastructure;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using System.Linq;
using Worki.Infrastructure.Helpers;
using System.Text.RegularExpressions;
using System.Globalization;

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
    public partial class Member : IJsonProvider<MemberJson>// : IDataErrorInfo
    {
        public const char UserDataSeparator = '|';

        public const string AvatarFolder = "avatar";

        #region IJsonProvider

        public MemberJson GetJson()
        {
            return new MemberJson
            {
                id = MemberId,
                firstName = MemberMainData.FirstName,
                lastName = MemberMainData.LastName,
                companyName = MemberMainData.CompanyName,
                city = MemberMainData.City,
                profile = MemberMainData.GetProfileType(MemberMainData.Profile),
                description = MemberMainData.Description,
                twitter = MemberMainData.Twitter,
                facebook = MemberMainData.Facebook,
                linkedin = MemberMainData.Linkedin,
                viadeo = MemberMainData.Viadeo
            };
        }

        public AuthJson GetAuthJson()
        {
            return new AuthJson
            {
                token = MemberMainData.Token,
                email = Email,
                firstName = MemberMainData.FirstName,
                lastName = MemberMainData.LastName,
                birthDate = MemberMainData.BirthDate.ToApiDate(),
                phoneNumber = MemberMainData.PhoneNumber,
                profile = MemberMainData.Profile,
                civility = MemberMainData.Civility,
                description = MemberMainData.Description,
                avatar = MemberMainData.Avatar,
                city = MemberMainData.City,
                postalCode = MemberMainData.PostalCode
            };
        }

        #endregion

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
        /// Get full display name
        /// </summary>
        /// <returns>the full display name</returns>
        public string GetFullDisplayName()
        {
            if (MemberMainData == null)
                return Username;
            return string.Format("{0} {1}", MemberMainData.FirstName, MemberMainData.LastName);
        }

		/// <summary>
		/// Get full display name
		/// </summary>
		/// <returns>the full display name</returns>
		public string GetAnonymousDisplayName()
		{
			if (MemberMainData == null)
				return Username;
			return string.Format("{0} {1}.", MemberMainData.FirstName, MemberMainData.LastName.Substring(0, 1));
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
        /// Check if member satisfy all edition access rules 
        /// </summary>
		/// <param name="adminRole">if user has admin role</param>
        /// <returns>string containing the reason if can't edit, empty string else</returns>
        public string HasEditionAccess(bool adminRole)
        {
			if (adminRole)
				return string.Empty;

            var now = DateTime.UtcNow;
            if (now - CreatedDate < RegisterWaitInterval)
                return Worki.Resources.Views.Profile.ProfileString.Wait2Days;
            var todayEdition = from item in MemberEditions where (now - item.ModificationDate).Ticks < TimeSpan.TicksPerDay select item;
            if (todayEdition != null && todayEdition.Count() > MaxEditionCount)
                return string.Format(Worki.Resources.Views.Profile.ProfileString.Edit24Hours, MaxEditionCount);
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
			var com = Comments.Count(c => c.HasPost());
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

        public class ValidationException : Exception
        {
            public ValidationException(string message) :
                base(message)
            {

            }
        }

        /// <summary>
        /// Tell if a member is valid (for login)
        /// </summary>
        /// <param name="member">member to validate</param>
        /// <returns>true if valid, throw exeption if not</returns>
        public static bool Validate(Member member)
        {
            //member don't exist
            if (member == null)
                throw new ValidationException(Worki.Resources.Validation.ValidationString.MailDoNotMatch);

			if (member.MemberMainData == null)
				throw new ValidationException(Worki.Resources.Validation.ValidationString.InvalidUser);

            // We found a user by the username

            // Not valid if not approved
            if (!member.IsApproved)
            {
                throw new ValidationException(Worki.Resources.Validation.ValidationString.AccountInactive);
            }
            //or locked out
            else if (member.IsLockedOut)
            {
                throw new ValidationException(Worki.Resources.Validation.ValidationString.AccountLocked);
            }

            return true;
        }

		/// <summary>
		/// Tell if a member is the valid owner
		/// </summary>
		/// <param name="member">member to validate</param>
		/// <param name="localisation">localisation to validate</param>
		/// <returns>true if valid, throw exeption if not</returns>
		public static bool ValidateOwner(Member member, Localisation localisation)
		{

			if (localisation == null || member == null)
				throw new Exception(Worki.Resources.Validation.ValidationString.UnknowError);

			if (localisation.OwnerID != member.MemberId)
				throw new Exception(Worki.Resources.Validation.ValidationString.InvalidUser);

			return true;
        }

		//#region Owner

		///// <summary>
		///// tell if the owner has the specified client
		///// </summary>
		///// <param name="clientId">id of the client</param>
		///// <returns>true if is already a client</returns>
		//public bool HasClient(int clientId)
		//{
		//    return MemberClients.Where(mc => mc.ClientId == clientId).Count() > 0;
		//}

		//#endregion
    }

	public class NewsItem
	{
		public DateTime Date { get; set; }
		public string DisplayName { get; set; }
		public string Link { get; set; }
		public bool Read { get; set; }
	}

    [Bind(Exclude = "MemberId,Username")]
    public class Member_Validation
    {
        [Email(ErrorMessageResourceName = "PatternEmail", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		[Display(Name = "Email", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string Email { get; set; }
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

        public static List<int> CivilityTypes = new List<int>()
        {
            (int)CivilityType.Mr,
            (int)CivilityType.Mme,
            (int)CivilityType.Mlle
        };

        public static string GetCivilityType(int type)
        {
            var enumType = (CivilityType)type;
            switch (enumType)
            {
                case CivilityType.Mr:
                    return Worki.Resources.Models.Localisation.Localisation.Mr;
                case CivilityType.Mlle:
                    return Worki.Resources.Models.Localisation.Localisation.Mlle;
                case CivilityType.Mme:
                    return Worki.Resources.Models.Localisation.Localisation.Mme;
                default:
                    return string.Empty;
            }
        }

        public static Dictionary<int, string> GetCivilityTypes()
        {
            return CivilityTypes.ToDictionary(t => t, t => GetCivilityType(t));
        }

        public static List<int> ProfileTypes = new List<int>()
        {
            (int)ProfileType.NotSelected,
            (int)ProfileType.LocalisationOwner,
            (int)ProfileType.Nomad,
            (int)ProfileType.Teleworker,
            (int)ProfileType.Independant,
			(int)ProfileType.Startup,
			(int)ProfileType.Company,
            (int)ProfileType.Student,
            (int)ProfileType.PunctualUser,
            (int)ProfileType.Other
        };

        public static string GetProfileType(int type)
        {
            var enumType = (ProfileType)type;
            switch (enumType)
            {
                case ProfileType.LocalisationOwner:
                    return Worki.Resources.Models.Profile.Profile.LocalisationOwner;
                case ProfileType.Nomad:
                    return Worki.Resources.Models.Profile.Profile.Nomad;
                case ProfileType.Teleworker:
                    return Worki.Resources.Models.Profile.Profile.Teleworker;
                case ProfileType.Independant:
                    return Worki.Resources.Models.Profile.Profile.Independant;
                case ProfileType.Startup:
                    return Worki.Resources.Models.Profile.Profile.Startup;
                case ProfileType.Company:
                    return Worki.Resources.Models.Profile.Profile.Company;
                case ProfileType.Student:
                    return Worki.Resources.Models.Profile.Profile.Student;
                case ProfileType.PunctualUser:
                    return Worki.Resources.Models.Profile.Profile.PunctualUser;
                case ProfileType.Other:
                    return Worki.Resources.Models.Profile.Profile.Other;
                case ProfileType.NotSelected:
                    return Worki.Resources.Models.Profile.Profile.NotSelected;
                default:
                    return string.Empty;
            }
        }

        public string GetProfile()
        {
            return MemberMainData.GetProfileType(Profile);
        }

        public static Dictionary<int, string> GetProfileTypes()
        {
            return ProfileTypes.ToDictionary(t => t, t => GetProfileType(t));
        }
    }

    [Bind(Exclude = "RelationId,MemberId,Avatar")]
    public class MemberMainData_Validation
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Civility", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        public int Civility { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "LastName", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "FirstName", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string FirstName { get; set; }

        [Display(Name = "CompanyName", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string CompanyName { get; set; }

        [Display(Name = "Address", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Address { get; set; }

        [Display(Name = "City", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string City { get; set; }

        [Display(Name = "PostalCode", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(10, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string PostalCode { get; set; }

        [Display(Name = "Country", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Country { get; set; }

        [Display(Name = "WorkCity", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string WorkCity { get; set; }

        [Display(Name = "WorkPostalCode", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(10, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string WorkPostalCode { get; set; }

        [Display(Name = "WorkCountry", ResourceType = typeof(Worki.Resources.Models.Localisation.Localisation))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
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
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Description { get; set; }

        [Display(Name = "FavoritePlaces", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string FavoritePlaces { get; set; }

        [Display(Name = "Facebook", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Facebook { get; set; }

        [Display(Name = "Twitter", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Twitter { get; set; }

        [Display(Name = "Linkedin", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Linkedin { get; set; }

        [Display(Name = "Viadeo", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Viadeo { get; set; }

        [Display(Name = "Website", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Website { get; set; }

        [Display(Name = "PaymentAddress", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [Email(ErrorMessageResourceName = "PatternEmail", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string PaymentAddress { get; set; }

		[Display(Name = "SiretNumber", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string SiretNumber { get; set; }

		[Display(Name = "TaxNumber", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
		[StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
		public string TaxNumber { get; set; }

		[Display(Name = "TaxRate", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
		public decimal TaxRate { get; set; }
    }

    #endregion

    #region ProfilFormViewModel

    public class ProfilFormViewModel : IValidatableObject
    {
        #region Properties

        public Member Member { get; set; }
        public SelectList CivilitySelectTypes { get; private set; }
        public SelectList ProfileSelectTypes { get; private set; }

        #endregion

        #region Ctor

        public ProfilFormViewModel()
        {
            CivilitySelectTypes = new SelectList(MemberMainData.GetCivilityTypes(), "Key", "Value", CivilityType.Mr);
            ProfileSelectTypes = new SelectList(MemberMainData.GetProfileTypes(), "Key", "Value", ProfileType.LocalisationOwner);
            Member = new Member();
            Member.MemberMainData = new MemberMainData();
        }

        #endregion

        #region IValidatableObject

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!String.IsNullOrEmpty(Member.MemberMainData.Description))
            {
                if (FormValidation.ValidateDescription(Member.MemberMainData.Description))
                {
                    var error = string.Format(Worki.Resources.Validation.ValidationString.ProhibitedString, Worki.Resources.Models.Offer.Offer.Description);
                    yield return new ValidationResult(error, new[] { "Member.MemberMainData.Description" });
                }
            }
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

	public  class ProfilConstants
	{
		public const string TabId = "TabId";
		public const string PageUrl = "PageUrl";

		public enum DashboardTab
		{
			FavLoc,
			AddedLoc
		}

		public enum ProfilItem
		{
			CommunityActivity,
			Edit,
			ChangePassword
		}
	}

    public class ProfilPublicModel
    {
        #region Properties

        public Member Member { get; set; }
        public PagingList<Localisation> FavoriteLocalisations { get; set; }
		public PagingList<Localisation> AddedLocalisations { get; set; }

        #endregion

		public const int LocalisationPageSize = 6;
		public const int CommentPageSize = 3;

		public static ProfilPublicModel GetProfilPublic(Member member, int p1 = 1, int p2 = 1)
		{
			//get fav localisations
			var favLocs = new List<Localisation>();
			foreach (var item in member.FavoriteLocalisations.Skip((p1 - 1) * LocalisationPageSize).Take(LocalisationPageSize))
			{
				favLocs.Add(item.Localisation);
			}
			//added localisations
			var addedLoc = member.Localisations.Skip((p2 - 1) * LocalisationPageSize).Take(LocalisationPageSize).ToList();

			var profilDashboard = new ProfilPublicModel
			{
				Member = member,
				FavoriteLocalisations = new PagingList<Localisation>
				{
					List = favLocs,
					PagingInfo = new PagingInfo { CurrentPage = p1, ItemsPerPage = LocalisationPageSize, TotalItems = member.FavoriteLocalisations.Count }
				},
				AddedLocalisations = new PagingList<Localisation>
				{
					List = addedLoc,
					PagingInfo = new PagingInfo { CurrentPage = p2, ItemsPerPage = LocalisationPageSize, TotalItems = member.Localisations.Count }
				}
			};

			return profilDashboard;
		}
    }

    #endregion

    public class MemberApiModel
    {
        public MemberApiModel()
        {

        }

        public void UpdateMember(Member member)
        {
            member.MemberMainData.FirstName = FirstName;
            member.MemberMainData.LastName = LastName;
            member.MemberMainData.Profile = Profile;
            member.MemberMainData.Civility = Civility;
            if (BirthDate.HasValue)
                member.MemberMainData.BirthDate = BirthDate;
            if (!string.IsNullOrEmpty(PhoneNumber))
                member.MemberMainData.PhoneNumber = PhoneNumber;
            if (!string.IsNullOrEmpty(City))
                member.MemberMainData.City = City;
            if (!string.IsNullOrEmpty(PostalCode))
                member.MemberMainData.PostalCode = PostalCode;
            if (!string.IsNullOrEmpty(Country))
                member.MemberMainData.Country = Country;
            if (!string.IsNullOrEmpty(Description))
                member.MemberMainData.Description = Description;
        }

        public string Token { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public DateTime? BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public int Profile { get; set; }
        public int Civility { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public long FacebookId { get; set; }
        public string FacebookLink { get; set; }
    }
}
