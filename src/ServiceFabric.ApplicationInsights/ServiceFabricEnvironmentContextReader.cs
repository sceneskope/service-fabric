using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ServiceFabric.ApplicationInsights
{
    internal sealed class ServiceFabricEnvironmentContextReader
    {
        public static ServiceFabricEnvironmentContextReader Instance { get; } = new ServiceFabricEnvironmentContextReader();

        public string NodeName { get; }
        public string NodeType { get; }


        private ServiceFabricEnvironmentContextReader()
        {
            var context = FabricRuntime.GetNodeContext();
            NodeName = context.NodeName;
            NodeType = context.NodeType;
        }
    }
}
