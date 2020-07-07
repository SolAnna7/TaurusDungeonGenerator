using System;
using System.Collections.Generic;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    public class DungeonNode : ITraversableTree<DungeonNode>
    {
        public List<DungeonNode> SubElements { get; }

        public string Style { get; }

        public Room Room { get; set; }

        public NodeMetaData MetaData { get; set; }

        public DungeonNode(string style, NodeMetaData metaData, List<DungeonNode> subElements = null)
        {
            SubElements = subElements ?? new List<DungeonNode>();
            Style = style;
            MetaData = metaData;
        }

        public void AddSubElement(DungeonNode newSub) => SubElements.Add(newSub);
        public void RemoveSubElement(DungeonNode newSub) => SubElements.Remove(newSub);

        public bool IsEndNode => SubElements == null || SubElements.Count == 0;
        public IEnumerable<ITraversableTree<DungeonNode>> Children => SubElements;
        public DungeonNode Node => this;

    }
}