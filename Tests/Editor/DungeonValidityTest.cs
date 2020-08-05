using System.Linq;
using NUnit.Framework;
using SnowFlakeGamesAssets.TaurusDungeonGenerator;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
using static SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure.DungeonElementBuilder;

// ReSharper disable once CheckNamespace
namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Tests
{
    public class DungeonValidityTest
    {
        [Test]
        public void TestSpecificDungeonGeneration()
        {
            int seed = 1755192844;
            var structure = AbstractDungeonStructure.Builder.SetStartElement(
                NodeElement("DungeonGenerationTest/CorrX").AddSubElement(
                    ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5, 10)).AddSubElement(
                        NodeElement("DungeonGenerationTest/EndRoom").Build()
                    ).Build()
                ).AddSubElement(
                    ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5)).AddSubElement(
                        NodeElement("DungeonGenerationTest/EndRoom").Build()
                    ).Build()
                ).AddSubElement(
                    ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5, 10)).AddSubElement(
                        NodeElement("DungeonGenerationTest/EndRoom").Build()
                    ).Build()
                ).AddSubElement(
                    ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(3, 4)).AddSubElement(
                        NodeElement("DungeonGenerationTest/EndRoom").Build()
                    ).Build()
                ).Build()).Build();
            structure.ValidateStructure();
            var generator = new PrototypeDungeonGenerator(structure, seed);
            generator.BuildPrototype();
        }

        [Test]
        public void TestOptionalPathGeneration3Path() => TestOptionalPathGeneration(3);

        [Test]
        public void TestOptionalPathGeneration2Path() => TestOptionalPathGeneration(2);

        [Test]
        public void TestOptionalPathGeneration1Path() => TestOptionalPathGeneration(1);

        [Test]
        public void TestOptionalPathGeneration0Path() => TestOptionalPathGeneration(0);

        //todo: specific error type
        [Test]
        public void TestOptionalPathGeneration4PathError() => Assert.Throws<System.Exception>(delegate { TestOptionalPathGeneration(4); });

        private static void TestOptionalPathGeneration(uint optionalPathNumber)
        {
            //GIVEN
            int seed = 123;
            var structure = AbstractDungeonStructure.Builder.SetStartElement(
                NodeElement("DungeonGenerationTest/CorrX").AddSubElement(
                    ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5, 10))
                        .SetMetaData(NodeMetaData.Builder.SetOptionalNode().Build())
                        .AddSubElement(
                            NodeElement("DungeonGenerationTest/EndRoom")
                                .SetMetaData(NodeMetaData.Builder.SetOptionalEndpoint().Build())
                                .Build()
                        ).Build()
                ).AddSubElement(
                    ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5))
                        .SetMetaData(NodeMetaData.Builder.SetOptionalNode().Build())
                        .AddSubElement(
                            NodeElement("DungeonGenerationTest/EndRoom")
                                .SetMetaData(NodeMetaData.Builder.SetOptionalEndpoint().Build())
                                .Build()
                        ).Build()
                ).AddSubElement(
                    ConnectionElement("DungeonGenerationTest/Corridors", new RangeI(5, 10))
                        .SetMetaData(NodeMetaData.Builder.SetOptionalNode().Build())
                        .AddSubElement(
                            NodeElement("DungeonGenerationTest/EndRoom")
                                .SetMetaData(NodeMetaData.Builder.SetOptionalEndpoint().Build())
                                .Build()
                        ).Build()
                ).Build()
            ).Build();
            structure.ValidateStructure();

            //WHEN
            var generator = new PrototypeDungeonGenerator(structure, seed, new PrototypeDungeonGenerator.GenerationParameters {RequiredOptionalEndpointNumber = optionalPathNumber});
            var prototypeDungeon = generator.BuildPrototype();

            //THEN
            var endpointCount = prototypeDungeon.Structure.StartElement.TraverseDepthFirst().Count(n => n.MetaData.OptionalEndpoint);
            Assert.AreEqual(optionalPathNumber, endpointCount);
        }
    }
}