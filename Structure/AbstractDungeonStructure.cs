using System;
using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Exceptions;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    public class AbstractDungeonStructure : IBranchDataHolder
    {
        public AbstractDungeonElement StartElement { get; private set; }

        public Dictionary<string, AbstractDungeonStructure> EmbeddedDungeons { get; set; } = new Dictionary<string, AbstractDungeonStructure>();

        public BranchDataWrapper BranchDataWrapper { get; set; }

        public AbstractDungeonStructure(AbstractDungeonElement startElement)
        {
            StartElement = startElement;
        }

        public void ValidateStructure()
        {
            List<string> messages = new List<string>();

            if (BranchDataWrapper != null && EmbeddedDungeons != null)
            {
                foreach (var branchPrototypeName in BranchDataWrapper.BranchPrototypeNames)
                {
                    if (!EmbeddedDungeons.Keys.Contains(branchPrototypeName))
                    {
                        messages.Add($"Branch prototype [{branchPrototypeName}] is not found in embedded dungeons!");
                    }
                }
            }

            if (messages.Count != 0)
                throw new DungeonValidationException("Error(s) in AbstractDungeonStructure validation:\n - " + String.Join("\n - ", messages));
        }
    }


    public abstract class AbstractDungeonElement : ITagHolder, IPropertyHolder
    {
        public string Style { get; protected set; }

        public List<AbstractDungeonElement> SubElements
        {
            get { return subElements; }
        }

        protected List<AbstractDungeonElement> subElements = new List<AbstractDungeonElement>();
        private readonly PropertyAndTagHolder _tagAndPropertyHolder = new PropertyAndTagHolder();
        public PropertyAndTagHolder TagAndPropertyHolder => _tagAndPropertyHolder;

        public bool IsOptional { get; set; } = false;
        public bool IsTansit { get; set; } = false;

        protected AbstractDungeonElement()
        {
        }

        protected AbstractDungeonElement(string style) => Style = style;

        protected AbstractDungeonElement(string style, params AbstractDungeonElement[] subElements) : this(style)
        {
            this.subElements = new List<AbstractDungeonElement>(subElements);
        }

        public void AddSubElement(AbstractDungeonElement newSub) => subElements.Add(newSub);

        #region Tag and property accessors

        public object GetProperty(string key) => _tagAndPropertyHolder.GetProperty(key);

        public T GetPropertyAs<T>(string key) => _tagAndPropertyHolder.GetPropertyAs<T>(key);

        public bool HasProperty(string key) => _tagAndPropertyHolder.HasProperty(key);

        public IEnumerable<Tuple<string, object>> GetProperties() => _tagAndPropertyHolder.GetProperties();

        public void AddProperty<T>(string key, T value) => _tagAndPropertyHolder.AddProperty(key, value);

        public void RemoveProperty(string key) => _tagAndPropertyHolder.RemoveProperty(key);
        public bool HasTag(string tag) => _tagAndPropertyHolder.HasTag(tag);

        public IEnumerable<string> GetTags() => _tagAndPropertyHolder.GetTags();

        public void AddTag(string tag) => _tagAndPropertyHolder.AddTag(tag);

        public void RemoveTag(string tag) => _tagAndPropertyHolder.RemoveTag(tag);

        #endregion
    }


    public class ConnectionElement : AbstractDungeonElement
    {
        public ConnectionElement(string style, RangeI length, params AbstractDungeonElement[] subElements) : base(style, subElements)
        {
            if (length.Min <= 0)
                throw new Exception("Length must be 1 or greater");
            Length = length;
        }

        public RangeI Length { get; private set; }
    }

    public class NodeElement : AbstractDungeonElement
    {
        public NodeElement(string style) : base(style)
        {
        }

        public NodeElement(string style, params AbstractDungeonElement[] subElements) : base(style, subElements)
        {
        }

        public bool IsEndNode
        {
            get { return subElements.Count == 0; }
        }
    }

    public class NestedDungeon : AbstractDungeonElement
    {
        public string Path { get; private set; }

        public NestedDungeon(string path)
        {
            Path = path;
        }

        public NestedDungeon(string path, params AbstractDungeonElement[] subElements)
        {
            Path = path;
            this.subElements = new List<AbstractDungeonElement>(subElements);
        }
    }
}