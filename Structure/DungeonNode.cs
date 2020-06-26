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
}