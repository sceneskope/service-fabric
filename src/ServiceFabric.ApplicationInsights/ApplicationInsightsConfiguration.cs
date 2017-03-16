using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using ServiceFabric.Utilities;

namespace ServiceFabric.ApplicationInsights
{
    public static class ApplicationInsightsConfiguration
    {
        public static void Initialise()
        {
            var applicationInsightsConfiguration = new FabricConfigurationProvider("ApplicationInsights");
            if (applicationInsightsConfiguration.HasConfiguration)
            {
                TelemetryConfiguration.Active.InstrumentationKey = applicationInsightsConfiguration.GetValue("InstrumentationKey");
            }
        }
    }
}
