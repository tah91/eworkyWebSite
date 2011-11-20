using System.Web.Mvc;

namespace Worki.Web.Model
{
	public class MenuItem
	{
		public MenuItem()
		{
			Selected = false;
		}

		public MvcHtmlString Link { get; set; }
		public bool Selected { get; set; }
	}
}