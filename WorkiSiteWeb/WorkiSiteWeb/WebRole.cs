using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure;
using Logging;


public class WebRole : RoleEntryPoint
{
	//public override bool OnStart()
	//{

	//    // For information on handling configuration changes
	//    // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

	//    //(CDLTLL) Configuration for Windows Azure settings 
	//    CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSettingPublisher) =>
	//        {
	//            var connectionString = RoleEnvironment.GetConfigurationSettingValue(configName);
	//            configSettingPublisher(connectionString);
	//        }
	//    );

	//    AzureAppender.ConfigureAzureDiagnostics();
	//    return base.OnStart();
	//}
}