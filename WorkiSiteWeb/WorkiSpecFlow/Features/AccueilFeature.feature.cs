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
    [NUnit.Framework.DescriptionAttribute("Accueil")]
    public partial class AccueilFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "AccueilFeature.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Accueil", "In order to search a working place\r\nAs a user\r\nI want to be able to view the list" +
                    " of localisations", GenerationTargetLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Acceuil lien Recherche")]
        [NUnit.Framework.CategoryAttribute("Accueil")]
        public virtual void AcceuilLienRecherche()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Acceuil lien Recherche", new string[] {
                        "Accueil"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("Je vais dans la page d\'acceuil");
#line 9
 testRunner.When("Je clique sur Recherche");
#line 10
 testRunner.Then("Je dois arriver sur la page de recherche");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Acceuil lien Ajout")]
        [NUnit.Framework.CategoryAttribute("Accueil")]
        public virtual void AcceuilLienAjout()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Acceuil lien Ajout", new string[] {
                        "Accueil"});
#line 13
this.ScenarioSetup(scenarioInfo);
#line 14
 testRunner.Given("Je vais dans la page d\'acceuil");
#line 15
 testRunner.When("Je clique sur Ajout");
#line 16
 testRunner.Then("Je dois arriver sur la page de Ajout");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Acceuil plus de critères")]
        [NUnit.Framework.CategoryAttribute("Accueil")]
        public virtual void AcceuilPlusDeCriteres()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Acceuil plus de critères", new string[] {
                        "Accueil"});
#line 19
this.ScenarioSetup(scenarioInfo);
#line 20
 testRunner.Given("Je vais dans la page d\'acceuil");
#line 21
 testRunner.When("Je clique sur plus de critères");
#line 22
 testRunner.Then("Je dois arriver sur la page de recherche");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Recherche Salon d\'affaire")]
        [NUnit.Framework.CategoryAttribute("Accueil")]
        public virtual void RechercheSalonDAffaire()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Recherche Salon d\'affaire", new string[] {
                        "Accueil"});
#line 25
this.ScenarioSetup(scenarioInfo);
#line 26
 testRunner.Given("Je vais dans la page d\'acceuil");
#line 27
  testRunner.And("Je tappe Paris dans la zone de recherche");
#line 28
  testRunner.And("Je selectionne Salon d\'affaire");
#line 29
 testRunner.When("Je clique sur Rechercher");
#line 30
 testRunner.Then("Il doit y avoir plus de 0 resultats");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Accueil a la une")]
        [NUnit.Framework.CategoryAttribute("Accueil")]
        public virtual void AccueilALaUne()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Accueil a la une", new string[] {
                        "Accueil"});
#line 33
this.ScenarioSetup(scenarioInfo);
#line 34
 testRunner.Given("Je vais dans la page d\'acceuil");
#line 35
 testRunner.Then("Je dois avoir A la une");
#line hidden
            testRunner.CollectScenarioErrors();
        }
    }
}
#endregion
