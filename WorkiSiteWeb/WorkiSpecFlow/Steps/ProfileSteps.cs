using System;
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
        public int Id;

        #region Aller sur son profil

        [When(@"Je clique sur mon profil")]
        public void WhenJeCliqueSurMonProfil()
        {
            WebBrowser.Current.Page<AccueilPage>().Lien_MonProfil.Click();
            var tab = WebBrowser.Current.Url.Split('/');
            int.TryParse(tab[tab.Length - 1], out Id);
        }

        [Then(@"Je dois arriver sur mon profil")]
        public void ThenJeDoisArriverSurMonProfil()
        {
            Assert.IsTrue(Id != 0);
            Assert.IsTrue(WebBrowser.Current.Url.Contains(StaticStringClass.URL.Dashboard + Id));
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
            WebBrowser.Current.Page<ProfilPage>().Profil_Type.Select("Travailleur nomade");
            WebBrowser.Current.Page<ProfilPage>().LastName.TypeText("AdminE");
            WebBrowser.Current.Page<ProfilPage>().FirstName.TypeText("AdminE");
            WebBrowser.Current.Page<ProfilPage>().DescriptionField.TypeText("Ceci est un Test auto Made by Mika");
            WebBrowser.Current.Page<ProfilPage>().Company.TypeText("eWorky");
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
            Assert.IsTrue(ie.ContainsText("AdminE AdminE") && ie.ContainsText("Travailleur nomade")
                        && ie.ContainsText("Ceci est un Test auto Made by Mika") && ie.ContainsText("eWorky"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Reinitialiser le profil

        [When(@"Je remet les champs de base")]
        public void WhenJeRemetLesChampsDeBase()
        {
            WebBrowser.Current.Page<ProfilPage>().Profil_Type.Select("Autre");
            WebBrowser.Current.Page<ProfilPage>().LastName.TypeText("Admin");
            WebBrowser.Current.Page<ProfilPage>().FirstName.TypeText("Admin");
            WebBrowser.Current.Page<ProfilPage>().DescriptionField.TypeText("");
            WebBrowser.Current.Page<ProfilPage>().Company.TypeText("Administrateur");
        }

        [Then(@"Le profil est reinitialiser")]
        public void ThenLeProfilEstReinitialiser()
        {
            var ie = WebBrowser.Current;
            Assert.IsTrue(ie.ContainsText("Admin Admin") && ie.ContainsText("Autre")
                        && !ie.ContainsText("Ceci est un Test auto Made by Mika") && ie.ContainsText("Administrateur"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Changer le Mot de passe

        [When(@"Je clique sur Modifier mon mot de passe")]
        public void WhenJeCliqueSurModifierMonMotDePasse()
        {
            WebBrowser.Current.Page<ProfilPage>().Change_Password.Click();
        }

        [Then(@"Je dois avoir la page de modification de mot de passe")]
        public void ThenJeDoisAvoirLaPageDeModificationDeMotDePasse()
        {
            Assert.IsTrue(Id != 0);
            Assert.That(WebBrowser.Current.Url.Contains(StaticStringClass.URL.ChangePassword + Id));
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
            get { return Document.Link(Find.ByText("Editer mon profil")); }
        }

        public Link Change_Password
        {
            get { return Document.Link(Find.ByText("Modifier mon mot de passe")); }
        }

        #endregion

        #region Textfield

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
    }

    #endregion
}
