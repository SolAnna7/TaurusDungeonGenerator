using System;
using System.Collections.Generic;
using System.Linq;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    public interface IPropertyHolder
    {
        object GetProperty(string key);
        T GetPropertyAs<T>(string key);
        bool HasProperty(string key);
        IEnumerable<Tuple<string, object>> GetProperties();
        void AddProperty<T>(string key, T value);
        void RemoveProperty(string key);
    }

    public interface ITagHolder
    {
        bool HasTag(string tag);
        IEnumerable<string> GetTags();
        void AddTag(string tag);
        void RemoveTag(string tag);
    }

    public class PropertyAndTagHolder : IPropertyHolder, ITagHolder
    {
        private Dictionary<string, object> _properties = new Dictionary<string, object>();
        private ISet<string> _tags = new HashSet<string>();

        public object GetProperty(string key) => _properties[key];

        public T GetPropertyAs<T>(string key) => (T) _properties[key];

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
    }
}