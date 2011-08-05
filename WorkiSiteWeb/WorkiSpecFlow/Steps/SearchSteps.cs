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
            WebBrowser.Current.TextField(Find.ByName("Criteria.Place")).TypeText(address);
        }

        [When(@"Je clique sur rechercher dans la page d'acceuil")]
        public void WhenJeCliqueSurRechercherDansLaPageDAcceuil()
        {
            //TODO: implement act (action) logic
            WebBrowser.Current.Button(Find.ById("btn_searchIndex")).Click();
        }

        [Then(@"Il doit y avoir plus de (.*) resultats")]
        public void ThenIlDoitYAvoirPlusDe200Resultats(int count)
        {
            //TODO: implement assert (verification) logic
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
            WebBrowser.Current.CheckBox("Outlet-General").Click();
        }

        [Given(@"Je coche l'equipement restaurant")]
        public void GivenJeCocheLEquipementRestaurant()
        {
            WebBrowser.Current.TextField("Restauration-General").TypeTextQuickly("True");
        }
         
        #endregion

        #region Recherche Erreur

        [Given(@"Je vais dans la page Recherche")]
        public void GivenJeVaisDansLaPageRecherche()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "recherche/recherche-lieu-travail");
        }

        [When(@"Je clique sur rechercher")]
        public void WhenJeCliqueSurRechercher()
        {
            WebBrowser.Current.Button(Find.ByValue("Rechercher")).Click();

        }

        #endregion

        #region Description Etudiant

        [When(@"Je clique sur Etudiant")]
        public void WhenJeCliqueSurEtudiant()
        {
            WebBrowser.Current.Link(Find.ByText("Etudiant")).Click();
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
            WebBrowser.Current.Link(Find.ByText("Entrepreneur")).Click();
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
            WebBrowser.Current.Link(Find.ByText("Grand compte")).Click();
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
            WebBrowser.Current.Link(Find.ByText("Indépendant")).Click();
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
            WebBrowser.Current.Link(Find.ByText("Nomade")).Click();
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
            WebBrowser.Current.Link(Find.ByText("Télétravailleur")).Click();
        }

        [Then(@"Je dois avoir la description Teletravailleur")]
        public void ThenJeDoisAvoirLaDescriptionTeletravailleur()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Vous êtes télétravailleur"));
        }

        #endregion
    }
}
