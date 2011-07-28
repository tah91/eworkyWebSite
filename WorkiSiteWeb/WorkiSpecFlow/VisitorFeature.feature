Feature: Page Visitor
	In order to search a working place
	As a user
	I want to be able to view the list of localisations

@visitor
Scenario: Connexion Erreur
	Given Je vais dans la page Visitor
	When Je clique sur connexion 
	Then Il doit y avoir des messages d'erreur

@visitor
Scenario: Connexion Errone
	Given Je vais dans la page Visitor
		And Je rentre mon identifiant
		And je rentre un mauvais mot de passe
	When Je clique sur connexion 
	Then Il doit y avoir des messages d'erreur

@visitor
Scenario: Connexion Admin
	Given Je vais dans la page Visitor
		And Je rentre mon identifiant
		And Je rentre mon mot de passe
	When Je clique sur connexion 
	Then Je dois arriver sur la page d'accueil

@visitor
Scenario: Mot de passe oublié
	Given Je vais dans la page Visitor
	When Je clique sur mot de passe oublié 
	Then Je dois arriver sur la page de reset
		And Je dois avoir le message envoie du nouveau mot de passe

@visitor
Scenario: Demande d'inscription Erreur
	Given Je vais dans la page Visitor
	When Je clique sur Go
	Then Il doit y avoir des messages d'erreur


@visitor
Scenario: Demande d'inscription
	Given Je vais dans la page Visitor
		And je rentre une adresse valide
	When Je clique sur Go
	Then Je dois arriver sur demande réussi
		And Le bon texte de demande inscription réussi doit être présent