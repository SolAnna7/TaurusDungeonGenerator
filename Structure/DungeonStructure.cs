using System;
using System.Collections.Generic;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    public class DungeonStructure
    {
        public DungeonNode StartElement { get; }

        public NodeMetaData NodeMetaData => StartElement.MetaData;
        public StructureMetaData StructureMetaData { get; } = new StructureMetaData();

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

    public class StructureMetaData : ITagHolder, IPropertyHolder
    {
        public float MarginUnit { get; set; }
        
        
        #region Tag and property data

        private readonly PropertyAndTagHolder _tagAndPropertyHolder = new PropertyAndTagHolder();

        public PropertyAndTagHolder TagAndPropertyHolder => _tagAndPropertyHolder;

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