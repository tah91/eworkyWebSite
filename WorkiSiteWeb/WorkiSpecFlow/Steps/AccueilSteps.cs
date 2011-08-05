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
    class AccueilSteps
    {
        #region Lien Recherche

            [Given(@"Je vais dans la page d'acceuil")]
            public void GivenJeVaisDansLaPageDAcceuil()
            {
                WebBrowser.Current.GoTo(WebBrowser.RootURL + "accueil/Index");
            }

            [When(@"Je clique sur Recherche")]
            public void WhenJeCliqueSurRecherche()
            {
                WebBrowser.Current.Link(Find.ByText("Recherche")).Click();
            }

            [Then(@"Je dois arriver sur la page de recherche")]
            public void ThenJeDoisArriverSurLaPageDeRecherche()
            {
                Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + "recherche/recherche-lieu-travail");
            }

        #endregion

        #region Lien Ajout

            [When(@"Je clique sur Ajout")]
            public void WhenJeCliqueSurAjout()
            {
                WebBrowser.Current.Link(Find.ByText("Ajout")).Click();
            }

            [Then(@"Je dois arriver sur la page de Ajout")]
            public void ThenJeDoisArriverSurLaPageDeAjout()
            {
                Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + "lieu-de-travail/ajouter");
            }

        #endregion

        #region Lien Plus de critères

            [When(@"Je clique sur plus de critères")]
            public void WhenJeCliqueSurPlusDeCriteres()
            {
                WebBrowser.Current.Link(Find.ByText("Plus de critères")).Click();
            }

        #endregion

        #region Acceuil Recherche Erreur

            [When(@"Je clique sur Rechercher")]
            public void WhenJeCliqueSurRechercher()
            {
                WebBrowser.Current.Button("btn_searchIndex").Click();
            }

            [Then(@"Je dois avoir un message d'erreur")]
            public void ThenJeDoisAvoirUnMessageDErreur()
            {
                Assert.IsTrue(WebBrowser.Current.ContainsText("Une erreur a été rencontrée"));
            }

        #endregion

        #region Recherche dans Accueil Salon d'affaire

            [Given(@"Je selectionne Salon d'affaire")]
            public void GivenJeSelectionneSalonDAffaire()
            {
                WebBrowser.Current.SelectList("Criteria_LocalisationOffer").Option(Find.ByValue("1")).Select();
            }

        #endregion

        #region A la une 
        
            [Then(@"Je dois avoir A la une")]
            public void ThenJeDoisAvoirALaUne()
            {
                WebBrowser.Current.Div("mycarousel");
            }

        #endregion
    }
}
