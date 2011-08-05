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
        public String myStatiqueRand;

        #region Ajout New Welcome People
        [Given(@"Je vais dans la page New Welcome People")]
        public void GivenJeVaisDansLaPageNewWelcomePeople()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "Admin/CreateWelcomePeople");
        }


        [When(@"Je remplis le formulaire")]
        public void WhenJeRemplisLeFormulaire()
        {
            Random myRand = new Random();
            WebBrowser.Current.TextField(Find.ById("FirstName")).TypeTextQuickly("Grégoire" + myRand.Next().ToString());
            WebBrowser.Current.TextField(Find.ById("LastName")).TypeTextQuickly("de Montety" + myRand.Next().ToString());
            WebBrowser.Current.TextField(Find.ById("Age")).TypeTextQuickly("20");
            WebBrowser.Current.TextField(Find.ById("Job")).TypeTextQuickly("Etudiant a plein temps ;)");
            WebBrowser.Current.TextField(Find.ById("Company")).TypeTextQuickly("La meilleur");
            WebBrowser.Current.TextField(Find.ById("Localisation")).TypeTextQuickly("Dans mon lit");
            WebBrowser.Current.TextField(Find.ById("Description")).TypeTextQuickly("un peu humide mais calme et agréable");
        }

        [When(@"Je valide le formulaire welcome people")]
        public void WhenJeValideLeFormulaireWelcomePeople()
        {
            WebBrowser.Current.Button(Find.ByValue("Créer")).ClickNoWait();
            System.Threading.Thread.Sleep(1000);
        }


        [Then(@"Je dois retrouver ce que j'ai remplis")]
        public void ThenJeDoisRetrouverCeQueJAiRemplis()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("La meilleur"));
        }
        #endregion

        #region Editer Welcome People


        [Given(@"Je vais sur Editer")]
        public void GivenJeCliqueSurEditer()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "Admin/EditWelcomePeople/5");
        
        }

        [When(@"Je modifie le formulaire")]
        public void WhenJeModifieLeFormulaire()
        {
            Random myRand = new Random();
            myStatiqueRand = myRand.Next().ToString();
            WebBrowser.Current.TextField(Find.ById("Company")).TypeTextQuickly(myStatiqueRand);
        }

        [When(@"Je valide save welcome people")]
        public void WhenJeValideSaveWelcomePeople()
        {
            WebBrowser.Current.Button(Find.ByValue("Save")).ClickNoWait();
            System.Threading.Thread.Sleep(1000);
        }

        [Then(@"Je dois retrouver ce que j'ai modifié")]
        public void ThenJeDoisRetrouverCeQueJAiModifie()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText(myStatiqueRand));
        }

        #endregion
    }
}
