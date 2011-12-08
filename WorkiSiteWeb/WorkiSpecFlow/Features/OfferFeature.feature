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
Scenario: 11 Je créé des offres sur un lieu
	Given Je me connecte à eWorky
		And Je vais dans la page admin
	When J'edite un lieu
		And je sélectionne une offre
		And je remplis des champs pour l'offre
		And je valide
	Then Je dois avoir l'offre présente et conforme

@Offer
Scenario: 12 Réserver une offre
	Given Je me connecte à eWorky
	When Je réserve une offre
		And Je clique sur bo
		And Je clique sur Reservation en cours
	Then Je dois avoir la demande de réservation côté utilisateur et gérant

@Offer
Scenario: 13 Demande de devis
	Given Je me connecte à eWorky
	When Je demande un devis
		And Je clique sur mon profil
		And Je clique sur Demande de devis
	Then Je dois avoir la demande de devis côté utilisateur et gérant

@Offer
Scenario: 14 Je dois avoir plusieur demande de devis
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Demande de devis
	Then Je dois avoir des résultats

@Offer
Scenario: 15 Je dois avoir plusieur reservations en cours
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Reservation en cours
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
Scenario: Annuler une demande de devis
	Given Je me connecte à eWorky
	When Je clique sur mon profil
		And Je clique sur Demande de devis
		And je clique sur Annuler
	Then Je dois avoir la demande de devis annuler

@Offer
Scenario: Annuler une demande de réservartion
	Given Je me connecte à eWorky
	When Je clique sur mon profil
		And Je clique sur Reservation en cours
		And je clique sur Annuler
	Then Je dois avoir la demande de réservation annuler

@Offer
Scenario: Accepter une demande de réservation
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Reservation en cours
		And je clique sur Accepter
	Then Je dois avoir la demande de réservation Accepter

@Offer
Scenario: Accepter une demande de devis
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Demande de devis
		And je clique sur Contacter
	Then Je dois avoir la demande de devis Accepter

@Offer
Scenario: Refuser une demande de réservation
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Reservation en cours
		And je clique sur Refuser
	Then Je dois avoir la demande de réservation Refuser

@Offer
Scenario: Refuser une demande de devis
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Demande de devis
		And je clique sur Refuser
	Then Je dois avoir la demande de devis Refuser

@Offer
Scenario: Mettre En ligne/Hors ligne une offre
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur un des lieux
		And je clique sur une des Offres
		And Je met l'offre en ligne/hors ligne
		And je lance la recherche
	Then Je dois avoir ou pas le résultat à l'écran

@Offer
Scenario: changer les informations de paiement
	Given Je me connecte à eWorky
	When Je clique sur bo
		And Je clique sur Options
		And je remplis adresse PayPal
	Then Je dois le message de confirmation