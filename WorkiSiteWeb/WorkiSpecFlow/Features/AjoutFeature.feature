Feature: Ajout
	In order to search a working place
	As a user
	I want to be able to view the list of localisations

@Ajout
Scenario: Ajout Erreur
	Given Je me connecte à eWorky
		And Je vais dans la page d'ajout
	When Je clique sur Envoyer
	Then Il doit y avoir des messages d'erreur

@Ajout
Scenario: Ajout mauvais pattern
	Given Je me connecte à eWorky
		And Je vais dans la page d'ajout
		And Je remplis mal le champs Email
		And je remplis mal les horaires
	When Je clique sur Envoyer
	Then Il doit y avoir des messages d'erreur

@Ajout
Scenario: Ajout de lieu deja present
	Given Je me connecte à eWorky
		And Je vais dans la page d'ajout
		And Je remplis lieux deja rentré
	When Je clique sur Envoyer
	Then Il doit y avoir des messages d'erreur

@Ajout
Scenario: Créer une fiche de lieu Gratuit
	Given Je me connecte à eWorky
		And Je vais dans la page d'ajout de lieux gratuits
		And Je remplis les champs
	When Je clique sur Envoyer
	Then Je dois retrouver les infos

@Ajout
Scenario: Editer une fiche de lieu Gratuit
	Given Je me connecte à eWorky
		And Je vais dans la page admin
		And Je clique sur editer
	When Je change les champs
		And Je clique sur Envoyer
	Then Je dois avoir retrouver les infos modifiées

@Ajout
Scenario: Supprimer une fiche de lieu Gratuit
	Given Je me connecte à eWorky
		And Je vais dans la page admin
	When Je clique sur Supprimer
	Then La fiche de lieu est supprimée

@Ajout
Scenario: Créer une fiche de lieu Payant
	Given Je me connecte à eWorky
		And Je vais dans la page d'ajout de lieux payant
		And Je remplis les champs 2
	When Je clique sur Envoyer
	Then Je dois retrouver les infos 2

@Ajout
Scenario: Editer une fiche de lieu Payant
	Given Je me connecte à eWorky
		And Je vais dans la page admin
		And Je clique sur editer
	When Je change les champs 2
		And Je clique sur Envoyer
	Then Je dois avoir retrouver les infos modifiées 2

@Ajout
Scenario: Supprimer une fiche de lieu Payant
	Given Je me connecte à eWorky
		And Je vais dans la page admin
	When Je clique sur Supprimer
	Then La fiche de lieu est supprimée