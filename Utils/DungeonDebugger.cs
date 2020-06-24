using System;
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
                bool isBranch = n.Tags.Contains(new Tag("BRANCH"));
                bool isTransit = n.StructureMetaData.IsTransit;

                float colorValue = (float) (0.5 + 0.5 * Math.Sin(d / Math.PI));

                Gizmos.color = new Color(isBranch ? colorValue : 0, !isBranch ? colorValue : 0, isTransit ? colorValue : 0, 0.95f);
                var bounds = n.Room.GetBounds();
                Gizmos.DrawCube(bounds.center, bounds.extents * 2.1f);
            });
        }

        public static void GenerateDungeonDebugStructure(DungeonStructure structure, Transform parent)
        {
            structure.StartElement.ForeachTopDownDepth((n, d) =>
            {
                bool isBranch = n.Tags.Contains(new Tag("BRANCH"));
                bool isTransit = n.StructureMetaData.IsTransit;
                bool isStart = d == 0;

                float colorValue = (float) (0.7 + 0.3 * Math.Sin(d / Math.PI + 5));

                Color color = new Color(
                    isStart || isBranch ? colorValue : 0,
                    isStart || !isBranch ? colorValue : 0,
                    isStart || isTransit ? colorValue : 0, 0.95f);
                var bounds = n.Room.GetBounds();
                // Gizmos.DrawCube(bounds.center, bounds.extents * 2.1f);

                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = bounds.center;
                cube.transform.localScale = bounds.extents * 2.01f;
                cube.transform.parent = parent;

                var material = cube.GetComponent<Renderer>().material;
                material.color = color;
                material.shader = Shader.Find("Unlit/Color");
            });
        }
    }
}