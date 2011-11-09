Feature: Page WelcomePeople
	In order to search a working place
	As a user
	I want to be able to view the list of localisations

@WelcomePeople
Scenario: Ajout New Welcome People
	Given Je me connecte à eWorky
		And Je vais sur la page Welcome People
	When Je remplis le formulaire
		And Je valide le formulaire welcome people
	Then Je dois retrouver ce que j'ai remplis

@WelcomePeople
Scenario: Editer Welcome People
	Given Je me connecte à eWorky
		And Je vais sur la page Welcome People
	When Je modifie le formulaire
		And Je valide save welcome people
	Then Je dois retrouver ce que j'ai modifié

@WelcomePeople
Scenario: Supprimer Welcome People
	Given Je me connecte à eWorky
		And Je vais sur la page Welcome People
	When Je clique sur Supprimer
	Then Welcome people est supprimé

@WelcomePeople
Scenario: Erreur Ajout Welcome People
	Given Je me connecte à eWorky
		And Je vais sur la page Welcome People
	When Je clique sur ajouter
		And Je valide le formulaire welcome people
	Then Il doit y avoir des messages d'erreur