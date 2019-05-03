using System;

namespace Augmentrex.Memory
{
    [Serializable]
    public struct MemoryOffset : IEquatable<MemoryOffset>, IComparable<MemoryOffset>
    {
        public static readonly MemoryOffset MinValue = new MemoryOffset(int.MinValue);

        public static readonly MemoryOffset MaxValue = new MemoryOffset(int.MaxValue);

        readonly int _offset;

        public bool IsZero => this == MinValue;

        public bool IsNegative => _offset < 0;

        public MemoryOffset(int offset)
        {
            _offset = offset;
        }

        public MemoryOffset(uint offset)
        {
            _offset = (int)offset;
        }

        public static explicit operator MemoryOffset(int offset)
        {
            return new MemoryOffset(offset);
        }

        public static explicit operator MemoryOffset(uint offset)
        {
            return new MemoryOffset(offset);
        }

        public static explicit operator int(MemoryOffset offset)
        {
            return offset._offset;
        }

        public static explicit operator uint(MemoryOffset offset)
        {
            return (uint)offset._offset;
        }

        public static bool operator ==(MemoryOffset left, MemoryOffset right)
        {
            return left._offset == right._offset;
        }

        public static bool operator !=(MemoryOffset left, MemoryOffset right)
        {
            return left._offset != right._offset;
        }

        public static bool operator >(MemoryOffset left, MemoryOffset right)
        {
            return left._offset > right._offset;
        }

        public static bool operator <(MemoryOffset left, MemoryOffset right)
        {
            return left._offset < right._offset;
        }

        public static bool operator >=(MemoryOffset left, MemoryOffset right)
        {
            return left._offset >= right._offset;
        }

        public static bool operator <=(MemoryOffset left, MemoryOffset right)
        {
            return left._offset <= right._offset;
        }

        public static MemoryOffset operator +(MemoryOffset left, int right)
        {
            return new MemoryOffset(left._offset + right);
        }

        public static MemoryOffset operator +(MemoryOffset left, uint right)
        {
            return new MemoryOffset(left._offset + (int)right);
        }

        public static MemoryOffset operator -(MemoryOffset left, int right)
        {
            return new MemoryOffset(left._offset - right);
        }

        public static MemoryOffset operator -(MemoryOffset left, uint right)
        {
            return new MemoryOffset(left._offset - (int)right);
        }

        public override int GetHashCode()
        {
            return _offset.GetHashCode();
        }

        public bool Equals(MemoryOffset other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is MemoryOffset offset && this == offset;
        }

        public int CompareTo(MemoryOffset other)
        {
            return _offset.CompareTo(other._offset);
        }

        public override string ToString()
        {
            return _offset == 0 ? "+0x0" :
                IsNegative ? $"-0x{-_offset:x}" : $"+0x{_offset:x}";
        }
    }
}
