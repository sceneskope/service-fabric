using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading.Tasks;

namespace SceneSkope.ServiceFabric.Utilities
{
    public class FabricConfigurationProvider : IConfigurationProvider
    {
        private KeyedCollection<string, ConfigurationProperty> _configurationProperties;

        public FabricConfigurationProvider(string configurationSectionName)
        {
            if (string.IsNullOrWhiteSpace(configurationSectionName))
            {
                throw new ArgumentNullException($"{nameof(configurationSectionName)}");
            }

            var activationContext = FabricRuntime.GetActivationContext();
            var configPackage = activationContext.GetConfigurationPackageObject("Config");
            UseConfiguration(configPackage, configurationSectionName);
        }

        public bool HasConfiguration => _configurationProperties != null;

        public string GetValue(string name) => _configurationProperties.ReadConfigurationString(name);
        public string TryGetValue(string name) => _configurationProperties.TryReadConfigurationString(name);

        private void UseConfiguration(ConfigurationPackage configPackage, string configurationSectionName)
        {
            if (!configPackage.Settings.Sections.Contains(configurationSectionName))
            {
                _configurationProperties = null;
            }
            else
            {
                _configurationProperties = configPackage.Settings.Sections[configurationSectionName].Parameters;
            }
        }
    }
}
