using System;
using System.IO;
using Microsoft.ServiceFabric.Data;

namespace ServiceFabric.Utilities
{
    public sealed class ByteArraySegmentStateSerializer : IStateSerializer<ArraySegment<byte>>
    {
        public static ArraySegment<byte> ReadArraySegment(BinaryReader binaryReader)
        {
            var bytes = (int)binaryReader.ReadByte();
            var buffer = binaryReader.ReadBytes(bytes);
            return new ArraySegment<byte>(buffer, 0, bytes);
        }

        public static void WriteArraySegment(ArraySegment<byte> segment, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(segment.Count);
            binaryWriter.Write(segment.Array, segment.Offset, segment.Count);
        }

        public ArraySegment<byte> Read(BinaryReader binaryReader) => ReadArraySegment(binaryReader);

        public ArraySegment<byte> Read(ArraySegment<byte> baseValue, BinaryReader binaryReader) => Read(binaryReader);

        public void Write(ArraySegment<byte> value, BinaryWriter binaryWriter) => WriteArraySegment(value, binaryWriter);

        public void Write(ArraySegment<byte> baseValue, ArraySegment<byte> targetValue, BinaryWriter binaryWriter) =>
            Write(targetValue, binaryWriter);
    }
}
