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
    public class DetailsSteps
    {
        public String myStatiqueRand;
        //[Given(@"Je tappe (.*) dans la zone de recherche")]
        //public void GivenJeTappeParisDansLaZoneDeRecherche(string address)
        //{
        //    WebBrowser.Current.TextField(Find.ByName("Criteria.Lieu")).TypeText(address);
        //}

        #region 
        [When(@"Je clique sur la fiche (.*)")]
        public void WhenJeCliqueSurLaFicheLeBistrotMarguerite(string lieu)
        {
            WebBrowser.Current.Link(Find.ByText(lieu)).ClickNoWait();
            System.Threading.Thread.Sleep(1000);
        }

        [When(@"Je met une note")]
        public void WhenJePosteUneNote()
        {
            WebBrowser.Current.TextField(Find.ByName("RatingPrice")).Value ="4";
            WebBrowser.Current.TextField(Find.ByName("RatingWifi")).Value="3";
            WebBrowser.Current.TextField(Find.ByName("RatingDispo")).Value = "4";
            WebBrowser.Current.TextField(Find.ByName("RatingWelcome")).Value = "3";
        }

        [When(@"Je poste un commentaire")]
        public void WhenJePosteUnCommentaire()
        {
            Random myRand = new Random();
            myStatiqueRand = myRand.Next().ToString();
            WebBrowser.Current.TextField(Find.ById("Post")).TypeTextQuickly(myStatiqueRand);
            WebBrowser.Current.Button(Find.ByValue("Envoyer")).ClickNoWait();
        }



        [Then(@"Je dois retrouver la note")]
        public void ThenJeDoisRetrouverLaNote()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Je dois retrouver le commentaire")]
        public void ThenJeDoisRetrouverLeCommentaire()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText(myStatiqueRand));
        }

        #endregion

        #region

        [When(@"Je clique sur le profil")]
        public void WhenJeCliqueSurLeProfil()
        {
            WebBrowser.Current.Link(Find.ById("IdForTest")).Click();
        }

        #endregion

    }
}
