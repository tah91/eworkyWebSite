using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Worki.Infrastructure;
using System.Collections.Generic;
using System;

namespace Worki.Data.Models
{
    public class BlogPost
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Image { get; set; }
    }

    public class IndexViewModel
    {
        #region Properties

        public int LocalisationCount { get; set; }
        public List<WelcomePeople> WelcomePeople { get; set; }
        public IEnumerable<BlogPost> BlogPosts { get; set; }

        #endregion
    }

	public enum IndexItem
	{
		WhoWeAre,
		Faq,
		Cgu,
		Press,
		Jobs,
		Legal
	}

	public class IndexMenuModel
	{
		public IndexItem IndexItem { get; set; }

		public string GetClass(string classContent, IndexItem item)
		{
			return IndexItem == item ? classContent : string.Empty;
		}
	}
}
