using System.Collections.Generic;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
using Room = SnowFlakeGamesAssets.TaurusDungeonGenerator.Component.Room;

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

    public class DungeonNode : ITraversableTree<DungeonNode>
    {
        public List<DungeonNode> SubElements { get; }

        public string Style { get; }

        public Room Room { get; set; }

        public ISet<Tag> Tags { get; } = new HashSet<Tag>();

        public StructureMetaData StructureMetaData { get; set; }

        public DungeonNode(string style, IEnumerable<Tag> tags, List<DungeonNode> subElements = null)
        {
            SubElements = subElements ?? new List<DungeonNode>();
            Style = style;
            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }

            StructureMetaData = new StructureMetaData();
        }

        public void AddSubElement(DungeonNode newSub) => SubElements.Add(newSub);

        public bool IsEndNode => SubElements == null || SubElements.Count == 0;
        public IEnumerable<ITraversableTree<DungeonNode>> Children => SubElements;
        public DungeonNode Node => this;
    }

    public class StructureMetaData : IBranchDataHolder
    {
        public bool IsTransit { get; set; } = false;

        //includes this node if it is transit
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