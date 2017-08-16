using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ServiceFabric.Utilities
{
    public static class ConfigurationHelpers
    {
        public static string ReadConfigurationString(this ConfigurationSection configSection, string parameterName, bool allowBlank = false) =>
            ReadConfigurationString(configSection.Parameters, parameterName, allowBlank);

        public static string ReadConfigurationString(this KeyedCollection<string, ConfigurationProperty> parameters, string parameterName, bool allowBlank = false)
        {
            var parameter = parameters.TryReadConfigurationString(parameterName);
            if (parameters == null)
            {
                throw new KeyNotFoundException($"Configuration parameter '{parameterName}' not found");
            }
            if (!allowBlank && string.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentException($"Parameter '{parameterName}' must not be null or blank");
            }
            return parameter;
        }

        public static string TryReadConfigurationString(this ConfigurationSection configSection, string parameterName) =>
            ReadConfigurationString(configSection.Parameters, parameterName);

        public static string TryReadConfigurationString(this KeyedCollection<string, ConfigurationProperty> parameters, string parameterName)
        {
            if (parameters.Contains(parameterName))
            {
                return parameters[parameterName].Value;
            }
            else
            {
                return null;
            }
        }

        public static bool TryReadConfiguration(this KeyedCollection<string, ConfigurationProperty> parameters, string parmaeterName,
            out ConfigurationProperty value)
        {
            if (parameters.Contains(parmaeterName))
            {
                value = parameters[parmaeterName];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public static int ReadConfigurationInt(this ConfigurationSection configSection, string parameterName) =>
            ReadConfigurationInt(configSection.Parameters, parameterName);

        public static int ReadConfigurationInt(this KeyedCollection<string, ConfigurationProperty> parameters, string parameterName)
        {
            var value = ReadConfigurationString(parameters, parameterName);
            if (!int.TryParse(value, out var result))
            {
                throw new ArgumentException($"Parameter '{parameterName}' can not be converted to an int ({value})");
            }
            return result;
        }

        public static bool ReadConfigurationBool(this ConfigurationSection configSection, string parameterName) =>
            ReadConfigurationBool(configSection.Parameters, parameterName);

        public static bool ReadConfigurationBool(this KeyedCollection<string, ConfigurationProperty> parameters, string parameterName)
        {
            var value = ReadConfigurationString(parameters, parameterName);
            if (!bool.TryParse(value, out var result))
            {
                throw new ArgumentException($"Parameter '{parameterName}' can not be converted to a bool ({value})");
            }
            return result;
        }

        public static ConfigurationSection ReadCustomConfigurationSection(this StatefulServiceBase service, string customConfigurationSectionName) =>
            ReadCustomConfigurationSection(service.Context.CodePackageActivationContext, customConfigurationSectionName);

        public static ConfigurationSection ReadCustomConfigurationSection(this StatelessService service, string customConfigurationSectionName) =>
            ReadCustomConfigurationSection(service.Context.CodePackageActivationContext, customConfigurationSectionName);

        public static ConfigurationSection ReadCustomConfigurationSection(this ICodePackageActivationContext context, string customConfigurationSectionName)
        {
            var settingsFile = context.GetConfigurationPackageObject("Config").Settings;
            if (!settingsFile.Sections.Contains(customConfigurationSectionName))
            {
                throw new KeyNotFoundException($"Settings does not contain a section for '{customConfigurationSectionName}'");
            }
            return settingsFile.Sections[customConfigurationSectionName];
        }
    }
}
