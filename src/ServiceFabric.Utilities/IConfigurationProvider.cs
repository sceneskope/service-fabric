using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFabric.Utilities
{
    public interface IConfigurationProvider
    {
        bool HasConfiguration { get; }

        string GetValue(string name);
        string TryGetValue(string name);
    }
}
