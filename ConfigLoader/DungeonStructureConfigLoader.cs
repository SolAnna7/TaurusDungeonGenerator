#if SFG_PISCES_CONFIG

using System;
using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.PiscesConfigLoader;
using SnowFlakeGamesAssets.PiscesConfigLoader.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
using static SnowFlakeGamesAssets.TaurusDungeonGenerator.ConfigLoader.DungeonStructureConfigKeys;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.ConfigLoader
{
    /// <summary>
    /// Loads a dungeon structure from assets using the PiscesConfigLoader
    /// Only works if the PiscesConfigLoader is included in the project
    /// </summary>
    public static class DungeonStructureConfigLoader
    {
        /// <summary>
        /// Loads the dungeon from the specified config path 
        /// Requires the GameConfig to be initiated
        /// </summary>
        /// <param name="dungeonPath">The absolute config path to the dungeon structure</param>
        /// <returns>The loaded AbstractDungeonStructure</returns>
        public static AbstractDungeonStructure BuildFromConfig(ConfigPath dungeonPath)
        {
            var dungeonStructureBaseNode = GameConfig.Query(dungeonPath).AsNode();
            return BuildFromConfigNode(dungeonStructureBaseNode);
        }

        private static AbstractDungeonStructure BuildFromConfigNode(ConfigNode dungeonStructureBaseNode)
        {
            ISet<String> nestedDungeonNameCollector = new HashSet<string>();
            Dictionary<string, AbstractDungeonStructure> nestedDungeons = new Dictionary<string, AbstractDungeonStructure>();

            var firstElement = ReadElement(dungeonStructureBaseNode.Query(START_NODE).AsNode(), nestedDungeonNameCollector);

            BranchDataWrapper branchData = ReadBranchData(dungeonStructureBaseNode);
            if (branchData != null)
                nestedDungeonNameCollector.UnionWith(branchData.BranchPrototypeNames);

            var inlineDungeons = dungeonStructureBaseNode.TryQuery("inline-nested");
            if (inlineDungeons.IsPresent)
            {
                var inlineNode = inlineDungeons.Get().AsNode();
                foreach (var name in inlineNode.GetKeys())
                {
                    nestedDungeons.Add(name, BuildFromConfigNode(inlineNode.Query(name).AsNode()));
                }
            }

            foreach (var nestedDungeonPath in nestedDungeonNameCollector)
            {
                if (!nestedDungeons.ContainsKey(nestedDungeonPath))
                {
                    var nestedDungeonConfigPath = new ConfigPath(nestedDungeonPath.Split('.'));
                    nestedDungeons.Add(nestedDungeonPath, BuildFromConfig(nestedDungeonConfigPath));
                }
            }

            var structureMetaData = ReadStructureMetaData(dungeonStructureBaseNode);
            AddParentTagsRecursive(firstElement, structureMetaData.GlobalNodePropertyAndTagHolder.GetTags());

            var abstractDungeonStructure = new AbstractDungeonStructure(firstElement, structureMetaData)
                {BranchDataWrapper = branchData, EmbeddedDungeons = nestedDungeons};
            abstractDungeonStructure.ValidateStructure();
            return abstractDungeonStructure;
        }

        private static StructureMetaData ReadStructureMetaData(ConfigNode dungeonStructureBaseNode)
        {
            var globalTags = new PropertyAndTagHolder();
            dungeonStructureBaseNode
                .TryQuery(GLOBAL_NODE_TAGS)
                .IfPresentGet(tagsNode => tagsNode.AsNodeList().Select(tagNode => tagNode.AsString()),
                    new HashSet<string>()).ForEach(globalTags.AddTag);

            var structureTags = new PropertyAndTagHolder();
            dungeonStructureBaseNode
                .TryQuery(STRUCTURE_TAGS)
                .IfPresentGet(tagsNode => tagsNode.AsNodeList().Select(tagNode => tagNode.AsString()),
                    new HashSet<string>()).ForEach(structureTags.AddTag);

            float marginUnit = dungeonStructureBaseNode.TryQuery(MARGIN_UNIT).IfPresentGet(x => x.AsFloat(), 0);

            return new StructureMetaData(marginUnit, structureTags, globalTags);
        }

        private static BranchDataWrapper ReadBranchData(ConfigNode node)
        {
            var maybeQueryResult = node.TryQuery(BRANCH_PROTOTYPES);
            if (maybeQueryResult.IsPresent)
            {
                var branchNames = maybeQueryResult.Get().AsNodeList().Select(x => x.AsString()).ToList();
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
                var style = node.AsString();

                var subElementsMaybe = config.TryQuery(SUBS);

                subElementsMaybe.IfPresent(
                    se => element = new NodeElement(style, CollectMetaData(config), se.AsNodeList().Select(subnode => ReadElement(subnode, nestedDungeonCollector)).ToArray()),
                    () => element = new NodeElement(style, CollectMetaData(config))
                );
            });
            if (element == null)
                config.TryQuery(CONNECTION).IfPresent(connection =>
                {
                    var style = connection.AsString();
                    var length = config.Query(LENGTH).AsRangeI().ToTaurusRange();
                    var subElementsMaybe = config.TryQuery(SUBS);

                    subElementsMaybe.IfPresent(
                        se => { element = new ConnectionElement(style, CollectMetaData(config), length, se.AsNodeList().Select(node => ReadElement(node, nestedDungeonCollector)).ToArray()); },
                        () => element = new ConnectionElement(style, CollectMetaData(config), length)
                    );
                });
            if (element == null)
                config.TryQuery(NESTED).IfPresent(nested =>
                {
                    var path = nested.AsString();
                    var subElementsMaybe = config.TryQuery(SUBS);

                    nestedDungeonCollector.Add(path);

                    subElementsMaybe.IfPresent(
                        se => { element = new NestedDungeon(path, CollectMetaData(config), se.AsNodeList().Select(node => ReadElement(node, nestedDungeonCollector)).ToArray()); },
                        () => element = new NestedDungeon(path, CollectMetaData(config))
                    );
                });

            if (element == null)
                throw new Exception("unknown dungeon element type!");

            return element;
        }

        private static IEnumerable<string> ReadTags(ConfigNode config)
        {
            return config.TryQuery(TAGS).IfPresentGet(tagsNode => tagsNode.AsNodeList().Select(tagNode => tagNode.AsString()), new List<string>());
        }

        private static void AddParentTagsRecursive(AbstractDungeonElement element, IEnumerable<string> tags)
        {
            element.SubElements.ForEach(s => AddParentTagsRecursive(s, tags));
            tags.ForEach(element.ElementMetaData.AddTag);
        }

        private static NodeMetaData CollectMetaData(ConfigNode config)
        {
            NodeMetaData metaData = new NodeMetaData(ReadBranchData(config),
                new PropertyAndTagHolder()
                    .Also(p => ReadTags(config).ForEach(p.AddTag)));

            metaData.IsTransit = config.TryQuery(TRANSIT).IsPresent;
            if (config.TryQuery(OPTIONAL).IsPresent)
                metaData.OptionalNodeData = new OptionalNodeData {Required = true};

            return metaData;
        }

        private static RangeI ToTaurusRange(this PiscesConfigLoader.Utils.RangeI source) => new RangeI(source.Min,source.Max);
    }
}
#endif