using System.Web.Mvc;
using Worki.Infrastructure;

namespace Worki.Web.Areas.Admin
{
	public class AdminAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Admin";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.CultureMapRoute(
				"Admin_default",
				"Admin/{controller}/{action}/{id}",
				new { action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
