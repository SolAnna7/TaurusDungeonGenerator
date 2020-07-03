using System;
using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
using UnityEngine.Profiling.Memory.Experimental;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    public class NodeMetaData : IBranchDataHolder, ITagHolder, IPropertyHolder, ICloneable
    {
        public BranchDataWrapper BranchDataWrapper { get; set; }
        
        [Obsolete]
        public bool IsTransit { get; set; } = false;
        
        //includes this node if it is a transit
        [Obsolete]
        public int SubTransitNum { get; set; } = 0;
        public List<DungeonNode> ChildOptionalNodes { get; set; } = new List<DungeonNode>();

        public OptionalNodeData OptionalNodeData = null;

        public NodeMetaData(BranchDataWrapper branchDataWrapper = null, PropertyAndTagHolder holder = null)
        {
            BranchDataWrapper = branchDataWrapper;

            if (holder != null)
            {
                holder.GetTags().ForEach(AddTag);
                holder.GetProperties().ForEach(tuple => AddProperty(tuple.Item1, tuple.Item2));
            }
        }

        public static NodeMetaData Empty()
        {
            return new NodeMetaData();
        }

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


        public object Clone()
        {
            var clone = new NodeMetaData((BranchDataWrapper) BranchDataWrapper?.Clone(), (PropertyAndTagHolder) _tagAndPropertyHolder.Clone());
            clone.OptionalNodeData = (OptionalNodeData) OptionalNodeData?.Clone();
            clone.IsTransit = IsTransit;
            clone.SubTransitNum = SubTransitNum;
            clone.ChildOptionalNodes = ChildOptionalNodes.ToList();
            return clone;
        }
    }

    public class OptionalNodeData : ICloneable
    {
        public bool Required { get; set; } = true;


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}