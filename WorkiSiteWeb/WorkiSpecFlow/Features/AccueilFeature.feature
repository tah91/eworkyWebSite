Feature: Accueil
	In order to search a working place
	As a user
	I want to be able to view the list of localisations

@Accueil
Scenario: Acceuil lien Recherche 
	Given Je vais dans la page d'acceuil
	When Je clique sur Recherche
	Then Je dois arriver sur la page de recherche
	
@Accueil
Scenario: Acceuil lien Ajout
	Given Je vais dans la page d'acceuil
	When Je clique sur Ajout
	Then Je dois arriver sur la page de Ajout

@Accueil
Scenario: Acceuil plus de critères
	Given Je vais dans la page d'acceuil
	When Je clique sur plus de critères
	Then Je dois arriver sur la page de recherche

@Accueil
Scenario: Recherche Salon d'affaire
	Given Je vais dans la page d'acceuil
		And Je tappe Paris dans la zone de recherche
		And Je selectionne Salon d'affaire
	When Je clique sur Rechercher
	Then Il doit y avoir plus de 5 resultats

@Accueil
Scenario: Accueil a la une
	Given Je vais dans la page d'acceuil
	Then Je dois avoir A la une	