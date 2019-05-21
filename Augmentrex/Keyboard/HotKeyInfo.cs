using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Augmentrex.Keyboard
{
    [Serializable]
    public sealed class HotKeyInfo : IEquatable<HotKeyInfo>
    {
        public Keys Key { get; }

        public bool Alt { get; }

        public bool Control { get; }

        public bool Shift { get; }

        internal int Id { get; set; }

        internal HashSet<HotKeyHandler> Handlers { get; } = new HashSet<HotKeyHandler>();

        public HotKeyInfo(Keys key, bool alt, bool control, bool shift)
        {
            Key = key;
            Alt = alt;
            Control = control;
            Shift = shift;
        }

        public static bool operator ==(HotKeyInfo left, HotKeyInfo right)
        {
            return left.Key == right.Key && left.Alt == right.Alt && left.Control == right.Control && left.Shift == right.Shift;
        }

        public static bool operator !=(HotKeyInfo left, HotKeyInfo right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            var hash = -1938637507;

            hash = hash * -1521134295 + Key.GetHashCode();
            hash = hash * -1521134295 + Alt.GetHashCode();
            hash = hash * -1521134295 + Control.GetHashCode();
            hash = hash * -1521134295 + Shift.GetHashCode();

            return hash;
        }

        public bool Equals(HotKeyInfo other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is HotKeyInfo info && this == info;
        }

        public override string ToString()
        {
            var str = string.Empty;

            if (Alt)
                str += "Alt+";

            if (Control)
                str += "Ctrl+";

            if (Shift)
                str += "Shift+";

            return $"{str}{Key}";
        }
    }
}
