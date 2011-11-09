Feature: Commentaires
	In order to search a working place
	As a user
	I want to be able to view the list of localisations

@Détails
Scenario: Je poste un commentaire
	Given Je me connecte à eWorky
		And Je vais dans la page Admin
	When Je clique sur detail
		And Je met une note et un commentaire
	Then Je dois retrouver le commentaire et la note

@Détails
Scenario: Supprimer le commentaire
	Given Je me connecte à eWorky
		And Je vais dans la page Admin
	When Je clique sur detail
		And Je supprime mon commentaire
	Then Le commentaire a été supprimé

@Détails
Scenario: Profil dans commentaire
	Given Je me connecte à eWorky
	When Je clique sur le profil
		And Je clique sur Mes commentaires
	Then Je dois retrouver mon commentaire