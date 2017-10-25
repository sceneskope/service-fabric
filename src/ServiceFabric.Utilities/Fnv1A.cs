using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Utilities
{
    public struct Fnv1A
    {
        private const ulong prime = 0x00000100000001B3;
        private const ulong offset = 0xcbf29ce484222325;

        private ulong _hash;

        public static Fnv1A Create() => new Fnv1A { _hash = offset };

        public Fnv1A Compute(byte[] array, int index, int count)
        {
            for (var i = index; i < count; i++)
            {
                unchecked
                {
                    _hash ^= array[i];
                    _hash *= prime;
                }
            }
            return this;
        }

        public unsafe Fnv1A Compute(byte* address, int count)
        {
            for (var i = 0; i < count; i++)
            {
                unchecked
                {
                    _hash ^= *address;
                    _hash *= prime;
                    address++;
                }
            }
            return this;
        }

        public Fnv1A Compute(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            return Compute(bytes, 0, bytes.Length);
        }

        public Fnv1A Compute(byte[] array) => Compute(array, 0, array.Length);

        public ulong Hash => _hash;

        public long HashLong => unchecked((long)_hash);

        public static ulong ComputeAll(params string[] values)
        {
            var algorithm = new Fnv1A();
            foreach (var value in values)
            {
                algorithm.Compute(value);
            }
            return algorithm.Hash;
        }

        public Fnv1A Compute(Guid guid) => Compute(guid.ToByteArray());

        public unsafe Fnv1A Compute(short value) => Compute((byte*)&value, sizeof(short));
        public unsafe Fnv1A Compute(int value) => Compute((byte*)&value, sizeof(int));
        public unsafe Fnv1A Compute(long value) => Compute((byte*)&value, sizeof(long));

        public unsafe Fnv1A Compute(ushort value) => Compute((byte*)&value, sizeof(ushort));
        public unsafe Fnv1A Compute(uint value) => Compute((byte*)&value, sizeof(uint));
        public unsafe Fnv1A Compute(ulong value) => Compute((byte*)&value, sizeof(ulong));

        public unsafe Fnv1A Compute(float value) => Compute((byte*)&value, sizeof(float));
        public unsafe Fnv1A Compute(double value) => Compute((byte*)&value, sizeof(double));

        public static ulong ComputeAll(params Guid[] values)
        {
            var algorithm = new Fnv1A();
            foreach (var value in values)
            {
                algorithm.Compute(value.ToByteArray());
            }
            return algorithm.Hash;
        }

        public static long ComputeAllLong(params string[] values) => unchecked((long)ComputeAll(values));

        public static long ComputeAllLong(params Guid[] values) => unchecked((long)ComputeAll(values));
    }
}
