using System;
using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    /// <summary>
    /// The node specific non-structural data
    /// </summary>
    public class NodeMetaData : IBranchDataHolder, ITagHolder, IPropertyHolder, ICloneable
    {
        public BranchDataWrapper BranchDataWrapper { get; set; }

        public bool OptionalEndpoint { get; set; } = false;
        public bool OptionalNode { get; set; } = false;

        public List<DungeonNode> ChildOptionalNodes { get; set; } = new List<DungeonNode>();
        
        public NodeMetaData(BranchDataWrapper branchDataWrapper = null, PropertyAndTagHolder holder = null)
        {
            BranchDataWrapper = branchDataWrapper;

            if (holder != null)
            {
                holder.GetTags().ForEach(AddTag);
                holder.GetProperties().ForEach(tuple => AddProperty(tuple.Item1, tuple.Item2));
            }
        }

        public static NodeMetaData Empty => new NodeMetaData();

        #region Tag and property data

        private readonly PropertyAndTagHolder _tagAndPropertyHolder = new PropertyAndTagHolder();

        public PropertyAndTagHolder TagAndPropertyHolder => _tagAndPropertyHolder;

        public object GetProperty(string key) => TagAndPropertyHolder.GetProperty(key);

        public T GetPropertyAs<T>(string key) => TagAndPropertyHolder.GetPropertyAs<T>(key);
        public bool TryGetPropertyAs<T>(string key, out T value) => TagAndPropertyHolder.TryGetPropertyAs(key, out value);

        public bool HasProperty(string key) => TagAndPropertyHolder.HasProperty(key);
        public IEnumerable<Tuple<string, object>> GetProperties() => TagAndPropertyHolder.GetProperties();

        public void AddProperty<T>(string key, T value) => TagAndPropertyHolder.AddProperty(key, value);

        public void RemoveProperty(string key) => TagAndPropertyHolder.RemoveProperty(key);

        public bool HasTag(string tag) => TagAndPropertyHolder.HasTag(tag);

        public IEnumerable<string> GetTags() => TagAndPropertyHolder.GetTags();

        public void AddTag(string tag) => TagAndPropertyHolder.AddTag(tag);

        public void RemoveTag(string tag) => TagAndPropertyHolder.RemoveTag(tag);

        #endregion


        public object Clone()
        {
            var clone = new NodeMetaData((BranchDataWrapper) BranchDataWrapper?.Clone(), (PropertyAndTagHolder) _tagAndPropertyHolder.Clone());
            clone.OptionalNode =  OptionalNode;
            clone.OptionalEndpoint = OptionalEndpoint;
            clone.ChildOptionalNodes = ChildOptionalNodes.ToList();
            return clone;
        }
    }

    /// <summary>
    /// Meta infos about the whole dungeon structure
    /// </summary>
    public class StructureMetaData : ITagHolder, IPropertyHolder, ICloneable
    {
        private uint? _minOptionalEndpointNum;
        private uint? _maxOptionalEndpointNum;
        public float MarginUnit { get; set; }

        public uint MinOptionalEndpointNum
        {
            get => _minOptionalEndpointNum ?? throw new Exception("MinOptionalEndpointNum is not set");
            set => _minOptionalEndpointNum = value;
        }

        public uint MaxOptionalEndpointNum
        {
            get => _maxOptionalEndpointNum ?? throw new Exception("MaxOptionalEndpointNum is not set");
            set => _maxOptionalEndpointNum = value;
        }

        public StructureMetaData(
            float marginUnit = 0,
            PropertyAndTagHolder structurePropertyAndTagHolder = null,
            PropertyAndTagHolder globalNodePropertyAndTagHolder = null)
        {
            MarginUnit = marginUnit;
            StructurePropertyAndTagHolder = structurePropertyAndTagHolder ?? new PropertyAndTagHolder();
            GlobalNodePropertyAndTagHolder = globalNodePropertyAndTagHolder ?? new PropertyAndTagHolder();
        }

        public static StructureMetaData Empty => new StructureMetaData();

        #region Tag and property data

        public PropertyAndTagHolder StructurePropertyAndTagHolder { get; }

        public PropertyAndTagHolder GlobalNodePropertyAndTagHolder { get; }

        public object GetProperty(string key) => StructurePropertyAndTagHolder.GetProperty(key);

        public T GetPropertyAs<T>(string key) => StructurePropertyAndTagHolder.GetPropertyAs<T>(key);
        public bool TryGetPropertyAs<T>(string key, out T value) => StructurePropertyAndTagHolder.TryGetPropertyAs(key, out value);

        public bool HasProperty(string key) => StructurePropertyAndTagHolder.HasProperty(key);
        public IEnumerable<Tuple<string, object>> GetProperties() => StructurePropertyAndTagHolder.GetProperties();

        public void AddProperty<T>(string key, T value) => StructurePropertyAndTagHolder.AddProperty(key, value);

        public void RemoveProperty(string key) => StructurePropertyAndTagHolder.RemoveProperty(key);

        public bool HasTag(string tag) => StructurePropertyAndTagHolder.HasTag(tag);

        public IEnumerable<string> GetTags() => StructurePropertyAndTagHolder.GetTags();

        public void AddTag(string tag) => StructurePropertyAndTagHolder.AddTag(tag);

        public void RemoveTag(string tag) => StructurePropertyAndTagHolder.RemoveTag(tag);

        #endregion

        public object Clone()
        {
            return new StructureMetaData(MarginUnit, (PropertyAndTagHolder) StructurePropertyAndTagHolder.Clone(), (PropertyAndTagHolder) GlobalNodePropertyAndTagHolder.Clone());
        }
    }
}