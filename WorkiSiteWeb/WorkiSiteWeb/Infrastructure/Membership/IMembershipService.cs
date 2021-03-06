﻿using System.Collections.Generic;
using System.Web.Security;
using Worki.Data.Models;

namespace Worki.Memberships
{
    // Le type de FormsAuthentication est sealed et contient des membres statiques ; par conséquent, il est difficile
    // d'effectuer un test unitaire sur du code qui appelle ses membres. L'interface et la classe d'assistance ci-dessous montrent
    // comment créer un wrapper abstrait autour d'un tel type afin de pouvoir tester
    // l'unité de code AccountController.

	public interface IMembershipService
	{
		int MinPasswordLength { get; }
		bool ValidateUser(string userName, string password);
		MembershipCreateStatus CreateUser(string userName, string password, string email, bool forceActivation = false);
		bool ChangePassword(string userName, string oldPassword, string newPassword);
		bool ResetPassword(string email);
		bool DeleteUser(string userName);
		MembershipUserCollection GetAllUsers(int pageValue, int pageSize, out int itemTotal);
        IEnumerable<MemberAdminModel> GetAdminMapping(IEnumerable<Member> members);
		MembershipUser GetUser(string username);
		string GetUserByMail(string email);
		string GetPassword(string username, string answer);
        bool TryCreateAccount(string email, MemberMainData memberData, out int memberId, bool forceActivation = true);
        bool TryCreateAccount(string email, string password, MemberMainData memberData, out int memberId, bool forceActivation = true);
        bool ActivateMember(string username, string key);
        bool UnlockMember(string username);
        AuthJson GetAuthData(string username);
	}
}
