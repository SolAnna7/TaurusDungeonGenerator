using System;
using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    /// <summary>
    /// Interface for storing extra objects in the database structure
    /// </summary>
    public interface IPropertyHolder
    {
        object GetProperty(string key);

        /// <summary>
        /// Returns property casted to type T
        /// </summary>
        /// <param name="key">The key of the property</param>
        /// <typeparam name="T">The type to cast</typeparam>
        T GetPropertyAs<T>(string key);
        bool TryGetPropertyAs<T>(string key, out T value);
        bool HasProperty(string key);
        IEnumerable<Tuple<string, object>> GetProperties();
        void AddProperty<T>(string key, T value);
        void RemoveProperty(string key);
    }

    /// <summary>
    /// Interface for storing string tags in the database structure
    /// </summary>
    public interface ITagHolder
    {
        bool HasTag(string tag);
        IEnumerable<string> GetTags();
        void AddTag(string tag);
        void RemoveTag(string tag);
    }

    /// <summary>
    /// Wrapper class to store tags and properties
    /// </summary>
    public class PropertyAndTagHolder : IPropertyHolder, ITagHolder, ICloneable
    {
        private Dictionary<string, object> _properties = new Dictionary<string, object>();
        private ISet<string> _tags = new HashSet<string>();

        public object GetProperty(string key) => _properties[key];

        public T GetPropertyAs<T>(string key) => (T) _properties[key];

        public bool TryGetPropertyAs<T>(string key, out T value)
        {
            bool res = _properties.TryGetValue(key, out object tmp);
            value = (T) tmp;
            return res;
        }

        public bool HasProperty(string key) => _properties.ContainsKey(key);

        public IEnumerable<Tuple<string, object>> GetProperties() => _properties.Keys.Select(k => new Tuple<string, object>(k, _properties[k]));

        public void AddProperty<T>(string key, T value) => _properties.Add(key, value);

        public void RemoveProperty(string key) => _properties.Remove(key);

        public bool HasTag(string tag) => _tags.Contains(tag);
        public IEnumerable<string> GetTags() => _tags.AsEnumerable();

        public void AddTag(string tag) => _tags.Add(tag);

        public void RemoveTag(string tag)
        {
            RemoveProperty(tag);
        }

        public object Clone()
        {
            var clone = new PropertyAndTagHolder();
            GetTags().ForEach(clone.AddTag);
            GetProperties().ForEach(p => clone.AddProperty(p.Item1, p.Item2));
            return clone;
        }
    }
}