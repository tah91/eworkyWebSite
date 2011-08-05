using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Worki.Web.Helpers;
using Worki.Web.Infrastructure;
using Worki.Web.Infrastructure.Membership;

namespace Worki.Web.Models
{

    #region Modèles

    public class ChangePasswordModel
    {        
        public string UserName { get; set; }

        [Required(ErrorMessageResourceName="Required", ErrorMessageResourceType= typeof(Worki.Resources.Validation.ValidationString))]
        [DataType(DataType.Password)]
        [Display(Name = "OldPassword", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
        [StringLength(128, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceName="Required", ErrorMessageResourceType= typeof(Worki.Resources.Validation.ValidationString))]
        [ValidatePasswordLength(ErrorMessageResourceName = "MinLetter", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
        [StringLength(128, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string NewPassword { get; set; }

        [Required(ErrorMessageResourceName="Required", ErrorMessageResourceType= typeof(Worki.Resources.Validation.ValidationString))]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
        [ValidatePasswordLength(ErrorMessageResourceName = "MinLetter", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [StringLength(128, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
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
            CivilitySelectTypes = new SelectList(MemberMainData.CivilityTypes, "Key", "Value", CivilityType.Mr);
            ProfileSelectTypes = new SelectList(MemberMainData.ProfileTypes, "Key", "Value", ProfileType.LocalisationOwner);
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
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
        [StringLength(128, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Worki.Resources.Models.Account.AccountModels))]
        [StringLength(128, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
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
        [StringLength(MiscHelpers.MaxLengh, ErrorMessageResourceName = "MaxLength", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        [Email(ErrorMessageResourceName = "PatternEmail", ErrorMessageResourceType = typeof(Worki.Resources.Validation.ValidationString))]
        public string EMail { get; set; }
    }

    #endregion

    #region Services

    public class AccountMembershipService : IMembershipService
    {
        private readonly MembershipProvider _Provider;

        public AccountMembershipService()
            : this(null)
        {
            
        }

        public AccountMembershipService(MembershipProvider provider)
        {
            _Provider = provider ?? Membership.Provider;
        }

        public int MinPasswordLength
        {
            get
            {
                return _Provider.MinRequiredPasswordLength;
            }
        }

        public bool ValidateUser(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName)) 
                throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "userName");
            if (String.IsNullOrEmpty(password))
                throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "password");

            return _Provider.ValidateUser(userName, password);
        }

        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "userName");
            if (String.IsNullOrEmpty(password))
                throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "password");
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "email");

            MembershipCreateStatus status;
            _Provider.CreateUser(userName, password, email, null, null, false, null, out status);
            return status;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "userName");
            if (String.IsNullOrEmpty(oldPassword))
                throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "oldPassword");
            if (String.IsNullOrEmpty(newPassword))
                throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "newPassword");

            // Le ChangePassword() sous-jacent lèvera une exception plutôt
            // que de retourner false dans certains scénarios de défaillance.
            try
            {
                MembershipUser currentUser = _Provider.GetUser(userName, true /* userIsOnline */);
                return currentUser.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }

        public bool ResetPassword(string email)
        {
            if (String.IsNullOrEmpty(email))
                throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "email");

            // Le ChangePassword() sous-jacent lèvera une exception plutôt
            // que de retourner false dans certains scénarios de défaillance.
            try
            {
                var userName = _Provider.GetUserNameByEmail(email);
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentException(Worki.Resources.Validation.ValidationString.MailDoNotMatch, "email");
                MembershipUser currentUser = _Provider.GetUser(userName, false);
                if (currentUser == null)
                    return false;
                var newPassword = currentUser.ResetPassword();
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }

        public bool DeleteUser(string userName) 
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "userName");

            // Le ChangePassword() sous-jacent lèvera une exception plutôt
            // que de retourner false dans certains scénarios de défaillance.
            try
            {
                MembershipUser currentUser = _Provider.GetUser(userName, false);
                if (currentUser == null)
                    return false;
                else
                {
                    _Provider.DeleteUser(userName, true);
                    return true;
                }
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }

        public MembershipUserCollection GetAllUsers(int pageValue, int PageSize, out int itemTotal)
        {
          return _Provider.GetAllUsers(pageValue, PageSize, out itemTotal);
        }

        public Dictionary<MembershipUser, bool> ListUserMember(MembershipUserCollection CollectionMemberShip)
        {
            Dictionary<MembershipUser, bool>  ListMemberShip = new Dictionary<MembershipUser, bool>();
            foreach (MembershipUser memberShip in CollectionMemberShip)
            {
                string[] _roles = Roles.GetRolesForUser(memberShip.UserName);
                if (_roles.Contains(MiscHelpers.AdminRole))
                {
                    ListMemberShip[memberShip] = true;
                }
                else
                {
                    ListMemberShip[memberShip] = false;
                }
            }
            return ListMemberShip;
        }

        public MembershipUser GetUser(string username)
        {
            var b = true;
            return _Provider.GetUser(username, b);            
        }

        public string GetUserByMail(string email)
        {
            return _Provider.GetUserNameByEmail(email);
        }

        public string GetPassword(string username, string answer)
        {
            return _Provider.GetPassword(username, answer);
        }
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignIn(string userName, string userData, bool createPersistentCookie, HttpResponseBase response)
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "userName");

            // Create the cookie that contains the forms authentication ticket
            HttpCookie authCookie = FormsAuthentication.GetAuthCookie(userName, createPersistentCookie);

            // Get the FormsAuthenticationTicket out of the encrypted cookie
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

            // Create a new FormsAuthenticationTicket that includes our custom User Data
            FormsAuthenticationTicket newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate, ticket.Expiration, ticket.IsPersistent, userData);

            // Update the authCookie's Value to use the encrypted version of newTicket
            authCookie.Value = FormsAuthentication.Encrypt(newTicket);

            // Manually add the authCookie to the Cookies collection
            response.Cookies.Add(authCookie);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
    #endregion

    #region Validation

    public static class AccountValidation
    {
        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // Consultez http://go.microsoft.com/fwlink/?LinkID=177550 pour
            // obtenir la liste complète des codes d'état.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return Worki.Resources.Validation.ValidationString.UsernameExist;

                case MembershipCreateStatus.DuplicateEmail:
                    return Worki.Resources.Validation.ValidationString.UsernameExistForThisMail;

                case MembershipCreateStatus.InvalidPassword:
                    return Worki.Resources.Validation.ValidationString.GivenPasswordNotValide;

                case MembershipCreateStatus.InvalidEmail:
                    return Worki.Resources.Validation.ValidationString.MailNotValide;

                case MembershipCreateStatus.InvalidAnswer:
                    return Worki.Resources.Validation.ValidationString.GetBackPasswordMailNotValide;

                case MembershipCreateStatus.InvalidQuestion:
                    return Worki.Resources.Validation.ValidationString.GetBackPasswordQuestionNotValide;

                case MembershipCreateStatus.InvalidUserName:
                    return Worki.Resources.Validation.ValidationString.UsernameNotValide;

                case MembershipCreateStatus.ProviderError:
                    return Worki.Resources.Validation.ValidationString.ReturnErrorTryAgain;

                case MembershipCreateStatus.UserRejected:
                    return Worki.Resources.Validation.ValidationString.CreateUsernameFail;

                default:
                    return Worki.Resources.Validation.ValidationString.InknowError;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidatePasswordLengthAttribute : ValidationAttribute
    {
        private readonly int _minCharacters = Membership.Provider.MinRequiredPasswordLength;

        public ValidatePasswordLengthAttribute()
            : base()
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name, _minCharacters);
        }

        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            return (valueAsString != null && valueAsString.Length >= _minCharacters);
        }
    }
    #endregion

}
