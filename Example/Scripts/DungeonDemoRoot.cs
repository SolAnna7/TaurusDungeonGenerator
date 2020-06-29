using System;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace TaurusDungeonGenerator.Example.Scripts
{
    public partial class DungeonDemoRoot : MonoBehaviour
    {
        public int seed = 1755192844;

        public InputField seedText;
        public Dropdown structureDropdown;
        public Slider branchSlider;

        private DungeonStructure _actualStructure;

        public event Action<DungeonStructure> OnDungeonRebuilt;

        void Start()
        {
            ReBuildDungeonFromSeed();
            structureDropdown.ClearOptions();
            structureDropdown.AddOptions(_inlineDungeonStructures.Keys.ToList());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ReBuildDungeonRandom();
            }
        }

        private void ReBuildDungeonRandom()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            seed = Random.Range(int.MinValue, int.MaxValue);
            //the value change invokes the regeneration
            seedText.text = seed.ToString();
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
            var key = _inlineDungeonStructures.Keys.ToList()[structureDropdown.value];
            return _inlineDungeonStructures[key];
        }

        private float GetBranchPercent()
        {
            // ReSharper disable once Unity.NoNullPropagation
            return branchSlider?.value ?? 50;
        }

        private void BuildDungeon(AbstractDungeonStructure structure, int generationSeed, float branchPercent)
        {
            structure.BranchDataWrapper = new BranchDataWrapper(_branches.Keys.ToList(), branchPercent);
            structure.EmbeddedDungeons = _branches;

            var generator = new PrototypeDungeonGenerator(structure, generationSeed);
            var prototypeDungeon = generator.BuildPrototype();
            _actualStructure = prototypeDungeon.BuildDungeonInUnitySpace(transform);
            OnDungeonRebuilt?.Invoke(_actualStructure);
        }
    }
}