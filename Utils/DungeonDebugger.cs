using System;
using System.Linq;
using System.Text;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils
{
    /// <summary>
    /// Provides debug information and visualization about the built dungeons
    /// </summary>
    public static class DungeonDebugger
    {
        /// <summary>
        /// Draws the dungeon debug data in editor scene view
        /// Should only be called from OnDrawGizmos or OnDrawGizmosSelected unity functions
        /// </summary>
        public static void DrawDungeonDebugGizmos(DungeonStructure structure)
        {
            structure.StartElement.ForeachDepthFirst((n, d) =>
            {
                Gizmos.color = ElementColorBasedOnPath(n, d);
                var bounds = n.Room.GetBounds();
                Gizmos.DrawCube(bounds.center, bounds.extents * 2.1f);
            });
        }

        /// <summary>
        /// Builds an object structure around the dungeon with the debug informations
        /// Can be used in built games
        /// </summary>
        public static void GenerateDungeonDebugStructure(DungeonStructure structure, Transform parent = null)
        {
            structure.StartElement.ForeachDepthFirst((n, d) => { BuildDebugCube(parent, n, ElementColorBasedOnPath(n, d)); });
        }

        /// <summary>
        /// Gets a debug information about a Room or DebugRoomWrapper
        /// </summary>
        /// <param name="gameObject">The object to get the information about</param>
        /// <returns>String with the debug data or null if object is not Room or DebugRoomWrapper</returns>
        public static string TryGetSummaryTextForObject(GameObject gameObject)
        {
            Room room = gameObject.GetComponent<Room>();
            if (room != null)
            {
                return GetSummaryTextForRoom(room);
            }

            var roomWrapper = gameObject.GetComponentInParent<DebugRoomWrapper>();
            if (roomWrapper != null)
            {
                return GetSummaryTextForRoom(roomWrapper.room);
            }

            return null;
        }

        private static void BuildDebugCube(Transform parent, DungeonNode n, Color color)
        {
            var bounds = n.Room.GetBounds();

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.AddComponent<DebugRoomWrapper>().room = n.Room;
            cube.transform.position = bounds.center;
            cube.transform.localScale = bounds.extents * 2.01f;
            cube.transform.parent = parent;

            var material = cube.GetComponent<Renderer>().material;
            material.color = color;
            material.shader = Shader.Find("Unlit/Color");
        }

        private static Color ElementColorBasedOnPath(DungeonNode node, int depth)
        {
            bool isBranch = node.MetaData.GetTags().Contains("BRANCH");
            bool isTransit = node.MetaData.IsTransit;
            bool isStart = depth == 0;

            float intensity = (float) (0.7 + 0.3 * Math.Sin(depth / Math.PI + 5));

            Color color = new Color(
                isStart || isBranch ? intensity : 0,
                isStart || !isBranch ? intensity : 0,
                isStart || isTransit ? intensity : 0, 0.95f);
            return color;
        }

        private static string GetSummaryTextForRoom(Room room)
        {
            var roomDungeonStructureNode = room.DungeonStructureNode;
            var tags = room.DungeonStructureNode.MetaData.GetTags().ToList();

            StringBuilder sb = new StringBuilder();
            sb.Append(roomDungeonStructureNode.Style);
            if (tags.Count > 0)
            {
                sb.Append("\nTags:\n - ");
                sb.Append(String.Join("\n - ", tags));
            }

            return sb.ToString();
        }

        private class DebugRoomWrapper : MonoBehaviour
        {
            public Room room;
        }
    }
}