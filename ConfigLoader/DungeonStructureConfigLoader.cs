#if SFG_PISCES_CONFIG

using System;
using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.PiscesConfigLoader;
using SnowFlakeGamesAssets.PiscesConfigLoader.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
using UnityEngine;
using static SnowFlakeGamesAssets.TaurusDungeonGenerator.ConfigLoader.DungeonStructureConfigKeys;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.ConfigLoader
{
    /// <summary>
    /// Loads a dungeon structure from assets using the PiscesConfigLoader
    /// Only works if the PiscesConfigLoader is included in the project
    /// </summary>
    public static class DungeonStructureConfigLoader
    {
        private static readonly Dictionary<string, Func<QueryResult, object>> PropertyLoaders = new Dictionary<string, Func<QueryResult, object>>();

        public static void RegisterPropertyLoader(string propertyKey, Func<QueryResult, object> loadingFunc) => PropertyLoaders.Add(propertyKey, loadingFunc);

        /// <summary>
        /// Loads the dungeon from the specified config path 
        /// Requires the GameConfig to be initiated
        /// </summary>
        /// <param name="dungeonPath">The absolute config path to the dungeon structure</param>
        /// <param name="configRoot">The root of the config structure</param>
        /// <returns>The loaded AbstractDungeonStructure</returns>
        public static AbstractDungeonStructure BuildFromConfig(ConfigPath dungeonPath, ConfigNode configRoot)
        {
            var dungeonStructureBaseNode = configRoot.Query(dungeonPath).AsNode();
            return BuildFromConfigNode(dungeonStructureBaseNode, configRoot);
        }

        private static AbstractDungeonStructure BuildFromConfigNode(ConfigNode dungeonStructureBaseNode, ConfigNode configRoot)
        {
            ISet<String> nestedDungeonNameCollector = new HashSet<string>();
            Dictionary<string, AbstractDungeonStructure> nestedDungeons = new Dictionary<string, AbstractDungeonStructure>();

            var firstElement = ReadElement(dungeonStructureBaseNode.Query(START_NODE).AsNode(), nestedDungeonNameCollector);

            BranchDataWrapper branchData = ReadBranchData(dungeonStructureBaseNode);
            if (branchData != null)
                nestedDungeonNameCollector.UnionWith(branchData.BranchPrototypeNames);

            var inlineDungeons = dungeonStructureBaseNode.TryQuery(INLINE_NESTED);
            if (inlineDungeons.IsPresent)
            {
                var inlineNode = inlineDungeons.Get().AsNode();
                foreach (var name in inlineNode.GetKeys())
                {
                    nestedDungeons.Add(name, BuildFromConfigNode(inlineNode.Query(name).AsNode(), configRoot));
                }
            }

            foreach (var nestedDungeonPath in nestedDungeonNameCollector)
            {
                if (!nestedDungeons.ContainsKey(nestedDungeonPath))
                {
                    var nestedDungeonConfigPath = new ConfigPath(nestedDungeonPath.Split('.'));
                    nestedDungeons.Add(nestedDungeonPath, BuildFromConfig(nestedDungeonConfigPath, configRoot));
                }
            }

            var structureMetaData = ReadStructureMetaData(dungeonStructureBaseNode);
            AddParentTagsRecursive(firstElement, structureMetaData.GlobalNodePropertyAndTagHolder.GetTags());

            var abstractDungeonStructure = AbstractDungeonStructure.Builder
                .SetMetaData(structureMetaData)
                .SetEmbeddedDungeons(nestedDungeons)
                .SetBranchData(branchData)
                .SetStartElement(firstElement)
                .Build();
            return abstractDungeonStructure;
        }

        private static StructureMetaData ReadStructureMetaData(ConfigNode dungeonStructureBaseNode)
        {
            var structureMetaDataBuilder = StructureMetaData.Builder;

            dungeonStructureBaseNode
                .TryQuery(GLOBAL_NODE_TAGS)
                .IfPresentGet(tagsNode => tagsNode.AsList().Select(tagNode => tagNode.AsString()),
                    new HashSet<string>()).ForEach(t => structureMetaDataBuilder.AddGlobalTag(t));

            dungeonStructureBaseNode
                .TryQuery(STRUCTURE_TAGS)
                .IfPresentGet(tagsNode => tagsNode.AsList().Select(tagNode => tagNode.AsString()),
                    new HashSet<string>()).ForEach(t => structureMetaDataBuilder.AddStructureTag(t));

            dungeonStructureBaseNode.TryQuery(STRUCTURE_PROPERTIES).IfPresent(
                propertiesNode => propertiesNode.AsNode().GetKeys().ForEach(
                    propertyKey =>
                    {
                        if (PropertyLoaders.ContainsKey(propertyKey))
                        {
                            structureMetaDataBuilder.AddStructureProperty(propertyKey, PropertyLoaders[propertyKey](propertiesNode.AsNode().Query(propertyKey)));
                        }
                        else
                        {
                            Debug.LogWarning($"PropertyLoader not found for property key: {propertyKey}");
                        }
                    }));

            float marginUnit = dungeonStructureBaseNode.TryQuery(MARGIN_UNIT).IfPresentGet(x => x.AsFloat(), 0);
            structureMetaDataBuilder.SetMargin(marginUnit);

            return structureMetaDataBuilder.Build();
        }

        private static BranchDataWrapper ReadBranchData(ConfigNode node)
        {
            var maybeQueryResult = node.TryQuery(BRANCH_PROTOTYPES);
            if (maybeQueryResult.IsPresent)
            {
                var branchNames = maybeQueryResult.Get().AsList().Select(x => x.AsString()).ToList();
                var maybeMaxBranchNum = node.TryQuery(BRANCH_MAX_NUM);
                var maybeMaxBranchPercent = node.TryQuery(BRANCH_MAX_PERCENT);

                if (maybeMaxBranchNum.IsPresent && maybeMaxBranchPercent.IsPresent)
                    throw new Exception("Branch number and branch percentage cannot be both set!");

                if (!maybeMaxBranchNum.IsPresent && !maybeMaxBranchPercent.IsPresent)
                    throw new Exception("No branch number or percentage set!");

                if (maybeMaxBranchNum.IsPresent)
                    return new BranchDataWrapper(branchNames, maybeMaxBranchNum.Get().AsInt());

                if (maybeMaxBranchPercent.IsPresent)
                    return new BranchDataWrapper(branchNames, maybeMaxBranchPercent.Get().AsFloat());
            }

            return null;
        }


        private static AbstractDungeonElement ReadElement(ConfigNode config, ISet<string> nestedDungeonCollector)
        {
            AbstractDungeonElement element = null;
            config.TryQuery(NODE).IfPresent(node =>
            {
                var nodeElementBuilder = DungeonElementBuilder
                    .NodeElement(node.AsString())
                    .SetMetaData(CollectMetaData(config));

                config.TryQuery(SUBS).IfPresent(subElementNodes =>
                    subElementNodes.AsList()
                        .Select(subNode => ReadElement(subNode.AsNode(), nestedDungeonCollector))
                        .ForEach(subElement => nodeElementBuilder.AddSubElement(subElement)));
                element = nodeElementBuilder.Build();
            });
            if (element == null)
                config.TryQuery(CONNECTION).IfPresent(connection =>
                {
                    var style = connection.AsString();
                    var length = config.Query(LENGTH).AsRangeI().ToTaurusRange();

                    var connectionElementBuilder = DungeonElementBuilder
                        .ConnectionElement(style, length)
                        .SetMetaData(CollectMetaData(config));

                    config.TryQuery(SUBS).IfPresent(subElementNodes =>
                        subElementNodes.AsList()
                            .Select(queryResult => ReadElement(queryResult.AsNode(), nestedDungeonCollector))
                            .ForEach(subElement => connectionElementBuilder.AddSubElement(subElement)));

                    element = connectionElementBuilder.Build();
                });
            if (element == null)
                config.TryQuery(NESTED).IfPresent(nested =>
                {
                    var path = nested.AsString();

                    nestedDungeonCollector.Add(path);
                    var nestedElementBuilder = DungeonElementBuilder
                        .NestedDungeonElement(path)
                        .SetMetaData(CollectMetaData(config));

                    config.TryQuery(SUBS).IfPresent(subElementNodes =>
                        subElementNodes.AsList()
                            .Select(node => ReadElement(node.AsNode(), nestedDungeonCollector))
                            .ForEach(subElement => nestedElementBuilder.AddSubElement(subElement)));

                    element = nestedElementBuilder.Build();
                });

            if (element == null)
                throw new Exception("unknown dungeon element type!");

            return element;
        }

        private static IEnumerable<string> ReadTags(ConfigNode config)
        {
            return config.TryQuery(TAGS).IfPresentGet(tagsNode => tagsNode.AsList().Select(tagNode => tagNode.AsString()), new List<string>());
        }

        private static void AddParentTagsRecursive(AbstractDungeonElement element, IEnumerable<string> tags)
        {
            element.SubElements.ForEach(s => AddParentTagsRecursive(s, tags));
            tags.ForEach(element.ElementMetaData.AddTag);
        }

        private static NodeMetaData CollectMetaData(ConfigNode config)
        {
            var nodeMetaDataBuilder = NodeMetaData.Builder
                .SetBranchData(ReadBranchData(config));
            if (config.TryQuery(OPTIONAL_ENDPOINT).IsPresent)
                nodeMetaDataBuilder.SetOptionalEndpoint();
            if (config.TryQuery(OPTIONAL).IsPresent)
                nodeMetaDataBuilder.SetOptionalNode();

            ReadTags(config).ForEach(t => nodeMetaDataBuilder.AddTag(t));

            return nodeMetaDataBuilder.Build();
        }

        private static RangeI ToTaurusRange(this PiscesConfigLoader.Utils.RangeI source) => new RangeI(source.Min, source.Max);
    }
}
#endif