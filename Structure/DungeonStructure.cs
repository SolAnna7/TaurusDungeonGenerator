using System;
using System.Collections.Generic;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    public class DungeonStructure
    {
        public DungeonNode StartElement { get; }

        public NodeMetaData NodeMetaData => StartElement.MetaData;
        public StructureMetaData StructureMetaData { get; }

        public AbstractDungeonStructure AbstractStructure { get; }

        public DungeonStructure(DungeonNode startElement, StructureMetaData structureMetaData, AbstractDungeonStructure abstractStructure)
        {
            StartElement = startElement;
            AbstractStructure = abstractStructure;
            StructureMetaData = structureMetaData;
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
}