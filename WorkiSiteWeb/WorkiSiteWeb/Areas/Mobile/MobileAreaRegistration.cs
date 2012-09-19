using System.Web.Mvc;
using Worki.Infrastructure;

namespace Worki.Web.Areas.Mobile
{
    public class MobileAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Mobile";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            //home mobile
			context.CultureMapRoute(
                "",
                "mobile",
                new { controller = "Home", action = "Index" },
                new string[] { "Worki.Web.Areas.Mobile.Controllers" }
            );

			context.CultureMapRoute(
                "", // Nom d'itinéraire
                "mobile/workplaces/{action}/{id}", // URL avec des paramètres
                new { controller = "Localisation", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
                new string[] { "Worki.Web.Areas.Mobile.Controllers" }
            );

			context.CultureMapRoute(
                "Mobile_default",
                "mobile/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
