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
        #region Commun
        
        [Given(@"Je vais dans la page Admin")]
        public void GivenJeVaisDansLaPageAdmin()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "Admin");
        }

        [When(@"Je clique sur detail")]
        public void WhenJeCliqueSurDetail()
        {
            WebBrowser.Current.Page<DetailPage>().Detail.Click();
        }

        #endregion

        #region Je poste un commentaire

        [When(@"Je met une note et un commentaire")]
        public void WhenJeMetUneNoteEtUnCommentaire()
        {
            WebBrowser.Current.Page<DetailPage>().CommentRate(4, 4, 4, 4);
            WebBrowser.Current.Page<DetailPage>().Msg.TypeText("Ceci est un test auto Made by Mika");
            WebBrowser.Current.Page<DetailPage>().Boutton_Envoyer.Click();
        }

        [Then(@"Je dois retrouver le commentaire et la note")]
        public void ThenJeDoisRetrouverLeCommentaireEtLaNote()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Ceci est un test auto Made by Mika"));
            Assert.That(int.Parse(WebBrowser.Current.Page<DetailPage>().Rateit.GetAttributeValue("data-rateit-value")) == 4);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Profil dans commentaire

        [When(@"Je clique sur le profil")]
        public void WhenJeCliqueSurLeProfil()
        {
            WebBrowser.Current.Page<DetailPage>().Lien_Profil.Click();
        }

        [When(@"Je clique sur Mes commentaires")]
        public void WhenJeCliqueSurMesCommentaires()
        {
            WebBrowser.Current.Page<DetailPage>().My_Comments.Click();
        }

        [Then(@"Je dois retrouver mon commentaire")]
        public void ThenJeDoisRetrouverMonCommentaire()
        {
            Assert.That(WebBrowser.Current.ContainsText("Ceci est un test auto Made by Mika"));
            WebBrowser.Current.Close();
        }


        #endregion

        #region Je supprime le commentaire

        [When(@"Je supprime mon commentaire")]
        public void WhenJeSupprimeMonCommentaire()
        {
            WebBrowser.Current.Link(Find.ByText("Effacer le commentaire")).Click();
        }

        [Then(@"Le commentaire a été supprimé")]
        public void ThenLeCommentaireAEteSupprime()
        {
            Assert.That(!WebBrowser.Current.ContainsText("Effacer le commentaire"));
        }

        #endregion
    }

    #region Detail Page

    public class DetailPage : Page
    {
        #region Button

        public Button Boutton_Envoyer
        {
            get { return Document.Button(Find.ByValue("Envoyer")); }
        }

        #endregion

        #region Link

        public Link Lien_Profil
        {
            get { return Document.Link(Find.ByText("Mon profil")); }
        }

        public Link My_Comments
        {
            get { return Document.Link(Find.ByText("Mes commentaires")); }
        }

        public Link Detail
        {
            get { return Document.Link(Find.ByText("Détails")); }
        }

        #endregion

        #region TextField

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

        #endregion

        #region Div

        public Div Rateit
        {
            get { return Document.Div(Find.BySelector("div[class='profilComment'] div[class='rateit']")); }
        }

        #endregion

        /// <summary>
        /// Function which rate the comment
        /// </summary>
        /// <param name="price">Price Rate</param>
        /// <param name="wifi">Wifi Rate</param>
        /// <param name="dispo">Dispo Rate</param>
        /// <param name="welcome">Welcome Rate</param>
        public void CommentRate(int price, int wifi, int dispo, int welcome)
        {
            WebBrowser.Current.Page<DetailPage>().RatingPrice.Value = price.ToString();
            WebBrowser.Current.Page<DetailPage>().RatingWifi.Value = wifi.ToString();
            WebBrowser.Current.Page<DetailPage>().RatingDispo.Value = dispo.ToString();
            WebBrowser.Current.Page<DetailPage>().RatingWelcome.Value = welcome.ToString();
        }
    }

    #endregion
}
