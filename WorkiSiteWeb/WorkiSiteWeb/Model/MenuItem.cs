using System.Web.Mvc;
using System;
using System.Collections.Generic;

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

	public class DropDownItem
	{
		public string DisplayName { get; set; }
		public string Link { get; set; }
	}

	public class DropDownModel
	{
		public string Title { get; set; }
		public List<DropDownItem> Items { get; set; }
	}
}