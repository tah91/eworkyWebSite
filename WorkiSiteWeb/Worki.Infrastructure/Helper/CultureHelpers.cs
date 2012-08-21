using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Worki.Infrastructure.Helpers
{
    public static class CultureHelpers
    {
        // Format ==> FR / EN
        public enum TimeFormat
        {
			/// <summary>
			/// HH:mm / HH:MM [AM/PM]
			/// </summary>
            Time,         
  			/// <summary>
			/// dd/MM/yyyy / MM/dd/yyyy
  			/// </summary>
            Date,  
         	/// <summary>
			/// dd/MM/yyyy HH:mm / MM/dd/yyyy HH:mm [AM/PM]
         	/// </summary>
            General,     
			/// <summary>
			/// dd/MM/yyyy HH:mm:ss / MM/dd/yyyy HH:mm:ss [AM/PM]
			/// </summary>
            LongGeneral,  
			/// <summary>
			/// Day dd Month yyyy HH:mm:ss / Day, Month dd, yyyy HH:mm:ss [AM/PM]
			/// </summary>
            LongFull
        }

        /// <summary>
        /// Display datetime with the right format according to the current culture
        /// </summary>
        /// <param name="date">Date to display</param>
        /// <param name="format">Format desired</param>
        /// <returns>Formatted datetime</returns>
        public static string GetSpecificFormat(DateTime? date, TimeFormat format = TimeFormat.Date)
        {
            if (date.HasValue)
            {
                var thread_format = System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat;

                if (format == TimeFormat.Date)
                    return string.Format(thread_format, "{0:d}", date.Value);
                if (format == TimeFormat.Time)
                    return string.Format(thread_format, "{0:t}", date.Value);
                if (format == TimeFormat.LongFull)
                    return string.Format(thread_format, "{0:F}", date.Value);
                if (format == TimeFormat.General)
                    return string.Format(thread_format, "{0:g}", date.Value);
                if (format == TimeFormat.LongGeneral)
                    return string.Format(thread_format, "{0:G}", date.Value);
            }

            return "";
        }

		/// <summary>
		/// Get Format for javascript date, depending on culture
		/// </summary>
		/// <param name="format">Format desired</param>
		/// <returns>Formatted datetime</returns>
		public static string GetFormat(TimeFormat format = TimeFormat.Date)
		{
			var isoName = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

			switch (isoName)
			{
				case "en":
					{
						if (format == TimeFormat.Date)
							return "MM/dd/yyyy";
						if (format == TimeFormat.General)
							return "MM/dd/yyyy hh:mm tt";
					}
					break;
				case "fr":
				case "es":
                case "de":
                case "nl":
				default:
					{
						if (format == TimeFormat.Date)
							return "dd/MM/yyyy";
						if (format == TimeFormat.General)
							return "dd/MM/yyyy HH:mm";
					}
					break;
			}

			return "";
		}

		/// <summary>
		/// Get factor to convert into miles if needed, depending on culture
		/// </summary>
		/// <returns>conversion factor</returns>
		public static float GetDistanceFactor()
		{
			switch(System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName)
			{
				case "en":
					return 1 / 1.609F;
				case "fr":
				case "es":
                case "de":
                case "nl":
				default:
					return 1;
			}
		}
    }
}
