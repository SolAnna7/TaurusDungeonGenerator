using System;
using System.Collections.Generic;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils
{
    public static class Extensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list, Random random)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static T AnyRandom<T>(this IList<T> list, Random random) => list[random.Next(list.Count)];

        public static void ForEach<T>(this IEnumerable<T> enu, Action<T> a)
        {
            foreach (var item in enu)
            {
                a(item);
            }
        }

    }
}