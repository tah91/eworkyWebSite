Feature: Ajout
	In order to search a working place
	As a user
	I want to be able to view the list of localisations

@Ajout
Scenario: Ajout Erreur
	Given Je vais dans la page d'ajout
	When Je clique sur Envoyer
	Then Il doit y avoir des messages d'erreur


@Ajout
Scenario: Ajout mauvais pattern
	Given Je vais dans la page d'ajout
		And Je remplis mal le champs Email
		And je remplis mal les horaires
	When Je clique sur Envoyer
	Then Il doit y avoir des messages d'erreur

@Ajout
Scenario: Ajout complet
	Given Je vais dans la page d'ajout
		And Je remplis la partie informations générales
		And Je coche quelques checkbox
		And je remplis Acces
		And Je remplis Horaires
	When Je clique sur Envoyer
	Then Je dois avoir le détail des informations générales
		And Je dois avoir le texte des checkbox
		And Je dois avoir le détail de l'acces
		And Je dois avoir le détail des horaires

@Ajout
Scenario: Ajout complet 2
	Given Je vais dans la page d'ajout
		And Je remplis la partie informations générales
		And Je coche quelques checkbox 2
		And je remplis Acces
		And Je remplis Horaires 2
	When Je clique sur Envoyer
	Then Je dois avoir le détail des informations générales
		And Je dois avoir le texte des checkbox 2
		And Je dois avoir le détail de l'acces
		And Je dois avoir le détail des horaires 2

@Ajout
Scenario: Ajout de lieu deja present
	Given Je vais dans la page d'ajout
		And Je remplis lieux deja rentré
	When Je clique sur Envoyer
	Then Il doit y avoir des messages d'erreur

