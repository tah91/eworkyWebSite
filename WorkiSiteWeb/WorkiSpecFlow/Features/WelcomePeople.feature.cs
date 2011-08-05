// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.6.1.0
//      SpecFlow Generator Version:1.6.0.0
//      Runtime Version:4.0.30319.235
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
    [NUnit.Framework.DescriptionAttribute("Page WelcomePeople")]
    public partial class PageWelcomePeopleFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "WelcomePeople.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Page WelcomePeople", "In order to search a working place\r\nAs a user\r\nI want to be able to view the list" +
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
        [NUnit.Framework.DescriptionAttribute("Ajout New Welcome People")]
        [NUnit.Framework.CategoryAttribute("WelcomePeople")]
        public virtual void AjoutNewWelcomePeople()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Ajout New Welcome People", new string[] {
                        "WelcomePeople"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("Je vais dans la page New Welcome People");
#line 9
 testRunner.When("Je remplis le formulaire");
#line 10
  testRunner.And("Je valide le formulaire welcome people");
#line 11
 testRunner.Then("Je dois retrouver ce que j\'ai remplis");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Erreur Ajout Welcome People")]
        [NUnit.Framework.CategoryAttribute("WelcomePeople")]
        public virtual void ErreurAjoutWelcomePeople()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Erreur Ajout Welcome People", new string[] {
                        "WelcomePeople"});
#line 14
this.ScenarioSetup(scenarioInfo);
#line 15
 testRunner.Given("Je vais dans la page New Welcome People");
#line 16
 testRunner.When("Je valide le formulaire welcome people");
#line 17
 testRunner.Then("Il doit y avoir des messages d\'erreur");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Editer Welcome People")]
        [NUnit.Framework.CategoryAttribute("WelcomePeople")]
        public virtual void EditerWelcomePeople()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Editer Welcome People", new string[] {
                        "WelcomePeople"});
#line 20
this.ScenarioSetup(scenarioInfo);
#line 21
 testRunner.Given("Je vais sur Editer");
#line 22
 testRunner.When("Je modifie le formulaire");
#line 23
  testRunner.And("Je valide save welcome people");
#line 24
 testRunner.Then("Je dois retrouver ce que j\'ai modifié");
#line hidden
            testRunner.CollectScenarioErrors();
        }
    }
}
#endregion
