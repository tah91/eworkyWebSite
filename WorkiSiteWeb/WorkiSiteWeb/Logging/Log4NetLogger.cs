using System;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using log4net;
using log4net.Config;
using WorkiSiteWeb.Infrastructure.Logging;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Logging
{
    public class Log4NetLogger : ILogger
    {
        private readonly ILog _logger;
        public Log4NetLogger()
        {
            XmlConfigurator.Configure();

			//azure logger
			if (RoleEnvironment.IsAvailable)
			{
				var appender = new AzureAppender();
				appender.ActivateOptions();
				BasicConfigurator.Configure(appender);
			}
            _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void Debug(string message)
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug(AddDebugInfo(message));
            }
        }

        public void Error(string message, Exception e)
        {
            if (_logger.IsErrorEnabled)
            {
				_logger.Error(AddDebugInfo(message + "[" + e.Message + "]" + " at [" + e.StackTrace + "]"));
            }
        }

        public void Error(string message)
        {
            if (_logger.IsErrorEnabled)
            {
                _logger.Error(AddDebugInfo(message));
            }
        }

        public void Info(string message)
        {
            if (_logger.IsInfoEnabled)
            {
                _logger.Info(AddDebugInfo(message));
            }
        }

        public void Warn(string message)
        {
            if (_logger.IsWarnEnabled)
            {
                _logger.Warn(AddDebugInfo(message));
            }
        }

        /// <summary>
        /// This method is necessary because the default implementation of
        /// log4net gets stack info from the calling method. The calling method
        /// will always be our wrapper methods above. This code goes down the 
        /// stack trace one step further and determines the correct calling info.
        /// 
        /// Provide specific information about the logged message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string AddDebugInfo(string message)
        {
            // Since we are tab seperating the items in the log line
            //	we should not allow any tabs in the raw message. This
            //	will make it difficult to parse later.
            //
            message = message.Replace('\t', ' ');

            // Get the stack frame the originated the log comment.
            //
            StackFrame frame1 = new StackFrame(2, true);
            string methodName = frame1.GetMethod().ToString();
            int lineNumber = frame1.GetFileLineNumber();
            string fileName = frame1.GetFileName();


            // Get the current http context if one exists
            //
            string httpContextUser = string.Empty;
            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null && httpContext.User != null)
            {
                httpContextUser = httpContext.User.Identity.Name;
            }

            // Format the log line
            //
            string newMessage = String.Format("{0}\t{1}\t{2}\t{3}\t{4}",
                                                message,
                                                lineNumber,
                                                methodName,
                                                fileName,
                                                httpContextUser);

            return newMessage;
        }
    }
}