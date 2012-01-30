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
		Contact
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

	public enum LocalisationMenu
	{
		Home,
		Offers
	}
}
