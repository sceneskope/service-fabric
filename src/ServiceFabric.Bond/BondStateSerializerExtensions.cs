using ServiceFabric.Utilities;
using System;

namespace ServiceFabric.Bond
{
    public static class BondStateSerializerExtensions
    {
        [Obsolete("Uses obsolete state serializer", false)]
        public static void RegisterBondStateSerializer<T>(this InitializationCallbackAdapter adapter) =>
            adapter.RegisterStateSerializer<T>(new BondStateSerializer<T>());
    }
}
