using System;
using System.Collections.Generic;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils
{
    /// <summary>
    /// Container of extensions used in Taurus
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Randomizes the order of elements in a list. The input list is changed!
        /// </summary>
        /// <returns>The randomized list</returns>
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

        /// <summary>
        /// Returns a random element of the list
        /// </summary>
        public static T GetRandomElement<T>(this IList<T> list, Random random) => list[random.Next(list.Count)];

        /// <summary>
        /// Java style foreach for IEnumerables
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> enu, Action<T> action)
        {
            foreach (var item in enu) action(item);
        }

        #region Kotlin style scope functions
        
        /// <summary>
        /// Kotlin style Also function
        /// Calls the specified action with this value as its argument and returns this value
        /// </summary>
        public static T Also<T>(this T obj, Action<T> a)
        {
            a(obj);
            return obj;
        }

        /// <summary>
        /// Kotlin style Let function
        /// Calls the specified function with this value as its argument and returns its result
        /// </summary>
        public static Tr Let<T, Tr>(this T obj, Func<T, Tr> f) => f(obj);

        #endregion
    }
}