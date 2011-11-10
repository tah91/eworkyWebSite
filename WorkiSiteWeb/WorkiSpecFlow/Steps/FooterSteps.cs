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
    class FooterSteps
    {
        [Then(@"Text présent sur la page: (.*)")]
        public void FindText(string TextPresent)
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText(TextPresent));
            WebBrowser.Current.Close();
        }

        #region Test Mentions Légale

        [When(@"Je vais dans la page mention légal")]
        public void WhenJeVaisDansLaPageMentionLegal()
        {
            WebBrowser.Current.Page<AccueilPage>().Lien_MentionLegal.Click();
        }

        [Then(@"Je dois avoir la page mention légal")]
        public void ThenJeDoisAvoirLaPageMentionLegal()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + StaticStringClass.URL.Legal);
        }

        #endregion

        #region Jobs

        [When(@"Je vais dans la page de jobs")]
        public void WhenJeVaisDansLaPageDeJobs()
        {
            WebBrowser.Current.Page<AccueilPage>().Lien_Jobs.Click();
        }

        [Then(@"Je dois avoir la page jobs")]
        public void ThenJeDoisAvoirLaPageJobs()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + StaticStringClass.URL.Job);
        }

        #endregion

        #region Presse

        [When(@"Je vais dans la page presse")]
        public void WhenJeVaisDansLaPagePresse()
        {
            WebBrowser.Current.Page<AccueilPage>().Lien_Presse.Click();
        }

        [Then(@"Je dois avoir la page presse")]
        public void ThenJeDoisAvoirLaPagePresse()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + StaticStringClass.URL.Press);
        }

        #endregion

        #region CGU

        [When(@"Je vais dans la page CGU")]
        public void WhenJeVaisDansLaPageCGU()
        {
            WebBrowser.Current.Page<AccueilPage>().Lien_CGU.Click();
        }

        [Then(@"Je dois avoir la page CGU")]
        public void ThenJeDoisAvoirLaPageCGU()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + StaticStringClass.URL.CGU);
        }

        #endregion

        #region FAQ

        [When(@"Je vais dans la page FAQ")]
        public void WhenJeVaisDansLaPageFAQ()
        {
            WebBrowser.Current.Page<AccueilPage>().Lien_FAQ.Click();
        }

        [Then(@"Je dois avoir la page FAQ")]
        public void ThenJeDoisAvoirLaPageFAQ()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + StaticStringClass.URL.FAQ);
        }

        #endregion

        #region Qui sommes nous

        [When(@"Je vais dans la page Qui sommes nous")]
        public void WhenJeVaisDansLaPageQuiSommesNous()
        {
            WebBrowser.Current.Page<AccueilPage>().Lien_WhoWeAre.Click();
        }

        [Then(@"Je dois avoir la page Qui sommes nous")]
        public void ThenJeDoisAvoirLaPageQuiSommesNous()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + StaticStringClass.URL.WhoWeAre);
            WebBrowser.Current.Close();
        }

        #endregion

        #region contact

        [When(@"Je vais dans la page Contact")]
        public void WhenJeVaisDansLaPageContact()
        {
            WebBrowser.Current.Page<AccueilPage>().Lien_Contact.Click();
        }

        [Then(@"Je dois avoir la page Contact")]
        public void ThenJeDoisAvoirLaPageContact()
        {
            Assert.AreEqual(WebBrowser.Current.Url, WebBrowser.RootURL + StaticStringClass.URL.Contact);
        }

        #endregion
    }
}
