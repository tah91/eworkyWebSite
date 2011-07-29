Feature: Details Localisation
	In order to search a working place
	As a user
	I want to be able to view the list of localisations

@Détails
Scenario: Je poste un commentaire
	Given Je vais dans la page Recherche
		And Je tappe Paris dans la zone de recherche
	When Je clique sur rechercher
		And Je clique sur la fiche Le Big Ben Bar
		And Je met une note
		And Je poste un commentaire
	Then Je dois retrouver le commentaire

@Détails
Scenario: Profil dans commentaire
	Given Je vais dans la page Recherche
		And Je tappe Paris dans la zone de recherche
	When Je clique sur rechercher
		And Je clique sur la fiche Le Big Ben Bar
		And Je met une note
		And Je poste un commentaire
		And Je clique sur le profil
	Then Je dois arriver sur mon profil