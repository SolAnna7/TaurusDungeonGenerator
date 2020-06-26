using System.Collections.Generic;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    public interface IPropertyHolder
    {
        object GetProperty(string key);
        T GetPropertyAs<T>(string key);
        bool HasProperty(string key);
        void AddProperty<T>(string key, T value);
        void RemoveProperty(string key);
    }

    public interface ITagHolder
    {
        bool HasTag(string tag);
        void AddTag(string tag);
        void RemoveTag<T>(string tag);
    }

    public class PropertyAndTagHolder : IPropertyHolder, ITagHolder
    {
        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        public object GetProperty(string key) => _properties[key];

        public T GetPropertyAs<T>(string key) => (T) _properties[key];

        public bool HasProperty(string key) => _properties.ContainsKey(key);

        public void AddProperty<T>(string key, T value) => _properties.Add(key, value);

        public void RemoveProperty(string key) => _properties.Remove(key);

        public bool HasTag(string tag) => HasProperty(tag);

        public void AddTag(string tag) => _properties.Add(tag, null);

        public void RemoveTag<T>(string tag)
        {
            RemoveProperty(tag);
        }
    }
}