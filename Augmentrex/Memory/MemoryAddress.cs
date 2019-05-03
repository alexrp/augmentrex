using System;

namespace Augmentrex.Memory
{
    [Serializable]
    public unsafe struct MemoryAddress : IEquatable<MemoryAddress>, IComparable<MemoryAddress>
    {
        public static readonly MemoryAddress MinValue = new MemoryAddress(uint.MinValue);

        public static readonly MemoryAddress MaxValue = new MemoryAddress(uint.MaxValue);

        readonly uint _address;

        public bool IsNull => this == MinValue;

        public MemoryAddress(void* address)
        {
            _address = (uint)address;
        }

        public MemoryAddress(int address)
        {
            _address = (uint)address;
        }

        public MemoryAddress(uint address)
        {
            _address = address;
        }

        public MemoryAddress(IntPtr address)
        {
            _address = (uint)address;
        }

        public MemoryAddress(UIntPtr address)
        {
            _address = (uint)address;
        }

        public static implicit operator MemoryAddress(void* address)
        {
            return new MemoryAddress(address);
        }

        public static implicit operator MemoryAddress(IntPtr address)
        {
            return new MemoryAddress(address);
        }

        public static implicit operator MemoryAddress(UIntPtr address)
        {
            return new MemoryAddress(address);
        }

        public static explicit operator MemoryAddress(int address)
        {
            return new MemoryAddress(address);
        }

        public static explicit operator MemoryAddress(uint address)
        {
            return new MemoryAddress(address);
        }

        public static implicit operator void*(MemoryAddress address)
        {
            return (void*)address._address;
        }

        public static implicit operator IntPtr(MemoryAddress address)
        {
            return (IntPtr)address._address;
        }

        public static implicit operator UIntPtr(MemoryAddress address)
        {
            return (UIntPtr)address._address;
        }

        public static explicit operator int(MemoryAddress address)
        {
            return (int)address._address;
        }

        public static explicit operator uint(MemoryAddress address)
        {
            return address._address;
        }

        public static bool operator true(MemoryAddress address)
        {
            return !address.IsNull;
        }

        public static bool operator false(MemoryAddress address)
        {
            return address.IsNull;
        }

        public static bool operator ==(MemoryAddress left, MemoryAddress right)
        {
            return left._address == right._address;
        }

        public static bool operator !=(MemoryAddress left, MemoryAddress right)
        {
            return left._address != right._address;
        }

        public static bool operator >(MemoryAddress left, MemoryAddress right)
        {
            return left._address > right._address;
        }

        public static bool operator <(MemoryAddress left, MemoryAddress right)
        {
            return left._address < right._address;
        }

        public static bool operator >=(MemoryAddress left, MemoryAddress right)
        {
            return left._address >= right._address;
        }

        public static bool operator <=(MemoryAddress left, MemoryAddress right)
        {
            return left._address <= right._address;
        }

        public static MemoryAddress operator +(MemoryAddress left, MemoryOffset right)
        {
            return new MemoryAddress(left._address + (uint)right);
        }

        public static MemoryAddress operator -(MemoryAddress left, MemoryOffset right)
        {
            return new MemoryAddress(left._address - (uint)right);
        }

        public static MemoryOffset operator -(MemoryAddress left, MemoryAddress right)
        {
            return new MemoryOffset(left._address - right._address);
        }

        public override int GetHashCode()
        {
            return _address.GetHashCode();
        }

        public bool Equals(MemoryAddress other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is MemoryAddress addr && this == addr;
        }

        public int CompareTo(MemoryAddress other)
        {
            return _address.CompareTo(other._address);
        }

        public override string ToString()
        {
            return $"0x{_address:x}";
        }
    }
}
