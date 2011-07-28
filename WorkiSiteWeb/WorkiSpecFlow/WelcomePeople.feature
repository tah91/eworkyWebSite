Feature: Page WelcomePeople
	In order to search a working place
	As a user
	I want to be able to view the list of localisations

@WelcomePeople
Scenario: Ajout New Welcome People
	Given Je vais dans la page New Welcome People
	When Je remplis le formulaire
		And Je valide le formulaire welcome people
	Then Je dois retrouver ce que j'ai remplis

@WelcomePeople
Scenario: Erreur Ajout Welcome People
	Given Je vais dans la page New Welcome People
	When  Je valide le formulaire welcome people
	Then Il doit y avoir des messages d'erreur

@WelcomePeople
Scenario: Editer Welcome People
	Given Je vais sur Editer
	When Je modifie le formulaire
		And Je valide save welcome people
	Then Je dois retrouver ce que j'ai modifié
