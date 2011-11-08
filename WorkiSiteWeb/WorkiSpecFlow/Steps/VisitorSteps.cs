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
    class VisitorSteps
    {
        #region Connexion Erreur

        [Given(@"Je vais dans la page Visitor")]
        public void GivenJeVaisDansLaPageVisitor()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "Visitor");
        }

        [When(@"Je clique sur connexion")]
        public void WhenJeCliqueSurConnexion()
        {
            WebBrowser.Current.Button(Find.ByName("login")).Click();
        } 

        [Then(@"Il doit y avoir des messages d'erreur")]
        public void ThenIlDoitYAvoirDesMessagesDErreur()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Une erreur a été rencontrée"));
            WebBrowser.Current.Close();
        }

        #endregion 

        #region Connexion Erroné

        [Given(@"Je rentre mon identifiant")]
        public void GivenJeRentreMonIdentifiant()
        {
            WebBrowser.Current.TextField(Find.ById("Login")).TypeTextQuickly("Admin");
        }

        [Given(@"je rentre un mauvais mot de passe")]
        public void GivenJeRentreUnMauvaisMotDePasse()
        {
            WebBrowser.Current.TextField(Find.ById("Password")).TypeText("fds");
        }

        #endregion

        #region Connexion Admin

        [Given(@"Je rentre mon mot de passe")]
        public void GivenJeRentreMonMotDePasse()
        {
            WebBrowser.Current.TextField("Password").TypeTextQuickly("Admin_Pass");
        }

        [Then(@"Je dois arriver sur la page d'accueil")]
        public void ThenJeDoisArriverSurLaPageDAccueil()
        {
            var currentURL = WebBrowser.Current.Url;
            Assert.AreEqual(currentURL, WebBrowser.RootURL + "accueil/Index");
            WebBrowser.Current.Close();
        }

        #endregion

        #region Mot de passe oublié

        [When(@"Je clique sur mot de passe oublié")]
        public void WhenJeCliqueSurMotDePasseOublie()
        {
            WebBrowser.Current.Link(Find.ByText("Mot de passe oublié")).Click();
        }

        [Then(@"Je dois arriver sur la page de reset")]
        public void ThenJeDoisArriverSurLaPageDeReset()
        {
            var currentURL = WebBrowser.Current.Url;
            Assert.AreEqual(currentURL, WebBrowser.RootURL + "compte/reset-mdp");
            WebBrowser.Current.Close();
        }

        [Then(@"Je dois avoir le message envoie du nouveau mot de passe")]
        public void ThenJeDoisAvoirLeMessageEnvoieDuNouveauMotDePasse()
        {
            Assert.IsTrue( WebBrowser.Current.ContainsText("vos identifiants vous seront envoyés."));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Demande d'inscription Erreur

        [When(@"Je clique sur Go")]
        public void WhenJeCliqueSurGo()
        {
            WebBrowser.Current.Button(Find.ByName("ok")).Click();
        }

        #endregion

        #region demande d'inscription correcte

        [Given(@"je rentre une adresse valide")]
        public void GivenJeRentreUneAdresseValide()
        {
            WebBrowser.Current.TextField(Find.ById("Email")).TypeTextQuickly("valide@nepasvalider.com");
        }

        [Then(@"Je dois arriver sur demande réussi")]
        public void ThenJeDoisArriverSurDemandeReussi()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + "Visitor/demande-reussie");
            WebBrowser.Current.Close();
        }

        [Then(@"Le bon texte de demande inscription réussi doit être présent")]
        public void ThenLeBonTexteDeDemandeInscriptionReussiDoitEtrePresent()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("demande a été envoyée"));
            WebBrowser.Current.Close();
        }

        #endregion
    }
}
