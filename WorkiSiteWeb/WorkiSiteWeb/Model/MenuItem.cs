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
        public string Css { get; set; }
        public string Id { get; set; }
	}

	public class DropDownModel
	{
		public const string ProfilDD = "profilDropdown";
		public const string OfferDD = "offerDropdown";
        public const string AdvancedSearchDD = "advancedSearchDropdown";

		public string Id { get; set; }
        public string Title { get; set; }
		public List<DropDownItem> Items { get; set; }
	}

	public enum OfferDropDownFilter
	{
		Booking,
		Quotation,
		None
	}

	public class OfferDropDownModel
	{
		public Offer Offer { get; set; }
		public Func<Offer, string> UrlMaker { get; set; }
		public OfferDropDownFilter Filter { get; set; }
	}

	public enum OfferMenuType
	{
		Config,
		Edit,
		Booking,
		Quotation,
        Schedule
	}

	public class OfferMenuItem
	{
		public string Link { get; set; }
		public string Text { get; set; }
		public bool Selected { get; set; }
	}

	public class OfferModel<T>
	{
		public T InnerModel { get; set; }
		public int OfferModelId { get; set; }
	}
}