using System;
using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Exceptions;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    /// <summary>
    /// The structure wrapper
    /// </summary>
    public class AbstractDungeonStructure : IBranchDataHolder
    {
        /// <summary>
        /// Root element of the tree
        /// </summary>
        public AbstractDungeonElement StartElement { get; private set; }

        /// <summary>
        /// Reusable embedded sub dungeons
        /// </summary>
        public Dictionary<string, AbstractDungeonStructure> EmbeddedDungeons { get; set; } = new Dictionary<string, AbstractDungeonStructure>();

        /// <summary>
        /// Data about the branches (optional extra paths) of the dungeon
        /// </summary>
        public BranchDataWrapper BranchDataWrapper { get; set; }

        /// <summary>
        /// Other data of the dungeon
        /// </summary>
        public StructureMetaData StructureMetaData { get; private set; }

        public static AbstractDungeonStructureBuilder Builder => new AbstractDungeonStructureBuilder();

        private AbstractDungeonStructure(AbstractDungeonElement startElement, StructureMetaData structureMetaData)
        {
            StartElement = startElement;
            StructureMetaData = structureMetaData;

            StructureMetaData.MaxOptionalEndpointNum = RecalculateMaxEndpointNum(startElement);
            StructureMetaData.MinOptionalEndpointNum = RecalculateMinEndpointNum(startElement);
        }

        private AbstractDungeonStructure()
        {
        }

        /// <summary>
        /// Validates the dungeon structure
        /// </summary>
        /// <exception cref="DungeonValidationException">Thrown one error with all the messages inside</exception>
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

        private uint RecalculateMinEndpointNum(AbstractDungeonElement element)
        {
            uint res = 0;
            if (element.ElementMetaData.OptionalNode)
                return res;
            if (element.ElementMetaData.OptionalEndpoint)
                res++;
            return element.SubElements.Aggregate(0u, (sum, e) => sum + RecalculateMinEndpointNum(e)) + res;
        }

        private uint RecalculateMaxEndpointNum(AbstractDungeonElement element)
        {
            uint res = 0;
            if (element.ElementMetaData.OptionalEndpoint)
                res++;
            return element.SubElements.Aggregate(0u, (sum, e) => sum + RecalculateMaxEndpointNum(e)) + res;
        }


        public class AbstractDungeonStructureBuilder
        {
            protected AbstractDungeonStructure newInstance;

            public AbstractDungeonStructureBuilder()
            {
                newInstance = new AbstractDungeonStructure();
                newInstance.StructureMetaData = StructureMetaData.Builder.Empty;
            }

            public AbstractDungeonStructureBuilder SetMetaData(StructureMetaData s) => this.Also(x => newInstance.StructureMetaData = s);
            public AbstractDungeonStructureBuilder SetBranchData(BranchDataWrapper wrapper) => this.Also(x => newInstance.BranchDataWrapper = wrapper);
            public AbstractDungeonStructureBuilder SetEmbeddedDungeons(Dictionary<string, AbstractDungeonStructure> dungeons) => this.Also(x => newInstance.EmbeddedDungeons = dungeons);

            public AbstractDungeonStructureBuilder AddEmbeddedDungeon(string key, AbstractDungeonStructure dungeonStructure) => this.Also(x =>
            {
                if (newInstance.EmbeddedDungeons == null)
                    newInstance.EmbeddedDungeons = new Dictionary<string, AbstractDungeonStructure>();
                newInstance.EmbeddedDungeons.Add(key, dungeonStructure);
            });

            public AbstractDungeonStructureBuilderFinisher SetStartElement(AbstractDungeonElement startElement) => new AbstractDungeonStructureBuilderFinisher(newInstance, startElement);
        }

        public class AbstractDungeonStructureBuilderFinisher : AbstractDungeonStructureBuilder
        {
            public AbstractDungeonStructureBuilderFinisher(AbstractDungeonStructure newInstance, AbstractDungeonElement startElement)
            {
                this.newInstance = newInstance;
                this.newInstance.StartElement = startElement;
            }

            public new AbstractDungeonStructureBuilderFinisher SetMetaData(StructureMetaData s) => (AbstractDungeonStructureBuilderFinisher) base.SetMetaData(s);
            public new AbstractDungeonStructureBuilderFinisher SetBranchData(BranchDataWrapper wrapper) => (AbstractDungeonStructureBuilderFinisher) base.SetBranchData(wrapper);
            public new AbstractDungeonStructureBuilderFinisher SetEmbeddedDungeons(Dictionary<string, AbstractDungeonStructure> dungeons) => (AbstractDungeonStructureBuilderFinisher) base.SetEmbeddedDungeons(dungeons);

            public AbstractDungeonStructure Build()
            {
                newInstance.StructureMetaData.MaxOptionalEndpointNum = newInstance.RecalculateMaxEndpointNum(newInstance.StartElement);
                newInstance.StructureMetaData.MinOptionalEndpointNum = newInstance.RecalculateMinEndpointNum(newInstance.StartElement);
                newInstance.ValidateStructure();
                return newInstance;
            }
        }
    }

    /// <summary>
    /// The abstract base class of elements
    /// </summary>
    public abstract class AbstractDungeonElement : ITraversableTreeNode<AbstractDungeonElement>
    {
        /// <summary>
        /// The type of rooms this element could be built of. A file path to a RoomCollection
        /// </summary>
        public string Style { get; }

        /// <summary>
        /// Child elements
        /// </summary>
        public List<AbstractDungeonElement> SubElements => _subElements;

        // ReSharper disable once InconsistentNaming
        protected List<AbstractDungeonElement> _subElements = new List<AbstractDungeonElement>();

        /// <summary>
        /// Meta data of the element
        /// </summary>
        public NodeMetaData ElementMetaData { get; set; }

        protected AbstractDungeonElement()
        {
        }

        protected AbstractDungeonElement(string style, NodeMetaData elementMetaData)
        {
            Style = style;
            ElementMetaData = elementMetaData;
        }

        protected AbstractDungeonElement(string style, NodeMetaData elementMetaData, params AbstractDungeonElement[] subElements) : this(style, elementMetaData)
        {
            _subElements = new List<AbstractDungeonElement>(subElements);
        }

        public IEnumerable<ITraversableTreeNode<AbstractDungeonElement>> ChildNodes => _subElements;
        public AbstractDungeonElement Value => this;
    }

    public static class DungeonElementBuilder
    {
        public static NodeElement.NodeElementBuilder NodeElement(string style) => new NodeElement.NodeElementBuilder(style);
        public static ConnectionElement.ConnectionElementBuilder ConnectionElement(string style, RangeI length) => new ConnectionElement.ConnectionElementBuilder(style, length);
        public static NestedDungeon.NestedDungeonElementBuilder NestedDungeonElement(string path) => new NestedDungeon.NestedDungeonElementBuilder(path);
    }

    public interface IElementBuilder<out T> where T : AbstractDungeonElement
    {
        T Build();
    }

    /// <summary>
    /// An abstract dungeon element representing a line of rooms
    /// </summary>
    public class NodeElement : AbstractDungeonElement
    {
        private NodeElement(string style) : base(style, NodeMetaData.Builder.Empty)
        {
        }

        /// <summary>
        /// True if this is a leaf in the tree
        /// </summary>
        public bool IsEndNode => _subElements.Count == 0;

        public class NodeElementBuilder : IElementBuilder<NodeElement>
        {
            private readonly NodeElement _element;

            public NodeElementBuilder(string style) => _element = new NodeElement(style);

            public NodeElementBuilder AddSubElement(AbstractDungeonElement element) => this.Also(x => _element.SubElements.Add(element));
            public NodeElementBuilder AddSubElement<T>(IElementBuilder<T> element) where T : AbstractDungeonElement => this.Also(x => _element.SubElements.Add(element.Build()));
            public NodeElementBuilder SetMetaData(NodeMetaData metaData) => this.Also(x => _element.ElementMetaData = metaData);

            public NodeElement Build() => _element;
        }
    }

    /// <summary>
    /// An abstract dungeon element representing a sequence of rooms
    /// </summary>
    public class ConnectionElement : AbstractDungeonElement
    {
        private ConnectionElement(string style, RangeI length, params AbstractDungeonElement[] subElements) : base(style, NodeMetaData.Builder.Empty, subElements)
        {
            if (length.Min <= 0)
                throw new Exception("Length must be 1 or greater");
            Length = length;
        }

        /// <summary>
        /// length (number of rooms) of the sequence
        /// </summary>
        public RangeI Length { get; }

        public class ConnectionElementBuilder : IElementBuilder<ConnectionElement>
        {
            private readonly ConnectionElement _element;

            public ConnectionElementBuilder(string style, RangeI length) => _element = new ConnectionElement(style, length);

            public ConnectionElementBuilder AddSubElement(AbstractDungeonElement element) => this.Also(x => _element.SubElements.Add(element));
            public ConnectionElementBuilder AddSubElement<T>(IElementBuilder<T> element) where T : AbstractDungeonElement => this.Also(x => _element.SubElements.Add(element.Build()));

            public ConnectionElementBuilder SetMetaData(NodeMetaData metaData) => this.Also(x => _element.ElementMetaData = metaData);

            public ConnectionElement Build() => _element;
        }
    }

    /// <summary>
    /// An abstract dungeon element representing another dungeon nested inside
    /// </summary>
    public class NestedDungeon : AbstractDungeonElement
    {
        /// <summary>
        /// Identifier path of the nested dungeon
        /// </summary>
        public string Path { get; }

        private NestedDungeon(string path)
        {
            Path = path;
            ElementMetaData = NodeMetaData.Builder.Empty;
        }

        public class NestedDungeonElementBuilder : IElementBuilder<NestedDungeon>
        {
            private readonly NestedDungeon _element;

            public NestedDungeonElementBuilder(string path) => _element = new NestedDungeon(path);

            public NestedDungeonElementBuilder AddSubElement(AbstractDungeonElement element) => this.Also(x => _element.SubElements.Add(element));

            public NestedDungeonElementBuilder AddSubElement<T>(IElementBuilder<T> element) where T : AbstractDungeonElement => this.Also(x => _element.SubElements.Add(element.Build()));
            
            public NestedDungeonElementBuilder SetMetaData(NodeMetaData metaData) => this.Also(x => _element.ElementMetaData = metaData);

            public NestedDungeon Build() => _element;
        }
    }
}