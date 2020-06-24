using System;
using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace TaurusDungeonGenerator.Example.Scripts
{
    public class DungeonDemoRoot : MonoBehaviour
    {
        public int seed = 1755192844;

        public Text seedText;
        public Dropdown structureDropdown;
        public Slider branchSlider;

        private DungeonStructure _actualStructure;

        public event Action<DungeonStructure> OnDungeonRebuilt;

        private readonly Dictionary<string, AbstractDungeonStructure> _structures = new Dictionary<string, AbstractDungeonStructure>
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

        void Start()
        {
            ReBuildDungeonFromSeed();
            structureDropdown.ClearOptions();
            structureDropdown.AddOptions(_structures.Keys.ToList());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ReBuildDungeonRandom();
            }
        }

        public void ReBuildDungeonRandom()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            seed = Random.Range(int.MinValue, int.MaxValue);
            BuildDungeon(GetSelectedStructure(), seed, GetBranchPercent());
        }


        public void ReBuildDungeonFromSeed()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            // ReSharper disable once Unity.NoNullPropagation
            BuildDungeon(GetSelectedStructure(), seedText?.text?.GetHashCode() ?? 0, GetBranchPercent());
        }

        private AbstractDungeonStructure GetSelectedStructure()
        {
            var key = _structures.Keys.ToList()[structureDropdown.value];
            return _structures[key];
        }

        private float GetBranchPercent()
        {
            // ReSharper disable once Unity.NoNullPropagation
            return branchSlider?.value ?? 50;
        }

        private void BuildDungeon(AbstractDungeonStructure structure, int seed, float branchPercent)
        {
            structure.BranchDataWrapper = new BranchDataWrapper(_branches.Keys.ToList(), branchPercent);
            structure.EmbeddedDungeons = _branches;

            var generator = new PrototypeDungeonGenerator(structure, seed);
            var prototypeDungeon = generator.BuildPrototype();
            _actualStructure = prototypeDungeon.BuildDungeonInUnitySpace(transform);
            OnDungeonRebuilt?.Invoke(_actualStructure);
        }
    }
}