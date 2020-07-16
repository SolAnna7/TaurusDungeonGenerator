using NUnit.Framework;
using SnowFlakeGamesAssets.TaurusDungeonGenerator;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

// ReSharper disable once CheckNamespace
namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Tests
{
    public class DungeonValidityTest
    {
        [Test]
        public void TestSpecificDungeonGeneration()
        {
            int seed = 1755192844;

            var structure = new AbstractDungeonStructure(
                new NodeElement("DungeonGenerationTest/CorrX", NodeMetaData.Empty(),
                    new ConnectionElement("DungeonGenerationTest/Corridors", NodeMetaData.Empty(), new RangeI(5, 10),
                        new NodeElement("DungeonGenerationTest/EndRoom", NodeMetaData.Empty())),
                    new ConnectionElement("DungeonGenerationTest/Corridors", NodeMetaData.Empty(), new RangeI(5),
                        new NodeElement("DungeonGenerationTest/EndRoom", NodeMetaData.Empty())),
                    new ConnectionElement("DungeonGenerationTest/Corridors", NodeMetaData.Empty(), new RangeI(5, 10),
                        new NodeElement("DungeonGenerationTest/EndRoom", NodeMetaData.Empty())),
                    new ConnectionElement("DungeonGenerationTest/Corridors", NodeMetaData.Empty(), new RangeI(3, 4),
                        new NodeElement("DungeonGenerationTest/EndRoom", NodeMetaData.Empty()))
                ), StructureMetaData.Empty());
            structure.ValidateStructure();
            var generator = new PrototypeDungeonGenerator(structure, seed);
            generator.BuildPrototype();
        }
    }
}