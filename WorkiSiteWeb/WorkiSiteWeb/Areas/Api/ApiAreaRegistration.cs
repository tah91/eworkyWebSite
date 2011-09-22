using System.Web.Mvc;
using Worki.Rest.Routing;

namespace Worki.Web.Areas.Api
{
    public class ApiAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Api";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Api_default",
                "Api/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
				new string[] { "Worki.Web.Areas.Api.Controllers" }
            );

            // Register REST api routes within path /api
            //RestRoutes.RegisterRoutes(context, "/api");
        }
    }
}
