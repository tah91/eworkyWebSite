using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using SHDocVw;
using WatiN.Core;
using NUnit.Framework;
using Worki.Data.Models;
namespace Worki.SpecFlow
{
    [Binding]
    class AccueilSteps
    {
        #region Lien Recherche

            [Given(@"Je vais dans la page d'acceuil")]
            public void GivenJeVaisDansLaPageDAcceuil()
            {
                WebBrowser.Current.GoTo(WebBrowser.RootURL);
            }

            [When(@"Je clique sur Recherche")]
            public void WhenJeCliqueSurRecherche()
            {
                WebBrowser.Current.Page<AccueilPage>().Lien_Recherche.Click();
            }

            [Then(@"Je dois arriver sur la page de recherche")]
            public void ThenJeDoisArriverSurLaPageDeRecherche()
            {
                Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + StaticStringClass.URL.Search);
                WebBrowser.Current.Close();
            }

        #endregion

        #region Lien Ajout

            [When(@"Je clique sur Ajout")]
            public void WhenJeCliqueSurAjout()
            {
                WebBrowser.Current.Page<AccueilPage>().Lien_AjoutHeader.Click();
            }

            [Then(@"Je dois arriver sur la page de Ajout")]
            public void ThenJeDoisArriverSurLaPageDeAjout()
            {
                Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + StaticStringClass.URL.AddSpace);
                WebBrowser.Current.Close();
            }

        #endregion

        #region Lien Plus de critères

            [When(@"Je clique sur plus de critères")]
            public void WhenJeCliqueSurPlusDeCriteres()
            {
                WebBrowser.Current.Page<AccueilPage>().Plus_Criteres.Click();
            }

        #endregion

        #region Acceuil Recherche Erreur

            [When(@"Je clique sur Rechercher")]
            public void WhenJeCliqueSurRechercher()
            {
                WebBrowser.Current.Page<AccueilPage>().Bouton_Recherche.Click();
            }

        #endregion

        #region Recherche dans Accueil Salon d'affaire

            [Given(@"Je selectionne Salon d'affaire")]
            public void GivenJeSelectionneSalonDAffaire()
            {
                WebBrowser.Current.Page<AccueilPage>().Type_Espace.Select(Worki.Resources.Models.Localisation.LocalisationFeatures.BuisnessLounge);
            }

        #endregion

        #region A la une 
        
            [Then(@"Je dois avoir A la une")]
            public void ThenJeDoisAvoirALaUne()
            {
                WebBrowser.Current.Div("mycarousel");
                WebBrowser.Current.Close();
            }

        #endregion
    }

    #region Accueil Page

    public class AccueilPage : Page
    {
        #region Button

        public Button Boutton_Connexion
        {
            get { return Document.Button(Find.BySelector("a[href='/compte/connexion']")); }
        }

        public Button Bouton_Recherche
        {
            get { return Document.Button(Find.ById("btn_searchIndex")); }
        }

        #endregion

        #region Link

        public Link Lien_Recherche
        {
            get { return Document.Link(Find.BySelector("a[href='/lieu-de-travail/recherche']")); }
        }

        public Link Lien_RechercheFooter
        {
            get { return Document.Link(Find.BySelector("div[class^='footer'] a[href^='/recherche/recherche-lieu-travail']")); }
        }

        public Link Lien_AjoutHeader
        {
            get { return Document.Link(Find.ByText("Ajout")); }
        }

        public Link Lien_AjoutBody
        {
            get { return Document.Link(Find.BySelector("div[class^='bottom'] a[href^='/lieu-de-travail/ajouter']")); }
        }

        public Link Lien_AjoutFooter
        {
            get { return Document.Link(Find.BySelector("div[class^='footer'] a[href^='/lieu-de-travail/ajouter']")); }
        }

        public Link Lien_Contact
        {
            get { return Document.Link(Find.ByText("Contact")); }
        }

        public Link Lien_Blog
        {
            get { return Document.Link(Find.ByText("Blog")); }
        }

        public Link Lien_WhoWeAre
        {
            get { return Document.Link(Find.ByText("Qui sommes-nous ?")); }
        }

        public Link Lien_FAQ
        {
            get { return Document.Link(Find.ByText("FAQ")); }
        }

        public Link Lien_CGU
        {
            get { return Document.Link(Find.ByText("CGU")); }
        }

        public Link Lien_Presse
        {
            get { return Document.Link(Find.ByText("Presse")); }
        }

        public Link Lien_Jobs
        {
            get { return Document.Link(Find.ByText("Jobs")); }
        }

        public Link Lien_ALaUne
        {
            get { return Document.Link(Find.BySelector("div[class='imageDescription'] a[href^='/lieu-de-travail/details/']")); }
        }

        public Link Lien_MentionLegal
        {
            get { return Document.Link(Find.ByText("Mentions légales")); }
        }

        public Link Lien_MonProfil
        {
            get { return Document.Link(Find.ByText("Mon profil")); }
        }

        public Link Lien_Deconnexion
        {
            get { return Document.Link(Find.ByText("Déconnexion")); }
        }

        public Link Lien_Administrateur
        {
            get { return Document.Link(Find.ByText("Administrateur")); }
        }

        public Link Lien_Profil
        {
            get { return Document.Link(Find.BySelector("a[href^='/profil/details/']")); }
        }

        public Link Plus_Criteres
        {
            get { return Document.Link(Find.ByText("Plus de critères")); }
        }

        #endregion

        #region TextField

        public TextField Champ_Recherche
        {
            get { return Document.TextField(Find.ById("Criteria_Place")); }
        }

        #endregion

        #region SelectList

        public SelectList Type_Espace
        {
            get { return Document.SelectList(Find.ById("Criteria_OfferData_Type")); }
        }

        #endregion
    }

    #endregion
}
