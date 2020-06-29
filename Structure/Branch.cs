using System.Collections.Generic;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    public interface IBranchDataHolder
    {
        BranchDataWrapper BranchDataWrapper { get; set; }
    }

    public class BranchDataWrapper
    {
        public List<string> BranchPrototypeNames { get; }
        public int? BranchCount { get; set; }
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
    }
}