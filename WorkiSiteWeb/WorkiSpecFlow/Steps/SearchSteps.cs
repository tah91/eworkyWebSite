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

        #region Recherche Simple

        [Given(@"Je tappe (.*) dans la zone de recherche")]
        public void GivenJeTappeParisDansLaZoneDeRecherche(string address)
        {
            WebBrowser.Current.Page<SearchPage>().Champ_Recherche.TypeText(address);
        }

        [When(@"Je clique sur rechercher dans la page d'acceuil")]
        public void WhenJeCliqueSurRechercherDansLaPageDAcceuil()
        {
            WebBrowser.Current.Page<SearchPage>().Recherche.Click();
        }

        [Then(@"Il doit y avoir plus de (.*) resultats")]
        public void ThenIlDoitYAvoirPlusDe0Resultats(int count)
        {
            var text = WebBrowser.Current.FindText(new System.Text.RegularExpressions.Regex("(.* )lieu"));

            var strings = text.Split(' ');
            var val = 0;
            foreach (var item in strings)
            {
                try
                {
                    val = Convert.ToInt32(item);
                    if (val > 0)
                        break;
                }
                catch
                {
                }
            }
            Assert.That(val, Is.GreaterThanOrEqualTo(count));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Recherche détailé

        [Given(@"Je coche la checkbox prise de courant")]
        public void GivenJeCocheLaCheckboxPriseDeCourant()
        {
            WebBrowser.Current.Page<SearchPage>().CheckBox_OutletGeneral.Click();
        }

        [Given(@"Je coche l'equipement restaurant")]
        public void GivenJeCocheLEquipementRestaurant()
        {
            WebBrowser.Current.Page<SearchPage>().Equipment_Resto.SetAttributeValue("value", "True");
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
            Assert.AreEqual(WebBrowser.Current.Page<SearchPage>().Search_Title.Text, "Etudiant");
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
            Assert.AreEqual(WebBrowser.Current.Page<SearchPage>().Search_Title.Text, "Entrepreneur");
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
            Assert.AreEqual(WebBrowser.Current.Page<SearchPage>().Search_Title.Text, "Grand compte");
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
            Assert.AreEqual(WebBrowser.Current.Page<SearchPage>().Search_Title.Text, "Indépendant");
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
            Assert.AreEqual(WebBrowser.Current.Page<SearchPage>().Search_Title.Text, "Nomade");
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
            Assert.AreEqual(WebBrowser.Current.Page<SearchPage>().Search_Title.Text, "Télétravailleur");
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
                get { return Document.Link(Find.ByText("Etudiant")); }
            }

            public Link Link_Nomade
            {
                get { return Document.Link(Find.ByText("Nomade")); }
            }

            public Link Link_Teletravailleur
            {
                get { return Document.Link(Find.ByText("Télétravailleur")); }
            }

            public Link Link_Entrepreneur
            {
                get { return Document.Link(Find.ByText("Entrepreneur")); }
            }

            public Link Link_GrdCompte
            {
                get { return Document.Link(Find.ByText("Grand compte")); }
            }

            public Link Link_Independant
            {
                get { return Document.Link(Find.ByText("Indépendant")); }
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
    }

    #endregion
}
