using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace Worki.SpecFlow
{
    public static class StaticStringClass
    {
        #region URL

        public static class URL
        {
            public const string AddFreeSpace = "lieu-de-travail/ajouter-lieu-gratuit";
            public const string AddNotFreeSpace = "lieu-de-travail/ajouter-lieu-payant";
            public const string AddSpace = "accueil/ajouter-espace";
            public const string Admin = "Admin/Sheet";
            public const string BOBooking = "Backoffice/Home/Booking";
            public const string BOClient = "Backoffice/Client/";
            public const string BOHome = "Backoffice/Home";
            public const string BOLoc = "Backoffice/Home/Localisations";
            public const string BOProfil = "Backoffice/Profil/";
            public const string BOQuotation = "Backoffice/Home/Quotation";
            public const string ChangePassword = "Dashboard/Profil/changer-mdp";
            public const string CGU = "accueil/cgu";
            public const string Contact = "accueil/contact";
            public const string Connexion = "compte/connexion";
            public const string Dashboard = "Dashboard/Profil/";
            public const string DashboardBooking = "Dashboard/Home/Booking";
            public const string DashboardQuotation = "Dashboard/Home/Quotation";
            public const string FAQ = "accueil/faq";
            public const string Inscription = "compte/inscription";
            public const string Job = "accueil/jobs";
            public const string Legal = "accueil/mentions-legales";
            public const string Press = "accueil/presse";
            public const string RentalResult = "annonces/resultats-annonces";
            public const string RentalAdd = "annonces/ajouter";
            public const string RentalDetail = "annonces/details/";
            public const string RentalIndex = "Admin/Sheet/IndexRental";
            public const string RentalSearch = "Rental/recherche";
            public const string RentalSendFriend = "annonces/envoyer-email-ami/";
            public const string Search = "lieu-de-travail/recherche";
            public const string WelcomePeopleIndex = "Admin/Activity/IndexWelcomePeople";
            public const string WhoWeAre = "accueil/a-propos";
        }

        #endregion

        #region Connexion

        public static class Connexion
        {
            public const string LocalLogin = "Admin";
            public const string OnlineLogin = "admin@eworky.com";
            public const string Password = "Admin_Pass";
        }

        #endregion

        #region Autre

        public static class Autre
        {
            public const string MsgPerso = "Ceci est un test auto Made by Mika";
            public const string MyLogin = "mika7869@gmail.com";
        }

        #endregion
    }
}
