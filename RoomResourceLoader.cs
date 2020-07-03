using System;
using System.Collections.Generic;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator
{
    public class RoomResourceLoader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dungeonStructure"></param>
        /// <returns></returns>
        internal static Dictionary<string, RoomCollection> LoadRoomPrototypes(DungeonStructure dungeonStructure)
        {
            var collector = new Dictionary<string, RoomCollection>();

            var startElement = dungeonStructure.StartElement;
            LoadRoomPrototypesRecur(startElement, collector);

            if (dungeonStructure.NodeMetaData.BranchDataWrapper != null)
                foreach (var prototypeName in dungeonStructure.NodeMetaData.BranchDataWrapper.BranchPrototypeNames)
                {
                    LoadRoomPrototypesRecur(dungeonStructure.AbstractStructure.EmbeddedDungeons[prototypeName].StartElement, collector);
                }

            return collector;
        }

        private static void LoadRoomPrototypesRecur(DungeonNode styledElement, IDictionary<string, RoomCollection> collector)
        {
            var style = styledElement.Style;
            if (!collector.ContainsKey(style))
                collector.Add(style, LoadRoom(style));
            styledElement.SubElements.ForEach(x => LoadRoomPrototypesRecur(x, collector));
        }

        private static void LoadRoomPrototypesRecur(AbstractDungeonElement styledElement, IDictionary<string, RoomCollection> collector)
        {
            var style = styledElement.Style;
            if (!collector.ContainsKey(style))
                collector.Add(style, LoadRoom(style));
            styledElement.SubElements.ForEach(element => LoadRoomPrototypesRecur(element, collector));
        }

        private static RoomCollection LoadRoom(string style)
        {
            var rooms = Resources.Load<RoomCollection>(style);
            if (rooms == null)
            {
                throw new Exception($"Room collection not found: Dungeons/{style}");
            }

            return rooms;
        }
    }
}