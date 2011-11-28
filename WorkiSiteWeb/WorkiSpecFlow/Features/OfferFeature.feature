Feature: Offer
	In order to check out my booking and quotation
	As a user
	I want to be able to view the list of booking/quotation

@Offer
Scenario: Aller sur la page backoffice
	Given Je me connecte à eWorky
	When Je clique sur bo
	Then Je dois arriver sur la page de backoffice

@Offer
Scenario: Aller sur la page backoffice Client
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Client
	Then Je dois arriver sur la page de backoffice client

@Offer
Scenario: Aller sur la page backoffice Profil
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Profil
	Then Je dois arriver sur la page de backoffice profil

@Offer
Scenario: Aller sur la page backoffice Espaces de travail
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Espaces de travail
	Then Je dois arriver sur la page de backoffice Espaces de travail

@Offer
Scenario: Aller sur la page backoffice Booking
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Reservation en cours
	Then Je dois arriver sur la page de backoffice Booking

@Offer
Scenario: Aller sur la page backoffice Devis
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Demande de devis
	Then Je dois arriver sur la page de backoffice Devis

@Offer
Scenario: Je dois avoir plusieur reservations en cours
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Reservation en cours
	Then Je dois avoir des résultats

@Offer
Scenario: Je dois avoir plusieur demande de devis
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Demande de devis
	Then Je dois avoir des résultats

@Offer
Scenario: Je dois avoir des lieux ajoutés
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Espaces de travail
	Then Je dois avoir des résultats de lieu

@Offer
Scenario: Je dois avoir des offres sur un des lieux
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur un des lieux
	Then Je dois avoir des offres associées à ce lieu

@Offer
Scenario: Je créé des offres sur un lieu
	Given Je me connecte à eWorky
		And Je vais dans la page admin
	When J'edite un lieu
		And je sélectionne une offre
		And je remplis des champs pour l'offre
		And je valide
	Then Je dois avoir l'offre présente et conforme