using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using SHDocVw;
using WatiN.Core;
using NUnit.Framework;
using Worki.Data.Models;
namespace Worki.SpecFlow
{
    [Binding]
    class AjoutSteps
    {
        #region Ajout Erreur

        [Given(@"Je vais dans la page d'ajout")]
        public void GivenJeVaisDansLaPageDAjout()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "lieu-de-travail/ajouter-lieu-gratuit");
        }

        [When(@"Je clique sur Envoyer")]
        public void WhenJeCliqueSurEnvoyer()
        {
            WebBrowser.Current.Page<AjoutPage>().Bouton_Add.Click();
        }

        #endregion

        #region Mauvais Patern ( change le verif erreur ) 

        [Given(@"Je remplis mal le champs Email")]
        public void GivenJeRemplisMalLeChampsEmail()
        {
            WebBrowser.Current.Page<AjoutPage>().Lieu_Mail.TypeText("emailpasbon");
        }

        [Given(@"je remplis mal les horaires")]
        public void GivenJeRemplisMalLesHoraires()
        {
            WebBrowser.Current.Page<AjoutPage>().MonOpen.TypeText("horairepasbonne");
            WebBrowser.Current.Page<AjoutPage>().MonClose.TypeText("27:60");
        }

        #endregion

        #region Ajout Complet

        [Given(@"Je remplis la partie informations générales")]
        public void GivenJeRemplisLaPartieInformationsGenerales()
        {
            Random myRand = new Random();
            WebBrowser.Current.Page<AjoutPage>().Lieu_Name.TypeText("Test 0 " + myRand.Next().ToString());
            WebBrowser.Current.Page<AjoutPage>().Lieu_Adress.TypeText("Test 1 " + myRand.Next().ToString());
            WebBrowser.Current.Page<AjoutPage>().Lieu_City.TypeText("Test 2");
            WebBrowser.Current.Page<AjoutPage>().Lieu_PostalCode.TypeText("Test 3");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Country.TypeText("Test 4");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Phone.TypeText("Test 5");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Fax.TypeText("Test 6");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Mail.TypeText("Test7@test7bis.ter");
            WebBrowser.Current.Page<AjoutPage>().Lieu_WebSite.TypeText("Test 8");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Description.TypeText("Test 9");
        }

        [Given(@"Je coche quelques checkbox")]
        public void GivenJeCocheQuelquesCheckbox()
        {
            WebBrowser.Current.Page<AjoutPage>().Espace((int)LocalisationType.CoffeeResto);
            WebBrowser.Current.Page<AjoutPage>().Check_Coffee.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_Handicap.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_Restauration.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_Parking.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_ErgonomicFurniture.Click();
        }

        [Given(@"je remplis Acces")]
        public void GivenJeRemplisAcces()
        {
            /*
            WebBrowser.Current.TextField(Find.ById("Localisation_PublicTransportation")).TypeTextQuickly("Test 10");
            WebBrowser.Current.TextField(Find.ById("Localisation_Station")).TypeTextQuickly("Test 11");
            WebBrowser.Current.TextField(Find.ById("Localisation_RoadAccess")).TypeTextQuickly("Test 12");
            */
            WebBrowser.Current.Page<AjoutPage>().PublicTransportation.TypeText("Test 10");
            WebBrowser.Current.Page<AjoutPage>().Station.TypeText("Test 11");
            WebBrowser.Current.Page<AjoutPage>().RoadAccess.TypeText("Test 12");
        }

        [Given(@"Je remplis Horaires")]
        public void GivenJeRemplisHoraires()
        {
            WebBrowser.Current.Page<AjoutPage>().MonOpen.TypeText("08:00");
            WebBrowser.Current.Page<AjoutPage>().MonClose.TypeText("18:00");
        }

        [Then(@"Je dois avoir le détail des informations générales")]
        public void ThenJeDoisAvoirLeDetailDesInformationsGenerales()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Test 2"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Test 5"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Test 6"));
            WebBrowser.Current.Close();
        }

        [Then(@"Je dois avoir le texte des checkbox")]
        public void ThenJeDoisAvoirLeTexteDesCheckbox()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Poste(s) de travail professionnels"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Domiciliation"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Scène"));
            WebBrowser.Current.Close();
        }

        [Then(@"Je dois avoir le détail de l'acces")]
        public void ThenJeDoisAvoirLeDetailDeLAcces()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Test 10"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Test 12"));
            Assert.IsTrue(WebBrowser.Current.ContainsText("Test 11"));
            WebBrowser.Current.Close();
        }

        [Then(@"Je dois avoir le détail des horaires")]
        public void ThenJeDoisAvoirLeDetailDesHoraires()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("8h00 à 18h00"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Ajout Complet 2

        [Given(@"Je coche quelques checkbox 2")]
        public void GivenJeCocheQuelquesCheckbox2()
        {
            WebBrowser.Current.Page<AjoutPage>().Check_AvoidMorning.Click();
        }

        [Given(@"Je remplis Horaires 2")]
        public void GivenJeRemplisHoraires2()
        {
            WebBrowser.Current.Page<AjoutPage>().Check_Access24.Click();
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
            WebBrowser.Current.Close();
        }

        [Then(@"Je dois avoir le détail des horaires 2")]
        public void ThenJeDoisAvoirLeDetailDesHoraires2()
        {
            Assert.IsTrue(WebBrowser.Current.ContainsText("Accès 24/7"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region ajout lieu déjà existant

        [Given(@"Je remplis lieux deja rentré")]
        public void GivenJeRemplisLieuxDejaRentre()
        {
            WebBrowser.Current.Page<AjoutPage>().Lieu_Name.TypeText("testnepaseffacer");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Adress.TypeText("testnepaseffacer");
            WebBrowser.Current.Page<AjoutPage>().Lieu_City.TypeText("testnepaseffacer");
            WebBrowser.Current.Page<AjoutPage>().Lieu_PostalCode.TypeText("testnepaseffacer");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Country.TypeText("testnepaseffacer");
        }

        #endregion
    }
        
    public class AjoutPage : Page
    {
        public TextField Latitude
        {
            get { return Document.TextField(Find.ById("Localisation_Latitude")); }
        }

        public TextField Longitude
        {
            get { return Document.TextField(Find.ById("Localisation_Longitude")); }
        }

        public CheckBox Check_Owner
        {
            get { return Document.CheckBox(Find.ById("f_LocalisationOwner")); }
        }

        public SelectList Type_Espace
        {
            get { return Document.SelectList(Find.ById("Localisation_TypeValue")); }
        }

        public CheckBox Check_FreeArea
        {
            get { return Document.CheckBox(Find.ById("FreeArea-WorkingPlace")); }
        }

        public CheckBox Check_WorkStation
        {
            get { return Document.CheckBox(Find.ById("Workstation-WorkingPlace")); }
        }

        public CheckBox Check_SeminarRoom
        {
            get { return Document.CheckBox(Find.ById("SeminarRoom-SeminarRoom")); }
        }

        public CheckBox Check_SingleDesk
        {
            get { return Document.CheckBox(Find.ById("SingleDesk-WorkingPlace")); }
        }

        public CheckBox Check_BuisnessRoom
        {
            get { return Document.CheckBox(Find.ById("BuisnessRoom-WorkingPlace")); }
        }

        public CheckBox Check_MeetingRoom
        {
            get { return Document.CheckBox(Find.ById("MeetingRoom-MeetingRoom")); }
        }

        public CheckBox Check_VisioRoom
        {
            get { return Document.CheckBox(Find.ById("VisioRoom-VisioRoom")); }
        }

        public TextField Lieu_Name
        {
            get { return Document.TextField(Find.ById("Localisation_Name")); }
        }

        public TextField Lieu_Adress
        {
            get { return Document.TextField(Find.ById("Localisation_Adress")); }
        }

        public TextField Lieu_City
        {
            get { return Document.TextField(Find.ById("Localisation_City")); }
        }

        public TextField Lieu_PostalCode
        {
            get { return Document.TextField(Find.ById("Localisation_PostalCode")); }
        }

        public TextField Lieu_Country
        {
            get { return Document.TextField(Find.ById("Localisation_Country")); }
        }

        public TextField Lieu_Phone
        {
            get { return Document.TextField(Find.ById("Localisation_PhoneNumber")); }
        }

        public TextField Lieu_Fax
        {
            get { return Document.TextField(Find.ById("Localisation_Fax")); }
        }

        public TextField Lieu_Mail
        {
            get { return Document.TextField(Find.ById("Localisation_Mail")); }
        }

        public TextField Lieu_WebSite
        {
            get { return Document.TextField(Find.ById("Localisation_WebSite")); }
        }

        public TextField Lieu_Description
        {
            get { return Document.TextField(Find.ById("Localisation_Description")); }
        }

        public CheckBox Check_SeminarRoom20_100
        {
            get { return Document.CheckBox(Find.ById("Room20_100-SeminarRoom")); }
        }        
        
        public CheckBox Check_MeetingRoom20Plus
        {
            get { return Document.CheckBox(Find.ById("Room20_plus-MeetingRoom")); }
        }

        public CheckBox Check_MeetingRoomProjector
        {
            get { return Document.CheckBox(Find.ById("Projector-MeetingRoom")); }
        }        

        public CheckBox Check_Scene
        {
            get { return Document.CheckBox(Find.ById("Scene-SeminarRoom")); }
        }

        public CheckBox Check_VisioRoom1_4
        {
            get { return Document.CheckBox(Find.ById("Room1_4-VisioRoom")); }
        }

        public CheckBox Check_VisioRoomDrinks
        {
            get { return Document.CheckBox(Find.ById("Drinks-VisioRoom")); }
        }

        public CheckBox Check_AvoidMorning
        {
            get { return Document.CheckBox(Find.ById("f_AvoidMorning")); }
        }

        public CheckBox Check_AvoidLunch
        {
            get { return Document.CheckBox(Find.ById("f_AvoidLunch")); }
        }

        public CheckBox Check_AvoidAfternoom
        {
            get { return Document.CheckBox(Find.ById("f_AvoidAfternoom")); }
        }

        public CheckBox Check_AvoidEvening
        {
            get { return Document.CheckBox(Find.ById("f_AvoidEvening")); }
        }

        public TextField Prix_Cafe
        {
            get { return Document.TextField(Find.ById("f_CoffeePrice")); }
        }

        public CheckBox Check_Handicap
        {
            get { return Document.CheckBox(Find.ById("f_Handicap")); }
        }
        
        public CheckBox Check_WifiFree
        {
            get { return Document.CheckBox(Find.ById("f_Wifi_Free")); }
        }

        public CheckBox Check_WifiNotFree
        {
            get { return Document.CheckBox(Find.ById("f_Wifi_Not_Free")); }
        }

        public CheckBox Check_Parking
        {
            get { return Document.CheckBox(Find.ById("f_Parking")); }
        }

        public CheckBox Check_Outlet
        {
            get { return Document.CheckBox(Find.ById("f_Outlet")); }
        }

        public CheckBox Check_FastInternet
        {
            get { return Document.CheckBox(Find.ById("f_FastInternet")); }
        }

        public CheckBox Check_SafeStorage
        {
            get { return Document.CheckBox(Find.ById("f_SafeStorage")); }
        }

        public CheckBox Check_Coffee
        {
            get { return Document.CheckBox(Find.ById("f_Coffee")); }
        }

        public CheckBox Check_Restauration
        {
            get { return Document.CheckBox(Find.ById("f_Restauration")); }
        }

        public CheckBox Check_AC
        {
            get { return Document.CheckBox(Find.ById("f_AC")); }
        }

        public CheckBox Check_ErgonomicFurniture
        {
            get { return Document.CheckBox(Find.ById("f_ErgonomicFurniture")); }
        }

        public CheckBox Check_Shower
        {
            get { return Document.CheckBox(Find.ById("f_Shower")); }
        }

        public CheckBox Check_NewsPaper
        {
            get { return Document.CheckBox(Find.ById("f_Newspaper")); }
        }

        public CheckBox Check_TV
        {
            get { return Document.CheckBox(Find.ById("f_TV")); }
        }

        public CheckBox Check_Domiciliation
        {
            get { return Document.CheckBox(Find.ById("Domiciliation-WorkingPlace")); }
        }

        public CheckBox Check_Secretariat
        {
            get { return Document.CheckBox(Find.ById("Secretariat-WorkingPlace")); }
        }

        public CheckBox Check_Courier
        {
            get { return Document.CheckBox(Find.ById("Courier-WorkingPlace")); }
        }

        public CheckBox Check_Printer
        {
            get { return Document.CheckBox(Find.ById("Printer-WorkingPlace")); }
        }

        public CheckBox Check_Computers
        {
            get { return Document.CheckBox(Find.ById("Computers-WorkingPlace")); }
        }

        public CheckBox Check_Archiving
        {
            get { return Document.CheckBox(Find.ById("Archiving-WorkingPlace")); }
        }

        public CheckBox Check_Concierge
        {
            get { return Document.CheckBox(Find.ById("Concierge-WorkingPlace")); }
        }

        public CheckBox Check_Pressing
        {
            get { return Document.CheckBox(Find.ById("Pressing-WorkingPlace")); }
        }

        public CheckBox Check_ComputerHelp
        {
            get { return Document.CheckBox(Find.ById("ComputerHelp-WorkingPlace")); }
        }

        public CheckBox Check_RoomService
        {
            get { return Document.CheckBox(Find.ById("RoomService-WorkingPlace")); }
        }

        public CheckBox Check_Community
        {
            get { return Document.CheckBox(Find.ById("Community-WorkingPlace")); }
        }

        public CheckBox Check_RelaxingArea
        {
            get { return Document.CheckBox(Find.ById("RelaxingArea-WorkingPlace")); }
        }

        public TextField PublicTransportation
        {
            get { return Document.TextField(Find.ById("Localisation_PublicTransportation")); }
        }

        public TextField Station
        {
            get { return Document.TextField(Find.ById("Localisation_Station")); }
        }

        public TextField RoadAccess
        {
            get { return Document.TextField(Find.ById("Localisation_RoadAccess")); }
        }

        public CheckBox Check_Access24
        {
            get { return Document.CheckBox(Find.ById("f_Access24")); }
        }

        public CheckBox Check_LunchClose
        {
            get { return Document.CheckBox(Find.ById("f_LunchClose")); }
        }
        
        public TextField MonOpen
        {
            get { return Document.TextField(Find.ById("Localisation_LocalisationData_MonOpenMorning")); }
        }

        public TextField MonClose
        {
            get { return Document.TextField(Find.ById("Localisation_LocalisationData_MonCloseAfter")); }
        }

        public TextField TueOpen
        {
            get { return Document.TextField(Find.ById("Localisation_LocalisationData_TueOpenMorning")); }
        }

        public TextField TueClose
        {
            get { return Document.TextField(Find.ById("Localisation_LocalisationData_TueCloseAfter")); }
        }

        public TextField WedOpen
        {
            get { return Document.TextField(Find.ById("Localisation_WedOpen")); }
        }

        public TextField WedClose
        {
            get { return Document.TextField(Find.ById("Localisation_WedClose")); }
        }

        public TextField ThuOpen
        {
            get { return Document.TextField(Find.ById("Localisation_ThuOpen")); }
        }

        public TextField ThuClose
        {
            get { return Document.TextField(Find.ById("Localisation_ThuClose")); }
        }

        public TextField FriOpen
        {
            get { return Document.TextField(Find.ById("Localisation_FriOpen")); }
        }

        public TextField FriClose
        {
            get { return Document.TextField(Find.ById("Localisation_FriClose")); }
        }

        public TextField SatOpen
        {
            get { return Document.TextField(Find.ById("Localisation_SatOpen")); }
        }

        public TextField SatClose
        {
            get { return Document.TextField(Find.ById("Localisation_SatClose")); }
        }

        public TextField SunOpen
        {
            get { return Document.TextField(Find.ById("Localisation_SunOpen")); }
        }

        public TextField SunClose
        {
            get { return Document.TextField(Find.ById("Localisation_SunClose")); }
        }

        public Link Lien_RechercheHeader
        {
            get { return Document.Link(Find.ByText("Recherche")); }
        }

        public Button Bouton_Add
        {
            get { return Document.Button(Find.ById("submit_add")); }
        }
        public Link Lien_RechercheFooter
        {
            get { return Document.Link(Find.BySelector("div[class^='footer'] a[href^='/recherche/recherche-lieu-travail']")); }
        }

        public Link Lien_AccueilHeader
        {
            get { return Document.Link(Find.ByText("Accueil")); }
        }

        public Link Lien_AjoutFooter
        {
            get { return Document.Link(Find.BySelector("div[class^='footer'] a[href^='/lieu-de-travail/ajouter']")); }
        }

        public Link Lien_Contact
        {
            get { return Document.Link(Find.ByText("Contact")); }
        }

        public Link Lien_Blog
        {
            get { return Document.Link(Find.ByText("Blog")); }
        }

        public Link Lien_WhoWeAre
        {
            get { return Document.Link(Find.ByText("Qui sommes-nous ?")); }
        }

        public Link Lien_FAQ
        {
            get { return Document.Link(Find.ByText("FAQ")); }
        }

        public Link Lien_CGU
        {
            get { return Document.Link(Find.ByText("CGU")); }
        }

        public Link Lien_Presse
        {
            get { return Document.Link(Find.ByText("Presse")); }
        }

        public Link Lien_Jobs
        {
            get { return Document.Link(Find.ByText("Jobs")); }
        }

        public Link Lien_MentionLegal
        {
            get { return Document.Link(Find.ByText("Mentions légales")); }
        }

        public Link Lien_MonProfil
        {
            get { return Document.Link(Find.ByText("Mon Profil")); }
        }

        public Link Lien_Deconnexion
        {
            get { return Document.Link(Find.ByText("Déconnexion")); }
        }

        public Link Lien_Administrateur
        {
            get { return Document.Link(Find.ByText("Administrateur")); }
        }

        public Image Picture
        {
            get { return Document.Image(Find.BySelector("table[class='files'] img[src$='.jpg']")); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="space"></param>
        public void Espace(int space)
        {
            Type_Espace.Option(Find.ByValue(space.ToString())).Select();
        }
    }
}
