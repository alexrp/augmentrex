using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Augmentrex.Memory
{
    [Serializable]
    public sealed class MemoryWindow
    {
        public MemoryAddress Address { get; }

        public uint Length { get; }

        public MemoryWindow(MemoryAddress address, uint length)
        {
            Address = address;
            Length = length;
        }

        public MemoryAddress ToAddress(MemoryOffset offset)
        {
            return Address + offset;
        }

        public MemoryOffset ToOffset(MemoryAddress address)
        {
            return address - Address;
        }

        public bool IsInRange(MemoryAddress address)
        {
            return address >= Address && address < Address + (MemoryOffset)Length;
        }

        public bool IsInRange(MemoryOffset offset)
        {
            var offs = (int)offset;

            return offs >= 0 && offs < Length;
        }

        public MemoryOffset[] Search(params byte?[][] patterns)
        {
            bool IsMatch(byte?[] pattern, MemoryOffset offset)
            {
                for (var j = 0; j < pattern.Length; j++)
                {
                    var b = pattern[j];

                    if (b != null && Read<byte>(offset + j) != b)
                        return false;
                }

                return true;
            }

            return (from i in Enumerable.Range(0, (int)Length)
                    from p in patterns
                    where i + p.Length < Length
                    let o = (MemoryOffset)i
                    where IsMatch(p, o)
                    select o).Distinct().ToArray();
        }

        public unsafe T Read<T>(MemoryOffset offset)
            where T : unmanaged
        {
            return Unsafe.Read<T>(ToAddress(offset));
        }

        public MemoryOffset ReadOffset(MemoryOffset offset)
        {
            return ToOffset((MemoryAddress)Read<uint>(offset));
        }

        public byte[] ReadBytes(MemoryOffset offset, int count)
        {
            var bytes = new byte[count];

            Marshal.Copy(ToAddress(offset), bytes, 0, count);

            return bytes;
        }

        public T ReadStructure<T>(MemoryOffset offset)
            where T : struct
        {
            return Marshal.PtrToStructure<T>(ToAddress(offset));
        }

        public unsafe void Write<T>(MemoryOffset offset, T value)
            where T : unmanaged
        {
            Unsafe.Write(ToAddress(offset), value);
        }

        public void WriteOffset(MemoryOffset offset, MemoryOffset value)
        {
            Write(offset, (uint)ToAddress(value));
        }

        public void WriteBytes(MemoryOffset offset, byte[] values)
        {
            Marshal.Copy(values, 0, ToAddress(offset), values.Length);
        }

        public void WriteStructure<T>(MemoryOffset offset, T value)
            where T : struct
        {
            Marshal.StructureToPtr(value, ToAddress(offset), false);
        }

        public void DestroyStructure<T>(MemoryOffset offset)
            where T : struct
        {
            Marshal.DestroyStructure<T>(ToAddress(offset));
        }
    }
}
