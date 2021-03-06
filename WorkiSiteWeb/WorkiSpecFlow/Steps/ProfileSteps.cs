﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using SHDocVw;
using WatiN.Core;
using NUnit.Framework;

namespace Worki.SpecFlow
{
    [Binding]
    class ProfileSteps
    {
        #region Aller sur son profil

        [When(@"Je clique sur mon profil")]
        public void WhenJeCliqueSurMonProfil()
        {
            WebBrowser.Current.Page<AccueilPage>().Lien_MonProfil.Click();
        }

        [Then(@"Je dois arriver sur mon profil")]
        public void ThenJeDoisArriverSurMonProfil()
        {
            Assert.IsTrue(WebBrowser.Current.Url.Contains(StaticStringClass.URL.Dashboard));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Editer le Profil

        [When(@"Je clique sur Editer Profil")]
        public void WhenJeCliqueSurEditerProfil()
        {
            WebBrowser.Current.Page<ProfilPage>().Lien_Editer.Click();
        }

        [When(@"Je change quelques champs")]
        public void WhenJeChangeQuelquesChamps()
        {
            WebBrowser.Current.Page<ProfilPage>().Profil_Type.Select(Worki.Resources.Models.Profile.Profile.Nomad);
            WebBrowser.Current.Page<ProfilPage>().LastName.TypeTextQuickly("AdminE");
            WebBrowser.Current.Page<ProfilPage>().FirstName.TypeTextQuickly("AdminE");
            WebBrowser.Current.Page<ProfilPage>().DescriptionField.TypeTextQuickly(StaticStringClass.Autre.MsgPerso);
            WebBrowser.Current.Page<ProfilPage>().Company.TypeTextQuickly("eWorky");
        }

        [When(@"Je valide le formulaire du profil")]
        public void WhenJeValideLeFormulaireDuProfil()
        {
            WebBrowser.Current.Page<ProfilPage>().Boutton_Valider.Click();
        }

        [Then(@"Je dois avoir les modifications faites")]
        public void ThenJeDoisAvoirLesModificationsFaites()
        {
            var ie = WebBrowser.Current;
            WebBrowser.Current.Page<ProfilPage>().Lien_Editer.Click();
            Assert.IsTrue(ie.ContainsText(Worki.Resources.Models.Profile.Profile.Nomad));
            Assert.IsTrue(ie.ContainsText(StaticStringClass.Autre.MsgPerso));
            Assert.IsTrue(ie.ContainsText("eWorky"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Reinitialiser le profil

        [When(@"Je remet les champs de base")]
        public void WhenJeRemetLesChampsDeBase()
        {
            WebBrowser.Current.Page<ProfilPage>().Profil_Type.Select("Autre");
            WebBrowser.Current.Page<ProfilPage>().LastName.TypeTextQuickly("Admin");
            WebBrowser.Current.Page<ProfilPage>().FirstName.TypeTextQuickly("Admin");
            WebBrowser.Current.Page<ProfilPage>().DescriptionField.TypeTextQuickly("");
            WebBrowser.Current.Page<ProfilPage>().Company.TypeTextQuickly("");
        }

        [Then(@"Le profil est reinitialiser")]
        public void ThenLeProfilEstReinitialiser()
        {
            var ie = WebBrowser.Current;
            WebBrowser.Current.Page<ProfilPage>().Lien_Editer.Click();
            Assert.IsTrue(ie.ContainsText("Admin"));
            Assert.IsTrue(ie.ContainsText("Autre"));
            Assert.IsTrue(!ie.ContainsText(StaticStringClass.Autre.MsgPerso));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Changer le Mot de passe

        [When(@"Je clique sur Modifier mon mot de passe")]
        public void WhenJeCliqueSurModifierMonMotDePasse()
        {
            WebBrowser.Current.Page<ProfilPage>().Lien_Editer.Click();
            WebBrowser.Current.Page<ProfilPage>().Change_Password.Click();
        }

        [Then(@"Je dois avoir la page de modification de mot de passe")]
        public void ThenJeDoisAvoirLaPageDeModificationDeMotDePasse()
        {
            Assert.That(WebBrowser.Current.Url.Contains(StaticStringClass.URL.ChangePassword));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Erreur de mot de passe

        [When(@"Je me trompe de mot de passe")]
        public void WhenJeMeTrompeDeMotDePasse()
        {
            WebBrowser.Current.Page<ProfilPage>().Login.TypeTextQuickly(StaticStringClass.Connexion.OnlineLogin);
            WebBrowser.Current.Page<ProfilPage>().Password.TypeTextQuickly("test");
            WebBrowser.Current.Page<ProfilPage>().Boutton_Valider.Click();
        }

        [Then(@"Je dois avoir le message d'erreur")]
        public void ThenJeDoisAvoirLeMessageDErreur()
        {
            Assert.That(WebBrowser.Current.ContainsText("L'adresse électronique ou le mot de passe fourni est incorrect."));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Erreur de mot de passe & compte bloqué

        [Given(@"Je vais sur la page connexion")]
        public void GivenJeVaisSurLaPageConnexion()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + StaticStringClass.URL.Connexion);
        }

        [When(@"Je me trompe de mot de passe 6 fois")]
        public void WhenJeMeTrompeDeMotDePasse6Fois()
        {
            WebBrowser.Current.Page<ProfilPage>().Login.TypeTextQuickly(StaticStringClass.Connexion.OnlineLogin);
            for (var i = 0; i < 6; i++)
            {
                WebBrowser.Current.Page<ProfilPage>().Password.TypeTextQuickly("test");
                WebBrowser.Current.Page<ProfilPage>().Boutton_Valider.Click();
            }
        }

        [Then(@"Je dois avoir le message d'erreur adéquat")]
        public void ThenJeDoisAvoirLeMessageDErreurAdequat()
        {
            Assert.That(WebBrowser.Current.ContainsText(Worki.Resources.Validation.ValidationString.AccountLocked));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Inscription tout les messages d'erreur

        [Given(@"Je vais sur la page d'inscription")]
        public void GivenJeVaisSurLaPageDInscription()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + StaticStringClass.URL.Inscription);
        }

        [When(@"Je valide")]
        public void WhenJeValide()
        {
            WebBrowser.Current.Page<ProfilPage>().AcceptCGU.Click();
            WebBrowser.Current.Page<ProfilPage>().Boutton_Valider.Click();
        }

        [Then(@"Je dois avoir le message d'erreur d'inscription")]
        public void ThenJeDoisAvoirLeMessageDErreurDInscription()
        {
            var ie = WebBrowser.Current;

            Assert.That(ie.ContainsText(Worki.Resources.Validation.ValidationString.MustAgreeCGU)
                    && ie.ContainsText("Vous devez saisir un(e) " + "\"Adresse électronique\"")
                    && ie.ContainsText("Vous devez saisir un(e) " + "\"Mot de passe\"")
                    && ie.ContainsText("Vous devez saisir un(e) " + "\"Confirmez le mot de passe\"")
                    && ie.ContainsText("Vous devez saisir un(e) " + "\"Prénom\"")
                    && ie.ContainsText("Vous devez saisir un(e) " + "\"Nom\"")
                    && ie.ContainsText(Worki.Resources.Validation.ValidationString.PleaseSelectProfile)
                    && ie.ContainsText(Worki.Resources.Validation.ValidationString.VerificationLettersWrong)
                );
            WebBrowser.Current.Close();
        }

        #endregion

    }

    #region Profil Page

    public class ProfilPage : Page
    {
        #region Link

        public Link Lien_Editer
        {
            get { return Document.Link(Find.ByText("Profil")); }
        }

        public Link Change_Password
        {
            get { return Document.Link(Find.ByText("Mot de passe")); }
        }

        #endregion

        #region Textfield

            #region General Information

            public TextField LastName
            {
                get { return Document.TextField(Find.ById("Member_MemberMainData_LastName")); }
            }

            public TextField FirstName
            {
                get { return Document.TextField(Find.ById("Member_MemberMainData_FirstName")); }
            }

            public TextField DescriptionField
            {
                get { return Document.TextField(Find.ById("Member_MemberMainData_Description")); }
            }

            public TextField Company
            {
                get { return Document.TextField(Find.ById("Member_MemberMainData_CompanyName")); }
            }

            #endregion

            #region Connexion

            public TextField Login
            {
                get { return Document.TextField(Find.ById("Login")); }
            }

            public TextField Password
            {
                get { return Document.TextField(Find.ById("Password")); }
            }

            #endregion

        #endregion

        #region Button

        public Button Boutton_Valider
        {
            get { return Document.Button(Find.BySelector("input[type='submit']")); }
        }

        #endregion

        #region SelectList

        public SelectList Profil_Type
        {
            get { return Document.SelectList(Find.ById("Member_MemberMainData_Profile")); }
        }

        #endregion

        #region Checkbox

            #region Inscription

            public CheckBox AcceptCGU
            {
                get { return Document.CheckBox(Find.ById("AcceptCGU")); }
            }

            #endregion

        #endregion
    }

    #endregion
}
