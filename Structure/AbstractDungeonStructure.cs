using System;
using System.Collections.Generic;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    public class AbstractDungeonStructure : IBranchDataHolder
    {
        public AbstractDungeonElement StartElement { get; private set; }

        public Dictionary<string, AbstractDungeonStructure> EmbeddedDungeons { get; private set; }

        public BranchDataWrapper BranchDataWrapper { get; set; }

        public AbstractDungeonStructure(AbstractDungeonElement startElement)
        {
            StartElement = startElement;
        }

        public AbstractDungeonStructure(AbstractDungeonElement startElement, Dictionary<string, AbstractDungeonStructure> embeddedDungeons, BranchDataWrapper branchDataWrapper)
        {
            StartElement = startElement;
            EmbeddedDungeons = embeddedDungeons;
            BranchDataWrapper = branchDataWrapper;
        }
    }

    public interface IBranchDataHolder
    {
        BranchDataWrapper BranchDataWrapper { get; set; }
    }

    public class BranchDataWrapper
    {
        public List<String> BranchPrototypeNames { get; }
        public int? BranchCount { get; }
        public float? BranchPercentage { get; }

        public BranchDataWrapper(List<string> branchPrototypeNames, int branchCount)
        {
            BranchPrototypeNames = branchPrototypeNames;
            BranchCount = branchCount;
        }        
        public BranchDataWrapper(List<string> branchPrototypeNames, float branchPercentage)
        {
            BranchPrototypeNames = branchPrototypeNames;
            BranchPercentage = branchPercentage;
        }
    }

    public abstract class AbstractDungeonElement
    {
        public List<AbstractDungeonElement> SubElements
        {
            get { return subElements; }
        }

        protected List<AbstractDungeonElement> subElements = new List<AbstractDungeonElement>();

        public List<Tag> Tags { get; } = new List<Tag>();

        public bool IsOptional { get; set; } = false;
        public bool IsTansit { get; set; } = false;

        public void AddTags(IEnumerable<Tag> tags) => Tags.AddRange(tags);

        protected AbstractDungeonElement()
        {
        }

        public void AddSubElement(AbstractDungeonElement newSub)
        {
            subElements.Add(newSub);
        }
    }

    public abstract class AbstractStyledDungeonElement : AbstractDungeonElement
    {
        public string Style { get; protected set; }

        protected AbstractStyledDungeonElement(string style)
        {
            Style = style;
        }

        protected AbstractStyledDungeonElement(string style, params AbstractDungeonElement[] subElements) : this(style)
        {
            this.subElements = new List<AbstractDungeonElement>(subElements);
        }
    }

    public class ConnectionElement : AbstractStyledDungeonElement
    {
        public ConnectionElement(string style, RangeI length, params AbstractDungeonElement[] subElements) : base(style, subElements)
        {
            if (length.Min <= 0)
                throw new Exception("Length must be 1 or greater");
            Length = length;
        }

        public RangeI Length { get; private set; }
    }

    public class NodeElement : AbstractStyledDungeonElement
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