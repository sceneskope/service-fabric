using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using SceneSkope.ServiceFabric.Utilities;

namespace SceneSkope.ServiceFabric.Actors.Utilities
{
    public static class ActorConfigurationHelper
    {
        public static ConfigurationSection ReadCustomConfigurationSection(this Actor actor, string customConfigurationSectionName) =>
            actor.ActorService.Context.CodePackageActivationContext.ReadCustomConfigurationSection(customConfigurationSectionName);
    }
}
