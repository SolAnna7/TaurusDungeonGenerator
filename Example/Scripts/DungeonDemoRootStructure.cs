using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace TaurusDungeonGenerator.Example.Scripts
{
    public partial class DungeonDemoRoot
    {
        private Dictionary<string, AbstractDungeonStructure> _dungeonStructures = new Dictionary<string, AbstractDungeonStructure>();

        private static Dictionary<string, AbstractDungeonStructure> CreateInlineDungeonStructures()
        {
            var branches = new Dictionary<string, AbstractDungeonStructure>
            {
                {
                    "b1",
                    new AbstractDungeonStructure(
                        new ConnectionElement("DungeonGenerationTest/Corridors", NodeMetaData.Empty, new RangeI(1, 5),
                            new NodeElement("DungeonGenerationTest/EndRoom", NodeMetaData.Empty)), StructureMetaData.Empty)
                },
                {
                    "b2",
                    new AbstractDungeonStructure(
                        new ConnectionElement("DungeonGenerationTest/Corridors", NodeMetaData.Empty, new RangeI(3, 7),
                            new NodeElement("DungeonGenerationTest/EndRoom", NodeMetaData.Empty)), StructureMetaData.Empty)
                }
            };

            var structures = new Dictionary<string, AbstractDungeonStructure>
            {
                {
                    "Cross",
                    new AbstractDungeonStructure(
                        new NodeElement("DungeonGenerationTest/CorrX", NodeMetaData.Empty,
                            new ConnectionElement("DungeonGenerationTest/Corridors", NodeMetaData.Empty, new RangeI(5, 10),
                                new NodeElement("DungeonGenerationTest/EndRoom", NodeMetaData.Empty)),
                            new ConnectionElement("DungeonGenerationTest/Corridors", NodeMetaData.Empty, new RangeI(5),
                                new NodeElement("DungeonGenerationTest/EndRoom", NodeMetaData.Empty)),
                            new ConnectionElement("DungeonGenerationTest/Corridors", NodeMetaData.Empty, new RangeI(5, 10),
                                new NodeElement("DungeonGenerationTest/EndRoom", NodeMetaData.Empty)),
                            new ConnectionElement("DungeonGenerationTest/Corridors", NodeMetaData.Empty, new RangeI(3, 4),
                                new NodeElement("DungeonGenerationTest/EndRoom", NodeMetaData.Empty))
                        ), StructureMetaData.Empty)
                },
                {
                    "002",
                    new AbstractDungeonStructure(
                        new NodeElement("DungeonGenerationTest/CorrX", NodeMetaData.Empty,
                            new ConnectionElement("DungeonGenerationTest/Corridors", NodeMetaData.Empty, new RangeI(10, 15),
                                new NodeElement("DungeonGenerationTest/EndRoom", NodeMetaData.Empty)),
                            new ConnectionElement("DungeonGenerationTest/Corridors", NodeMetaData.Empty, new RangeI(3, 4),
                                new NodeElement("DungeonGenerationTest/EndRoom", NodeMetaData.Empty))
                        ), StructureMetaData.Empty)
                }
            };

            structures.ForEach(x =>
            {
                x.Value.EmbeddedDungeons = branches;
                x.Value.BranchDataWrapper = new BranchDataWrapper(branches.Keys.ToList(), 0);
            });

            return structures;
        }
    }
}