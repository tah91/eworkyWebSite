using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using Worki.Web.Infrastructure.Repository;
using Ninject;
using Worki.Web.Infrastructure.Logging;

namespace Worki.Web.Models
{
    public sealed class WorkiMembershipProvider : MembershipProvider
    {
        //cant get machine key on 1&1 hosting
        //private MachineKeySection _MachineKey;
		[Inject]
		public IMemberRepository MemberRepository { get; set; }

		[Inject]
		public ILogger Logger { get; set; }

        /*************************************************************************
         * Class initialization
         *************************************************************************/

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "WorkiMembershipProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Worki Membership Provider");
            }

            // Initialize base class
            base.Initialize(name, config);

            _applicationName = GetConfigValue(config["applicationName"],
                System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);

            // This is a non-standard helper setting.
            _providerName = GetConfigValue(config["providerName"], name);

            // Sets the default parameters for all the Membership Provider settings

            _requiresUniqeEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            _requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "true"));
            _minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "5"));
            _minRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"],
                "0"));
            _enablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            _enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "false"));
            _passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            _passwordStrengthRegularExpression = GetConfigValue(config["passwordStrengthRegularExpression"], "");
            _maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"],

                "5"));

            string passFormat = config["passwordFormat"];

            // If no format is specified, the default format will be hashed.
            if (passFormat == null)
                passFormat = "hashed";

            switch (passFormat.ToLower())
            {
                case "hashed":
                    _passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "encrypted":
                    _passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "clear":
                    _passwordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format '" + passFormat + "' is not supported. Check your web.config file.");
            }

            //Configuration cfg = WebConfigurationManager.OpenWebConfiguration(
            //    System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);

            //_MachineKey = (MachineKeySection)WebConfigurationManager.GetSection("system.web/machineKey");

            //if (_MachineKey.ValidationKey.Contains("AutoGenerate"))
            //    if (PasswordFormat != MembershipPasswordFormat.Clear)
            //        throw new ProviderException("Hashed or Encrypted passwords cannot be used with auto-generated keys.");

            //MembershipSection membership = (MembershipSection)WebConfigurationManager.GetSection("system.web/membership");
            //_userIsOnlineTimeWindow = membership.UserIsOnlineTimeWindow;
        }

        /*************************************************************************
         * General settings
         *************************************************************************/

        private string _applicationName;
        public override string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        private bool _requiresUniqeEmail;
        public override bool RequiresUniqueEmail
        {
            get { return _requiresUniqeEmail; }
        }

        /*************************************************************************
         * Private settings
         *************************************************************************/

        private string _providerName;
        public string ProviderName
        {
            get { return _providerName; }
        }

        private TimeSpan _userIsOnlineTimeWindow = TimeSpan.MinValue;
        public TimeSpan UserIsOnlineTimeWindow
        {
            get { return _userIsOnlineTimeWindow; }
        }

        /*************************************************************************
         * Password settings
         *************************************************************************/

        private int _minRequiredNonAlphanumericCharacters;
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _minRequiredNonAlphanumericCharacters; }
        }

        private int _minRequiredPasswordLength;
        public override int MinRequiredPasswordLength
        {
            get { return _minRequiredPasswordLength; }
        }

        private bool _enablePasswordReset;
        public override bool EnablePasswordReset
        {
            get { return _enablePasswordReset; }
        }

        private bool _enablePasswordRetrieval;
        public override bool EnablePasswordRetrieval
        {
            get { return _enablePasswordRetrieval; }
        }

        private int _passwordAttemptWindow;
        public override int PasswordAttemptWindow
        {
            get { return _passwordAttemptWindow; }
        }

        private string _passwordStrengthRegularExpression;
        public override string PasswordStrengthRegularExpression
        {
            get { return _passwordStrengthRegularExpression; }
        }

        private MembershipPasswordFormat _passwordFormat;
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _passwordFormat; }
        }

        private int _maxInvalidPasswordAttempts;
        public override int MaxInvalidPasswordAttempts
        {
            get { return _maxInvalidPasswordAttempts; }
        }

        private bool _requiresQuestionAndAnswer;
        public override bool RequiresQuestionAndAnswer
        {
            get { return _requiresQuestionAndAnswer; }
        }

        /*************************************************************************
         * User related methods : create, update, unlock, delete methods.
         *************************************************************************/

        /// <summary>
        /// Creates a new user with a given set of default values
        /// </summary>
        public override MembershipUser CreateUser(string username,
                                                    string password,
                                                    string email,
                                                    string passwordQuestion,
                                                    string passwordAnswer,
                                                    bool isApproved,
                                                    object providerUserKey,
                                                    out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && !string.IsNullOrEmpty(GetUserNameByEmail(email)))
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            // If no user with this name already exists
            if (GetUser(username, false) == null)
            {
                DateTime createdDate = DateTime.Now;

                string salt = "";
                if (PasswordFormat == MembershipPasswordFormat.Hashed)
                {
                    salt = GenerateSalt();
                    password = password + salt;
                }

                Member m = new Member();
                m.Username = username;
                m.Password = EncodePassword(password);
                m.PasswordSalt = salt;
                m.Email = email;

                // Set the password retrieval question and answer if they are required
                if (RequiresQuestionAndAnswer)
                {
                    m.PasswordQuestion = passwordQuestion;
                    m.PasswordAnswer = EncodePassword(passwordAnswer);
                }

                m.IsApproved = isApproved;
                m.IsLockedOut = false;
                m.Comment = "";
                m.CreatedDate = createdDate;
                m.LastLockoutDate = createdDate;
                m.LastLoginDate = createdDate;
                m.LastActivityDate = createdDate;
                m.LastPasswordChangedDate = createdDate;
                m.FailedPasswordAttemptCount = 0;
                m.FailedPasswordAttemptWindowStart = createdDate;
                m.FailedPasswordAnswerAttemptCount = 0;
                m.FailedPasswordAnswerAttemptWindowStart = createdDate;
                m.EmailKey = Member.GenerateKey();

                try
                {
                    MemberRepository.Add(m);

                    // User creation was a success
                    status = MembershipCreateStatus.Success;

                    // Return the newly craeted user
                    return GetUserFromMember(m);
                }
                catch(Exception ex)
                {
                    // Something was wrong and the user was rejected
                    status = MembershipCreateStatus.UserRejected;
					Logger.Error("CreateUser", ex);
                }
            }
            else
            {
                // There is already a user with this name
                status = MembershipCreateStatus.DuplicateUserName;
            }

            // Something went wrong if we got this far without some sort of status or retun
            if (status != MembershipCreateStatus.UserRejected && status != MembershipCreateStatus.DuplicateUserName)
                status = MembershipCreateStatus.ProviderError;

            return null;
        }

        /// <summary>
        /// Updates an existing user with new settings
        /// </summary>
        /// <param name="user">MembershipUser object to modify</param>
        public override void UpdateUser(MembershipUser user)
        {
			var m = MemberRepository.GetMember(user.UserName);
            MemberRepository.Update(m.MemberId, member =>
            {
                member.Comment = user.Comment;
                member.Email = user.Email;
                member.IsApproved = user.IsApproved;
            });
        }

        /// <summary>
        /// Unlocks a user (after too many login attempts perhaps)
        /// </summary>
        /// <param name="userName">Username to unlock</param>
        /// <returns>True if successful. Defaults to false.</returns>
        public override bool UnlockUser(string userName)
        {
            bool ret = false;
            try
            {
                var m = MemberRepository.GetMember(userName);
                MemberRepository.Update(m.MemberId, member =>
                {
					member.IsLockedOut = false;
                });

                // A user was found and nothing was thrown
                ret = true;
            }
            catch(Exception ex)
            {
                // Couldn't find the user or there was an error
                ret = false;
				Logger.Error("UnlockUser", ex);
            }
            return ret;
        }

        /// <summary>
        /// Permanently deletes a user from the database
        /// </summary>
        /// <param name="username">Username to delete</param>
        /// <param name="deleteAllRelatedData">Should or shouldn't delete related user data</param>
        /// <returns>True if successful. Defaults to false.</returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            // Return status defaults to false.
            // When in doubt, always say "NO".
            bool ret = false;

            try
            {
				var m = MemberRepository.GetMember(username);
                MemberRepository.Delete(m.MemberId);
                // Nothing was thrown, so go ahead and return true
                ret = true;
            }
            catch(Exception ex)
            {
                // Couldn't find the user or was not able to delete
                ret = false;
				Logger.Error("DeleteUser", ex);
            }

            return ret;
        }

        /*************************************************************************
         * User authentication methods
         *************************************************************************/
        /// <summary>
        /// Authenticates a user with the given username and password
        /// </summary>
        /// <param name="password">The login username</param>
        /// <param name="username">Login password</param>
        /// <returns>True if successful. Defaults to false.</returns>
        public override bool ValidateUser(string username, string password)
        {
            // Return status defaults to false.
            bool ret = false;

            try
            {
				var member = MemberRepository.GetMember(username);
                MemberRepository.Update(member.MemberId, m =>
                {
                    // We found a user by the username
                    if (m != null)
                    {
                        // A user cannot login if not approved or locked out
                        if ((!m.IsApproved) || m.IsLockedOut)
                        {
                            ret = false;
                        }
                        else
                        {
                            // Trigger period
                            DateTime dt = DateTime.Now;

                            // Check the given password and the one stored (and salt, if it exists)
                            if (CheckPassword(password, m.Password, m.PasswordSalt))
                            {
                                m.LastLoginDate = dt;
                                m.LastActivityDate = dt;

                                // Reset past failures
                                ResetAuthenticationFailures(ref m, dt);

                                ret = true;
                            }
                            else
                            {
                                // The login failed... Increment the login attempt count
                                m.FailedPasswordAttemptCount = (int)m.FailedPasswordAttemptCount + 1;

                                if (m.FailedPasswordAttemptCount >= MaxInvalidPasswordAttempts)
                                    m.IsLockedOut = true;

                                m.FailedPasswordAttemptWindowStart = dt;

                            }
                        }
                    }
                });
            }
            catch(Exception ex)
            {
                // Nothing was thrown, so go ahead and return true
                ret = false;
				Logger.Error("ValidateUser", ex);
            }

            return ret;
        }

        /// <summary>
        /// Gets the current password of a user (provided it isn't hashed)
        /// </summary>
        /// <param name="username">User the password is being retrieved for</param>
        /// <param name="answer">Password retrieval answer</param>
        /// <returns>User's passsword</returns>
        public override string GetPassword(string username, string answer)
        {
            // Default password is empty
            string password = String.Empty;

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException("Hashed passwords cannot be retrieved. They must be reset.");
            }
            else
            {
                try
                {
                    Member m = MemberRepository.GetMember(username);
                    password = UnEncodePassword(m.Password);
                }
                catch(Exception ex)
				{
					Logger.Error("GetPassword", ex);
				}
            }
            return password;
        }

        /// <summary>
        /// Resets the passwords with a generated value
        /// </summary>
        /// <param name="username">User the password is being reset for</param>
        /// <param name="answer">Password retrieval answer</param>
        /// <returns>Newly generated password</returns>
		public override string ResetPassword(string username, string answer)
		{
			// Default password is empty
			string pass = String.Empty;

			try
			{
				Member member = MemberRepository.GetMember(username);
				MemberRepository.Update(member.MemberId, m =>
				{
					// We found a user by that name
					if (m != null)
					{
						// Check if the returned password answer matches
						if (answer == m.PasswordAnswer)
						{
							// Create a new password with the minimum number of characters
							pass = GeneratePassword(MinRequiredPasswordLength);

							// If the password format is hashed, there must be a salt added
							string salt = "";
							if (PasswordFormat == MembershipPasswordFormat.Hashed)
							{
								salt = GenerateSalt();
								pass = pass + salt;
							}

							m.Password = EncodePassword(pass);
							m.PasswordSalt = salt;
							//to change password, same process has activation link
							m.EmailKey = Member.GenerateKey();

							// Reset everyting
							ResetAuthenticationFailures(ref m, DateTime.Now);

							//MemberRepository.Save();
						}
					}
				});
			}
			catch (Exception ex)
			{
				Logger.Error("ResetPassword", ex);
			}
			return pass;
		}

        /// <summary>
        /// Change the current password for a new one. Note: Both are required.
        /// </summary>
        /// <param name="username">Username the password is being changed for</param>
        /// <param name="oldPassword">Old password to verify owner</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if successful. Defaults to false.</returns>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (!ValidateUser(username, oldPassword))
                return false;

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, false);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Password change has been cancelled due to a validation failure.");

            bool ret = false;
            try
            {
                Member member = MemberRepository.GetMember(username);
				MemberRepository.Update(member.MemberId, m =>
                {
                    string salt = "";
                    if (PasswordFormat == MembershipPasswordFormat.Hashed)
                    {
                        salt = GenerateSalt();
                        newPassword = newPassword + salt;
                    }

                    m.Password = EncodePassword(newPassword);
                    m.PasswordSalt = salt;
                    m.EmailKey = null;

                    // Reset everything
                    ResetAuthenticationFailures(ref m, DateTime.Now);
                });
                ret = true;
            }
            catch(Exception ex)
            {
                ret = false;
				Logger.Error("ChangePassword", ex);
            }

            return ret;
        }

        /// <summary>
        /// Change the password retreival/reset question and answer pair
        /// </summary>
        /// <param name="username">Username the question and answer are being changed for</param>
        /// <param name="password">Current password</param>
        /// <param name="newPasswordQuestion">New password question</param>
        /// <param name="newPasswordAnswer">New password answer (will also be encrypted)</param>
        /// <returns>True if successful. Defaults to false.</returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password,
            string newPasswordQuestion, string newPasswordAnswer)
        {
            if (!ValidateUser(username, password))
                return false;

            bool ret = false;
            try
            {
				Member m = MemberRepository.GetMember(username);
                MemberRepository.Update(m.MemberId, member =>
                {
                    member.PasswordQuestion = newPasswordQuestion;
                    member.PasswordAnswer = EncodePassword(newPasswordAnswer);
                });
                ret = true;
            }
            catch(Exception ex)
            {
                ret = false;
				Logger.Error("ChangePasswordQuestionAndAnswer", ex);
            }
            return ret;
        }

        /*************************************************************************
         * User information retreival methods
         *************************************************************************/

        /// <summary>
        /// Gets the username by a given matching email address
        /// </summary>
        public override string GetUserNameByEmail(string email)
        {
            string username = String.Empty;

            try
            {
                username = MemberRepository.GetUserName(email);
            }
            catch(Exception ex)
            {
				Logger.Error("GetUser", ex);
            }
            return username;
        }

        /// <summary>
        /// Gets a MembershipUser object with a given key
        /// </summary>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            MembershipUser u = null;
            try
            {
                Member m = MemberRepository.Get(Convert.ToInt32(providerUserKey));

                if (m != null)
                    u = GetUserFromMember(m);
            }
			catch (Exception ex)
            {
				Logger.Error("GetUser", ex);
			}

            return u;
        }

        /// <summary>
        /// Gets a MembershipUser object with a given username
        /// </summary>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            MembershipUser u = null;

            try
            {
                Member m = MemberRepository.GetMember(username);

                if (m != null)
                    u = GetUserFromMember(m);
            }
            catch(Exception ex)
            {
				Logger.Error("GetUser", ex);
			}

            return u;
        }

        /// <summary>
        /// Gets all the users in the database
        /// </summary>
        /// <param name="pageIndex">Current page index</param>
        /// <param name="pageSize">Number of results per page</param>
        /// <param name="totalRecords">Total number of users returned</param>
        /// <returns>MembershpUserCollection object with a list of users on the page</returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize,
            out int totalRecords)
        {
            MembershipUserCollection users = new MembershipUserCollection();
            totalRecords = 0;

            try
            {
                int start = pageSize * pageIndex;
                int end = start + pageSize;

                totalRecords = MemberRepository.GetCount();

				IList<Member> mlist = MemberRepository.Get(start, pageSize, m => m.MemberId);

                foreach (Member m in mlist)
                    users.Add(GetUserFromMember(m));
            }
            catch(Exception ex)
			{
				Logger.Error("GetAllUsers", ex);
			}

            return users;
        }

        /// <summary>
        /// Gets the total number of users that are currently online.
        /// </summary>
        /// <returns>Returns user count (within UserIsOnlineTimeWindow minutes)</returns>
        public override int GetNumberOfUsersOnline()
        {
            int c = 0;
            try
            {
                c = (from members in MemberRepository.GetAll()
                     where members.LastActivityDate.Add(UserIsOnlineTimeWindow) >= DateTime.Now
                     select members).Count();
            }
            catch { }

            return c;
        }

        /// <summary>
        /// Finds a list of users with a matching email address
        /// </summary>
        /// <param name="emailToMatch">Given email to search</param>
        /// <param name="pageIndex">Current page index</param>
        /// <param name="pageSize">Number of results per page</param>
        /// <param name="totalRecords">Total number of users returned</param>
        /// <returns>MembershpUserCollection object with a list of users on the page</returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch,
            int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = new MembershipUserCollection();
            totalRecords = 0;

            try
            {
                int start = pageSize * pageIndex;
                int end = start + pageSize;

                totalRecords = (from members in MemberRepository.GetAll()
                                where members.Email.Contains(emailToMatch)
                                select members).Count();

                List<Member> mlist = (from members in MemberRepository.GetAll()
                                      where members.Email.Contains(emailToMatch)
                                      select members).Skip(start).Take(pageSize).ToList();

                foreach (Member m in mlist)
                    users.Add(GetUserFromMember(m));
            }
            catch { }

            return users;
        }

        /// <summary>
        /// Gets a list of users with a matching username
        /// </summary>
        /// <param name="usernameToMatch">Username to search for</param>
        /// <param name="pageIndex">Current page index</param>
        /// <param name="pageSize">Number of results per page</param>
        /// <param name="totalRecords">Total number of users returned</param>
        /// <returns>MembershpUserCollection object with a list of users on the page</returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch,
            int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = new MembershipUserCollection();
            totalRecords = 0;

            try
            {
                int start = pageSize * pageIndex;
                int end = start + pageSize;

                totalRecords = MemberRepository.GetCount();

                List<Member> mlist = (from members in MemberRepository.GetAll()
                                      where members.Username.Contains(usernameToMatch)
                                      select members).Skip(start).Take(pageSize).ToList();

                foreach (Member m in mlist)
                    users.Add(GetUserFromMember(m));
            }
            catch { }

            return users;
        }

        /*************************************************************************
         * Private password helper methods
         *************************************************************************/

        /// <summary>
        /// Compares a given password with one stored in the database and an optional salt
        /// </summary>
        private bool CheckPassword(string password, string dbpassword, string dbsalt)
        {
            string pass1 = password;
            string pass2 = dbpassword;
            bool ret = false;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbpassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password + dbsalt);
                    break;
                default:
                    break;
            }

            if (pass1 == pass2)
                ret = true;

            return ret;
        }

        /// <summary>
        /// Encodes a given password using the default MembershipPasswordFormat setting
        /// </summary>
        /// <param name="password">Password (plus salt as per above functions if necessary)</param>
        /// <returns>Clear form, Encrypted or Hashed password.</returns>
        private string EncodePassword(string password)
        {
            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword =
                      Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                //case MembershipPasswordFormat.Hashed:
                //    HMACSHA1 hash = new HMACSHA1();
                //    hash.Key = HexToByte(_MachineKey.ValidationKey);
                //    encodedPassword =
                //      Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                //    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return encodedPassword;
        }

        /// <summary>
        /// Decodes a given stored password into a cleartype or unencrypted form. Provided it isn't hashed.
        /// </summary>
        /// <param name="encodedPassword">Stored, encrypted password</param>
        /// <returns>Unecncrypted password</returns>
        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password =
                      Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot decode hashed passwords.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        /// <summary>
        /// Converts a string into a byte array
        /// </summary>
        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// Salt generation helper (this is essentially the same as the one in SqlMembershipProviders
        /// </summary>
        private string GenerateSalt()
        {
            byte[] buf = new byte[16];
            (new RNGCryptoServiceProvider()).GetBytes(buf);
            return Convert.ToBase64String(buf);
        }

        /// <summary>
        /// Generates a random password of given length (MinRequiredPasswordLength)
        /// </summary>
        private string GeneratePassword(int passLength)
        {
            string _range = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Byte[] _bytes = new Byte[passLength];
            char[] _chars = new char[passLength];

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            rng.GetBytes(_bytes);

            for (int i = 0; i < passLength; i++)
                _chars[i] = _range[_bytes[i] % _range.Length];

            return new string(_chars);
        }

        /*************************************************************************
         * Private helper methods
         *************************************************************************/

        /// <summary>
        /// Used in the initializtion, key in web.config or the default setting if null.
        /// </summary>
        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }

        /// <summary>
        /// Upon a successful login or password reset, this changes all the previous failure markers to defaults.
        /// </summary>
        private static void ResetAuthenticationFailures(ref Member m, DateTime dt)
        {
            m.LastPasswordChangedDate = dt;
            m.FailedPasswordAttemptCount = 0;
            m.FailedPasswordAttemptWindowStart = dt;
            m.FailedPasswordAnswerAttemptCount = 0;
            m.FailedPasswordAnswerAttemptWindowStart = dt;
        }

        /// <summary>
        /// Converts a Member object into a MembershipUser object using its assigned settings
        /// </summary>
        private MembershipUser GetUserFromMember(Member m)
        {
            return new MembershipUser(this.ProviderName,
                        m.Username,
                        m.MemberId,
                        m.Email,
                        m.PasswordQuestion,
                        m.Comment,
                        m.IsApproved,
                        m.IsLockedOut,
                        m.CreatedDate,
                        m.LastLoginDate,
                        m.LastLoginDate,
                        m.LastPasswordChangedDate,
                        m.LastLockoutDate);
        }
    }
}
