using System.Web.Mvc;
using Worki.Web.Helpers;

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
        }
    }
}
