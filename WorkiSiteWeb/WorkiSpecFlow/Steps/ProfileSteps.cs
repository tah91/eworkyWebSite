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
        public String myStatiqueRand;

        #region Aller sur son profil

        [When(@"Je clique sur mon pofil")]
        public void WhenJeCliqueSurMonPofil()
        {
            // WebBrowser.Current.Link(Find.ByText("Mon profil")).ClickNoWait();
            // System.Threading.Thread.Sleep(1000);
            WebBrowser.Current.Page<AccueilPage>().Lien_MonProfil.Click();
        }

        [Then(@"Je dois arriver sur mon profil")]
        public void ThenJeDoisArriverSurMonProfil()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Ambiance de travail recherchée"));
        }

        #endregion

        #region

        [When(@"Je clique sur Editer Profil")]
        public void WhenJeCliqueSurEditerProfil()
        {
            // WebBrowser.Current.Link(Find.ByText("Editer mon profil")).Click();
            WebBrowser.Current.Page<ProfilPage>().Lien_Editer.Click();
        }


        [When(@"Je change quelques champs")]
        public void WhenJeChangeQuelquesChamps()
        {
            Random myRand = new Random();
            myStatiqueRand = myRand.Next().ToString();
            WebBrowser.Current.Page<ProfilPage>().Description.TypeText(myStatiqueRand);
      
        }

        [When(@"Je valide le formulaire du profil")]
        public void WhenJeValideLeFormulaireDuProfil()
        {
            // WebBrowser.Current.Button(Find.ByValue("Valider")).ClickNoWait();
            // System.Threading.Thread.Sleep(1000);
            WebBrowser.Current.Page<ProfilPage>().Boutton_Valider.Click();
        }

        [Then(@"Je dois avoir les modifications faites")]
        public void ThenJeDoisAvoirLesModificationsFaites()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText(myStatiqueRand));
        }

        #endregion
    }

    public class ProfilPage : Page
    {
        public Link Lien_Editer
        {
            get { return Document.Link(Find.BySelector("a[href^='/profil/editer/']")); }
        }

        public TextField Description
        {
            get { return Document.TextField(Find.ById("Member_MemberMainData_Description")); }
        }

        public Button Boutton_Valider
        {
            get { return Document.Button(Find.ByValue("Valider")); }
        }        
    }
}
