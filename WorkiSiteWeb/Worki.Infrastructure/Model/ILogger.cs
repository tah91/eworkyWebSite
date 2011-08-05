using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Worki.Infrastructure.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// Debugs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Debug(string message);

        /// <summary>
        /// Errors the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="e">The e.</param>
        void Error(string message, System.Exception e);

        /// <summary>
        /// Errors the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Error(string message);

        /// <summary>
        /// Infoes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Info(string message);

        /// <summary>
        /// Warns the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Warn(string message);
    }
}
