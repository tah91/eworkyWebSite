Feature: Vérif Page
	In order to search a working place
	As a user
	I want to be able to view the list of localisations

@Footer
Scenario: Mentions légale
	Given Je vais dans la page d'acceuil
	When Je vais dans la page mention légal
	Then Je dois avoir la page mention légal
		And Text présent sur la page: Omnicentre SAS

@Footer
Scenario: Jobs
	Given Je vais dans la page d'acceuil
	When Je vais dans la page de jobs
	Then Je dois avoir la page jobs
		And Text présent sur la page: L’esprit eWorky
		And Text présent sur la page: eWorky recherche des développeurs
		And Text présent sur la page: stagiaire en communication


@Footer
Scenario: Presse
	Given Je vais dans la page d'acceuil
	When Je vais dans la page presse
	Then Je dois avoir la page presse
		And Text présent sur la page: Vous êtes journaliste

@Footer
Scenario: CGU
	Given Je vais dans la page d'acceuil
	When Je vais dans la page CGU
	Then Je dois avoir la page CGU
		And Text présent sur la page: Informations sur le fournisseur

@Footer
Scenario: FAQ
	Given Je vais dans la page d'acceuil
	When Je vais dans la page FAQ
	Then Je dois avoir la page FAQ
		And Text présent sur la page: A quoi sert eWorky ?

@Footer
Scenario: Qui sommes nous
	Given Je vais dans la page d'acceuil
	When Je vais dans la page Qui sommes nous
	Then Je dois avoir la page Qui sommes nous
		And Text présent sur la page: Supélec ou de Centrale Lyon

@Footer
Scenario: Contact
	Given Je vais dans la page d'acceuil
	When Je vais dans la page Contact
	Then Je dois avoir la page Contact
		And Text présent sur la page: Message