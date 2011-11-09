Feature: Page Profil
	In order to search a working place
	As a user
	I want to be able to view the list of localisations

@profil
Scenario: Aller sur son profil
	Given Je me connecte à eWorky
	When Je clique sur mon profil
	Then Je dois arriver sur mon profil

@profil
Scenario: Editer son profil
	Given Je me connecte à eWorky
	When Je clique sur mon profil
		And Je clique sur Editer Profil
		And Je change quelques champs
		And Je valide le formulaire du profil
	Then Je dois avoir les modifications faites

@profil
Scenario: Reinitialiser le profil
	Given Je me connecte à eWorky
	When Je clique sur mon profil
		And Je clique sur Editer Profil
		And Je remet les champs de base
		And Je valide le formulaire du profil
	Then Le profil est reinitialiser

@profil
Scenario: Changer le Mot de passe
	Given Je me connecte à eWorky
	When Je clique sur mon profil
		And Je clique sur Modifier mon mot de passe
	Then Je dois avoir la page de modification de mot de passe