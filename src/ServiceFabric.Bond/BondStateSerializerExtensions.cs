using ServiceFabric.Utilities;
using System;

namespace ServiceFabric.Bond
{
    public static class BondStateSerializerExtensions
    {
        public static void RegisterBondStateSerializer<T>(this InitializationCallbackAdapter adapter) =>
            adapter.RegisterStateSerializer<T>(new BondStateSerializer<T>());
    }
}
