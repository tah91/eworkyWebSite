Feature: Rental
	In order to search a rental
	As a user
	I want to be able to view the list of rental

@Rental
Scenario: Page Recherche
	Given Je vais dans la page d'acceuil
	When Je clique sur administrateur
		And Je clique sur recherche location
	Then Je dois arriver sur la page de recherche location

@Rental
Scenario: Erreur Recherche
	Given Je vais dans la page d'acceuil
	When Je clique sur recherche location
		And Je clique sur Rechercher location
	Then Je dois arriver sur la page d'erreur

@Rental
Scenario: Lancer une Recherche
	Given Je vais dans la page d'acceuil
	When Je clique sur recherche location
		And Je remplis des champs
		And Je clique sur Rechercher location
	Then Je dois arriver sur la page de resultat location
		And Tous les résultats doivent respecter les critères

@Rental
Scenario: Recherche Paris
	Given Je vais dans la page d'acceuil
	When Je clique sur recherche location
		And Je remplis le champs location avec Paris
		And Je clique sur Rechercher location
	Then Je dois avoir au moins 2 pages de résultat