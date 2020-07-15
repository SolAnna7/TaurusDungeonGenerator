using System;
using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.PiscesConfigLoader;
using SnowFlakeGamesAssets.PiscesConfigLoader.Component;
using SnowFlakeGamesAssets.PiscesConfigLoader.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.ConfigLoader;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
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
        public Slider marginSlider;
        public Text marginText;

        private DungeonStructure _actualStructure;

        public event Action<DungeonStructure> OnDungeonRebuilt;

        void Start()
        {
            structureDropdown.ClearOptions();
            _dungeonStructures = LoadStructure();
            structureDropdown.AddOptions(_dungeonStructures.Keys.ToList());
            ReBuildDungeonFromSeed();
        }

        private Dictionary<string, AbstractDungeonStructure> LoadStructure()
        {
#if SFG_PISCES_CONFIG
            Dictionary<string, AbstractDungeonStructure> result = new Dictionary<string, AbstractDungeonStructure>();
            gameObject.AddComponent<ConfigReaderComponent>();
            GameConfig.InitConfig();

            GameConfig.Query("dungeons").AsNode().GetKeys()
                .Select(key => (key, DungeonStructureConfigLoader.BuildFromConfig(new ConfigPath("dungeons",key))))
                .Where(x => !x.Item2.StructureMetaData.HasTag("NESTED_ONLY"))
                .ForEach(k => result.Add(k.key, k.Item2));
            return result;
#else
            CreateInlineDungeonStructures();
#endif
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
            BuildDungeon(GetSelectedStructure(), seedText?.text?.GetHashCode() ?? 0, GetBranchPercent(), GetMargin());
        }

        private float GetMargin()
        {
            // ReSharper disable once Unity.NoNullPropagation
            var marginSliderValue = marginSlider?.value ?? 0;
            marginText.text = $"{marginSliderValue} Unit";
            return marginSliderValue;
        }

        private AbstractDungeonStructure GetSelectedStructure()
        {
            var key = _dungeonStructures.Keys.ToList()[structureDropdown.value];
            return _dungeonStructures[key];
        }

        private float GetBranchPercent()
        {
            // ReSharper disable once Unity.NoNullPropagation
            return branchSlider?.value ?? 50;
        }

        private void BuildDungeon(AbstractDungeonStructure structure, int generationSeed, float branchPercent, float margin)
        {
            if (structure.BranchDataWrapper != null)
                structure.BranchDataWrapper.BranchPercentage = branchPercent;

            structure.StructureMetaData.MarginUnit = margin;

            var generator = new PrototypeDungeonGenerator(structure, generationSeed);
            var prototypeDungeon = generator.BuildPrototype();
            _actualStructure = prototypeDungeon.BuildDungeonInUnitySpace(transform);
            OnDungeonRebuilt?.Invoke(_actualStructure);
        }
    }
}