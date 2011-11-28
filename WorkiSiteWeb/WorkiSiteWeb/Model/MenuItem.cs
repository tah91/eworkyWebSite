using System.Web.Mvc;
using System;

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