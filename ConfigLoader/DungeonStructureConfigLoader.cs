#if SFG_PISCES_CONFIG

using System;
using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.PiscesConfigLoader;
using SnowFlakeGamesAssets.PiscesConfigLoader.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.ConfigLoader
{
    public static class DungeonStructureConfigLoader
    {
        public static AbstractDungeonStructure BuildFromConfig(ConfigPath relativeDungeonPath)
        {
            var dungeonStructureBaseNode = GameConfig.Query(new ConfigPath("dungeons").Add(relativeDungeonPath)).AsNode();

            return BuildFromConfigNode(dungeonStructureBaseNode);
        }

        private static AbstractDungeonStructure BuildFromConfigNode(ConfigNode dungeonStructureBaseNode)
        {
            ISet<String> nestedDungeonNameCollector = new HashSet<string>();
            var firstElement = ReadElement(dungeonStructureBaseNode.Query("start-node").AsNode(), nestedDungeonNameCollector);

            BranchDataWrapper branchData = ReadBranchData(dungeonStructureBaseNode);
            if (branchData != null)
                nestedDungeonNameCollector.UnionWith(branchData.BranchPrototypeNames);

            Dictionary<string, AbstractDungeonStructure> nestedDungeons = new Dictionary<string, AbstractDungeonStructure>();

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
                .TryQuery("global-node-tags")
                .IfPresentGet(tagsNode => tagsNode.AsNodeList().Select(tagNode => tagNode.AsString()),
                    new HashSet<string>()).ForEach(globalTags.AddTag);
            
            var structureTags = new PropertyAndTagHolder();
            dungeonStructureBaseNode
                .TryQuery("structure-tags")
                .IfPresentGet(tagsNode => tagsNode.AsNodeList().Select(tagNode => tagNode.AsString()),
                    new HashSet<string>()).ForEach(structureTags.AddTag);

            float marginUnit = dungeonStructureBaseNode.TryQuery("margin-unit").IfPresentGet(x => x.AsFloat(), 0);

            return new StructureMetaData(marginUnit,structureTags,globalTags);
        }

        private static BranchDataWrapper ReadBranchData(ConfigNode node)
        {
            var maybeQueryResult = node.TryQuery("branch-prototypes");
            if (maybeQueryResult.IsPresent)
            {
                var branchNames = maybeQueryResult.Get().AsNodeList().Select(x => x.AsString()).ToList();
                var maybeMaxBranchNum = node.TryQuery("branch-max-num");
                var maybeMaxBranchPercent = node.TryQuery("branch-max-percent");

                if (maybeMaxBranchNum.IsPresent && maybeMaxBranchPercent.IsPresent)
                {
                    throw new Exception("Branch number and branch percentage cannot be both set!");
                }

                if (!maybeMaxBranchNum.IsPresent && !maybeMaxBranchPercent.IsPresent)
                {
                    throw new Exception("No branch number or percentage set!");
                }

                if (maybeMaxBranchNum.IsPresent)
                {
                    return new BranchDataWrapper(branchNames, maybeMaxBranchNum.Get().AsInt());
                }

                if (maybeMaxBranchPercent.IsPresent)
                {
                    return new BranchDataWrapper(branchNames, maybeMaxBranchPercent.Get().AsFloat());
                }
            }

            return null;
        }


        private static AbstractDungeonElement ReadElement(ConfigNode config, ISet<string> nestedDungeonCollector)
        {
            AbstractDungeonElement element = null;

            {
                var maybeNode = config.TryQuery("node");
                if (maybeNode.IsPresent)
                {
                    var style = maybeNode.Get().AsString();

                    var subElementsMaybe = config.TryQuery("subs");

                    subElementsMaybe.IfPresent(
                        se => element = new NodeElement(style, CollectMetaData(config), se.AsNodeList().Select(node => ReadElement(node, nestedDungeonCollector)).ToArray()),
                        () => element = new NodeElement(style, CollectMetaData(config))
                    );
                }
            }
            {
                var maybeConnection = config.TryQuery("connection");
                if (element == null && maybeConnection.IsPresent)
                {
                    var style = maybeConnection.Get().AsString();
                    var length = config.Query("length").AsRangeI();
                    var subElementsMaybe = config.TryQuery("subs");

                    subElementsMaybe.IfPresent(
                        se => { element = new ConnectionElement(style, CollectMetaData(config), length, se.AsNodeList().Select(node => ReadElement(node, nestedDungeonCollector)).ToArray()); },
                        () => element = new ConnectionElement(style, CollectMetaData(config), length)
                    );
                }
            }
            {
                var maybeNested = config.TryQuery("nested");
                if (element == null && maybeNested.IsPresent)
                {
                    var path = maybeNested.Get().AsString();
                    var subElementsMaybe = config.TryQuery("subs");

                    nestedDungeonCollector.Add(path);

                    subElementsMaybe.IfPresent(
                        se => { element = new NestedDungeon(path, CollectMetaData(config), se.AsNodeList().Select(node => ReadElement(node, nestedDungeonCollector)).ToArray()); },
                        () => element = new NestedDungeon(path, CollectMetaData(config))
                    );
                }
            }

            if (element == null)
                throw new Exception("unknown dungeon element type!");

            return element;
        }

        private static IEnumerable<string> ReadTags(ConfigNode config)
        {
            IEnumerable<string> tags = new HashSet<string>();
            config.TryQuery("tags").IfPresent(tagsNode => { tags = tagsNode.AsNodeList().Select(tagNode => tagNode.AsString()); });
            return tags;
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

            metaData.IsTransit = config.TryQuery("transit").IsPresent;
            if (config.TryQuery("optional").IsPresent)
                metaData.OptionalNodeData = new OptionalNodeData {Required = true};

            return metaData;
        }
    }
}
#endif