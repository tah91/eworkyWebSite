using System;
using System.Collections.Generic;

namespace Worki.Data.Models
{
	/// <summary>
	/// class to handle month year for invoices
	/// </summary>
	public class MonthYear : IComparable
	{
		#region Propoerties

		public int Month { get; set; }
		public int Year { get; set; }

		#endregion

		#region Override

		public override string ToString()
		{
			return String.Format("{0}-{1}", Month, Year);
		}

		public override int GetHashCode()
		{
			return Month ^ Year;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MonthYear))
				return false;

			return this.CompareTo((MonthYear)obj) == 0;
		}

		#endregion

		#region Static

		public static MonthYear FromDateTime(DateTime date)
		{
			return new MonthYear { Month = date.Month, Year = date.Year };
		}

		public static MonthYear Parse(string monthYear)
		{
			try
			{
				var dates = monthYear.Split('-');
				return new MonthYear { Month = int.Parse(dates[0]), Year = int.Parse(dates[1]) };
			}
			catch (Exception)
			{
				return GetCurrent();
			}
		}

		public static MonthYear GetCurrent()
		{
			return FromDateTime(DateTime.Now);
		}

		public static int GetMonthYearBetween(MonthYear start, MonthYear end)
		{
			if (start == end)
				return 0;

			var count = 0;
			while (start.CompareTo(end) < 0)
			{
				start = MonthYear.FromDateTime(start.GetDateTime().AddMonths(1));
				count++;
				if (count == 100)
					break;
			}
			return count;
		}

		#endregion
		
		public bool EqualDate(DateTime date)
		{
			return date.Month == Month && date.Year == Year;
		}

		public DateTime GetDateTime()
		{
			try
			{
				return new DateTime(Year, Month, 1);
			}
			catch (Exception)
			{
				return DateTime.Now;
			}
		}

		public MonthYear Add(int month)
		{
			return FromDateTime(GetDateTime().AddMonths(month));
		}

		#region Comparaison

		public int CompareTo(object obj)
		{
			if (obj is MonthYear)
			{
				var to = obj as MonthYear;
				if (Year < to.Year)
					return -1;
				else if (Year == to.Year && Month < to.Month)
					return -1;
				else if (Year == to.Year && Month == to.Month)
					return 0;
				return 1;
			}
			else
			{
				throw new ArgumentException();
			}
		}

		public static bool operator <(MonthYear obj1, MonthYear obj2)
		{
			return obj1.CompareTo(obj2) < 0;
		}

		public static bool operator >(MonthYear obj1, MonthYear obj2)
		{
			return obj1.CompareTo(obj2) > 0;
		}

		public static bool operator ==(MonthYear obj1, MonthYear obj2)
		{
			return obj1.CompareTo(obj2) == 0;
		}

		public static bool operator !=(MonthYear obj1, MonthYear obj2)
		{
			return obj1.CompareTo(obj2) != 0;
		}

		#endregion
	}

	public class MonthYearList<T>
	{
		public IList<T> List { get; set; }
		public MonthYear Current { get; set; }
		public MonthYear Initial { get; set; }
	}
}