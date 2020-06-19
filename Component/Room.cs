using System;
using System.Collections.Generic;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Component
{
    [Serializable]
    public partial class Room : MonoBehaviour

    {
        [Header("Debug")] public bool ShowBounds;
        public Bounds BoundsOverwrite;
        public DungeonNode DungeonStructureNode { get; set; }

        private void Reset()
        {
            ShowBounds = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (!ShowBounds)
                return;

            if (BoundsOverwrite != default(Bounds))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(BoundsOverwrite.center, BoundsOverwrite.size);
            }
            else
            {
                Gizmos.color = Color.green;
                var bounds = GetBounds();
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }


        public Bounds GetBounds()
        {
            {
                Bounds bounds = new Bounds();
                Renderer[] renderers = GetComponentsInChildren<Renderer>();

                if (renderers.Length > 0)
                {
                    //Find first enabled renderer to start encapsulate from it
                    foreach (Renderer renderer in renderers)
                    {
                        if (renderer.enabled)
                        {
                            bounds = renderer.bounds;
                            break;
                        }
                    }

                    //Encapsulate for all renderers
                    foreach (Renderer rend in renderers)
                        if (rend.enabled)
                            bounds.Encapsulate(rend.bounds);
                }

                return bounds;
            }
        }

        public IEnumerable<RoomConnector> GetConnections()
        {
            return GetComponentsInChildren<RoomConnector>();
        }

        public float GetArea()
        {
            var bounds = GetBounds();

            return bounds.size.x * bounds.size.z;
        }
    }
}