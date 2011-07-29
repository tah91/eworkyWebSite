using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using SHDocVw;
using WatiN.Core;
using NUnit.Framework;

namespace WorkiSpecFlow
{
    [Binding]
    class FooterSteps
    {
        [Then(@"Text présent sur la page: (.*)")]
        public void FindText(string TextPresent)
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText(TextPresent));
        }

        #region Test Mentions Légale

        [When(@"Je vais dans la page mention légal")]
        public void WhenJeVaisDansLaPageMentionLegal()
        {
            WebBrowser.Current.Link(Find.ByText("Mentions légales")).Click();
        }

        [Then(@"Je dois avoir la page mention légal")]
        public void ThenJeDoisAvoirLaPageMentionLegal()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + "accueil/mentions-legales");
        }

        #endregion

        #region Jobs

        [When(@"Je vais dans la page de jobs")]
        public void WhenJeVaisDansLaPageDeJobs()
        {
            WebBrowser.Current.Link(Find.ByText("Jobs")).Click();
        }

        [Then(@"Je dois avoir la page jobs")]
        public void ThenJeDoisAvoirLaPageJobs()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + "accueil/jobs");
        }

        #endregion

        #region Presse

        [When(@"Je vais dans la page presse")]
        public void WhenJeVaisDansLaPagePresse()
        {
            WebBrowser.Current.Link(Find.ByText("Presse")).Click();
        }

        [Then(@"Je dois avoir la page presse")]
        public void ThenJeDoisAvoirLaPagePresse()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + "accueil/presse");
        }

        #endregion

        #region CGU

        [When(@"Je vais dans la page CGU")]
        public void WhenJeVaisDansLaPageCGU()
        {
            WebBrowser.Current.Link(Find.ByText("CGU")).Click();
        }

        [Then(@"Je dois avoir la page CGU")]
        public void ThenJeDoisAvoirLaPageCGU()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + "accueil/cgu");
        }

        #endregion

        #region FAQ

        [When(@"Je vais dans la page FAQ")]
        public void WhenJeVaisDansLaPageFAQ()
        {
            WebBrowser.Current.Link(Find.ByText("FAQ")).Click();
        }

        [Then(@"Je dois avoir la page FAQ")]
        public void ThenJeDoisAvoirLaPageFAQ()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + "accueil/faq");
        }

        #endregion

        #region Qui sommes nous

        [When(@"Je vais dans la page Qui sommes nous")]
        public void WhenJeVaisDansLaPageQuiSommesNous()
        {
            WebBrowser.Current.Link(Find.ByText("Qui sommes-nous ?")).Click();
        }

        [Then(@"Je dois avoir la page Qui sommes nous")]
        public void ThenJeDoisAvoirLaPageQuiSommesNous()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + "accueil/a-propos");
        }

        #endregion

        #region contact

        [When(@"Je vais dans la page Contact")]
        public void WhenJeVaisDansLaPageContact()
        {
            WebBrowser.Current.Link(Find.ByText("Contact")).Click();
        }

        [Then(@"Je dois avoir la page Contact")]
        public void ThenJeDoisAvoirLaPageContact()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + "accueil/contact");
        }

        #endregion
    }
}
