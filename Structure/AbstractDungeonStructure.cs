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
        public AbstractDungeonElement StartElement { get; }

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
        public StructureMetaData StructureMetaData { get; }

        public AbstractDungeonStructure(AbstractDungeonElement startElement, StructureMetaData structureMetaData)
        {
            StartElement = startElement;
            StructureMetaData = structureMetaData;

            StructureMetaData.MaxOptionalEndpointNum = RecalculateMaxEndpointNum(startElement);
            StructureMetaData.MinOptionalEndpointNum = RecalculateMinEndpointNum(startElement);
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
            this._subElements = new List<AbstractDungeonElement>(subElements);
        }

        // public void AddSubElement(AbstractDungeonElement newSub) => _subElements.Add(newSub);
        public IEnumerable<ITraversableTreeNode<AbstractDungeonElement>> ChildNodes => _subElements;
        public AbstractDungeonElement Value => this;
    }


    /// <summary>
    /// An abstract dungeon element representing a sequence of rooms
    /// </summary>
    public class ConnectionElement : AbstractDungeonElement
    {
        public ConnectionElement(string style, NodeMetaData elementMetaData, RangeI length, params AbstractDungeonElement[] subElements) : base(style, elementMetaData, subElements)
        {
            if (length.Min <= 0)
                throw new Exception("Length must be 1 or greater");
            Length = length;
        }

        /// <summary>
        /// length (number of rooms) of the sequence
        /// </summary>
        public RangeI Length { get; }
    }

    /// <summary>
    /// An abstract dungeon element representing a line of rooms
    /// </summary>
    public class NodeElement : AbstractDungeonElement
    {
        public NodeElement(string style, NodeMetaData elementMetaData) : base(style, elementMetaData)
        {
        }

        public NodeElement(string style, NodeMetaData elementMetaData, params AbstractDungeonElement[] subElements) : base(style, elementMetaData, subElements)
        {
        }

        /// <summary>
        /// True if this is a leaf in the tree
        /// </summary>
        public bool IsEndNode => _subElements.Count == 0;
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

        public NestedDungeon(string path, NodeMetaData elementMetaData)
        {
            Path = path;
            ElementMetaData = elementMetaData;
        }

        public NestedDungeon(string path, NodeMetaData elementMetaData, params AbstractDungeonElement[] subElements)
        {
            Path = path;
            _subElements = new List<AbstractDungeonElement>(subElements);
            ElementMetaData = elementMetaData;
        }
    }
}