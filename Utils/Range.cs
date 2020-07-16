using System;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils
{
    /// <summary>
    /// Represents an Integer range with a Min and Max value
    /// Immutable
    /// </summary>
    public struct RangeI
    {
        public int Min { get; }
        public int Max { get; }

        public RangeI(int min, int max)
        {
            if (min > max)
                throw new Exception($"Min value [{min}] is greater then max value [{max}]");

            Min = min;
            Max = max;
        }

        /// <summary>
        /// The min and the max values are the same
        /// </summary>
        public RangeI(int minmax) : this()
        {
            Min = minmax;
            Max = minmax;
        }

        /// <summary>
        /// Returns a random number between the min and max value. Both are inclusive.
        /// </summary>
        public int RandomNumberInRange(Random r)
        {
            return r.Next(Min, Max + 1);
        }
    }
}