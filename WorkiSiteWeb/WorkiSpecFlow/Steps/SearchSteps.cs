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
        public void ThenIlDoitYAvoirPlusDe200Resultats(int count)
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
            Assert.That(val, Is.GreaterThan(count));
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

        [Given(@"Je vais dans la page Recherche")]
        public void GivenJeVaisDansLaPageRecherche()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "lieu-de-travail/recherche");
        }

        [When(@"Je clique sur rechercher")]
        public void WhenJeCliqueSurRechercher()
        {
            WebBrowser.Current.Page<SearchPage>().Recherche.Click();
        }

        #region Description Etudiant

        [When(@"Je clique sur Etudiant")]
        public void WhenJeCliqueSurEtudiant()
        {
            WebBrowser.Current.Page<SearchPage>().Link_Etudiant.Click();
        }

        [Then(@"Je dois avoir la description Etudiant")]
        public void ThenJeDoisAvoirLaDescriptionEtudiant()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Vous êtes étudiant"));
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
            Assert.IsTrue(WebBrowser.Current.ContainsText("Vous êtes entrepreneur"));
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
            Assert.IsTrue(WebBrowser.Current.ContainsText("grand groupe"));
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
            Assert.IsTrue(WebBrowser.Current.ContainsText("profession libérale"));
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
            Assert.IsTrue(WebBrowser.Current.ContainsText("Vous êtes souvent en déplacement"));
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
            Assert.IsTrue(WebBrowser.Current.ContainsText("Vous êtes télétravailleur"));
        }

        #endregion
    }

    public class SearchPage : Page
    {
        public Link Type_Lieu
        {
            get { return Document.Link(Find.ByText("Rechercher par type de lieu")); }
        }

        public Link Type_Prestation
        {
            get { return Document.Link(Find.ByText("Rechercher par type de prestation")); }
        }

        public TextField Champ_Recherche
        {
            get { return Document.TextField(Find.ById("Criteria_Place")); }
        }

        public Button Recherche
        {
            get { return Document.Button(Find.ByValue("Rechercher")); }
        }

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

        public CheckBox CheckBox_OutletGeneral
        {
            get { return Document.CheckBox(Find.ById("Outlet-General")); }
        }

        public CheckBox CheckBox_FastInternet
        {
            get { return Document.CheckBox(Find.ById("FastInternet-General")); }
        }

        public CheckBox CheckBox_AC
        {
            get { return Document.CheckBox(Find.ById("AC-General")); }
        }

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

        public TextField Equipment_Wifi
        {
            get { return Document.TextField(Find.ById("Wifi_Free-General")); }
        }

        public TextField Equipment_Resto
        {
            get { return Document.TextField(Find.ById("Restauration-General")); }
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

        public Link Lien_AccueilHeader
        {
            get { return Document.Link(Find.ByText("Accueil")); }
        }

        public Link Lien_AjoutHeader
        {
            get { return Document.Link(Find.ByText("Ajout")); }
        }

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
    }
}
