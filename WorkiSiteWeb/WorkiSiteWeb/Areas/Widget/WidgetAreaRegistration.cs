using System.Web.Mvc;
using Worki.Web.Helpers;
using Worki.Infrastructure;
using Worki.Infrastructure.Helpers;

namespace Worki.Web.Areas.Widget
{
    public class WidgetAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Widget";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.CultureMapRoute(
                "Widget_default",
                "Widget/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );

            var routes = context.Routes;
            for (var i = 0; i < routes.Count; i++)
            {
                var route = routes[i];
                if (route.GetAreaToken() != "Widget")
                    continue;

                routes[i] = new QueryPropagatingRoute(route, MiscHelpers.WidgetConstants.ParamToKeep);
            }            
        }
    }
}
