using System.Web.Mvc;
using System;
using System.Collections.Generic;
using Worki.Data.Models;

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
		public const string ProfilDD = "profilDropdown";
		public const string OfferDD = "offerDropdown";

		public string Id { get; set; }
		public string Title { get; set; }
		public List<DropDownItem> Items { get; set; }
	}

	public class OfferDropDownModel
	{
		public Offer Offer { get; set; }
		public Func<Offer, string> UrlMaker { get; set; }
	}
}