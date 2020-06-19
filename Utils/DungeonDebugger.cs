using System;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils
{
    public class DungeonDebugger
    {
        public static void DrawDungeonDebugMode(DungeonStructure structure)
        {
            structure.StartElement.ForeachTopDownDepth((n, d) =>
            {
                bool isBranch = n.Tags.Contains(new Tag("BRANCH"));
                bool isTransit = n.StructureMetaData.IsTransit;

                float colorValue = (float) (0.5 + 0.5 * Math.Sin( d / Math.PI));

                Gizmos.color = new Color(isBranch ? colorValue : 0, !isBranch ? colorValue : 0, isTransit ? colorValue : 0, 0.95f);
                var bounds = n.Room.GetBounds();
                Gizmos.DrawCube(bounds.center, bounds.extents * 2.1f);
            });
        }
    }
}