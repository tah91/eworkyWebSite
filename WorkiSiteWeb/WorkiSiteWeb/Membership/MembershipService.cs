using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;
using System.Linq;
using Worki.Infrastructure.Repository;

namespace Worki.Memberships
{
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

        public MembershipCreateStatus CreateUser(string userName, string password, string email, bool forceActivation = false)
		{
			if (String.IsNullOrEmpty(userName))
				throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "userName");
			if (String.IsNullOrEmpty(password))
				throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "password");
			if (String.IsNullOrEmpty(email))
				throw new ArgumentException(Worki.Resources.Validation.ValidationString.CannotBeEmpty, "email");

			MembershipCreateStatus status;
            _Provider.CreateUser(userName, password, email, null, null, forceActivation, null, out status);
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
				MembershipUser currentUser = _Provider.GetUser(userName, false);
				var newPassword = currentUser.ResetPassword();
				return true;
			}
            catch (Member.ValidationException ex)
			{
                throw ex;
			}
			catch (Exception)
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

        public IEnumerable<MemberAdminModel> GetAdminMapping(IEnumerable<Member> members)
        {
            var toRet = (from item
                             in members
                         select new MemberAdminModel
                         {
                             MemberId = item.MemberId,
                             UserName = item.Username,
                             IsAdmin = Roles.IsUserInRole(item.Username, MiscHelpers.AdminConstants.AdminRole),
                             Locked = item.IsLockedOut,
                             LastName = item.GetFullDisplayName()
                         });
            return toRet.ToList();
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

		/// <summary>
		/// Try to create an account for the given mail and data, already activated
		/// if account aleady exists, do nothing
		/// </summary>
		/// <param name="email">email of the account to create</param>
		/// <param name="memberData">member data of the account</param>
		/// <param name="memberId">filled by the fectched account</param>
		/// <returns>true if account created</returns>
		public bool TryCreateAccount(string email, MemberMainData memberData, out int memberId)
		{
			//check if email match an account
			var createAccountContext = ModelFactory.GetUnitOfWork();
			var createAccountmRepo = ModelFactory.GetRepository<IMemberRepository>(createAccountContext);
			try
			{
				var member = createAccountmRepo.GetMember(email);
				if (member != null)
				{
					memberId = member.MemberId;
					return false;
				}
				else
				{
					//if not create an account from memberdata
					var status = CreateUser(email, MiscHelpers.AdminConstants.DummyPassword, email, true);
					if (status != System.Web.Security.MembershipCreateStatus.Success)
					{
						var error = AccountValidation.ErrorCodeToString(status);
						throw new Exception(error);
					}
					var created = createAccountmRepo.GetMember(email);
					created.MemberMainData = memberData;
					createAccountContext.Commit();

					if (!ResetPassword(email))
					{
						throw new Exception("ResetPassword failed");
					}

					memberId = created.MemberId;
					return true;
				}
			}
			catch (Exception ex)
			{
				createAccountContext.Complete();
				throw ex;
			}
		}

        public bool ActivateMember(string username, string key)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var member = mRepo.GetMember(username);
            if (member == null)
                return false;
            try
            {
                if (string.Compare(key, member.EmailKey) == 0)
                {
                    member.IsApproved = true;
                    member.LastActivityDate = DateTime.UtcNow;
                    member.EmailKey = null;
                    context.Commit();
                    return true;
                }
                else
                {
                    context.Complete();
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UnlockMember(string username)
        {
            try
            {
                _Provider.UnlockUser(username);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
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
					return Worki.Resources.Validation.ValidationString.UnknowError;
			}
		}
	}
}
