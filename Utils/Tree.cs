using System;
using System.Collections.Generic;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils
{
    public interface ITraversableTree<T>
    {
        IEnumerable<ITraversableTree<T>> Children { get; }
        T Node { get; }
    }

    public static class TreeExtensions
    {
        public static IEnumerable<T> TraverseDownToTop<T>(this ITraversableTree<T> traversableTree)
        {
            foreach (ITraversableTree<T> child in traversableTree.Children)
            {
                foreach (T collectedNodes in child.TraverseDownToTop())
                {
                    yield return collectedNodes;
                }
            }

            yield return traversableTree.Node;
        }

        public static IEnumerable<T> TraverseTopToDown<T>(this ITraversableTree<T> traversableTree)
        {
            yield return traversableTree.Node;

            foreach (ITraversableTree<T> child in traversableTree.Children)
            {
                foreach (T collectedNodes in child.TraverseTopToDown())
                {
                    yield return collectedNodes;
                }
            }
        }

        public static void ForeachTopDownDepth<T>(this ITraversableTree<T> traversableTree, Action<T, int> action) => ForeachTopDownDepthR(traversableTree, action, 0);

        private static void ForeachTopDownDepthR<T>(ITraversableTree<T> traversableTree, Action<T, int> action, int depth)
        {
            action(traversableTree.Node, depth);
            foreach (ITraversableTree<T> child in traversableTree.Children)
            {
                ForeachTopDownDepthR<T>(child, action, depth + 1);
            }
        }
    }
}