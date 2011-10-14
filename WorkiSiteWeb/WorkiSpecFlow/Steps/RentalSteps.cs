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
    public class RentalSteps
    {
        [Given(@"Je vais dans la page recherche")]
        public void GivenJeVaisDansLaPageRecherche()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "Rental/recherche");
        }

        [When(@"Je clique sur administrateur")]
        public void WhenJeCliqueSurAdministrateur()
        {
            WebBrowser.Current.Page<RentalPage>().Lien_Administrateur.Click();
        }

        [When(@"Je clique sur recherche location")]
        public void WhenJeCliqueSurRechercheLocation()
        {
            WebBrowser.Current.Page<RentalPage>().Lien_Recherche.Click();
        }

        [Then(@"Je dois arriver sur la page de recherche location")]
        public void ThenJeDoisArriverSurLaPageDeRechercheLocation()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + "Rental/recherche");
        }

        [When(@"Je clique sur Rechercher location")]
        public void WhenJeCliqueSurRechercherLocation()
        {
            WebBrowser.Current.Page<RentalPage>().Boutton_Rechercher.Click();
        }

        [Then(@"Je dois arriver sur la page de resultat location")]
        public void ThenJeDoisArriverSurLaPageDeResultatLocation()
        {
            var url = WebBrowser.Current.Url.Split('?');
            Assert.AreEqual(url[0], WebBrowser.RootURL + "Rental/resultats-annonces");
        }

        [When(@"Je remplis des champs")]
        public void WhenJeRemplisDesChamps()
        {
            WebBrowser.Current.Page<RentalPage>().MaxRate.TypeText("3000");
            WebBrowser.Current.Page<RentalPage>().MinSurface.TypeText("10");
        }

        [Then(@"Tous les résultats doivent respecter les critères")]
        public void ThenTousLesResultatsDoiventRespecterLesCriteres()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"Je remplis le champs location avec (.*)")]
        public void WhenJeRemplisLeChampsLocationAvecParis(string address)
        {
            WebBrowser.Current.Page<RentalPage>().Place.TypeText(address);
        }

        [Then(@"Je dois avoir au moins (.*) pages de résultat")]
        public void ThenJeDoisAvoirAuMoins2PagesDeResultat(int nb_page)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Je dois arriver sur la page d'erreur")]
        public void ThenJeDoisArriverSurLaPageDErreur()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Une erreur a été rencontrée"));
        }
    }

    #region RentalPage

    public class RentalPage : Page
    {
        public Link Lien_Administrateur
        {
            get { return Document.Link(Find.ByText("Administrateur")); }
        }

        public Link Lien_Recherche
        {
            get { return Document.Link(Find.ByText("Location")); }
        }

        public Button Boutton_Rechercher
        {
            get { return Document.Button(Find.ByValue("Rechercher")); }
        }

        public TextField MinRate
        {
            get { return Document.TextField(Find.ById("MinRate")); }
        }

        public TextField MaxRate
        {
            get { return Document.TextField(Find.ById("MaxRate")); }
        }

        public TextField MinSurface
        {
            get { return Document.TextField(Find.ById("MinSurface")); }
        }

        public TextField MaxSurface
        {
            get { return Document.TextField(Find.ById("MaxSurface")); }
        }

        public TextField Place
        {
            get { return Document.TextField(Find.ById("placeAutocomplete")); }
        }
    }

    #endregion
}
