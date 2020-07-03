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
        
        public StructureMetaData StructureMetaData { get; }

        public AbstractDungeonStructure(AbstractDungeonElement startElement, StructureMetaData structureMetaData)
        {
            StartElement = startElement;
            StructureMetaData = structureMetaData;
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


    public abstract class AbstractDungeonElement
    {
        public string Style { get; }

        public List<AbstractDungeonElement> SubElements => subElements;

        protected List<AbstractDungeonElement> subElements = new List<AbstractDungeonElement>();

        public NodeMetaData ElementMetaData { get; set; }

        public bool IsOptional { get; set; } = false;
        [Obsolete] public bool IsTansit { get; set; } = false;

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
            this.subElements = new List<AbstractDungeonElement>(subElements);
        }

        public void AddSubElement(AbstractDungeonElement newSub) => subElements.Add(newSub);
    }


    public class ConnectionElement : AbstractDungeonElement
    {
        public ConnectionElement(string style, NodeMetaData elementMetaData, RangeI length, params AbstractDungeonElement[] subElements) : base(style, elementMetaData, subElements)
        {
            if (length.Min <= 0)
                throw new Exception("Length must be 1 or greater");
            Length = length;
        }

        public RangeI Length { get; private set; }
    }

    public class NodeElement : AbstractDungeonElement
    {
        public NodeElement(string style, NodeMetaData elementMetaData) : base(style, elementMetaData)
        {
        }

        public NodeElement(string style, NodeMetaData elementMetaData, params AbstractDungeonElement[] subElements) : base(style, elementMetaData, subElements)
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

        public NestedDungeon(string path, NodeMetaData elementMetaData)
        {
            Path = path;
            ElementMetaData = elementMetaData;
        }

        public NestedDungeon(string path, NodeMetaData elementMetaData, params AbstractDungeonElement[] subElements)
        {
            Path = path;
            this.subElements = new List<AbstractDungeonElement>(subElements);
            ElementMetaData = elementMetaData;
        }
    }
}