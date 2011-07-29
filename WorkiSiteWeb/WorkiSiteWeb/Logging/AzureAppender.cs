using System;
using System.Diagnostics;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics.Management;
using WorkiSiteWeb.Helpers;

namespace Logging
{
	public sealed class AzureAppender : AppenderSkeleton
	{
		private const string ConnectionStringKey = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString";
		private const string LevelKey = "Diagnostics.Level";
		private const string LayoutKey = "Diagnostics.Layout";
		private const string ScheduledTransferPeriodKey = "Diagnostics.ScheduledTransferPeriod";
		private const string EventLogsKey = "Diagnostics.EventLogs";

		public AzureAppender()
		{
			ScheduledTransferPeriod = GetScheduledTransferPeriod();
			Layout = new log4net.Layout.PatternLayout(GetLayout());
			Level = GetLevel();
		}

		public int ScheduledTransferPeriod { get; set; }

		public string Level { get; set; }

		protected override void Append(LoggingEvent loggingEvent)
		{
			var logString = RenderLoggingEvent(loggingEvent);

			System.Diagnostics.Trace.WriteLine(logString);
		}

		public override void ActivateOptions()
		{
			ConfigureThreshold();

			base.ActivateOptions();

			//ConfigureAzureDiagnostics();
		}

		private void ConfigureThreshold()
		{
			var rootRepository = (Hierarchy)log4net.LogManager.GetRepository();
			Threshold = rootRepository.LevelMap[Level];
		}

		public static void ConfigureAzureDiagnostics()
		{
			var traceListener = new DiagnosticMonitorTraceListener();
			Trace.Listeners.Add(traceListener);

			CloudStorageAccount storageAccount = CloudStorageAccount.FromConfigurationSetting(MiscHelpers.DataConnectionString);

			RoleInstanceDiagnosticManager roleInstanceDiagnosticManager = storageAccount.CreateRoleInstanceDiagnosticManager(RoleEnvironment.DeploymentId, RoleEnvironment.CurrentRoleInstance.Role.Name, RoleEnvironment.CurrentRoleInstance.Id);
			DiagnosticMonitorConfiguration dmc = roleInstanceDiagnosticManager.GetCurrentConfiguration();
			if (dmc == null)
			    dmc = DiagnosticMonitor.GetDefaultInitialConfiguration();
			//set threshold to verbose, what gets logged is controled by the log4net level
			dmc.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;

			ScheduleTransfer(dmc);

			ConfigureWindowsEventLogsToBeTransferred(dmc);

			//DiagnosticMonitor.Start(MiscHelpers.GetDataConnectionStringSetting());
			roleInstanceDiagnosticManager.SetCurrentConfiguration(dmc);			
		}

		static void ScheduleTransfer(DiagnosticMonitorConfiguration dmc)
		{
			var transferPeriod = TimeSpan.FromSeconds(GetScheduledTransferPeriod());
			//var transferPeriod = TimeSpan.FromMinutes(5);
			dmc.Logs.ScheduledTransferPeriod = transferPeriod;
			dmc.WindowsEventLog.ScheduledTransferPeriod = transferPeriod;
		}

		private static void ConfigureWindowsEventLogsToBeTransferred(DiagnosticMonitorConfiguration dmc)
		{
			var eventLogs = GetEventLogs().Split(';');
			foreach (var log in eventLogs)
			{
				dmc.WindowsEventLog.DataSources.Add(log);
			}
		}

		private static string GetLevel()
		{
			try
			{
				return RoleEnvironment.GetConfigurationSettingValue(LevelKey);
			}
			catch (Exception)
			{
				return "Error";
			}
		}

		private static string GetLayout()
		{
			try
			{
				return RoleEnvironment.GetConfigurationSettingValue(LayoutKey);
			}
			catch (Exception)
			{
				return "%d [%t] %-5p %c [%x] <%X{auth}> - %m%n";
			}
		}

		private static int GetScheduledTransferPeriod()
		{
			try
			{
				return int.Parse(RoleEnvironment.GetConfigurationSettingValue(ScheduledTransferPeriodKey));
			}
			catch (Exception)
			{
				return 5;
			}
		}

		private static string GetEventLogs()
		{
			try
			{
				return RoleEnvironment.GetConfigurationSettingValue(EventLogsKey);
			}
			catch (Exception)
			{
				return "Application!*;System!*";
			}
		}
	}
}