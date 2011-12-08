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
    class WelcomePeopleSteps
    {
        #region Ajout New Welcome People

        [Given(@"Je vais sur la page Welcome People")]
        public void GivenJeVaisSurLaPageWelcomePeople()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + StaticStringClass.URL.WelcomePeopleIndex);
        }

        [When(@"Je remplis le formulaire")]
        public void WhenJeRemplisLeFormulaire()
        {
            WebBrowser.Current.Page<WelcomePeoplePage>().Creer.Click();
            WebBrowser.Current.Page<WelcomePeoplePage>().Email.TypeTextQuickly(StaticStringClass.Connexion.OnlineLogin);
            WebBrowser.Current.Page<WelcomePeoplePage>().Localisation.TypeTextQuickly("Le Bistrot Marguerite");
            WebBrowser.Current.Page<WelcomePeoplePage>().WelcomePeopleDescription.TypeTextQuickly(StaticStringClass.Autre.MsgPerso);
        }

        [When(@"Je valide le formulaire welcome people")]
        public void WhenJeValideLeFormulaireWelcomePeople()
        {
            WebBrowser.Current.Page<WelcomePeoplePage>().Envoyer.Click();
        }

        [Then(@"Je dois retrouver ce que j'ai remplis")]
        public void ThenJeDoisRetrouverCeQueJAiRemplis()
        {
            WebBrowser.Current.Page<WelcomePeoplePage>().Detail.Click();
            Assert.IsTrue(WebBrowser.Current.ContainsText(StaticStringClass.Connexion.OnlineLogin));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Le Bistrot Marguerite"));
            Assert.IsTrue(WebBrowser.Current.ContainsText(StaticStringClass.Autre.MsgPerso));
            WebBrowser.Current.Close();
        }
        #endregion

        #region Editer Welcome People

        [When(@"Je modifie le formulaire")]
        public void WhenJeModifieLeFormulaire()
        {
            WebBrowser.Current.Page<WelcomePeoplePage>().Edit.Click();
            WebBrowser.Current.Page<WelcomePeoplePage>().WelcomePeopleDescription.TypeTextQuickly(StaticStringClass.Autre.MsgPerso + " Edit");
        }

        [When(@"Je valide save welcome people")]
        public void WhenJeValideSaveWelcomePeople()
        {
            WebBrowser.Current.Page<WelcomePeoplePage>().Save.Click();
        }

        [Then(@"Je dois retrouver ce que j'ai modifié")]
        public void ThenJeDoisRetrouverCeQueJAiModifie()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText(Worki.Resources.Views.Admin.AdminString.WelcomePeopleHaveBeenEdit));
            WebBrowser.Current.Page<WelcomePeoplePage>().Detail.Click();
            Assert.IsTrue(WebBrowser.Current.ContainsText(StaticStringClass.Autre.MsgPerso + " Edit"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Supprimer Welcome People

        [Then(@"Welcome people est supprimé")]
        public void ThenWelcomePeopleEstSupprime()
        {
            Assert.That(WebBrowser.Current.ContainsText(Worki.Resources.Views.Admin.AdminString.WelcomePeopleHaveBeenDel));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Erreur Ajout Welcome People

        [When(@"Je clique sur ajouter")]
        public void WhenJeCliqueSurAjouter()
        {
            WebBrowser.Current.Page<WelcomePeoplePage>().Creer.Click();
        }

        [Then(@"Il doit y avoir des messages d'erreur")]
        public void ThenIlDoitYAvoirDesMessagesDErreur()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Une erreur a été rencontrée. "));
            WebBrowser.Current.Close();
        }

        #endregion
    }

    #region WelcomePeople Page

    public class WelcomePeoplePage : Page
    {
        #region TextField

        public TextField Email
        {
            get { return Document.TextField(Find.ById("Email")); }
        }

        public TextField Localisation
        {
            get { return Document.TextField(Find.ById("LocalisationName")); }
        }

        public TextField WelcomePeopleDescription
        {
            get { return Document.TextField(Find.ById("WelcomePeople_Description")); }
        }

        #endregion

        #region Button

        public Button Envoyer
        {
            get { return Document.Button(Find.ByValue("Envoyer")); }
        }

        public Button Save
        {
            get { return Document.Button(Find.ByValue("Save")); }
        }

        #endregion

        #region Link

        public Link Edit
        {
            get { return Document.Link(Find.ByText("Editer")); }
        }

        public Link Detail
        {
            get { return Document.Link(Find.ByText("Détails")); }
        }

        public Link Creer
        {
            get { return Document.Link(Find.ByText("Créer un nouveau profil")); }
        }

        #endregion
    }

    #endregion
}
