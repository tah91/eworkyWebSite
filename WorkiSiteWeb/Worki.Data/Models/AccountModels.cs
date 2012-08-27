using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using Worki.Infrastructure;
using Worki.Infrastructure.Helpers;

namespace Worki.Data.Models
{
    #region Modèles

    public class ChangePasswordModel
    {
		public int MemberId { get; set; }

        public string Token { get; set; }

        [Required(ErrorMessageResourceName="Required", ErrorMessageResourceType= typeof(Worki.Resources.Validation.ValidationString))]
        [DataType(DataType.Password)]
        [Display(Name = "OldPassword", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
		[StringLength(128, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceName="Required", ErrorMessageResourceType= typeof(Worki.Resources.Validation.ValidationString))]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
		[StringLength(128, MinimumLength = MiscHelpers.Constants.MinRequiredPasswordLength, ErrorMessageResourceName = "MinMaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string NewPassword { get; set; }

        [Required(ErrorMessageResourceName="Required", ErrorMessageResourceType= typeof(Worki.Resources.Validation.ValidationString))]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
		[StringLength(128, MinimumLength = MiscHelpers.Constants.MinRequiredPasswordLength, ErrorMessageResourceName = "MinMaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Compare("NewPassword", ErrorMessageResourceName = "TheTwoNotMatch", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required(ErrorMessageResourceName="Required", ErrorMessageResourceType= typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Login", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
        [StringLength(128, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Login { get; set; }

        [Required(ErrorMessageResourceName="Required", ErrorMessageResourceType= typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Password", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
        [DataType(DataType.Password)]
        [StringLength(128, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Password { get; set; }

        [Display(Name = "RememberMe", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
        public bool RememberMe { get; set; }
    }

	public class RegisterModel : IDataErrorInfo
    {
        #region Ctor

        public RegisterModel()
        {
            CivilitySelectTypes = new SelectList(MemberMainData.GetCivilityTypes(), "Key", "Value", CivilityType.Mr);
            ProfileSelectTypes = new SelectList(MemberMainData.GetProfileTypes(), "Key", "Value", ProfileType.LocalisationOwner);
            MemberMainData = new MemberMainData();
			AcceptCGU = true;
        }

        #endregion

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
					case "AcceptCGU":
						{
							if (!AcceptCGU)
							{
								return Worki.Resources.Validation.ValidationString.MustAgreeCGU;
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

		#region CGU

		public bool AcceptCGU { get; set; }

		#endregion

		#region Member Table

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [DataType(DataType.EmailAddress)]
        [Email(ErrorMessageResourceName = "PatternEmail", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Email", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
		[StringLength(128, MinimumLength = MiscHelpers.Constants.MinRequiredPasswordLength, ErrorMessageResourceName = "MinMaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
		[StringLength(128, MinimumLength = MiscHelpers.Constants.MinRequiredPasswordLength, ErrorMessageResourceName = "MinMaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Compare("Password", ErrorMessageResourceName = "TheTwoNotMatch", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string ConfirmPassword { get; set; }

        #endregion

        #region MemberMainData Table

        public MemberMainData MemberMainData { get; set; }
        public SelectList CivilitySelectTypes { get; private set; }
        public SelectList ProfileSelectTypes { get; private set; }

        #endregion
    }

    public class ResetPasswordModel
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Email", ResourceType = typeof(Worki.Resources.Models.Contact.Contact))]
        [StringLength(MiscHelpers.Constants.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Email(ErrorMessageResourceName = "PatternEmail", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string EMail { get; set; }
    }

    public class PaymentInfoModel
    {
        public PaymentInfoModel()
        {

        }

        public PaymentInfoModel(Member member)
        {
            PaymentAddress = member.MemberMainData != null ? member.MemberMainData.PaymentAddress : string.Empty;
        }

        public void ChangePaymentInformation(Member member)
        {
            member.MemberMainData.PaymentAddress = PaymentAddress;
        }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "PaymentAddress", ResourceType = typeof(Worki.Resources.Models.Profile.Profile))]
        [Email(ErrorMessageResourceName = "PatternEmail", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string PaymentAddress { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Display(Name = "Password", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
        [DataType(DataType.Password)]
        [StringLength(128, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string WorkiPassword { get; set; }
    }

    #endregion
}
