using System.IO;
using Bond;
using Bond.IO.Unsafe;
using Bond.Protocols;
using Microsoft.ServiceFabric.Data;

namespace SceneSkope.ServiceFabric.Bond
{
    public sealed class BondStateSerializer<T> : IStateSerializer<T>
    {
#pragma warning disable RCS1158 // Static member in generic type should use a type parameter.
        private static readonly Serializer<CompactBinaryWriter<OutputBuffer>> _serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(typeof(T));
        private static readonly Deserializer<CompactBinaryReader<InputBuffer>> _deserializer = new Deserializer<CompactBinaryReader<InputBuffer>>(typeof(T));
#pragma warning restore RCS1158 // Static member in generic type should use a type parameter.

        public T Read(BinaryReader binaryReader)
        {
            var count = binaryReader.ReadInt32();
            var bytes = binaryReader.ReadBytes(count);
            return _deserializer.Deserialize<T>(
                new CompactBinaryReader<InputBuffer>(
                    new InputBuffer(bytes)
                )
            );
        }

        public T Read(T baseValue, BinaryReader binaryReader) => Read(binaryReader);

        public void Write(T value, BinaryWriter binaryWriter)
        {
            var output = new OutputBuffer();
            var writer = new CompactBinaryWriter<OutputBuffer>(output);
            _serializer.Serialize(value, writer);
            binaryWriter.Write(output.Data.Count);
            binaryWriter.Write(output.Data.Array, output.Data.Offset, output.Data.Count);
        }

        public void Write(T baseValue, T targetValue, BinaryWriter binaryWriter) => Write(targetValue, binaryWriter);
    }
}
