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
            WebBrowser.Current.Page<BackOfficePage>().Links.Single(x => x.Text == "bo").Click();
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
            WebBrowser.Current.Page<BackOfficePage>().Links.Single(x => x.Text == "Profil").Click();
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
            WebBrowser.Current.Page<BackOfficePage>().Links.Single(x => x.Text == "Espaces de travail").Click();
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
            WebBrowser.Current.Page<BackOfficePage>().Links.Single(x => x.Text == "Réservations en cours").Click();
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
    }

    #region BackOffice Page

    public class BackOfficePage : Page
    {
        #region Links

        public Link[] Links
        {
            get { return Document.Links.ToArray(); }
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
            get { return Document.ElementsWithTag("li").ToList(); }
        }

        #endregion
    }
    #endregion
}
