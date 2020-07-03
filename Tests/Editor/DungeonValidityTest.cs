using NUnit.Framework;
using SnowFlakeGamesAssets.TaurusDungeonGenerator;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

// ReSharper disable once CheckNamespace
namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Tests
{
    public class DungeonValidityTest
    {
        public int DungeonGenerationCount = 10;

        // [Test]
        // public void TestMassiveDungeonGeneration()
        // {
        //     Debug.Log("TestDungeonGeneration started");
        //
        //     GameConfig.Clear();
        //     GameConfig.RegisterReader(TestConfigReader.FromPath("Config"));
        //     GameConfig.InitConfig();
        //
        //     var dungeons = GameConfig.Query("dungeons").AsNode().GetKeys().ToList();
        //
        //     Debug.Log($"Dungeons found:\n{string.Join("\n", dungeons)}");
        //
        //     foreach (var dungeon in dungeons)
        //     {
        //         Debug.Log($"- Loading dungeon {dungeon}");
        //         var structure = DungeonStructureConfigManager.BuildFromConfig(new ConfigPath(dungeon));
        //
        //         Debug.Log($"Generating dungeon {dungeon} {DungeonGenerationCount} times");
        //
        //         System.Random r = new System.Random(0);
        //         for (int i = 0; i < DungeonGenerationCount; i++)
        //         {
        //             int seed = r.Next();
        //
        //             Debug.Log($"- Generating dungeon {dungeon} with seed {seed}");
        //
        //             Stopwatch sw = new Stopwatch();
        //             sw.Start();
        //
        //             var generator = new PrototypeDungeonGenerator(structure, seed);
        //             generator.BuildPrototype();
        //
        //             sw.Stop();
        //             Debug.Log($"- Generating dungeon {dungeon} with seed {seed} finished. Took {sw.ElapsedMilliseconds}ms");
        //         }
        //     }
        // }

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