using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator
{
    /// <summary>
    /// Internal generation class used to create a DungeonStructure from an AbstractDungeonStructure
    /// </summary>
    internal static class DungeonStructureConcretizer
    {
        // ReSharper disable once InconsistentNaming
        public static string NESTED_TAG => "#NESTED";

        internal static DungeonStructure ConcretizeStructure(AbstractDungeonStructure inputStructure, Random random)
        {
            var structure = new DungeonStructure(ConcretizeDungeonTree(inputStructure.StartElement, random,
                new ReadOnlyDictionary<string, AbstractDungeonStructure>(inputStructure.EmbeddedDungeons)),
                inputStructure.StructureMetaData,
                inputStructure);
            structure.NodeMetaData.BranchDataWrapper = inputStructure.BranchDataWrapper;
            return structure;
        }


        internal static DungeonNode ConcretizeDungeonTree(AbstractDungeonElement inputElement, Random random, ReadOnlyDictionary<string, AbstractDungeonStructure> embeddedDungeons)
        {
            {
                if (inputElement is NodeElement node)
                {
                    DungeonNode copyNode;
                    if (node.IsEndNode)
                        copyNode = new DungeonNode(node.Style, (NodeMetaData) node.ElementMetaData.Clone());
                    else
                    {
                        List<DungeonNode> subElements = node.SubElements.Select(element => ConcretizeDungeonTree(element, random, embeddedDungeons)).ToList();
                        copyNode = new DungeonNode(node.Style, (NodeMetaData) node.ElementMetaData.Clone(), subElements);
                    }
                    
                    return copyNode;
                }
            }
            {
                if (inputElement is ConnectionElement connection)
                {
                    var connectionLength = connection.Length.RandomNumberInRange(random);
                    DungeonNode replacementNode = null;
                    if (connection.SubElements.Count == 1)
                    {
                        DungeonNode nextElement = ConcretizeDungeonTree(connection.SubElements.Single(), random, embeddedDungeons);
                        replacementNode = nextElement;
                    }

                    for (int i = 0; i < connectionLength; i++)
                    {
                        replacementNode = replacementNode != null
                            ? new DungeonNode(connection.Style, (NodeMetaData) connection.ElementMetaData.Clone(), new List<DungeonNode> {replacementNode})
                            : new DungeonNode(connection.Style, (NodeMetaData) connection.ElementMetaData.Clone());
                    }

                    if (replacementNode == null)
                        throw new Exception("This should not happen");

                    return replacementNode;
                }
            }

            {
                if (inputElement is NestedDungeon nested)
                {
                    DungeonNode[] abstractSubElements = nested.SubElements.Select(element => ConcretizeDungeonTree(element, random, embeddedDungeons)).ToArray();
                    DungeonNode nestedStartNode = ConcretizeStructure(embeddedDungeons[nested.Path], random).StartElement;

                    nestedStartNode.TraverseDepthFirst().ForEach(x=>x.MetaData.AddTag(NESTED_TAG));
                    
                    if (abstractSubElements.Length > 0)
                    {
                        List<DungeonNode> endNodeCollector = new List<DungeonNode>();
                        CollectEndNodes(nestedStartNode, endNodeCollector);

                        endNodeCollector.Shuffle(random);

                        for (int i = 0; i < abstractSubElements.Length; i++)
                        {
                            endNodeCollector[i].AddSubElement(abstractSubElements[i]);
                        }
                    }

                    return nestedStartNode;
                }
            }

            throw
                new Exception($"Unknown AbstractDungeonElement type: {inputElement.GetType()}");
        }

        private static void CollectEndNodes(DungeonNode element, List<DungeonNode> endNodeCollector)
        {
            if (element.IsEndNode)
            {
                endNodeCollector.Add(element);
            }

            foreach (var subElement in element.SubElements)
            {
                CollectEndNodes(subElement, endNodeCollector);
            }
        }
    }
}