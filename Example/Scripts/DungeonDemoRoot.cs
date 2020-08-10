using System;
using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
#if SFG_PISCES_CONFIG
using SnowFlakeGamesAssets.PiscesConfigLoader;
using SnowFlakeGamesAssets.PiscesConfigLoader.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.ConfigLoader;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
#endif

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
        public Slider optionalSlider;
        public Text optionalText;
        public Text descriptionText;

        private DungeonStructure _actualStructure;

        public event Action<DungeonStructure> OnDungeonRebuilt;

        void Start()
        {
            structureDropdown.ClearOptions();
            _dungeonStructures = LoadStructure();
            structureDropdown.AddOptions(_dungeonStructures.Select(x =>
                    x.Value.StructureMetaData.StructurePropertyAndTagHolder.TryGetPropertyAs("name", out string structureName) ? structureName : x.Key).ToList()
            );
            ReBuildDungeonFromSeed();
        }

        private Dictionary<string, AbstractDungeonStructure> LoadStructure()
        {
#if SFG_PISCES_CONFIG
            Dictionary<string, AbstractDungeonStructure> result = new Dictionary<string, AbstractDungeonStructure>();
            var configRoot = new ConfigBuilder().ParseTextResourceFiles("ExampleConfig", new ConfigBuilder.YamlTextConfigParser()).Build();

            DungeonStructureConfigLoader.RegisterPropertyLoader("description", queryResult => queryResult.AsString());
            DungeonStructureConfigLoader.RegisterPropertyLoader("name", queryResult => queryResult.AsString());

            configRoot.Query("example-dungeons").AsNode().GetKeys()
                .Select(key => (key, DungeonStructureConfigLoader.BuildFromConfig(new ConfigPath("example-dungeons", key), configRoot)))
                .Where(x => !x.Item2.StructureMetaData.HasTag("NESTED_ONLY"))
                .ForEach(k => result.Add(k.key, k.Item2));
            return result;
#else
            return CreateInlineDungeonStructures();
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
            var selectedStructure = GetSelectedStructure();

            branchSlider.interactable = selectedStructure.HasBranches();

            var structureMetaData = selectedStructure.StructureMetaData;
            descriptionText.text = structureMetaData.TryGetPropertyAs("description", out string desc) ? desc : "";
            if (optionalSlider.minValue != structureMetaData.MinOptionalEndpointNum || optionalSlider.maxValue != structureMetaData.MaxOptionalEndpointNum)
            {
                var tmp = optionalSlider.onValueChanged;
                optionalSlider.onValueChanged = new Slider.SliderEvent();
                optionalSlider.minValue = structureMetaData.MinOptionalEndpointNum;
                optionalSlider.maxValue = structureMetaData.MaxOptionalEndpointNum;
                optionalSlider.value = optionalSlider.maxValue;
                optionalSlider.onValueChanged = tmp;

                optionalSlider.interactable = !(Math.Abs(optionalSlider.maxValue - optionalSlider.minValue) < 0.01f);
            }

            optionalText.text = $"{optionalSlider.minValue}/{optionalSlider.value}/{optionalSlider.maxValue}";

            BuildDungeon(selectedStructure, seedText?.text?.GetHashCode() ?? 0, GetBranchPercent(), GetMargin(),
                new PrototypeDungeonGenerator.GenerationParameters
                {
                    RequiredOptionalEndpointNumber = (uint) optionalSlider.value
                });
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

        private void BuildDungeon(
            AbstractDungeonStructure structure,
            int generationSeed,
            float branchPercent,
            float margin,
            PrototypeDungeonGenerator.GenerationParameters parameters)
        {
            if (structure.BranchDataWrapper != null)
                structure.BranchDataWrapper.BranchPercentage = branchPercent;

            structure.StructureMetaData.MarginUnit = margin;

            var generator = new PrototypeDungeonGenerator(structure, generationSeed, parameters);
            var prototypeDungeon = generator.BuildPrototype();
            _actualStructure = prototypeDungeon.BuildDungeonInUnitySpace(transform);
            OnDungeonRebuilt?.Invoke(_actualStructure);
        }
    }
}