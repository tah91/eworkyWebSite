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
		public int ID { get; set; }
		public string Name { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public string Description { get; set; }
		public string MainPic { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string TypeString { get; set; }
		public string Url { get; set; }
		public List<CommentJson> Comments { get; set; }
	}

	public class CommentJson
	{
		public int ID { get; set; }
		public System.DateTime Date { get; set; }
		public string Post { get; set; }
		public int Rating { get; set; }
		public int RatingPrice { get; set; }
		public int RatingWifi { get; set; }
		public int RatingDispo { get; set; }
		public int RatingWelcome { get; set; }
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