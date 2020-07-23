using System;
using System.Collections.Generic;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    /// <summary>
    /// Interface for classes containing branch data
    /// </summary>
    public interface IBranchDataHolder
    {
        BranchDataWrapper BranchDataWrapper { get; set; }
    }

    public static class IBranchDataHolderExtensions
    {
        public static bool HasBranches(this IBranchDataHolder holder) =>
            holder?.BranchDataWrapper != null && (
                holder.BranchDataWrapper.BranchCount.HasValue || holder.BranchDataWrapper.BranchPercentage.HasValue);
    }

    /// <summary>
    /// Dungeon branch (optional extra paths) data
    /// </summary>
    public class BranchDataWrapper : ICloneable
    {
        /// <summary>
        /// The possible names of nested dungeons which from the branches should be created
        /// </summary>
        public List<string> BranchPrototypeNames { get; }

        /// <summary>
        /// Max number of branches in the dungeon. Either this or BranchPercentage should be filled (but not both)
        /// </summary>
        public int? BranchCount { get; set; }

        /// <summary>
        /// Max percentage of free connections where the generator should add a branch in the dungeon. Either this or BranchCount should be filled (but not both)
        /// </summary>
        public float? BranchPercentage { get; set; }

        public BranchDataWrapper(List<string> branchPrototypeNames, int branchCount)
        {
            BranchPrototypeNames = branchPrototypeNames;
            BranchCount = branchCount;
        }

        public BranchDataWrapper(List<string> branchPrototypeNames, float branchPercentage)
        {
            BranchPrototypeNames = branchPrototypeNames;
            BranchPercentage = branchPercentage;
        }

        public object Clone()
        {
            if (BranchCount.HasValue)
                return new BranchDataWrapper(BranchPrototypeNames, BranchCount.Value);
            if (BranchPercentage.HasValue)
                return new BranchDataWrapper(BranchPrototypeNames, BranchPercentage.Value);
            throw new Exception("One value must be filled!");
        }
    }
}