using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator
{
    public class DungeonStructureConcretizer
    {
        public static DungeonStructure ConcretizeStructure(AbstractDungeonStructure inputStructure, Random random)
        {
            var structure = new DungeonStructure(ConcretizeDungeonTree(inputStructure.StartElement, random,
                new ReadOnlyDictionary<string, AbstractDungeonStructure>(inputStructure.EmbeddedDungeons)), inputStructure);
            structure.StructureMetaData.BranchDataWrapper = inputStructure.BranchDataWrapper;
            return structure;
        }


        public static DungeonNode ConcretizeDungeonTree(AbstractDungeonElement inputElement, Random random, ReadOnlyDictionary<string, AbstractDungeonStructure> embeddedDungeons)
        {
            {
                if (inputElement is NodeElement node)
                {
                    DungeonNode copyNode;
                    if (node.IsEndNode)
                        copyNode = new DungeonNode(node.Style, node.Tags);
                    else
                    {
                        List<DungeonNode> subElements = node.SubElements.Select(element => ConcretizeDungeonTree(element, random, embeddedDungeons)).ToList();
                        copyNode = new DungeonNode(node.Style, node.Tags, subElements);
                    }

                    if (node.IsOptional)
                        copyNode.StructureMetaData.OptionalNodeData = new OptionalNodeData();

                    if (node.IsTansit)
                        copyNode.StructureMetaData.IsTransit = true;

                    return copyNode;
                }
            }
            {
                if (inputElement is ConnectionElement connection)
                {
                    var connectionLength = connection.Length.GetRandom(random);
                    DungeonNode replacementNode = null;
                    if (connection.SubElements.Count == 1)
                    {
                        DungeonNode nextElement = ConcretizeDungeonTree(connection.SubElements.Single(), random, embeddedDungeons);
                        replacementNode = nextElement;
                    }

                    for (int i = 0; i < connectionLength; i++)
                    {
                        replacementNode = replacementNode != null
                            ? new DungeonNode(connection.Style, connection.Tags, new List<DungeonNode> {replacementNode})
                            : new DungeonNode(connection.Style, connection.Tags);
                    }

                    if (replacementNode == null)
                        throw new Exception("This should not happen");

                    if (connection.IsOptional)
                        replacementNode.StructureMetaData.OptionalNodeData = new OptionalNodeData();

                    if (connection.IsTansit)
                        replacementNode.StructureMetaData.IsTransit = true;

                    return replacementNode;
                }
            }

            {
                if (inputElement is NestedDungeon nested)
                {
                    DungeonNode[] abstractSubElements = nested.SubElements.Select(element => ConcretizeDungeonTree(element, random, embeddedDungeons)).ToArray();
                    DungeonNode nestedStartNode = ConcretizeStructure(embeddedDungeons[nested.Path], random).StartElement;

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

                    if (nested.IsOptional)
                        nestedStartNode.StructureMetaData.OptionalNodeData = new OptionalNodeData();

                    if (nested.IsTansit)
                        nestedStartNode.StructureMetaData.IsTransit = true;

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