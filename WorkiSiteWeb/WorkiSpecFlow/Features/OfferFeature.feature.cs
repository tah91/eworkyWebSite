﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.6.1.0
//      SpecFlow Generator Version:1.6.0.0
//      Runtime Version:4.0.30319.488
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
namespace Worki.SpecFlow.Features
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.6.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Offer")]
    public partial class OfferFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "OfferFeature.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Offer", "In order to check out my booking and quotation\r\nAs a user\r\nI want to be able to v" +
                    "iew the list of booking/quotation", GenerationTargetLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Aller sur la page backoffice")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void AllerSurLaPageBackoffice()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Aller sur la page backoffice", new string[] {
                        "Offer"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("Je me connecte à eWorky");
#line 9
 testRunner.When("Je clique sur bo");
#line 10
 testRunner.Then("Je dois arriver sur la page de backoffice");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Aller sur la page backoffice Client")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void AllerSurLaPageBackofficeClient()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Aller sur la page backoffice Client", new string[] {
                        "Offer"});
#line 13
this.ScenarioSetup(scenarioInfo);
#line 14
 testRunner.Given("Je me connecte à eWorky");
#line 15
 testRunner.When("Je clique sur bo");
#line 16
  testRunner.And("Je clique sur Client");
#line 17
 testRunner.Then("Je dois arriver sur la page de backoffice client");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Aller sur la page backoffice Profil")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void AllerSurLaPageBackofficeProfil()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Aller sur la page backoffice Profil", new string[] {
                        "Offer"});
#line 20
this.ScenarioSetup(scenarioInfo);
#line 21
 testRunner.Given("Je me connecte à eWorky");
#line 22
 testRunner.When("Je clique sur bo");
#line 23
  testRunner.And("Je clique sur Profil");
#line 24
 testRunner.Then("Je dois arriver sur la page de backoffice profil");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Aller sur la page backoffice Espaces de travail")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void AllerSurLaPageBackofficeEspacesDeTravail()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Aller sur la page backoffice Espaces de travail", new string[] {
                        "Offer"});
#line 27
this.ScenarioSetup(scenarioInfo);
#line 28
 testRunner.Given("Je me connecte à eWorky");
#line 29
 testRunner.When("Je clique sur bo");
#line 30
  testRunner.And("Je clique sur Espaces de travail");
#line 31
 testRunner.Then("Je dois arriver sur la page de backoffice Espaces de travail");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Aller sur la page backoffice Booking")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void AllerSurLaPageBackofficeBooking()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Aller sur la page backoffice Booking", new string[] {
                        "Offer"});
#line 34
this.ScenarioSetup(scenarioInfo);
#line 35
 testRunner.Given("Je me connecte à eWorky");
#line 36
 testRunner.When("Je clique sur bo");
#line 37
  testRunner.And("Je clique sur Reservation en cours");
#line 38
 testRunner.Then("Je dois arriver sur la page de backoffice Booking");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Aller sur la page backoffice Devis")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void AllerSurLaPageBackofficeDevis()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Aller sur la page backoffice Devis", new string[] {
                        "Offer"});
#line 41
this.ScenarioSetup(scenarioInfo);
#line 42
 testRunner.Given("Je me connecte à eWorky");
#line 43
 testRunner.When("Je clique sur bo");
#line 44
  testRunner.And("Je clique sur Demande de devis");
#line 45
 testRunner.Then("Je dois arriver sur la page de backoffice Devis");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("11 Je créé des offres sur un lieu")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void _11JeCreeDesOffresSurUnLieu()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("11 Je créé des offres sur un lieu", new string[] {
                        "Offer"});
#line 48
this.ScenarioSetup(scenarioInfo);
#line 49
 testRunner.Given("Je me connecte à eWorky");
#line 50
  testRunner.And("Je vais dans la page admin");
#line 51
 testRunner.When("J\'edite un lieu");
#line 52
  testRunner.And("je sélectionne une offre");
#line 53
  testRunner.And("je remplis des champs pour l\'offre");
#line 54
  testRunner.And("je valide");
#line 55
 testRunner.Then("Je dois avoir l\'offre présente et conforme");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("12 Réserver une offre")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void _12ReserverUneOffre()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("12 Réserver une offre", new string[] {
                        "Offer"});
#line 58
this.ScenarioSetup(scenarioInfo);
#line 59
 testRunner.Given("Je me connecte à eWorky");
#line 60
 testRunner.When("Je réserve une offre");
#line 61
  testRunner.And("Je clique sur bo");
#line 62
  testRunner.And("Je clique sur Reservation en cours");
#line 63
 testRunner.Then("Je dois avoir la demande de réservation côté utilisateur et gérant");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("13 Demande de devis")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void _13DemandeDeDevis()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("13 Demande de devis", new string[] {
                        "Offer"});
#line 66
this.ScenarioSetup(scenarioInfo);
#line 67
 testRunner.Given("Je me connecte à eWorky");
#line 68
 testRunner.When("Je demande un devis");
#line 69
  testRunner.And("Je clique sur mon profil");
#line 70
  testRunner.And("Je clique sur Demande de devis");
#line 71
 testRunner.Then("Je dois avoir la demande de devis côté utilisateur et gérant");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("14 Je dois avoir plusieur demande de devis")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void _14JeDoisAvoirPlusieurDemandeDeDevis()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("14 Je dois avoir plusieur demande de devis", new string[] {
                        "Offer"});
#line 74
this.ScenarioSetup(scenarioInfo);
#line 75
 testRunner.Given("Je me connecte à eWorky");
#line 76
 testRunner.When("Je clique sur bo");
#line 77
  testRunner.And("Je clique sur Demande de devis");
#line 78
 testRunner.Then("Je dois avoir des résultats");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("15 Je dois avoir plusieur reservations en cours")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void _15JeDoisAvoirPlusieurReservationsEnCours()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("15 Je dois avoir plusieur reservations en cours", new string[] {
                        "Offer"});
#line 81
this.ScenarioSetup(scenarioInfo);
#line 82
 testRunner.Given("Je me connecte à eWorky");
#line 83
 testRunner.When("Je clique sur bo");
#line 84
  testRunner.And("Je clique sur Reservation en cours");
#line 85
 testRunner.Then("Je dois avoir des résultats");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Je dois avoir des lieux ajoutés")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void JeDoisAvoirDesLieuxAjoutes()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Je dois avoir des lieux ajoutés", new string[] {
                        "Offer"});
#line 88
this.ScenarioSetup(scenarioInfo);
#line 89
 testRunner.Given("Je me connecte à eWorky");
#line 90
 testRunner.When("Je clique sur bo");
#line 91
  testRunner.And("Je clique sur Espaces de travail");
#line 92
 testRunner.Then("Je dois avoir des résultats de lieu");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Je dois avoir des offres sur un des lieux")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void JeDoisAvoirDesOffresSurUnDesLieux()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Je dois avoir des offres sur un des lieux", new string[] {
                        "Offer"});
#line 95
this.ScenarioSetup(scenarioInfo);
#line 96
 testRunner.Given("Je me connecte à eWorky");
#line 97
 testRunner.When("Je clique sur bo");
#line 98
  testRunner.And("Je clique sur un des lieux");
#line 99
 testRunner.Then("Je dois avoir des offres associées à ce lieu");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Annuler une demande de devis")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void AnnulerUneDemandeDeDevis()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Annuler une demande de devis", new string[] {
                        "Offer"});
#line 102
this.ScenarioSetup(scenarioInfo);
#line 103
 testRunner.Given("Je me connecte à eWorky");
#line 104
 testRunner.When("Je clique sur mon profil");
#line 105
  testRunner.And("Je clique sur Demande de devis");
#line 106
  testRunner.And("je clique sur Annuler");
#line 107
 testRunner.Then("Je dois avoir la demande de devis annuler");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Annuler une demande de réservartion")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void AnnulerUneDemandeDeReservartion()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Annuler une demande de réservartion", new string[] {
                        "Offer"});
#line 110
this.ScenarioSetup(scenarioInfo);
#line 111
 testRunner.Given("Je me connecte à eWorky");
#line 112
 testRunner.When("Je clique sur mon profil");
#line 113
  testRunner.And("Je clique sur Reservation en cours");
#line 114
  testRunner.And("je clique sur Annuler");
#line 115
 testRunner.Then("Je dois avoir la demande de réservation annuler");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Accepter une demande de réservation")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void AccepterUneDemandeDeReservation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Accepter une demande de réservation", new string[] {
                        "Offer"});
#line 118
this.ScenarioSetup(scenarioInfo);
#line 119
 testRunner.Given("Je me connecte à eWorky");
#line 120
 testRunner.When("Je clique sur bo");
#line 121
  testRunner.And("Je clique sur Reservation en cours");
#line 122
  testRunner.And("je clique sur Accepter");
#line 123
 testRunner.Then("Je dois avoir la demande de réservation Accepter");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Accepter une demande de devis")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void AccepterUneDemandeDeDevis()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Accepter une demande de devis", new string[] {
                        "Offer"});
#line 126
this.ScenarioSetup(scenarioInfo);
#line 127
 testRunner.Given("Je me connecte à eWorky");
#line 128
 testRunner.When("Je clique sur bo");
#line 129
  testRunner.And("Je clique sur Demande de devis");
#line 130
  testRunner.And("je clique sur Contacter");
#line 131
 testRunner.Then("Je dois avoir la demande de devis Accepter");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Refuser une demande de réservation")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void RefuserUneDemandeDeReservation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Refuser une demande de réservation", new string[] {
                        "Offer"});
#line 134
this.ScenarioSetup(scenarioInfo);
#line 135
 testRunner.Given("Je me connecte à eWorky");
#line 136
 testRunner.When("Je clique sur bo");
#line 137
  testRunner.And("Je clique sur Reservation en cours");
#line 138
  testRunner.And("je clique sur Refuser");
#line 139
 testRunner.Then("Je dois avoir la demande de réservation Refuser");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Refuser une demande de devis")]
        [NUnit.Framework.CategoryAttribute("Offer")]
        public virtual void RefuserUneDemandeDeDevis()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Refuser une demande de devis", new string[] {
                        "Offer"});
#line 142
this.ScenarioSetup(scenarioInfo);
#line 143
 testRunner.Given("Je me connecte à eWorky");
#line 144
 testRunner.When("Je clique sur bo");
#line 145
  testRunner.And("Je clique sur Demande de devis");
#line 146
  testRunner.And("je clique sur Refuser");
#line 147
 testRunner.Then("Je dois avoir la demande de devis Refuser");
#line hidden
            testRunner.CollectScenarioErrors();
        }
    }
}
#endregion
