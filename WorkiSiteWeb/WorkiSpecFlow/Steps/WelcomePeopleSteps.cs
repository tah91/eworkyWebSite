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
        [Given(@"Je vais dans la page New Welcome People")]
        public void GivenJeVaisDansLaPageNewWelcomePeople()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "Admin/CreateWelcomePeople");
        }


        [When(@"Je remplis le formulaire")]
        public void WhenJeRemplisLeFormulaire()
        {
            WebBrowser.Current.Page<WelcomePeoplePage>().Email.TypeText("mika7869@gmail.com");
            WebBrowser.Current.Page<WelcomePeoplePage>().Localisation.TypeText("Le Bistrot Marguerite");
            WebBrowser.Current.Page<WelcomePeoplePage>().WelcomePeopleDescription.TypeText("Ceci est un test ...");
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
            Assert.IsTrue(WebBrowser.Current.ContainsText("mika7869@gmail.com"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Le Bistrot Marguerite"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Ceci est un test ..."));
            WebBrowser.Current.Close();
        }
        #endregion

        #region Editer Welcome People


        [Given(@"Je vais sur Editer")]
        public void GivenJeCliqueSurEditer()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "Admin/IndexWelcomePeople");
            WebBrowser.Current.Page<WelcomePeoplePage>().Edit.Click();
        }

        [When(@"Je modifie le formulaire")]
        public void WhenJeModifieLeFormulaire()
        {
            WebBrowser.Current.Page<WelcomePeoplePage>().WelcomePeopleDescription.TypeText("Ceci est un test ... j'ai édité cette description ...");
        }

        [When(@"Je valide save welcome people")]
        public void WhenJeValideSaveWelcomePeople()
        {
            WebBrowser.Current.Page<WelcomePeoplePage>().Save.Click();
        }

        [Then(@"Je dois retrouver ce que j'ai modifié")]
        public void ThenJeDoisRetrouverCeQueJAiModifie()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("L'entrepreneur star a été édité."));
            WebBrowser.Current.Close();
        }

        #endregion
    }

    public class WelcomePeoplePage : Page
    {
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

        public Button Envoyer
        {
            get { return Document.Button(Find.ByValue("Envoyer")); }
        }

        public Button Save
        {
            get { return Document.Button(Find.ByValue("Save")); }
        }

        public Link Edit
        {
            get { return Document.Link(Find.ByText("Editer")); }
        }

        public Link Detail
        {
            get { return Document.Link(Find.ByText("Détails")); }
        }
    }
}
