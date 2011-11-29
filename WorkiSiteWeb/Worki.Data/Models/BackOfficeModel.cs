﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Worki.Infrastructure;
using System.Collections.Generic;
using System;

namespace Worki.Data.Models
{
    public class BackOfficeConstants
    {
        public const int NewsCount = 10;
        public const int LocalisationCount = 10;
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

    public class BookingListViewModel
    {
        public string Title { get; set; }
        public PagingList<MemberBooking> Bookings { get; set; }
    }
}
