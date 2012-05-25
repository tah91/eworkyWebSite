using System.Web.Mvc;

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
            context.MapRoute(
                "Widget_default",
                "Widget/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
