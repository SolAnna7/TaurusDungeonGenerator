using System;
using System.Linq;
using System.Text;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils
{
    public class DungeonDebugger
    {
        public static void DrawDungeonDebugModeEditor(DungeonStructure structure)
        {
            structure.StartElement.ForeachTopDownDepth((n, d) =>
            {
                Gizmos.color = ElementColorBasedOnPath(n, d);
                var bounds = n.Room.GetBounds();
                Gizmos.DrawCube(bounds.center, bounds.extents * 2.1f);
            });
        }

        public static void GenerateDungeonDebugStructure(DungeonStructure structure, Transform parent)
        {
            structure.StartElement.ForeachTopDownDepth((n, d) =>
            {
                BuildDebugCube(parent, n, ElementColorBasedOnPath(n, d));
            });
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
            bool isBranch = node.Tags.Contains(new Tag("BRANCH"));
            bool isTransit = node.StructureMetaData.IsTransit;
            bool isStart = depth == 0;

            float intensity = (float) (0.7 + 0.3 * Math.Sin(depth / Math.PI + 5));

            Color color = new Color(
                isStart || isBranch ? intensity : 0,
                isStart || !isBranch ? intensity : 0,
                isStart || isTransit ? intensity : 0, 0.95f);
            return color;
        }

        public static string GetSummaryTextForObject(GameObject gameObject)
        {
            Room room = gameObject.GetComponent<Room>();
            if (room != null)
            {
                return GetSummaryTextForRoom(room);
            }

            var roomWrapper = gameObject.GetComponent<DebugRoomWrapper>();
            if (roomWrapper != null)
            {
                return GetSummaryTextForRoom(roomWrapper.room);
            }

            return null;
        }

        private static string GetSummaryTextForRoom(Room room)
        {
            var roomDungeonStructureNode = room.DungeonStructureNode;
            var tags = room.DungeonStructureNode.Tags;

            StringBuilder sb = new StringBuilder();
            sb.Append(roomDungeonStructureNode.Style);
            if (tags.Count > 0)
            {
                sb.Append("\nTags:\n - ");
                sb.Append(String.Join("\n - ", tags.Select(tag => tag.Value)));
            }

            return sb.ToString();
        }
    }
}