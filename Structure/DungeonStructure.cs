using System;
using System.Collections.Generic;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    public class DungeonStructure
    {
        public DungeonNode StartElement { get; }
        
        public StructureMetaData StructureMetaData => StartElement.StructureMetaData;

        public AbstractDungeonStructure AbstractStructure { get; }
        
        public DungeonStructure(DungeonNode startElement, AbstractDungeonStructure abstractStructure)
        {
            StartElement = startElement;
            AbstractStructure = abstractStructure;
        }

        [Obsolete]
        public HashSet<DungeonNode> GetNodeSet()
        {
            HashSet<DungeonNode> nodesCollector = new HashSet<DungeonNode>();
            GetNodeSetRecur(nodesCollector, StartElement);
            return nodesCollector;
        }

        private void GetNodeSetRecur(HashSet<DungeonNode> nodesCollector, DungeonNode actualNode)
        {
            if (nodesCollector.Contains(actualNode))
                return;

            nodesCollector.Add(actualNode);

            foreach (var subElement in actualNode.SubElements)
            {
                GetNodeSetRecur(nodesCollector, subElement);
            }
        }
    }

    public class StructureMetaData : IBranchDataHolder
    {
        public bool IsTransit { get; set; } = false;

        //includes this node if it is a transit
        public int SubTransitNum { get; set; } = 0;
        public List<DungeonNode> ChildOptionalNodes { get; set; } = new List<DungeonNode>();
        public OptionalNodeData OptionalNodeData = null;
        public BranchDataWrapper BranchDataWrapper { get; set; }
    }

    public class OptionalNodeData
    {
        public bool Required { get; set; } = true;
    }
}