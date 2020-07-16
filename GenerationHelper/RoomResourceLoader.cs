using System;
using System.Collections.Generic;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.GenerationHelper
{
    /// <summary>
    /// Internal generation class used to load room resources
    /// </summary>
    public static class RoomResourceLoader
    {
        internal static Dictionary<string, RoomCollection> LoadRoomPrototypes(DungeonStructure dungeonStructure, Func<string, RoomCollection> roomLoaderOverwrite = null)
        {
            var collector = new Dictionary<string, RoomCollection>();

            var roomLoader = roomLoaderOverwrite ?? LoadRoom;

            var startElement = dungeonStructure.StartElement;
            LoadRoomPrototypesRecur(startElement, collector, roomLoader);

            if (dungeonStructure.NodeMetaData.BranchDataWrapper != null)
                foreach (var prototypeName in dungeonStructure.NodeMetaData.BranchDataWrapper.BranchPrototypeNames)
                {
                    LoadRoomPrototypesRecur(dungeonStructure.AbstractStructure.EmbeddedDungeons[prototypeName].StartElement, collector, roomLoader);
                }

            return collector;
        }

        private static void LoadRoomPrototypesRecur(DungeonNode styledElement, IDictionary<string, RoomCollection> collector, Func<string, RoomCollection> roomLoader)
        {
            var style = styledElement.Style;
            if (!collector.ContainsKey(style))
                collector.Add(style, LoadRoom(style));
            styledElement.SubElements.ForEach(node => LoadRoomPrototypesRecur(node, collector, roomLoader));
        }

        private static void LoadRoomPrototypesRecur(AbstractDungeonElement styledElement, IDictionary<string, RoomCollection> collector, Func<string, RoomCollection> roomLoader)
        {
            var style = styledElement.Style;
            if (!collector.ContainsKey(style))
                collector.Add(style, roomLoader(style));
            styledElement.SubElements.ForEach(element => LoadRoomPrototypesRecur(element, collector, roomLoader));
        }

        private static RoomCollection LoadRoom(string style)
        {
            var rooms = Resources.Load<RoomCollection>(style);
            if (rooms == null)
            {
                throw new Exception($"Room collection not found: {style}");
            }

            return rooms;
        }
    }
}