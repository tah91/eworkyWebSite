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
		Faq,
		Contact
	}

    public enum DashboardMenu
    {
        Home,
        Community,
        Historic,
        Profil
    }

    public enum BackofficeMenu
    {
        Home,
        Clients,
        Profil
    }
}
