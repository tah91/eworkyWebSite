using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Worki.Infrastructure;
using System.Collections.Generic;
using System;

namespace Worki.Data.Models
{
	public enum AboutPages
	{
		WhoAreWe,
		Press,
		Jobs,
		Cgu,
		Legal,
        Contact,
        Team
	}

	public enum TutorialPages
	{
		HowItWorks,
		UserTutorial,
		OwnerTutorial,
        SharedOffice,
        Faq
	}

    public enum DashboardMenu
    {
        Home,
        Community,
        Historic,
        Profil
    }

	public enum AdminMenu
    {
        Sheet,
        Member,
        Activity,
        Moderation
    }

    public enum BackofficeMenu
    {
        Home,
        Clients,
        Profil
    }

	public enum LocalisationMainMenu
	{
		Home,
		Schedule,
		Clients,
		Offers
	}

	public enum LocalisationMenu
	{
		Home,
		Bookings,
		Quotations,
		Edit
	}

	public enum OfferMenu
	{
		Config,
		Edit,
		Booking,
		Quotation,
		Schedule,
		Prices
	}
}
