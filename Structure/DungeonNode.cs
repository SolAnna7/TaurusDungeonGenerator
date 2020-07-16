using System;
using System.Collections.Generic;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    /// <summary>
    /// Node of the DungeonStructure tree. Used in the generation process
    /// </summary>
    public class DungeonNode : ITraversableTreeNode<DungeonNode>
    {
        /// <summary>
        /// Child nodes
        /// </summary>
        public List<DungeonNode> SubElements { get; }

        /// <summary>
        /// The type of rooms this element could be built of. A file path to a RoomCollection
        /// </summary>
        public string Style { get; }

        /// <summary>
        /// Built room set after building
        /// </summary>
        public Room Room { get; set; }
        
        /// <summary>
        /// Meta data of the node
        /// </summary>
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
        public IEnumerable<ITraversableTreeNode<DungeonNode>> ChildNodes => SubElements;
        public DungeonNode Value => this;

    }
}