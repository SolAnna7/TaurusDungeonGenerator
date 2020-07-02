using System;
using System.Collections.Generic;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    public class DungeonNode : ITraversableTree<DungeonNode>, ITagHolder, IPropertyHolder
    {
        private readonly PropertyAndTagHolder _tagAndPropertyHolder = new PropertyAndTagHolder();
        public List<DungeonNode> SubElements { get; }

        public string Style { get; }

        public Room Room { get; set; }

        public StructureMetaData StructureMetaData { get; set; }

        public DungeonNode(string style, PropertyAndTagHolder holder = null, List<DungeonNode> subElements = null)
        {
            SubElements = subElements ?? new List<DungeonNode>();
            Style = style;

            StructureMetaData = new StructureMetaData();

            if (holder != null)
            {
                holder.GetTags().ForEach(AddTag);
                holder.GetProperties().ForEach(tuple => AddProperty(tuple.Item1, tuple.Item2));
            }
        }

        public void AddSubElement(DungeonNode newSub) => SubElements.Add(newSub);

        public bool IsEndNode => SubElements == null || SubElements.Count == 0;
        public IEnumerable<ITraversableTree<DungeonNode>> Children => SubElements;
        public DungeonNode Node => this;

        public PropertyAndTagHolder TagAndPropertyHolder => _tagAndPropertyHolder;

        #region Tag and property accessors

        public object GetProperty(string key) => TagAndPropertyHolder.GetProperty(key);

        public T GetPropertyAs<T>(string key) => TagAndPropertyHolder.GetPropertyAs<T>(key);

        public bool HasProperty(string key) => TagAndPropertyHolder.HasProperty(key);
        public IEnumerable<Tuple<string, object>> GetProperties() => TagAndPropertyHolder.GetProperties();

        public void AddProperty<T>(string key, T value) => TagAndPropertyHolder.AddProperty(key, value);

        public void RemoveProperty(string key) => TagAndPropertyHolder.RemoveProperty(key);

        public bool HasTag(string tag) => TagAndPropertyHolder.HasTag(tag);

        public IEnumerable<string> GetTags() => TagAndPropertyHolder.GetTags();

        public void AddTag(string tag) => TagAndPropertyHolder.AddTag(tag);

        public void RemoveTag(string tag) => TagAndPropertyHolder.RemoveTag(tag);

        #endregion
    }
}