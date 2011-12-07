using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using SHDocVw;
using WatiN.Core;
using NUnit.Framework;
using Worki.Data.Models;
using Worki.Infrastructure.Helpers;

namespace Worki.SpecFlow
{
    [Binding]
    class OfferSteps
    {
        #region Aller sur la page BackOffice

        [When(@"Je clique sur bo")]
        public void WhenJeCliqueSurBo()
        {
            WebBrowser.Current.Page<BackOfficePage>().Links.Single(x => x.Text == "Espace gérant").Click();
        }

        [Then(@"Je dois arriver sur la page de backoffice")]
        public void ThenJeDoisArriverSurLaPageDeBackoffice()
        {
            Assert.IsTrue(WebBrowser.Current.Url.Contains(StaticStringClass.URL.BOHome));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Aller sur la page backoffice Client

        [When(@"Je clique sur Client")]
        public void WhenJeCliqueSurClient()
        {
            WebBrowser.Current.Page<BackOfficePage>().Links.Single(x => x.Text == "Clients").Click();
        }

        [Then(@"Je dois arriver sur la page de backoffice client")]
        public void ThenJeDoisArriverSurLaPageDeBackofficeClient()
        {
            Assert.IsTrue(WebBrowser.Current.Url.Contains(StaticStringClass.URL.BOClient));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Aller sur la page backoffice Profil

        [When(@"Je clique sur Profil")]
        public void WhenJeCliqueSurProfil()
        {
            WebBrowser.Current.Page<BackOfficePage>().Links.Single(x => x.Text == "Options").Click();
        }

        [Then(@"Je dois arriver sur la page de backoffice profil")]
        public void ThenJeDoisArriverSurLaPageDeBackofficeProfil()
        {
            Assert.IsTrue(WebBrowser.Current.Url.Contains(StaticStringClass.URL.BOProfil));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Aller sur la page backoffice Espaces de travail

        [When(@"Je clique sur Espaces de travail")]
        public void WhenJeCliqueSurEspacesDeTravail()
        {
            WebBrowser.Current.Page<BackOfficePage>().Links.Single(x => x.Text == "Mes espaces en ligne").Click();
        }

        [Then(@"Je dois arriver sur la page de backoffice Espaces de travail")]
        public void ThenJeDoisArriverSurLaPageDeBackofficeEspacesDeTravail()
        {
            Assert.IsTrue(WebBrowser.Current.Url.Contains(StaticStringClass.URL.BOLoc));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Aller sur la page backoffice Booking

        [When(@"Je clique sur Reservation en cours")]
        public void WhenJeCliqueSurReservationEnCours()
        {
            WebBrowser.Current.Page<BackOfficePage>().Links.Single(x => x.Text == "Demandes de réservation").Click();
        }

        [Then(@"Je dois arriver sur la page de backoffice Booking")]
        public void ThenJeDoisArriverSurLaPageDeBackofficeBooking()
        {
            Assert.IsTrue(WebBrowser.Current.Url.Contains(StaticStringClass.URL.BOBooking));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Aller sur la page backoffice Devis

        [When(@"Je clique sur Demande de devis")]
        public void WhenJeCliqueSurDemandeDeDevis()
        {
            WebBrowser.Current.Page<BackOfficePage>().Links.Single(x => x.Text == "Demandes de devis").Click();
        }

        [Then(@"Je dois arriver sur la page de backoffice Devis")]
        public void ThenJeDoisArriverSurLaPageDeBackofficeDevis()
        {
            Assert.IsTrue(WebBrowser.Current.Url.Contains(StaticStringClass.URL.BOQuotation));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Je dois avoir plusieurs [reservations en cours/demande de devis]

        [Then(@"Je dois avoir des résultats")]
        public void ThenJeDoisAvoirDesResultats()
        {
            Assert.True(WebBrowser.Current.Page<BackOfficePage>().BookItems.Count > 0);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Je dois avoir des résultats de lieu

        [Then(@"Je dois avoir des résultats de lieu")]
        public void ThenJeDoisAvoirDesResultatsDeLieu()
        {
            Assert.True(WebBrowser.Current.Page<BackOfficePage>().LocItem.Count > 0);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Je dois avoir des offres sur un des lieux

        [When(@"Je clique sur un des lieux")]
        public void WhenJeCliqueSurUnDesLieux()
        {
            WebBrowser.Current.Links.First(x => x.GetAttributeValue("href").Contains("/Backoffice/Localisation/Index/")).Click();
        }

        [Then(@"Je dois avoir des offres associées à ce lieu")]
        public void ThenJeDoisAvoirDesOffresAssocieesACeLieu()
        {
            Assert.True(WebBrowser.Current.Page<BackOfficePage>().Offers.Count > 0);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Je créé des offres sur un lieu

        [When(@"J'edite un lieu")]
        public void WhenJEditeUnLieu()
        {
            WebBrowser.Current.Links.First(x => x.GetAttributeValue("href").Contains("/lieu-de-travail/editer/")).Click();
        }

        [When(@"je sélectionne une offre")]
        public void WhenJeSelectionneUneOffre()
        {
            var url = WebBrowser.Current.Url.Split('/').ToList();
            var id = int.Parse(url[url.Count - 1]);
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "Offer/Create/" + id + "?type=3");
        }

        [When(@"je remplis des champs pour l'offre")]
        public void WhenJeRemplisDesChampsPourLOffre()
        {
            WebBrowser.Current.TextField(Find.ById("Offer_Name")).TypeTextQuickly("Bureau 1");
            WebBrowser.Current.CheckBox(Find.ById("o_Desktop100Plus")).Click();
            WebBrowser.Current.CheckBox(Find.ById("o_Equipped")).Click();
        }

        [When(@"je valide")]
        public void WhenJeValide()
        {
            WebBrowser.Current.Button(Find.ByValue("Valider")).Click();
            WebBrowser.Current.Button(Find.ByValue("Valider")).Click();
        }

        [Then(@"Je dois avoir l'offre présente et conforme")]
        public void ThenJeDoisAvoirLOffrePresenteEtConforme()
        {
            var ie = WebBrowser.Current;

            Assert.That(ie.ContainsText("Bureaux") && ie.ContainsText("> 100 m2") && ie.ContainsText("Equipé et câblé"));
            ie.Close();
        }

        #endregion

        #region Réserver une offre

        public string localisation_name { get; set; }

        [When(@"Je réserve une offre")]
        public void WhenJeReserveUneOffre()
        {
            WebBrowser.Current.Page<AccueilPage>().Champ_Recherche.TypeText("Paris");
            WebBrowser.Current.Page<AccueilPage>().Type_Espace.SelectByValue("2");
            WebBrowser.Current.Page<AccueilPage>().Bouton_Recherche.Click();
            WebBrowser.Current.Link(Find.BySelector("div[class='contentBlock resultItem'] a[href^='/lieu-de-travail/resultats-detail/']")).Click();
            WebBrowser.Current.Page<BackOfficePage>().BookIt.Click();
            WebBrowser.Current.Page<BackOfficePage>().BookIt.Click();
            WebBrowser.Current.TextField(Find.ById("MemberBooking_ToDate")).TypeText("31/12/2013 08:00");
            WebBrowser.Current.Page<BackOfficePage>().Buttons.Where(x => !string.IsNullOrEmpty(x.Text) && x.Text.Equals("Réserver")).First().Click();
        }

        [Then(@"Je dois avoir la demande de réservation côté utilisateur et gérant")]
        public void ThenJeDoisAvoirLaDemandeDeReservationCoteUtilisateurEtGerant()
        {
            Assert.IsTrue(WebBrowser.Current.Page<BackOfficePage>().BookItems.Count > 0);
            WebBrowser.Current.GoTo(WebBrowser.RootURL + StaticStringClass.URL.DashboardBooking);
            Assert.IsTrue(WebBrowser.Current.Page<BackOfficePage>().BookItems.Count > 0);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Demande de devis

        [When(@"Je demande un devis")]
        public void WhenJeDemandeUnDevis()
        {
            WebBrowser.Current.Page<AccueilPage>().Champ_Recherche.TypeText("New York");
            WebBrowser.Current.Page<AccueilPage>().Type_Espace.SelectByValue("3");
            WebBrowser.Current.Page<AccueilPage>().Bouton_Recherche.Click();
            WebBrowser.Current.Link(Find.BySelector("div[class='contentBlock resultItem'] a[href^='/lieu-de-travail/resultats-detail/']")).Click();
            WebBrowser.Current.Page<BackOfficePage>().QuoteIt.Click();
            WebBrowser.Current.Page<BackOfficePage>().QuoteIt.Click();
            WebBrowser.Current.TextField(Find.ById("MemberQuotation_Surface")).TypeText("45");
            WebBrowser.Current.Page<BackOfficePage>().Buttons.Where(x => !string.IsNullOrEmpty(x.Text) && x.Text.Equals("Valider la demande de devis")).First().Click();
        }

        [Then(@"Je dois avoir la demande de devis côté utilisateur et gérant")]
        public void ThenJeDoisAvoirLaDemandeDeDevisCoteUtilisateurEtGerant()
        {
            Assert.IsTrue(WebBrowser.Current.Page<BackOfficePage>().BookItems.Count > 0);
            WebBrowser.Current.GoTo(WebBrowser.RootURL + StaticStringClass.URL.DashboardQuotation);
            Assert.IsTrue(WebBrowser.Current.Page<BackOfficePage>().BookItems.Count > 0);
            WebBrowser.Current.Close();
        }

        #endregion

        #region Annuler une demande de devis

        [When(@"je clique sur Annuler")]
        public void WhenJeCliqueSurAnnuler()
        {
            WebBrowser.Current.Page<BackOfficePage>().Cancel.Click();
        }

        [Then(@"Je dois avoir la demande de devis annuler")]
        public void ThenJeDoisAvoirLaDemandeDeDevisAnnuler()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Demande de devis annulé"));
            WebBrowser.Current.Close();
        }

        #endregion
    }

    #region BackOffice Page

    public class BackOfficePage : Page
    {
        #region TextFields

        public TextField[] TextFields
        {
            get { return Document.TextFields.ToArray(); }
        }

        #endregion

        #region Links

        public Link[] Links
        {
            get { return Document.Links.ToArray(); }
        }

        public Link BookIt 
        {
            get { return Document.Link(Find.ByText("Réserver")); }
        }

        public Link QuoteIt
        {
            get { return Document.Link(Find.ByText("Demande de devis")); }
        }

        public Link Cancel
        {
            get { return Document.Link(Find.ByText("Annuler")); }
        }

        #endregion

        #region SelectLists

        public SelectList[] SelectLists
        {
            get { return Document.SelectLists.ToArray(); }
        }

        #endregion

        #region CheckBoxes

        public CheckBox[] CheckBoxes
        {
            get { return Document.CheckBoxes.ToArray(); }
        }

        #endregion

        #region Buttons

        public Button[] Buttons
        {
            get { return Document.Buttons.ToArray(); }
        }

        #endregion

        #region List

        public List<Element> BookItems
        {
            get { return Document.Elements.Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.Equals("bookingItem")).ToList(); }
        }

        public List<Div> LocItem
        {
            get { return Document.Divs.Where(x => !string.IsNullOrEmpty(x.ClassName) && x.ClassName.Equals("localisationTag float-left")).ToList(); }
        }

        public List<Element> Offers
        {
            get { return Document.Elements.Where(x => (!string.IsNullOrEmpty(x.TagName) && x.TagName.ToLower().Equals("table"))).ToList(); }
        }

        #endregion
    }
    #endregion
}
