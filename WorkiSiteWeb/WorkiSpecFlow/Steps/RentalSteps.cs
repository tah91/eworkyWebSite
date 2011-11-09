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
    public class RentalSteps
    {
        [Given(@"Je me connecte à eWorky")]
        public void GivenJeMeConnecteAEWorky()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "compte/connexion");
            WebBrowser.Current.TextField(Find.ById("Login")).TypeText("Admin");
            WebBrowser.Current.TextField(Find.ById("Password")).TypeText("Admin_Pass");
            WebBrowser.Current.Button(Find.BySelector("input[type='submit']")).Click();
        }

        #region Lancer une Recherche

        [Given(@"Je vais dans la page recherche")]
        public void GivenJeVaisDansLaPageRecherche()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "Rental/recherche");
        }

        [When(@"Je remplis des champs")]
        public void WhenJeRemplisDesChamps()
        {
            WebBrowser.Current.Page<RentalPage>().MaxRate.TypeText("3000");
            WebBrowser.Current.Page<RentalPage>().MinSurface.TypeText("10");
        }

        [When(@"Je clique sur Rechercher location")]
        public void WhenJeCliqueSurRechercherLocation()
        {
            WebBrowser.Current.Page<RentalPage>().Boutton_Rechercher.Click();
        }

        [Then(@"Je dois arriver sur la page de resultat location")]
        public void ThenJeDoisArriverSurLaPageDeResultatLocation()
        {
            var url = WebBrowser.Current.Url.Split('?');
            Assert.AreEqual(url[0], WebBrowser.RootURL + "annonces/resultats-annonces");
        }

        [Then(@"Tous les résultats doivent respecter les critères")]
        public void ThenTousLesResultatsDoiventRespecterLesCriteres()
        {
            var text_euro = WebBrowser.Current.FindText(new System.Text.RegularExpressions.Regex("(.* )€ cc"));
            var text_surface = WebBrowser.Current.FindText(new System.Text.RegularExpressions.Regex("(.* )m²"));

            var euro_tab = text_euro.Split(' ');
            var surface_tab = text_surface.Split(' ');
            var test_euro = 0;
            foreach (var euro in euro_tab)
            {
                try
                {
                    test_euro = Convert.ToInt32(euro);
                    if (test_euro > 0)
                        break;
                }
                catch
                {
                }
            }
            var test_surface = 0;
            foreach (var surface in surface_tab)
            {
                try
                {
                    test_surface = Convert.ToInt32(surface);
                    if (test_surface > 0)
                        break;
                }
                catch
                {
                }
            }
            Assert.That(test_euro, Is.LessThanOrEqualTo(3000));
            Assert.That(test_surface, Is.GreaterThanOrEqualTo(10));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Recherche Paris

        [When(@"Je remplis le champs location avec (.*)")]
        public void WhenJeRemplisLeChampsLocationAvecParis(string address)
        {
            WebBrowser.Current.Page<RentalPage>().Place.TypeText(address);
        }

        [Then(@"Je dois avoir au moins (.*) pages de résultat")]
        public void ThenJeDoisAvoirAuMoins2PagesDeResultat(int nb_page)
        {
            var text = WebBrowser.Current.FindText(new System.Text.RegularExpressions.Regex("(.* )lieu"));

            var tab = text.Split(' ');
            var count = 0;
            foreach (var test in tab)
            {
                try
                {
                    count = Convert.ToInt32(test);
                    if (count > 0)
                        break;
                }
                catch
                {
                }
            }
            Assert.That(count, Is.GreaterThanOrEqualTo(3));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Créer une fiche de location

        [Given(@"Je vais sur la page de création de location")]
        public void GivenJeVaisSurLaPageDeCreationDeLocation()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "annonces/ajouter");
        }

        [When(@"Je remplis le formulaire de location")]
        public void WhenJeRemplisLeFormulaireDeLocation()
        {
            WebBrowser.Current.Page<RentalPage>().Rental_Type.Select("Bureaux & locaux professionnels");
            WebBrowser.Current.Page<RentalPage>().Ref.TypeText("Ref Test Auto");
            WebBrowser.Current.Page<RentalPage>().Lease_Type.Select("Bail 3/6/9");
            WebBrowser.Current.Page<RentalPage>().AvailableNow.Click();
            WebBrowser.Current.Page<RentalPage>().AvailableDate.TypeText("21/12/2012");
            WebBrowser.Current.Page<RentalPage>().Rate.TypeText("2000");
            WebBrowser.Current.Page<RentalPage>().Charges.TypeText("100");
            WebBrowser.Current.Page<RentalPage>().Surface.TypeText("120");
            WebBrowser.Current.Page<RentalPage>().HeatingType.Select("Gaz");
            WebBrowser.Current.Page<RentalPage>().GreenHouse.Select("B");
            WebBrowser.Current.Page<RentalPage>().Description.TypeText("Ceci est un test auto Made by Mika");
            WebBrowser.Current.Page<RentalPage>().Adress.TypeText("57 rue de Lille");
            WebBrowser.Current.Page<RentalPage>().Postal.TypeText("75007");
            WebBrowser.Current.Page<RentalPage>().Town.TypeText("Paris");
            WebBrowser.Current.Page<RentalPage>().Country.TypeText("France");
            WebBrowser.Current.Page<RentalPage>().Startup.Click();
            WebBrowser.Current.Page<RentalPage>().New.Click();
            WebBrowser.Current.Page<RentalPage>().Parking.Click();
            WebBrowser.Current.Page<RentalPage>().Quiet.Click();
        }

        [When(@"Je clique sur Valider")]
        public void WhenJeCliqueSurValider()
        {
            WebBrowser.Current.Page<RentalPage>().Validate_Button.Click();
        }

        [Then(@"Je dois avoir arriver sur la page détail")]
        public void ThenJeDoisAvoirArriverSurLaPageDetail()
        {
            Assert.IsTrue(WebBrowser.Current.Url.Contains("annonces/details/"));
        }

        [Then(@"Je dois retrouver les bonnes informations")]
        public void ThenJeDoisRetrouverLesBonnesInformations()
        {
            var ie = WebBrowser.Current;
            Assert.That(ie.ContainsText("Start-up friendly") && ie.ContainsText("Diagnostic énergétique : B")
                    && ie.ContainsText("Gaz") && ie.ContainsText("Bail 3/6/9") && ie.ContainsText("Disponibilité : 21/12/2012")
                    && ie.ContainsText("Bureaux & locaux professionnels - Paris (75007)")
                    && ie.ContainsText("Etat neuf") && ie.ContainsText("Parking") && ie.ContainsText("Environnement calme")
                    && ie.ContainsText("2100 € cc") && ie.ContainsText("120 m²"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Editer une fiche de location

        [Given(@"Je vais sur la page d'édition de location")]
        public void GivenJeVaisSurLaPageDEditionDeLocation()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "Admin/IndexRental");
            WebBrowser.Current.Page<RentalPage>().Editer.Click();
        }

        [When(@"Je modifie le formulaire de location")]
        public void WhenJeModifieLeFormulaireDeLocation()
        {
            WebBrowser.Current.Page<RentalPage>().Rental_Type.Select("Fonds de commerce");
            WebBrowser.Current.Page<RentalPage>().Ref.TypeText("Ref Test Auto Edit");
            WebBrowser.Current.Page<RentalPage>().Lease_Type.Select("Bail précaire 24 mois");
            WebBrowser.Current.Page<RentalPage>().AvailableDate.TypeText("");
            WebBrowser.Current.Page<RentalPage>().AvailableNow.Click();
            WebBrowser.Current.Page<RentalPage>().Rate.TypeText("4000");
            WebBrowser.Current.Page<RentalPage>().Charges.TypeText("200");
            WebBrowser.Current.Page<RentalPage>().Surface.TypeText("170");
            WebBrowser.Current.Page<RentalPage>().HeatingType.Select("Electrique");
            WebBrowser.Current.Page<RentalPage>().GreenHouse.Select("D");
            WebBrowser.Current.Page<RentalPage>().Description.TypeText("Ceci est un test auto Edit Made by Mika");
            WebBrowser.Current.Page<RentalPage>().Startup.Click();
            WebBrowser.Current.Page<RentalPage>().Parking.Click();
        }

        [Then(@"Les informations doivent avoir changées")]
        public void ThenLesInformationsDoiventAvoirChangees()
        {
            var ie = WebBrowser.Current;
            Assert.That(!ie.ContainsText("Start-up friendly") && ie.ContainsText("Diagnostic énergétique : D")
                    && ie.ContainsText("Electrique") && ie.ContainsText("Bail précaire 24 mois") && ie.ContainsText("Disponibilité immédiate")
                    && ie.ContainsText("Fonds de commerce - Paris (75007)")
                    && ie.ContainsText("Etat neuf") && !ie.ContainsText("Parking") && ie.ContainsText("Environnement calme")
                    && ie.ContainsText("4200 € cc") && ie.ContainsText("170 m²"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Supprimer une fiche de location

        [Given(@"Je vais sur la page admin des locations")]
        public void GivenJeVaisSurLaPageAdminDesLocations()
        {
            WebBrowser.Current.GoTo(WebBrowser.RootURL + "Admin/IndexRental");
        }

        [When(@"Je clique sur supprimer de la derniere location")]
        public void WhenJeCliqueSurSupprimerDeLaDerniereLocation()
        {
            WebBrowser.Current.Page<RentalPage>().Supprimer.Click();
        }

        [When(@"Je valide la suppression")]
        public void WhenJeValideLaSuppression()
        {
            WebBrowser.Current.Page<RentalPage>().Validate_Button.Click();
        }

        [Then(@"La location doit avoir disparu")]
        public void ThenLaLocationDoitAvoirDisparu()
        {
            Assert.That(WebBrowser.Current.ContainsText("L'annonce a été supprimée."));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Envoyer à un ami

        [When(@"Je clique sur envoyer à un ami")]
        public void WhenJeCliqueSurEnvoyerAUnAmi()
        {
            WebBrowser.Current.Link(Find.ByText("Recommander à un ami")).Click();
        }

        [Given(@"Je clique sur Detail")]
        public void GivenJeCliqueSurDetail()
        {
            WebBrowser.Current.Page<RentalPage>().Detail.Click();
        }

        [Then(@"Je dois arriver sur la page d'envoi à un ami")]
        public void ThenJeDoisArriverSurLaPageDEnvoiAUnAmi()
        {
            Assert.That(WebBrowser.Current.Url.Contains("annonces/envoyer-email-ami/"));
            WebBrowser.Current.Close();
        }

        #endregion

        #region Envoyer au propriétaire

        [When(@"Je clique sur envoyer au proprietaire")]
        public void WhenJeCliqueSurEnvoyerAuProprietaire()
        {
            WebBrowser.Current.Link(Find.ByText("Envoyer un e-mail au propriétaire")).Click();
        }

        [Then(@"Je dois arriver sur la page d'envoi au proprietaire")]
        public void ThenJeDoisArriverSurLaPageDEnvoiAuProprietaire()
        {
            Assert.That(WebBrowser.Current.ContainsText("Message Privé"));
            WebBrowser.Current.Close();
        }

        #endregion
    }

    #region RentalPage

    public class RentalPage : Page
    {
        #region SelectList

        public SelectList HeatingType
        {
            get { return Document.SelectList(Find.ById("Rental_HeatingType")); }
        }

        public SelectList GreenHouse
        {
            get { return Document.SelectList(Find.ById("Rental_GreenHouse")); }
        }

        public SelectList Lease_Type
        {
            get { return Document.SelectList(Find.ById("Rental_LeaseType")); }
        }

        public SelectList Rental_Type
        {
            get { return Document.SelectList(Find.ById("Rental_Type")); }
        }

        #endregion

        #region Button

        public Button Validate_Button
        {
            get { return Document.Button(Find.BySelector("input[type='submit']")); }
        }

        public Button Boutton_Rechercher
        {
            get { return Document.Button(Find.ByValue("Rechercher")); }
        }

        #endregion

        #region Link

        public Link Detail
        {
            get { return Document.Link(Find.ByText("Détail")); }
        }

        public Link Editer
        {
            get { return Document.Link(Find.ByText("Editer")); }
        }

        public Link Supprimer
        {
            get { return Document.Link(Find.ByText("Supprimer")); }
        }

        public Link Lien_Administrateur
        {
            get { return Document.Link(Find.ByText("Administrateur")); }
        }

        public Link Lien_Recherche
        {
            get { return Document.Link(Find.ByText("Location")); }
        }

        #endregion

        #region TextField

            #region Search
            
            public TextField MinRate
            {
                get { return Document.TextField(Find.ById("MinRate")); }
            }

            public TextField MaxRate
            {
                get { return Document.TextField(Find.ById("MaxRate")); }
            }

            public TextField MinSurface
            {
                get { return Document.TextField(Find.ById("MinSurface")); }
            }

            public TextField MaxSurface
            {
                get { return Document.TextField(Find.ById("MaxSurface")); }
            }

            public TextField Place
            {
                get { return Document.TextField(Find.ById("placeAutocomplete")); }
            }
            
            #endregion

            #region Form

            public TextField Adress
            {
                get { return Document.TextField(Find.ById("Rental_Address")); }
            }

            public TextField Postal
            {
                get { return Document.TextField(Find.ById("Rental_PostalCode")); }
            }

            public TextField Town
            {
                get { return Document.TextField(Find.ById("Rental_City")); }
            }

            public TextField Country
            {
                get { return Document.TextField(Find.ById("Rental_Country")); }
            }

            public TextField Description
            {
                get { return Document.TextField(Find.ById("Rental_Description")); }
            }

            public TextField AvailableDate
            {
                get { return Document.TextField(Find.ById("Rental_AvailableDate")); }
            }

            public TextField Ref
            {
                get { return Document.TextField(Find.ById("Rental_Reference")); }
            }

            public TextField Rate
            {
                get { return Document.TextField(Find.ById("Rental_Rate")); }
            }

            public TextField Charges
            {
                get { return Document.TextField(Find.ById("Rental_Charges")); }
            }

            public TextField Surface
            {
                get { return Document.TextField(Find.ById("Rental_Surface")); }
            }

            #endregion

        #endregion

        #region Checkbox

        public CheckBox AvailableNow
        {
            get { return Document.CheckBox(Find.ById("Rental_AvailableNow")); }
        }

        public CheckBox Startup
        {
            get { return Document.CheckBox(Find.ById("StartUpFriendly")); }
        }

        public CheckBox New
        {
            get { return Document.CheckBox(Find.ById("New")); }
        }

        public CheckBox Parking
        {
            get { return Document.CheckBox(Find.ById("Parking")); }
        }

        public CheckBox Quiet
        {
            get { return Document.CheckBox(Find.ById("Quiet")); }
        }

        #endregion
    }

    #endregion
}
