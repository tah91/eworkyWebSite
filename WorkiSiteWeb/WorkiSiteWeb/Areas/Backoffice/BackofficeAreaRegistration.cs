﻿using System.Web.Mvc;
using Worki.Infrastructure;

namespace Worki.Web.Areas.Backoffice
{
    public class BackofficeAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Backoffice";
            }
        }

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.CultureMapRoute(
				"Backoffice_default",
				"Backoffice/{controller}/{action}/{id}",
				new { action = "Index", id = UrlParameter.Optional }
			);
		}
    }
}
