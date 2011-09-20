using System;
using System.Collections.Generic;

namespace Worki.Data.Models
{
	public interface IJsonProvider<T>
	{
		T GetJson();
	}

	public class LocalisationJson
	{
        public LocalisationJson()
        {
            comments = new List<CommentJson>();
            fans = new List<MemberJson>();
			amenities = new List<string>();
        }

		public int id { get; set; }
		public string name { get; set; }
		public double latitude { get; set; }
		public double longitude { get; set; }
		public string description { get; set; }
		public string image { get; set; }
		public string address { get; set; }
		public string city { get; set; }
		public string type { get; set; }
		public string url { get; set; }
        public double rating { get; set; }
		public List<string> amenities { get; set; }
		public List<CommentJson> comments { get; set; }
        public List<MemberJson> fans { get; set; }
	}

	public class CommentJson
	{
		public int id { get; set; }
		public DateTime date { get; set; }
        public MemberJson author { get; set; }
		public string post { get; set; }
		public int rating { get; set; }
		public int ratingPrice { get; set; }
		public int ratingWifi { get; set; }
		public int ratingDispo { get; set; }
		public int ratingWelcome { get; set; }
	}

    public class MemberJson
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
		public string avatar { get; set; }
        public string companyName { get; set; }
        public string city { get; set; }
        public string profile { get; set; }
        public string description { get; set; }
        public string twitter { get; set; }
        public string facebook { get; set; }
        public string linkedin { get; set; }
        public string viadeo { get; set; }
    }

	public class ImageJson
	{
		public string name { get; set; }
		public int size { get; set; }
		public string url { get; set; }
		public string thumbnail_url { get; set; }
		public string delete_url { get; set; }
		public string delete_type { get; set; }
		public string is_default { get; set; }
		public string is_logo { get; set; }
	}
}