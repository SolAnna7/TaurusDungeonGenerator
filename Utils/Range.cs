using System;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils
{
    public struct RangeI
    {
        public int Min { get; private set; }
        public int Max { get; private set; }

        public RangeI(int min, int max)
        {
            if (min > max)
                throw new Exception($"Min value [{min}] is greater then max value [{max}]");

            Min = min;
            Max = max;
        }

        public int GetRandom(Random r)
        {
            return r.Next(Min, Max + 1);
        }
    }
}