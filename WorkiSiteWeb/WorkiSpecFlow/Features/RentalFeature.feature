Feature: Rental
	In order to search a rental
	As a user
	I want to be able to view the list of rental

@Rental
Scenario: Lancer une Recherche
	Given Je vais dans la page recherche
	When Je remplis des champs
		And Je clique sur Rechercher location
	Then Je dois arriver sur la page de resultat location
		And Tous les résultats doivent respecter les critères

@Rental
Scenario: Recherche Paris
	Given Je vais dans la page recherche
	When Je remplis le champs location avec Paris
		And Je clique sur Rechercher location
	Then Je dois avoir au moins 2 pages de résultat

@Rental
Scenario: Créer une fiche de location à Paris
	Given Je me connecte à eWorky
		And Je vais sur la page de création de location
	When Je remplis le formulaire de location
		And Je clique sur Valider
	Then Je dois avoir arriver sur la page détail
		And Je dois retrouver les bonnes informations

@Rental
Scenario: Editer une fiche de location
	Given Je me connecte à eWorky
		And Je vais sur la page d'édition de location
	When Je modifie le formulaire de location
		And Je clique sur Valider
	Then Les informations doivent avoir changées

@Rental
Scenario: Supprimer une fiche de location
	Given Je me connecte à eWorky
		And Je vais sur la page admin des locations
	When Je clique sur supprimer de la derniere location
		And Je valide la suppression
	Then La location doit avoir disparu

@Rental
Scenario: Envoyer à un ami
	Given Je me connecte à eWorky
		And Je vais sur la page admin des locations
		And Je clique sur Detail
	When Je clique sur envoyer à un ami
	Then Je dois arriver sur la page d'envoi à un ami

@Rental
Scenario: Envoyer au propriétaire
	Given Je me connecte à eWorky
		And Je vais sur la page admin des locations
		And Je clique sur Detail
	When Je clique sur envoyer au proprietaire
	Then Je dois arriver sur la page d'envoi au proprietaire