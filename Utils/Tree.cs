using System;
using System.Collections.Generic;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils
{
    /// <summary>
    /// Interface for an abstract tree node element. Used together with the extension functions
    /// </summary>
    public interface ITraversableTreeNode<out T>
    {
        IEnumerable<ITraversableTreeNode<T>> ChildNodes { get; }
        T Value { get; }
    }

    /// <summary>
    /// Extension methods for the ITraversableTreeNode interface
    /// </summary>
    public static class TreeExtensions
    {
        /// <summary>
        /// Returns an IEnumerable of the tree elements in reverse Depth First order (takes the leaves first)
        /// </summary>
        public static IEnumerable<T> TraverseDepthFirstReverse<T>(this ITraversableTreeNode<T> traversableTreeNode)
        {
            foreach (ITraversableTreeNode<T> child in traversableTreeNode.ChildNodes)
            {
                foreach (T collectedNodes in child.TraverseDepthFirstReverse())
                {
                    yield return collectedNodes;
                }
            }

            yield return traversableTreeNode.Value;
        }

        /// <summary>
        /// Returns an IEnumerable of the tree elements in Depth First order (takes the root first)
        /// </summary>
        public static IEnumerable<T> TraverseDepthFirst<T>(this ITraversableTreeNode<T> traversableTreeNode)
        {
            yield return traversableTreeNode.Value;

            foreach (ITraversableTreeNode<T> child in traversableTreeNode.ChildNodes)
            {
                foreach (T collectedNodes in child.TraverseDepthFirst())
                {
                    yield return collectedNodes;
                }
            }
        }

        public static void ForeachDepthFirst<T>(this ITraversableTreeNode<T> traversableTreeNode, Action<T, int> action) => ForeachDepthFirstRecursive(traversableTreeNode, action, 0);

        private static void ForeachDepthFirstRecursive<T>(ITraversableTreeNode<T> traversableTreeNode, Action<T, int> action, int depth)
        {
            action(traversableTreeNode.Value, depth);
            foreach (ITraversableTreeNode<T> child in traversableTreeNode.ChildNodes)
            {
                ForeachDepthFirstRecursive(child, action, depth + 1);
            }
        }
    }
}