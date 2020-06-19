using System;
using System.Collections.Generic;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils
{
    public class Tag
    {
        public readonly string Value;

        public Tag(string value)
        {
            Value = value;
        }

        protected bool Equals(Tag other)
        {
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class ValueEqualityComparer : IEqualityComparer<Tag>
        {
            public bool Equals(Tag x, Tag y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.Value, y.Value);
            }

            public int GetHashCode(Tag obj)
            {
                return (obj.Value != null ? obj.Value.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<Tag> ValueComparer { get; } = new ValueEqualityComparer();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Tag;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
        }
    }
}