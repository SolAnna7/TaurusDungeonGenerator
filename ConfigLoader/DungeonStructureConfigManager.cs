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
    public static class DungeonStructureConfigManager
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

            var dungeonTags = ReadTags(dungeonStructureBaseNode);

            BranchDataWrapper branchData = ReadBranchData(dungeonStructureBaseNode);
            if (branchData != null)
                nestedDungeonNameCollector.UnionWith(branchData.BranchPrototypeNames);

            AddParentTagsRecursive(firstElement, dungeonTags);

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

            var abstractDungeonStructure = new AbstractDungeonStructure(firstElement) {BranchDataWrapper = branchData, EmbeddedDungeons = nestedDungeons};
            abstractDungeonStructure.ValidateStructure();
            return abstractDungeonStructure;
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
                        se => element = new NodeElement(style, se.AsNodeList().Select(node => ReadElement(node, nestedDungeonCollector)).ToArray()),
                        () => element = new NodeElement(style)
                    );
                    element.IsOptional = config.TryQuery("optional").IsPresent;
                    element.IsTansit = config.TryQuery("transit").IsPresent;
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
                        se => { element = new ConnectionElement(style, length, se.AsNodeList().Select(node => ReadElement(node, nestedDungeonCollector)).ToArray()); },
                        () => element = new ConnectionElement(style, length)
                    );
                    element.IsOptional = config.TryQuery("optional").IsPresent;
                    element.IsTansit = config.TryQuery("transit").IsPresent;
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
                        se => { element = new NestedDungeon(path, se.AsNodeList().Select(node => ReadElement(node, nestedDungeonCollector)).ToArray()); },
                        () => element = new NestedDungeon(path)
                    );
                    element.IsOptional = config.TryQuery("optional").IsPresent;
                    element.IsTansit = config.TryQuery("transit").IsPresent;
                }
            }

            if (element == null)
                throw new Exception("unknown dungeon element type!");

            ReadAndAddTags(config, element);

            return element;
        }

        private static void ReadAndAddTags(ConfigNode config, AbstractDungeonElement element)
        {
            element.AddTags(ReadTags(config));
        }

        private static IEnumerable<Tag> ReadTags(ConfigNode config)
        {
            IEnumerable<Tag> tags = new List<Tag>();
            config.TryQuery("tags").IfPresent(tagsNode => { tags = tagsNode.AsNodeList().Select(tag => new Tag(tag.AsString())); });
            return tags;
        }

        private static void AddParentTagsRecursive(AbstractDungeonElement element, IEnumerable<Tag> tags)
        {
            element.SubElements.ForEach(s => AddParentTagsRecursive(s, tags));

            element.AddTags(tags);
        }
    }
}
#endif

