using System;

namespace Worki.Data.Models
{
	public class CalandarJson
	{
		public const string Orange = "#f7a33a";
		public const string Red = "#d25657";
		public const string Yellow = "#f5f322";
		public const string Grey = "#808080";

		public static DateTime UNIXStart = new DateTime(1970, 01, 01);

		public int id { get; set; }
		public string title { get; set; }
		public bool allDay { get; set; }
		public string start { get; set; }
		public string end { get; set; }
		public string url { get; set; }
		public string className { get; set; }
		public bool editable { get; set; }
		public string color { get; set; }
		public string backgroundColor { get; set; }
		public string borderColor { get; set; }
		public string textColor { get; set; }
	}
}
