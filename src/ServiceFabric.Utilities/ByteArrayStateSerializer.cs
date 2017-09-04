using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Utilities
{
    public class ByteArrayStateSerializer : IStateSerializer<byte[]>
    {
        public byte[] Read(BinaryReader binaryReader)
        {
            var count = binaryReader.ReadInt32();
            var bytes = binaryReader.ReadBytes(count);
            return bytes;
        }

        public byte[] Read(byte[] baseValue, BinaryReader binaryReader) => Read(binaryReader);

        public void Write(byte[] value, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(value.Length);
            binaryWriter.Write(value, 0, value.Length);
        }

        public void Write(byte[] baseValue, byte[] targetValue, BinaryWriter binaryWriter) =>
            Write(targetValue, binaryWriter);
    }
}
