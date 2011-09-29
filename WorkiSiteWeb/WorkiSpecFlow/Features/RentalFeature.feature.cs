﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.6.1.0
//      SpecFlow Generator Version:1.6.0.0
//      Runtime Version:4.0.30319.468
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
    [NUnit.Framework.DescriptionAttribute("Rental")]
    public partial class RentalFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "RentalFeature.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Rental", "In order to search a rental\r\nAs a user\r\nI want to be able to view the list of ren" +
                    "tal", GenerationTargetLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Page Recherche")]
        [NUnit.Framework.CategoryAttribute("Rental")]
        public virtual void PageRecherche()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Page Recherche", new string[] {
                        "Rental"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("Je vais dans la page d\'acceuil");
#line 9
 testRunner.When("Je clique sur administrateur");
#line 10
  testRunner.And("Je clique sur recherche location");
#line 11
 testRunner.Then("Je dois arriver sur la page de recherche location");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Erreur Recherche")]
        [NUnit.Framework.CategoryAttribute("Rental")]
        public virtual void ErreurRecherche()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Erreur Recherche", new string[] {
                        "Rental"});
#line 14
this.ScenarioSetup(scenarioInfo);
#line 15
 testRunner.Given("Je vais dans la page d\'acceuil");
#line 16
 testRunner.When("Je clique sur administrateur");
#line 17
  testRunner.And("Je clique sur recherche location");
#line 18
  testRunner.And("Je clique sur Rechercher location");
#line 19
 testRunner.Then("Je dois arriver sur la page d\'erreur");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Lancer une Recherche")]
        [NUnit.Framework.CategoryAttribute("Rental")]
        public virtual void LancerUneRecherche()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Lancer une Recherche", new string[] {
                        "Rental"});
#line 22
this.ScenarioSetup(scenarioInfo);
#line 23
 testRunner.Given("Je vais dans la page d\'acceuil");
#line 24
 testRunner.When("Je clique sur administrateur");
#line 25
  testRunner.And("Je clique sur recherche location");
#line 26
  testRunner.And("Je remplis des champs");
#line 27
  testRunner.And("Je clique sur Rechercher location");
#line 28
 testRunner.Then("Je dois arriver sur la page de resultat location");
#line 29
  testRunner.And("Tous les résultats doivent respecter les critères");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Recherche Paris")]
        [NUnit.Framework.CategoryAttribute("Rental")]
        public virtual void RechercheParis()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Recherche Paris", new string[] {
                        "Rental"});
#line 32
this.ScenarioSetup(scenarioInfo);
#line 33
 testRunner.Given("Je vais dans la page d\'acceuil");
#line 34
 testRunner.When("Je clique sur administrateur");
#line 35
  testRunner.And("Je clique sur recherche location");
#line 36
  testRunner.And("Je remplis le champs location avec Paris");
#line 37
  testRunner.And("Je clique sur Rechercher location");
#line 38
 testRunner.Then("Je dois avoir au moins 2 pages de résultat");
#line hidden
            testRunner.CollectScenarioErrors();
        }
    }
}
#endregion
