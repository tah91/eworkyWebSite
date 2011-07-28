﻿using System.Collections.Generic;
using System.Web.Security;

namespace WorkiSiteWeb.Infrastructure.Membership
{
    // Le type de FormsAuthentication est sealed et contient des membres statiques ; par conséquent, il est difficile
    // d'effectuer un test unitaire sur du code qui appelle ses membres. L'interface et la classe d'assistance ci-dessous montrent
    // comment créer un wrapper abstrait autour d'un tel type afin de pouvoir tester
    // l'unité de code AccountController.

    public interface IMembershipService
    {
        int MinPasswordLength { get; }
        bool ValidateUser(string userName, string password);
        MembershipCreateStatus CreateUser(string userName, string password, string email);
        bool ChangePassword(string userName, string oldPassword, string newPassword);
        bool ResetPassword(string email);
        bool DeleteUser(string userName);
        MembershipUserCollection GetAllUsers(int pageValue, int pageSize, out int itemTotal);
        Dictionary<MembershipUser, bool> ListUserMember(MembershipUserCollection collectionMemberShip);
        MembershipUser GetUser(string username);
        string GetUserByMail(string email);
        string GetPassword(string username, string answer);
    }
}
