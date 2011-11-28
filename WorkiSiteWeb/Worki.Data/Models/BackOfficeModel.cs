using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Worki.Infrastructure;
using System.Collections.Generic;
using System;

namespace Worki.Data.Models
{
    public class HomeViewModel
    {
        public IEnumerable<NewsItem> News { get; set; }
		public IEnumerable<Localisation> Places { get; set; }
    }

	public class LocalisationHomeViewModel
	{
		public IEnumerable<NewsItem> News { get; set; }
		public Localisation Localisation { get; set; }
	}
}
