using System.Collections.Generic;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace TaurusDungeonGenerator.Example.Scripts
{
    public partial class DungeonDemoRoot
    {
        private readonly Dictionary<string, AbstractDungeonStructure> _inlineDungeonStructures = new Dictionary<string, AbstractDungeonStructure>
        {
            {
                "Cross",
                new AbstractDungeonStructure(
                    new NodeElement("DungeonGenerationTest/CorrX",
                        new ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5, 10),
                            new NodeElement("DungeonGenerationTest/EndRoom")),
                        new ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5),
                            new NodeElement("DungeonGenerationTest/EndRoom")),
                        new ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5, 10),
                            new NodeElement("DungeonGenerationTest/EndRoom")),
                        new ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(3, 4),
                            new NodeElement("DungeonGenerationTest/EndRoom"))
                    ))
            },
            {
                "002",
                new AbstractDungeonStructure(
                    new NodeElement("DungeonGenerationTest/CorrX",
                        new ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(10, 15),
                            new NodeElement("DungeonGenerationTest/EndRoom")),
                        new ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(3, 4),
                            new NodeElement("DungeonGenerationTest/EndRoom"))
                    ))
            }
        };
        
        
        private readonly Dictionary<string, AbstractDungeonStructure> _branches = new Dictionary<string, AbstractDungeonStructure>
        {
            {
                "b1",
                new AbstractDungeonStructure(
                    new ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(1, 5),
                        new NodeElement("DungeonGenerationTest/EndRoom")))
            },
            {
                "b2",
                new AbstractDungeonStructure(
                    new ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(3, 7),
                        new NodeElement("DungeonGenerationTest/EndRoom")))
            }
        };

    }
}