using System;
using System.Collections.Generic;
using Boo.Lang;

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

        //Kotlin style scope functions
        
        public static T Also<T>(this T obj, Action<T> a)
        {
            a(obj);
            return obj;
        }

        public static TR Let<T, TR>(this T obj, Function<T, TR> f)
        {
            return f(obj);
        }
    }
}