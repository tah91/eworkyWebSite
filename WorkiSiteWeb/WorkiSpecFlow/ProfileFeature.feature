Feature: Page Profil
	In order to search a working place
	As a user
	I want to be able to view the list of localisations

@profil
Scenario: Aller sur son profil
	Given Je vais dans la page d'acceuil
	When Je clique sur mon pofil
	Then Je dois arriver sur mon profil

@profil
Scenario: Editer son profil
	Given Je vais dans la page d'acceuil
	When Je clique sur mon pofil
		And Je clique sur Editer Profil
		And Je change quelques champs
		And Je valide le formulaire du profil
	Then Je dois avoir les modifications faites