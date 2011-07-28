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
    class AjoutSteps
    {
        #region Ajout Erreur

        [Given(@"Je vais dans la page d'ajout")]
        public void GivenJeVaisDansLaPageDAjout()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "lieu-de-travail/ajouter");
        }

        [When(@"Je clique sur Envoyer")]
        public void WhenJeCliqueSurEnvoyer()
        {
            WebBrowser.Current.Button("submit_add").ClickNoWait();
            System.Threading.Thread.Sleep(4000);
        }

        #endregion

        #region Mauvais Patern ( change le verif erreur ) 

        [Given(@"Je remplis mal le champs Email")]
        public void GivenJeRemplisMalLeChampsEmail()
        {
            WebBrowser.Current.TextField(Find.ById("Localisation_Mail")).TypeTextQuickly("emailpasbon");
        }

        [Given(@"je remplis mal les horaires")]
        public void GivenJeRemplisMalLesHoraires()
        {
            WebBrowser.Current.TextField(Find.ById("Localisation_MonOpen")).TypeTextQuickly("horairepasbonne");
            WebBrowser.Current.TextField(Find.ById("Localisation_MonClose")).TypeTextQuickly("27:60");
        }

        #endregion

        #region Ajout Complet

        [Given(@"Je remplis la partie informations générales")]
        public void GivenJeRemplisLaPartieInformationsGenerales()
        {
            Random myRand = new Random();
            WebBrowser.Current.TextField(Find.ById("Localisation_Name")).TypeTextQuickly("Test 0 " + myRand.Next().ToString());
            WebBrowser.Current.TextField(Find.ById("Localisation_Adress")).TypeTextQuickly("Test 1 " + myRand.Next().ToString());
            WebBrowser.Current.TextField(Find.ById("Localisation_City")).TypeTextQuickly("Test 2");
            WebBrowser.Current.TextField(Find.ById("Localisation_PostalCode")).TypeTextQuickly("Test 3");
            WebBrowser.Current.TextField(Find.ById("Localisation_Country")).TypeTextQuickly("Test 4");
            WebBrowser.Current.TextField(Find.ById("Localisation_PhoneNumber")).TypeTextQuickly("Test 5");
            WebBrowser.Current.TextField(Find.ById("Localisation_Fax")).TypeTextQuickly("Test 6");
            WebBrowser.Current.TextField(Find.ById("Localisation_Mail")).TypeTextQuickly("Test7@test7bis.ter");
            WebBrowser.Current.TextField(Find.ById("Localisation_WebSite")).TypeTextQuickly("Test 8");
            WebBrowser.Current.TextField(Find.ById("Localisation_Description")).TypeTextQuickly("Test 9");

        }

        [Given(@"Je coche quelques checkbox")]
        public void GivenJeCocheQuelquesCheckbox()
        {
            WebBrowser.Current.SelectList("Localisation_TypeValue").Option(Find.ByValue("1")).Select();
            WebBrowser.Current.CheckBox("FreeArea-WorkingPlace").Click();
            WebBrowser.Current.CheckBox("SeminarRoom-SeminarRoom").Click();
            WebBrowser.Current.CheckBox("Workstation-WorkingPlace").Click();
            WebBrowser.Current.CheckBox("SingleDesk-WorkingPlace").Click();
            WebBrowser.Current.CheckBox("Coffee-General").Click();
            WebBrowser.Current.CheckBox("Handicap-General").Click();
            WebBrowser.Current.CheckBox("Restauration-General").Click();
            WebBrowser.Current.CheckBox("Parking-General").Click();
            WebBrowser.Current.CheckBox("ErgonomicFurniture-General").Click();
            WebBrowser.Current.CheckBox("Domiciliation-WorkingPlace").Click();
            WebBrowser.Current.CheckBox("Concierge-WorkingPlace").Click();
            WebBrowser.Current.CheckBox("Room20_100-SeminarRoom").Click();
            WebBrowser.Current.CheckBox("Scene-SeminarRoom").Click();
        }

        [Given(@"je remplis Acces")]
        public void GivenJeRemplisAcces()
        {
            WebBrowser.Current.TextField(Find.ById("Localisation_PublicTransportation")).TypeTextQuickly("Test 10");
            WebBrowser.Current.TextField(Find.ById("Localisation_Station")).TypeTextQuickly("Test 11");
            WebBrowser.Current.TextField(Find.ById("Localisation_RoadAccess")).TypeTextQuickly("Test 12");
        }

        [Given(@"Je remplis Horaires")]
        public void GivenJeRemplisHoraires()
        {
            WebBrowser.Current.TextField(Find.ById("Localisation_MonOpen")).TypeTextQuickly("08:00");
            WebBrowser.Current.TextField(Find.ById("Localisation_MonClose")).TypeTextQuickly("18:00");
        }

        [Then(@"Je dois avoir le détail des informations générales")]
        public void ThenJeDoisAvoirLeDetailDesInformationsGenerales()
        {
            //Assert.IsTrue(WebBrowser.Current.ContainsText("Test 0"));
            //Assert.IsTrue(WebBrowser.Current.ContainsText("Test 1"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Test 2"));
            //Assert.IsTrue(WebBrowser.Current.ContainsText("Test 3"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Test 5"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Test 6"));
            //Assert.IsTrue(WebBrowser.Current.ContainsText("Test 7"));
            //Assert.IsTrue(WebBrowser.Current.ContainsText("Test 9"));
            //Assert.IsTrue(WebBrowser.Current.ContainsText("Test 10"));
        }

        [Then(@"Je dois avoir le texte des checkbox")]
        public void ThenJeDoisAvoirLeTexteDesCheckbox()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Poste(s) de travail professionnels"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Domiciliation"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Scène"));
        }

        [Then(@"Je dois avoir le détail de l'acces")]
        public void ThenJeDoisAvoirLeDetailDeLAcces()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Test 10"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Test 12"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Test 11"));
        }

        [Then(@"Je dois avoir le détail des horaires")]
        public void ThenJeDoisAvoirLeDetailDesHoraires()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("8h00 à 18h00"));
        }

        #endregion

        #region Ajout Complet 2

        [Given(@"Je coche quelques checkbox 2")]
        public void GivenJeCocheQuelquesCheckbox2()
        {
            WebBrowser.Current.CheckBox("BuisnessRoom-WorkingPlace").Click();
            WebBrowser.Current.CheckBox("MeetingRoom-MeetingRoom").Click();
            WebBrowser.Current.CheckBox("VisioRoom-VisioRoom").Click();
            WebBrowser.Current.CheckBox("AvoidMorning-General").Click();
            WebBrowser.Current.CheckBox("Room20_plus-MeetingRoom").Click();
            WebBrowser.Current.CheckBox("Projector-MeetingRoom").Click();
            WebBrowser.Current.CheckBox("Drinks-VisioRoom").Click();
            WebBrowser.Current.CheckBox("Room1_4-VisioRoom").Click();
        }

        [Given(@"Je remplis Horaires 2")]
        public void GivenJeRemplisHoraires2()
        {
            WebBrowser.Current.CheckBox("Access24-General").Click();
        }

        [Then(@"Je dois avoir le texte des checkbox 2")]
        public void ThenJeDoisAvoirLeTexteDesCheckbox2()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Salon d'affaires"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Supérieure à 20 personnes"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Rétroprojecteur"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Service de boissons"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("De 1 à 4 personnes"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Salle(s) de téléprésence"));
        }

        [Then(@"Je dois avoir le détail des horaires 2")]
        public void ThenJeDoisAvoirLeDetailDesHoraires2()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Accès 24/7"));
        }

        #endregion

        #region ajout lieu déjà existant

        [Given(@"Je remplis lieux deja rentré")]
        public void GivenJeRemplisLieuxDejaRentre()
        {
            WebBrowser.Current.TextField(Find.ById("Localisation_Name")).TypeTextQuickly("testnepaseffacer");
            WebBrowser.Current.TextField(Find.ById("Localisation_Adress")).TypeTextQuickly("testnepaseffacer");
            WebBrowser.Current.TextField(Find.ById("Localisation_City")).TypeTextQuickly("testnepaseffacer");
            WebBrowser.Current.TextField(Find.ById("Localisation_PostalCode")).TypeTextQuickly("test");
            WebBrowser.Current.TextField(Find.ById("Localisation_Country")).TypeTextQuickly("testnepaseffacer");
        }

        #endregion
    }
}
