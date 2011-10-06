using System.Web.Mvc;

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
            context.MapRoute(
                "",
                "mobile",
                new { controller = "Home", action = "Index" },
                new string[] { "Worki.Web.Areas.Mobile.Controllers" }
            );

            context.MapRoute(
                "", // Nom d'itinéraire
                "lieu-de-travail/{action}/{id}", // URL avec des paramètres
                new { controller = "Localisation", action = "Index", id = UrlParameter.Optional }, // Paramètres par défaut
                new string[] { "Worki.Web.Areas.Mobile.Controllers" }
            );

            context.MapRoute(
                "Mobile_default",
                "Mobile/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
