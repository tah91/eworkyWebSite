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
            Time,           // Format ==> HH:mm / HH:MM [AM/PM]
            Date,           // Format ==> dd/MM/yyyy / MM/dd/yyyy
            General,        // Format ==> dd/MM/yyyy HH:mm / MM/dd/yyyy HH:mm [AM/PM]
            LongGeneral,    // Format ==> dd/MM/yyyy HH:mm:ss / MM/dd/yyyy HH:mm:ss [AM/PM]
            LongFull        // Format ==> Day dd Month yyyy HH:mm:ss / Day, Month dd, yyyy HH:mm:ss [AM/PM]
        }

        /// <summary>
        /// Display datetime with the right format according to the current culture
        /// </summary>
        /// <param name="date">Date to display</param>
        /// <param name="format">Format desired</param>
        /// <returns>Formatted datetime</returns>
        public static string GetSpecificFormat(DateTime? date, TimeFormat format)
        {
            if (date.HasValue)
            {
                if (format == TimeFormat.Date)
                    return string.Format(System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat, "{0:d}", date.Value);
                if (format == TimeFormat.Time)
                    return string.Format(System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat, "{0:t}", date.Value);
                if (format == TimeFormat.LongFull)
                    return string.Format(System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat, "{0:F}", date.Value);
                if (format == TimeFormat.General)
                    return string.Format(System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat, "{0:g}", date.Value);
                if (format == TimeFormat.LongGeneral)
                    return string.Format(System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat, "{0:G}", date.Value);
            }

            return "";
        }
    }
}
