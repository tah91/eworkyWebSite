using System.Web.Mvc;
using Worki.Web.Helpers;

namespace Worki.Web.Areas.Dashboard
{
	public class DashboardAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Dashboard";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.CultureMapRoute(
				"Dashboard_default",
				"Dashboard/{controller}/{action}/{id}",
				new { action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
