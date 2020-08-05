using System.Collections.Generic;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
using static SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure.DungeonElementBuilder;

namespace TaurusDungeonGenerator.Example.Scripts
{
    public partial class DungeonDemoRoot
    {
        private Dictionary<string, AbstractDungeonStructure> _dungeonStructures = new Dictionary<string, AbstractDungeonStructure>();

        private static Dictionary<string, AbstractDungeonStructure> CreateInlineDungeonStructures()
        {
            var structures = new Dictionary<string, AbstractDungeonStructure>
            {
                {
                    "realistic-dungeon-layout-1",
                    AbstractDungeonStructure.Builder
                        .SetEmbeddedDungeons(new Dictionary<string, AbstractDungeonStructure>
                        {
                            {
                                //branch type 1 definition
                                "inline-branch-1",
                                AbstractDungeonStructure.Builder.SetStartElement(
                                    ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(4, 7))
                                        .AddSubElement(
                                            NodeElement("DungeonGenerationTest/MiddleRoom")
                                        ).Build()).Build()
                            },
                            {
                                //branch type 2 definition
                                "inline-branch-2",
                                AbstractDungeonStructure.Builder.SetStartElement(
                                    ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(2, 5))
                                        .AddSubElement(
                                            NodeElement("DungeonGenerationTest/CorrX")
                                        ).Build()).Build()
                            }
                        })
                        .SetBranchData(new BranchDataWrapper(
                            // the types of dungeons used as branches
                            new List<string> {"inline-branch-1", "inline-branch-2"},
                            // maximum percentage of empty connections where branches will be built
                            50f))
                        .SetMetaData(StructureMetaData.Builder
                            // meta data objects for the structure
                            .AddStructureProperty("name", "Realistic dungeon layout")
                            .AddStructureProperty("description", "A realistic layout with one miniboss room, one boss room and one to three exits.")
                            // tags for the structure
                            .AddStructureTag("structure-tag-1")
                            .AddStructureTag("structure-tag-2")
                            // tags for every element
                            .AddGlobalTag("global-node-tag-1")
                            .Build())
                        // the actual structure of the dungeon graph
                        .SetStartElement(
                            // a single room chosen from the DungeonGenerationTest/EndRoom RoomCollection
                            NodeElement("DungeonGenerationTest/EndRoom")
                                // tags for this node
                                .SetMetaData(NodeMetaData.Builder.AddTag("entrance").Build())
                                .AddSubElement(
                                    // a sequence of connected rooms chosen from the DungeonGenerationTest/Corridors RoomCollection
                                    // the length of the sequence is between 5 and 10 rooms randomly chosen at generation
                                    ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5, 10))
                                        .AddSubElement(
                                            NodeElement("DungeonGenerationTest/MiddleRoom")
                                                .SetMetaData(NodeMetaData.Builder.AddTag("small-boss-room").Build())
                                                .AddSubElement(
                                                    ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5, 10))
                                                        .AddSubElement(
                                                            NodeElement("DungeonGenerationTest/CorridorsNormalBig")
                                                                .AddSubElement(
                                                                    ConnectionElement("DungeonGenerationTest/CorridorsBig", new RangeI(3))
                                                                        .AddSubElement(
                                                                            NodeElement("DungeonGenerationTest/BigRoom")
                                                                                .SetMetaData(NodeMetaData.Builder.AddTag("big-boss-room").Build())
                                                                                .AddSubElement(
                                                                                    NodeElement("DungeonGenerationTest/CorridorsNormalBig")
                                                                                        .AddSubElement(
                                                                                            ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5, 10))
                                                                                                .AddSubElement(
                                                                                                    NodeElement("DungeonGenerationTest/MiddleRoom")
                                                                                                        .AddSubElement(
                                                                                                            ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5, 10))
                                                                                                                .AddSubElement(NodeElement("DungeonGenerationTest/EndRoom")
                                                                                                                    .SetMetaData(NodeMetaData.Builder.AddTag("exit-1-static").Build())
                                                                                                                    .Build())
                                                                                                                .Build())
                                                                                                        .AddSubElement(
                                                                                                            ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5, 10))
                                                                                                                // this part of the tree is optional
                                                                                                                .SetMetaData(NodeMetaData.Builder.SetOptionalNode().Build())
                                                                                                                .AddSubElement(NodeElement("DungeonGenerationTest/EndRoom")
                                                                                                                    .SetMetaData(NodeMetaData.Builder
                                                                                                                        .AddTag("exit-2-optional")
                                                                                                                        // end of an optional tree
                                                                                                                        .SetOptionalEndpoint()
                                                                                                                        .Build())
                                                                                                                    .Build())
                                                                                                                .Build())
                                                                                                        .AddSubElement(
                                                                                                            ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5, 10))
                                                                                                                .SetMetaData(NodeMetaData.Builder.SetOptionalNode().Build())
                                                                                                                .AddSubElement(NodeElement("DungeonGenerationTest/EndRoom")
                                                                                                                    .SetMetaData(NodeMetaData.Builder
                                                                                                                        .AddTag("exit-3-optional")
                                                                                                                        .SetOptionalEndpoint()
                                                                                                                        .Build())
                                                                                                                )))))))))))
                                .Build())
                        .Build()
                }
            };

            return structures;
        }
    }
}