using System.Web.Mvc;
using System;
using System.Collections.Generic;
using Worki.Data.Models;

namespace Worki.Web.Model
{
	public class BackOfficeConstants
	{
		public const int NewsCount = 10;
		public const int LocalisationCount = 3;
	}

	public class BackOfficeHomeViewModel
	{
		public Member Owner { get; set; }
		public IEnumerable<NewsItem> News { get; set; }
		public IEnumerable<Localisation> Places { get; set; }
	}

	public class BackOfficeLocalisationHomeViewModel
	{
		public IEnumerable<NewsItem> News { get; set; }
		public Localisation Localisation { get; set; }
	}

	public class DashoboardHomeViewModel
	{
		public Member Member { get; set; }
		public IEnumerable<NewsItem> News { get; set; }
	}

	public class DescriptionItem
	{
		public string Title { get; set; }
		public string Value { get; set; }
        public bool Bold { get; set; }
        public string Url { get; set; }
	}

	public class LocalisationItem
	{
		public Localisation Localisation { get; set; }
		public string Url { get; set; }
	}

	public class SummaryViewModel
	{
		public string ImagePath { get; set; }
		public string ImageAlt { get; set; }
		public IEnumerable<DescriptionItem> Descriptions { get; set; }
	}

	public class PagedListViewModel
	{
		public const int PageSize = 5;
		public string Title { get; set; }
		public Func<int, string> UrlMaker { get; set; }
		public string EmptyMessage { get; set; }
	}

	public class BookingListViewModel : PagedListViewModel
	{
		public PagingList<MemberBooking> Bookings { get; set; }
	}

	public class QuotationListViewModel : PagedListViewModel
	{
		public PagingList<MemberQuotation> Quotations { get; set; }
	}

    public class RefuseBookingModel
    {
        public int BookingId { get; set; }
        public string ReturnUrl { get; set; }
    }

	public class RefuseQuotationModel
	{
		public int QuotationId { get; set; }
		public string ReturnUrl { get; set; }
	}

	public class AlertSumaryModel
	{
		public int AlertCount { get; set; }
		public string AlertLink { get; set; }
		public string AlertTitle { get; set; }
	}

	public class ScheduleModel
	{
		public string SourceFeed { get; set; }
		public string IsEditable { get; set; }
	}
}