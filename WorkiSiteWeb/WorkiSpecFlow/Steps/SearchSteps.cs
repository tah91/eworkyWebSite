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
    public class SearchSteps
    {
        #region Communs

        [Given(@"Je vais dans la page Recherche")]
        public void GivenJeVaisDansLaPageRecherche()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + StaticStringClass.URL.Search);
            WebBrowser.Current.AutoClose = true;
        }

        [When(@"Je clique sur rechercher")]
        public void WhenJeCliqueSurRechercher()
        {
            WebBrowser.Current.Page<SearchPage>().Recherche.Click();
        }

        #endregion

        #region Recherche Simple

        [Given(@"Je tappe (.*) dans la zone de recherche")]
        public void GivenJeTappeParisDansLaZoneDeRecherche(string address)
        {
            WebBrowser.Current.Page<SearchPage>().Champ_Recherche.TypeTextQuickly(address);
        }

        [When(@"Je clique sur rechercher dans la page d'acceuil")]
        public void WhenJeCliqueSurRechercherDansLaPageDAcceuil()
        {
            WebBrowser.Current.Page<SearchPage>().Recherche.Click();
        }

        [Then(@"Il doit y avoir plus de 1 resultats")]
        public void ThenIlDoitYAvoirPlusDe1Resultats()
        {
            Assert.True(WebBrowser.Current.Page<SearchPage>().Locs.Count >= 1);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Description Etudiant

        [When(@"Je clique sur Etudiant")]
        public void WhenJeCliqueSurEtudiant()
        {
            WebBrowser.Current.Page<SearchPage>().Link_Etudiant.Click();
        }

        [Then(@"Je dois avoir la description Etudiant")]
        public void ThenJeDoisAvoirLaDescriptionEtudiant()
        {
            Assert.AreEqual(WebBrowser.Current.Page<SearchPage>().Search_Title.Text, Worki.Resources.Models.Profile.Profile.Student);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Description Entrepreneur

        [When(@"Je clique sur Entrepreneur")]
        public void WhenJeCliqueSurEntrepreneur()
        {
            WebBrowser.Current.Page<SearchPage>().Link_Entrepreneur.Click();
        }

        [Then(@"Je dois avoir la description Entrepreneur")]
        public void ThenJeDoisAvoirLaDescriptionEntrepreneur()
        {
            Assert.AreEqual(WebBrowser.Current.Page<SearchPage>().Search_Title.Text, Worki.Resources.Views.Search.SearchString.Entrepreneur);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Description GrandCompte

        [When(@"Je clique sur GrandCompte")]
        public void WhenJeCliqueSurGrandCompte()
        {
            WebBrowser.Current.Page<SearchPage>().Link_GrdCompte.Click();
        }

        [Then(@"Je dois avoir la description GrandCompte")]
        public void ThenJeDoisAvoirLaDescriptionGrandCompte()
        {
            Assert.AreEqual(WebBrowser.Current.Page<SearchPage>().Search_Title.Text, Worki.Resources.Models.Profile.Profile.Company);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Description Indépendant

        [When(@"Je clique sur Indépendant")]
        public void WhenJeCliqueSurIndependant()
        {
            WebBrowser.Current.Page<SearchPage>().Link_Independant.Click();
        }

        [Then(@"Je dois avoir la description Indépendant")]
        public void ThenJeDoisAvoirLaDescriptionIndependant()
        {
            Assert.AreEqual(WebBrowser.Current.Page<SearchPage>().Search_Title.Text, Worki.Resources.Views.Search.SearchString.Independant);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Description Nomade

        [When(@"Je clique sur Nomade")]
        public void WhenJeCliqueSurNomade()
        {
            WebBrowser.Current.Page<SearchPage>().Link_Nomade.Click();
        }

        [Then(@"Je dois avoir la description Nomade")]
        public void ThenJeDoisAvoirLaDescriptionNomade()
        {
            Assert.AreEqual(WebBrowser.Current.Page<SearchPage>().Search_Title.Text, Worki.Resources.Views.Search.SearchString.Nomad);
            WebBrowser.Current.Close();
        }
        #endregion

        #region Description Teletravailleur

        [When(@"Je clique sur Teletravailleur")]
        public void WhenJeCliqueSurTeletravailleur()
        {
            WebBrowser.Current.Page<SearchPage>().Link_Teletravailleur.Click();
        }

        [Then(@"Je dois avoir la description Teletravailleur")]
        public void ThenJeDoisAvoirLaDescriptionTeletravailleur()
        {
            Assert.AreEqual(WebBrowser.Current.Page<SearchPage>().Search_Title.Text, Worki.Resources.Models.Profile.Profile.Teleworker);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Lieux A la Une

        [When(@"Je clique sur Plus de critère")]
        public void WhenJeCliqueSurPlusDeCritere()
        {
            WebBrowser.Current.Page<AccueilPage>().Lien_Recherche.Click();
        }

        [Then(@"Il doit y avoir des lieux dans le bloc A la Une")]
        public void ThenIlDoitYAvoirDesLieuxDansLeBlocALaUne()
        {
            Assert.That(WebBrowser.Current.Div(Find.BySelector("li[jcarouselindex='1']")) != null);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Faire Défiler les pages de résultats

        int count = 0;

        [When(@"Je fais défiler les pages")]
        public void WhenJeFaisDefilerLesPages()
        {
            while (WebBrowser.Current.ContainsText(">"))
            {
                WebBrowser.Current.Link(Find.ByText(">")).Click();
                count++;
                if (count > 4)
                    break;
            }
        }

        [Then(@"Je dois avoir parcouru les pages")]
        public void ThenJeDoisAvoirParcouruLesPages()
        {
            Assert.IsTrue(count != 0);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Recherche par nom

        [Given(@"Je vais dans la page Recherche par nom")]
        public void GivenJeVaisDansLaPageRechercheParNom()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + StaticStringClass.URL.SearchByName);
        }

        public string venue_name = "test";

        [Given(@"Je tappe test dans la barre de recherche")]
        public void GivenJeTappeTestDansLaBarreDeRecherche()
        {
            WebBrowser.Current.TextField(Find.ById("Criteria_LocalisationData_Name")).TypeTextQuickly(venue_name);
        }

        [Then(@"Tout les résultats doivent contenir le mot cherché")]
        public void ThenToutLesResultatsDoiventContenirLeMotCherche()
        {
            bool b = true;

            foreach (var item in WebBrowser.Current.Page<SearchPage>().Locs)
            {
                if (!item.Text.Contains(venue_name))
                {
                    b = false;
                    break;
                }
            }
            Assert.IsTrue(b);
            WebBrowser.Current.Close();
        }

        #endregion
    }

    #region Search Page

    public class SearchPage : Page
    {
        #region Div

        public Div Search_Title
        {
            get { return Document.Div(Find.BySelector("div[class*='titleDiv']")); }
        }

        #endregion

        #region Link

            #region Type de Recherche

            public Link Type_Lieu
            {
                get { return Document.Link(Find.ByText("Rechercher par type de lieu")); }
            }

            public Link Type_Prestation
            {
                get { return Document.Link(Find.ByText("Rechercher par type de prestation")); }
            }

            #endregion

            #region Acces Direct

            public Link Link_Etudiant
            {
                get { return Document.Link(Find.ByText(Worki.Resources.Models.Profile.Profile.Student)); }
            }

            public Link Link_Nomade
            {
                get { return Document.Link(Find.ByText(Worki.Resources.Views.Search.SearchString.Nomad)); }
            }

            public Link Link_Teletravailleur
            {
                get { return Document.Link(Find.ByText(Worki.Resources.Models.Profile.Profile.Teleworker)); }
            }

            public Link Link_Entrepreneur
            {
                get { return Document.Link(Find.ByText(Worki.Resources.Views.Search.SearchString.Entrepreneur)); }
            }

            public Link Link_GrdCompte
            {
                get { return Document.Link(Find.ByText(Worki.Resources.Models.Profile.Profile.Company)); }
            }

            public Link Link_Independant
            {
                get { return Document.Link(Find.ByText(Worki.Resources.Views.Search.SearchString.Independant)); }
            }

            #endregion

            #region Header

            public Link Lien_AccueilHeader
            {
                get { return Document.Link(Find.ByText("Accueil")); }
            }

            public Link Lien_AjoutHeader
            {
                get { return Document.Link(Find.ByText("Ajout")); }
            }

            #endregion

            #region Footer

            public Link Lien_RechercheFooter
            {
                get { return Document.Link(Find.BySelector("div[class^='footer'] a[href^='/recherche/recherche-lieu-travail']")); }
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
                get { return Document.Link(Find.BySelector("div[class=imageDescription] a[href^='/lieu-de-travail/details/']")); }
            }

            public Link Lien_MentionLegal
            {
                get { return Document.Link(Find.ByText("Mentions légales")); }
            }

            #endregion

            #region Profil/Deconnexion/Admin

            public Link Lien_MonProfil
            {
                get { return Document.Link(Find.ByText("Mon Profil")); }
            }

            public Link Lien_Deconnexion
            {
                get { return Document.Link(Find.ByText("Déconnexion")); }
            }

            public Link Lien_Administrateur
            {
                get { return Document.Link(Find.ByText("Administrateur")); }
            }

            #endregion

        #endregion

        #region TextField

        public TextField Champ_Recherche
        {
            get { return Document.TextField(Find.ById("Criteria_Place")); }
        }

        public TextField Equipment_Wifi
        {
            get { return Document.TextField(Find.ById("Wifi_Free-General")); }
        }

        public TextField Equipment_Resto
        {
            get { return Document.TextField(Find.ById("f_Restauration")); }
        }

        public TextField Equipment_Cafe
        {
            get { return Document.TextField(Find.ById("Coffee-General")); }
        }

        public TextField Equipment_Parking
        {
            get { return Document.TextField(Find.ById("Parking-General")); }
        }

        public TextField Equipment_Handicap
        {
            get { return Document.TextField(Find.ById("Handicap-General")); }
        }

        #endregion

        #region Button

        public Button Recherche
        {
            get { return Document.Button(Find.ByValue("Rechercher")); }
        }

        #endregion

        #region CheckBox

            #region Type de Lieux

            public CheckBox CheckBox_Tous
            {
                get { return Document.CheckBox(Find.ById("Criteria_Everything")); }
            }

            public CheckBox CheckBox_Spot_Wifi
            {
                get { return Document.CheckBox(Find.ById("Criteria_SpotWifi")); }
            }

            public CheckBox CheckBox_Cafe_Restaurant
            {
                get { return Document.CheckBox(Find.ById("Criteria_CoffeeResto")); }
            }

            public CheckBox CheckBox_Biblioteque_Musee
            {
                get { return Document.CheckBox(Find.ById("Criteria_Biblio")); }
            }

            public CheckBox CheckBox_Espace_Public
            {
                get { return Document.CheckBox(Find.ById("Criteria_PublicSpace")); }
            }

            public CheckBox CheckBox_Voyageur
            {
                get { return Document.CheckBox(Find.ById("Criteria_TravelerSpace")); }
            }

            public CheckBox CheckBox_Hotel
            {
                get { return Document.CheckBox(Find.ById("Criteria_Hotel")); }
            }

            public CheckBox CheckBox_Telecentre
            {
                get { return Document.CheckBox(Find.ById("Criteria_Telecentre")); }
            }

            public CheckBox CheckBox_BuisnessCenter
            {
                get { return Document.CheckBox(Find.ById("Criteria_BuisnessCenter")); }
            }

            public CheckBox CheckBox_CoworkingSpace
            {
                get { return Document.CheckBox(Find.ById("Criteria_CoworkingSpace")); }
            }

            public CheckBox CheckBox_WorkingHotel
            {
                get { return Document.CheckBox(Find.ById("Criteria_WorkingHotel")); }
            }

            public CheckBox CheckBox_PrivateArea
            {
                get { return Document.CheckBox(Find.ById("Criteria_PrivateArea")); }
            }

            #endregion

            #region Caractéristiques

            public CheckBox CheckBox_OutletGeneral
            {
                get { return Document.CheckBox(Find.ById("f_Outlet")); }
            }

            public CheckBox CheckBox_FastInternet
            {
                get { return Document.CheckBox(Find.ById("FastInternet-General")); }
            }

            public CheckBox CheckBox_AC
            {
                get { return Document.CheckBox(Find.ById("AC-General")); }
            }

            #endregion

        #endregion

        #region List

        public List<Element> Locs
        {
            get { return Document.Elements.Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.Contains("contentBlock resultItem")).ToList(); }
        }

        #endregion
    }

    #endregion
}
