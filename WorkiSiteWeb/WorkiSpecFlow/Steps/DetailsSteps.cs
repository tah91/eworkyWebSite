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
            /*
            WebBrowser.Current.TextField(Find.ByName("RatingPrice")).Value ="4";
            WebBrowser.Current.TextField(Find.ByName("RatingWifi")).Value="3";
            WebBrowser.Current.TextField(Find.ByName("RatingDispo")).Value = "4";
            WebBrowser.Current.TextField(Find.ByName("RatingWelcome")).Value = "3";
             */
            WebBrowser.Current.Page<DetailPage>().CommentRate(4, 3, 4, 3);
        }

        [When(@"Je poste un commentaire")]
        public void WhenJePosteUnCommentaire()
        {
            Random myRand = new Random();
            myStatiqueRand = myRand.Next().ToString();
            // WebBrowser.Current.TextField(Find.ById("Post")).TypeTextQuickly(myStatiqueRand);
            // WebBrowser.Current.Button(Find.ByValue("Envoyer")).ClickNoWait();
            WebBrowser.Current.Page<DetailPage>().Msg.TypeText(myStatiqueRand);
            WebBrowser.Current.Page<DetailPage>().Boutton_Envoyer.Click();
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
            // WebBrowser.Current.Link(Find.ById("IdForTest")).Click();
            WebBrowser.Current.Page<DetailPage>().Lien_Profil.Click();
        }

        #endregion

    }

    public class DetailPage : Page
    {
        public Button Boutton_Envoyer
        {
            get { return Document.Button(Find.ByValue("Envoyer")); }
        }

        public Link Lien_Profil
        {
            get { return Document.Link(Find.BySelector("a[href^='/profil/details/']")); }
        }

        public TextField RatingPrice
        {
            get { return Document.TextField(Find.ByName("RatingPrice")); }
        }

        public TextField RatingWifi
        {
            get { return Document.TextField(Find.ByName("RatingWifi")); }
        }

        public TextField RatingDispo
        {
            get { return Document.TextField(Find.ByName("RatingDispo")); }
        }

        public TextField RatingWelcome
        {
            get { return Document.TextField(Find.ByName("RatingWelcome")); }
        }

        public TextField Msg
        {
            get { return Document.TextField(Find.ById("Post")); }
        }
        
        public Link Lien_Editer
        {
            get { return Document.Link(Find.BySelector("a[href^='/lieu-de-travail/editer/']")); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="price"></param>
        /// <param name="wifi"></param>
        /// <param name="dispo"></param>
        /// <param name="welcome"></param>
        public void CommentRate(int price, int wifi, int dispo, int welcome)
        {
            WebBrowser.Current.Page<DetailPage>().RatingPrice.Value = price.ToString();
            WebBrowser.Current.Page<DetailPage>().RatingWifi.Value = wifi.ToString();
            WebBrowser.Current.Page<DetailPage>().RatingDispo.Value = dispo.ToString();
            WebBrowser.Current.Page<DetailPage>().RatingWelcome.Value = welcome.ToString();
        }
    }
}
