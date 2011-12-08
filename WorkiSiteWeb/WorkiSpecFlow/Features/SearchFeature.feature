Feature: Recherche Lieu
	In order to search a working place
	As a user
	I want to be able to view the list of localisations

@search
Scenario: Recherche Simple
	Given Je vais dans la page Recherche
		And Je tappe Paris dans la zone de recherche
	When Je clique sur rechercher
	Then Il doit y avoir plus de 1 resultats

@search
Scenario: Recherche Detailer
	Given Je vais dans la page Recherche
		And Je tappe Paris dans la zone de recherche
		And Je selectionne Salon d'affaire
	When Je clique sur rechercher
	Then Il doit y avoir plus de 1 resultats
	
@search
Scenario: Description Etudiant
	Given Je vais dans la page Recherche
	When Je clique sur Etudiant
	Then Je dois avoir la description Etudiant

@search
Scenario: Description Nomade
	Given Je vais dans la page Recherche
	When Je clique sur Nomade
	Then Je dois avoir la description Nomade

@search
Scenario: Description Teletravailleur
	Given Je vais dans la page Recherche
	When Je clique sur Teletravailleur
	Then Je dois avoir la description Teletravailleur

@search
Scenario: Description Entrepreneur
	Given Je vais dans la page Recherche
	When Je clique sur Entrepreneur
	Then Je dois avoir la description Entrepreneur

@search
Scenario: Description GrandCompte
	Given Je vais dans la page Recherche
	When Je clique sur GrandCompte
	Then Je dois avoir la description GrandCompte

@search
Scenario: Description Indépendant
	Given Je vais dans la page Recherche
	When Je clique sur Indépendant
	Then Je dois avoir la description Indépendant

@search
Scenario: Présence de lieux A la Une
	Given Je vais dans la page d'acceuil
	When Je clique sur Plus de critère
	Then Il doit y avoir des lieux dans le bloc A la Une

@search
Scenario: Faire Défiler les pages de résultats
	Given Je vais dans la page d'acceuil
		And Je tappe Paris dans la zone de recherche
	When Je clique sur rechercher
		And Je fais défiler les pages
	Then Je dois avoir parcouru les pages