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
        #region Créer une fiche de lieu Gratuit

        [Given(@"Je vais dans la page d'ajout de lieux gratuits")]
        public void GivenJeVaisDansLaPageDAjoutDeLieuxGratuits()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "lieu-de-travail/ajouter-lieu-gratuit");
        }

        [Given(@"Je remplis les champs")]
        public void GivenJeRemplisLesChamps()
        {
            WebBrowser.Current.Page<AjoutPage>().Type_Espace.Select("Espace public numérique / Cybercafé");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Name.TypeText("eWorky");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Adress.TypeText("47 rue de Lille");
            WebBrowser.Current.Page<AjoutPage>().Lieu_City.TypeText("Paris");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Phone.TypeText("0177170421");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Mail.TypeText("contact@eworky.com");
            WebBrowser.Current.Page<AjoutPage>().Lieu_WebSite.TypeText("www.eworky.com");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Description.TypeText("Ceci est un test auto Made by Mika");
            WebBrowser.Current.Page<AjoutPage>().Check_AvoidMorning.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_AvoidEvening.Click();
            WebBrowser.Current.Page<AjoutPage>().Prix_Cafe.TypeText("1");
            WebBrowser.Current.Page<AjoutPage>().Check_Outlet.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_FastInternet.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_Coffee.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_Restauration.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_SafeStorage.Click();
            WebBrowser.Current.Page<AjoutPage>().PublicTransportation.TypeText("Près de la station Solférino");
            WebBrowser.Current.Page<AjoutPage>().Check_Access24.Click();
        }

        [When(@"Je clique sur Envoyer")]
        public void WhenJeCliqueSurEnvoyer()
        {
            WebBrowser.Current.Page<AjoutPage>().Bouton_Add.Click();
        }

        [Then(@"Je dois retrouver les infos")]
        public void ThenJeDoisRetrouverLesInfos()
        {
            var ie = WebBrowser.Current;

            Assert.That(ie.ContainsText("Prix du café : 1,00 €") && ie.ContainsText("Wifi gratuit") && ie.ContainsText("Prise de courant")
                     && ie.ContainsText("Internet très haut débit") && ie.ContainsText("Rangement sécurisé") && ie.ContainsText("Café / Bar")
                     && ie.ContainsText("Restauration") && ie.ContainsText("eWorky") && ie.ContainsText("Espace public numérique / Cybercafé") && ie.ContainsText("47 rue de Lille - 75007 Paris")
                     && ie.ContainsText("01 77 17 04 21") && ie.ContainsText("Accès 24/7") && ie.ContainsText("contact@eworky.com") && ie.ContainsText("Eviter les periodes : matinée, soirée.")
                     && ie.ContainsText("Transports en commun : Près de la station Solférino") && ie.ContainsText("Ceci est un test auto Made by Mika"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Editer une fiche de lieu Gratuit

        [Given(@"Je vais dans la page admin")]
        public void GivenJeVaisDansLaPageAdmin()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "Admin");
        }

        [Given(@"Je clique sur editer")]
        public void GivenJeCliqueSurEditer()
        {
            WebBrowser.Current.Page<AjoutPage>().Editer.Click();
        }

        [When(@"Je change les champs")]
        public void WhenJeChangeLesChamps()
        {
            WebBrowser.Current.Page<AjoutPage>().Type_Espace.Select("Café / Restaurant");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Name.TypeText("Greenworking");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Adress.TypeText("62 Rue Chabot Charny");
            WebBrowser.Current.Page<AjoutPage>().Lieu_City.TypeText("Dijon");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Phone.TypeText("0177198721");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Mail.TypeText("contact@greenworking.fr");
            WebBrowser.Current.Page<AjoutPage>().Lieu_WebSite.TypeText("http://www.greenworking.fr/");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Description.TypeText("Ceci est un test auto Made by Mika Edit");
            WebBrowser.Current.Page<AjoutPage>().Check_AvoidMorning.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_AvoidEvening.Click();
            WebBrowser.Current.Page<AjoutPage>().Prix_Cafe.TypeText("10");
            WebBrowser.Current.Page<AjoutPage>().Check_Outlet.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_FastInternet.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_Coffee.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_Restauration.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_SafeStorage.Click();
            WebBrowser.Current.Page<AjoutPage>().PublicTransportation.TypeText("");
            WebBrowser.Current.Page<AjoutPage>().Check_Access24.Click();
        }

        [Then(@"Je dois avoir retrouver les infos modifiées")]
        public void ThenJeDoisAvoirRetrouverLesInfosModifiees()
        {
            var ie = WebBrowser.Current;

            Assert.That(ie.ContainsText("Prix du café : 10,00 €") && ie.ContainsText("Wifi gratuit") && !ie.ContainsText("Prise de courant")
                     && !ie.ContainsText("Internet très haut débit") && !ie.ContainsText("Rangement sécurisé") && !ie.ContainsText("Café / Bar")
                     && !ie.ContainsText("Restauration") && ie.ContainsText("Greenworking") && ie.ContainsText("Café / Restaurant") && ie.ContainsText("62 Rue Chabot Charny - 21000 Dijon")
                     && ie.ContainsText("01 77 19 87 21") && !ie.ContainsText("Accès 24/7") && ie.ContainsText("contact@eworky.com") && !ie.ContainsText("Eviter les periodes : matinée, soirée.")
                     && !ie.ContainsText("Transports en commun : Près de la station Solférino") && ie.ContainsText("Ceci est un test auto Made by Mika Edit"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Supprimer une fiche de lieu Gratuit/Payant

        [When(@"Je clique sur Supprimer")]
        public void WhenJeCliqueSurSupprimer()
        {
            WebBrowser.Current.Page<AjoutPage>().Supprimer.Click();
            WebBrowser.Current.Page<AjoutPage>().ConfirmDelete.Click();
        }
        [Then(@"La fiche de lieu est supprimée")]
        public void ThenLaFicheDeLieuEstSupprimee()
        {
            WebBrowser.Current.ContainsText("Le lieu a été supprimé.");
            WebBrowser.Current.Close();
        }

        #endregion

        #region Créer une fiche de lieu Payant

        [Given(@"Je vais dans la page d'ajout de lieux payant")]
        public void GivenJeVaisDansLaPageDAjoutDeLieuxPayant()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "lieu-de-travail/ajouter-lieu-payant");
        }

        [Given(@"Je remplis les champs 2")]
        public void GivenJeRemplisLesChamps2()
        {
            WebBrowser.Current.Page<AjoutPage>().Type_Espace.Select("Télécentre");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Name.TypeText("eWorky");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Adress.TypeText("47 rue de Lille");
            WebBrowser.Current.Page<AjoutPage>().Lieu_City.TypeText("Paris");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Phone.TypeText("0177170421");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Mail.TypeText("contact@eworky.com");
            WebBrowser.Current.Page<AjoutPage>().Lieu_WebSite.TypeText("www.eworky.com");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Description.TypeText("Ceci est un test auto Made by Mika");
            WebBrowser.Current.Page<AjoutPage>().Check_Outlet.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_FastInternet.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_Coffee.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_Restauration.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_SafeStorage.Click();
            WebBrowser.Current.Page<AjoutPage>().PublicTransportation.TypeText("Près de la station Solférino");
            WebBrowser.Current.Page<AjoutPage>().Check_Access24.Click();
            WebBrowser.Current.Page<AjoutPage>().Type_Offer.Select("Bureau");
            WebBrowser.Current.Page<AjoutPage>().Add_Offer.Click();
            WebBrowser.Current.Page<AjoutPage>().OfferName.TypeText("Bureau 1");
            WebBrowser.Current.Page<AjoutPage>().Desktop25_50.Click();
            WebBrowser.Current.Page<AjoutPage>().Equipped.Click();
            WebBrowser.Current.Page<AjoutPage>().AllInclusive.Click();
            WebBrowser.Current.Page<AjoutPage>().Validate.Click();
        }

        [Then(@"Je dois retrouver les infos 2")]
        public void ThenJeDoisRetrouverLesInfos2()
        {
            var ie = WebBrowser.Current;

            Assert.That(ie.ContainsText("Wifi gratuit") && ie.ContainsText("Prise de courant")
                     && ie.ContainsText("Internet très haut débit") && ie.ContainsText("Rangement sécurisé") && ie.ContainsText("Café / Bar")
                     && ie.ContainsText("Restauration") && ie.ContainsText("eWorky") && ie.ContainsText("Télécentre") && ie.ContainsText("47 rue de Lille - 75007 Paris")
                     && ie.ContainsText("01 77 17 04 21") && ie.ContainsText("Accès 24/7") && ie.ContainsText("contact@eworky.com")
                     && ie.ContainsText("Transports en commun : Près de la station Solférino") && ie.ContainsText("Bureaux") && ie.ContainsText("De 25 à 50 m2") && ie.ContainsText("Equipé et câblé")
                     && ie.ContainsText("Tout compris (charges, taxes, nettoyage, etc.)") && ie.ContainsText("Ceci est un test auto Made by Mika"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Editer une fiche de lieu Payant

        [When(@"Je change les champs 2")]
        public void WhenJeChangeLesChamps2()
        {
            WebBrowser.Current.Page<AjoutPage>().Type_Espace.Select("Espace de coworking");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Name.TypeText("Greenworking");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Adress.TypeText("62 Rue Chabot Charny");
            WebBrowser.Current.Page<AjoutPage>().Lieu_City.TypeText("Dijon");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Phone.TypeText("0177198721");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Mail.TypeText("contact@greenworking.fr");
            WebBrowser.Current.Page<AjoutPage>().Lieu_WebSite.TypeText("http://www.greenworking.fr/");
            WebBrowser.Current.Page<AjoutPage>().Lieu_Description.TypeText("Ceci est un test auto Made by Mika Edit");
            WebBrowser.Current.Page<AjoutPage>().Check_Outlet.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_FastInternet.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_Coffee.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_Restauration.Click();
            WebBrowser.Current.Page<AjoutPage>().Check_SafeStorage.Click();
            WebBrowser.Current.Page<AjoutPage>().PublicTransportation.TypeText("");
            WebBrowser.Current.Page<AjoutPage>().DeleteOffer.Click();
            WebBrowser.Current.Page<AjoutPage>().ConfirmDelete.Click();
        }

        [Then(@"Je dois avoir retrouver les infos modifiées 2")]
        public void ThenJeDoisAvoirRetrouverLesInfosModifiees2()
        {
            var ie = WebBrowser.Current;

            Assert.That(ie.ContainsText("Wifi gratuit") && !ie.ContainsText("Prise de courant")
                     && !ie.ContainsText("Internet très haut débit") && !ie.ContainsText("Rangement sécurisé") && !ie.ContainsText("Café / Bar")
                     && !ie.ContainsText("Restauration") && ie.ContainsText("Greenworking") && ie.ContainsText("Espace de coworking") && ie.ContainsText("62 Rue Chabot Charny - 21000 Dijon")
                     && ie.ContainsText("01 77 19 87 21") && ie.ContainsText("contact@greenworking.fr")
                     && !ie.ContainsText("Transports en commun : Près de la station Solférino") && !ie.ContainsText("Bureaux") && !ie.ContainsText("De 25 à 50 m2") && !ie.ContainsText("Equipé et câblé")
                     && !ie.ContainsText("Tout compris (charges, taxes, nettoyage, etc.)") && ie.ContainsText("Ceci est un test auto Made by Mika Edit"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Ajout Erreur

        [Given(@"Je vais dans la page d'ajout")]
        public void GivenJeVaisDansLaPageDAjout()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "lieu-de-travail/ajouter-lieu-gratuit");
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

    #region Ajout Page

    public class AjoutPage : Page
    {
        #region Textfield

            #region GeoLocalisation

            public TextField Latitude
            {
                get { return Document.TextField(Find.ById("Localisation_Latitude")); }
            }

            public TextField Longitude
            {
                get { return Document.TextField(Find.ById("Localisation_Longitude")); }
            }

            #endregion

            #region Info General

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

            public TextField Lieu_Description
            {
                get { return Document.TextField(Find.ById("Localisation_Description")); }
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

            #endregion

            #region Espace Travail Gratuit

            public TextField Prix_Cafe
            {
                get { return Document.TextField(Find.ById("f_CoffeePrice")); }
            }

            #endregion

            #region Access

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

            #endregion

            #region Horaires

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

            #endregion

            #region Offer

            public TextField OfferName
            {
                get { return Document.TextField(Find.ById("Offer_Name")); }
            }

            #endregion

        #endregion

        #region CheckBox

            #region Caracteristiques

            public CheckBox Check_Outlet
            {
                get { return Document.CheckBox(Find.ById("f_Outlet")); }
            }

            public CheckBox Check_FastInternet
            {
                get { return Document.CheckBox(Find.ById("f_FastInternet")); }
            }

            public CheckBox Check_AC
            {
                get { return Document.CheckBox(Find.ById("f_AC")); }
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

            #endregion

            #region Espace Travail Gratuit

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

            #endregion

            #region Service Proposé

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

            #endregion

            #region Horaires

            public CheckBox Check_Access24
            {
                get { return Document.CheckBox(Find.ById("f_Access24")); }
            }

            public CheckBox Check_LunchClose
            {
                get { return Document.CheckBox(Find.ById("f_LunchClose")); }
            }

            #endregion

            #region Offer

            public CheckBox Desktop25_50
            {
                get { return Document.CheckBox(Find.ById("o_Desktop25_50")); }
            }

            public CheckBox Equipped
            {
                get { return Document.CheckBox(Find.ById("o_Equipped")); }
            }

            public CheckBox AllInclusive
            {
                get { return Document.CheckBox(Find.ById("o_AllInclusive")); }
            }

            #endregion

        #endregion

        #region SelectList

        public SelectList Type_Espace
        {
            get { return Document.SelectList(Find.ById("Localisation_TypeValue")); }
        }

        public SelectList Type_Offer
        {
            get { return Document.SelectList(Find.ById("NewOfferType")); }
        }

        #endregion

        #region Button

        public Button Bouton_Add
        {
            get { return Document.Button(Find.ById("submit_add")); }
        }

        public Button Add_Offer
        {
            get { return Document.Button(Find.ByName("addOffer")); }
        }

        public Button Validate
        {
            get { return Document.Button(Find.ByValue("Valider")); }
        }

        public Button ConfirmDelete
        {
            get { return Document.Button(Find.ByValue("Supprimer")); }
        }

        #endregion

        #region Link

            #region Editer/Supprimer

            public Link Editer
                {
                    get { return Document.Link(Find.ByText("Editer")); }
                }

                public Link Supprimer
                {
                    get { return Document.Link(Find.ByText("Supprimer")); }
                }

                public Link DeleteOffer
                {
                    get { return Document.Link(Find.BySelector("a[href^='/offre/Delete/']")); }
                }

            #endregion

            #region Footer

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

            public Link Lien_RechercheFooter
            {
                get { return Document.Link(Find.BySelector("div[class^='footer'] a[href^='/recherche/recherche-lieu-travail']")); }
            }

            public Link Lien_AjoutFooter
            {
                get { return Document.Link(Find.BySelector("div[class^='footer'] a[href^='/lieu-de-travail/ajouter']")); }
            }

            #endregion

            #region Header

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

            public Link Lien_RechercheHeader
            {
                get { return Document.Link(Find.ByText("Recherche")); }
            }

            public Link Lien_AccueilHeader
            {
                get { return Document.Link(Find.ByText("Accueil")); }
            }

            #endregion

        #endregion

        #region Image

        public Image Picture
        {
            get { return Document.Image(Find.BySelector("table[class='files'] img[src$='.jpg']")); }
        }

        #endregion
    }

    #endregion
}
