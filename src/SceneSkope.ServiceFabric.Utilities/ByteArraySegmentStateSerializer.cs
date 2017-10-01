using System;
using System.IO;
using Microsoft.ServiceFabric.Data;

namespace SceneSkope.ServiceFabric.Utilities
{
    public sealed class ByteArraySegmentStateSerializer : IStateSerializer<ArraySegment<byte>>
    {
        public ArraySegment<byte> Read(BinaryReader binaryReader)
        {
            var count = binaryReader.ReadInt32();
            var bytes = binaryReader.ReadBytes(count);
            return new ArraySegment<byte>(bytes);
        }

        public ArraySegment<byte> Read(ArraySegment<byte> baseValue, BinaryReader binaryReader) => Read(binaryReader);

        public void Write(ArraySegment<byte> value, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(value.Count);
            binaryWriter.Write(value.Array, value.Offset, value.Count);
        }

        public void Write(ArraySegment<byte> baseValue, ArraySegment<byte> targetValue, BinaryWriter binaryWriter) =>
            Write(targetValue, binaryWriter);
    }
}
