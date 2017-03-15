using System.Configuration;
using System.Reflection;
using Microsoft.ApplicationInsights.Channel;

namespace SharePointAdminBot
{
    public class AppInsightsInitializer : Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer  
    {
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Component.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            telemetry.Context.Properties["tags"] = "SPAdminBot";
            telemetry.Context.InstrumentationKey = ConfigurationManager.AppSettings["iKey"];
        }
    }
}