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
            public const string Admin = "Admin";
            public const string ChangePassword = "profil/changer-mdp/";
            public const string CGU = "accueil/cgu";
            public const string Contact = "accueil/contact";
            public const string Connexion = "compte/connexion";
            public const string Dashboard = "profil/dashboard/";
            public const string FAQ = "accueil/faq";
            public const string Inscription = "compte/inscription";
            public const string Job = "accueil/jobs";
            public const string Legal = "accueil/mentions-legales";
            public const string Press = "accueil/presse";
            public const string RentalResult = "annonces/resultats-annonces";
            public const string RentalAdd = "annonces/ajouter";
            public const string RentalDetail = "annonces/details/";
            public const string RentalIndex = "Admin/IndexRental";
            public const string RentalSearch = "Rental/recherche";
            public const string RentalSendFriend = "annonces/envoyer-email-ami/";
            public const string Search = "lieu-de-travail/recherche";
            public const string WelcomePeopleIndex = "Admin/IndexWelcomePeople";
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
    }
}
