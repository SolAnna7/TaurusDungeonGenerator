using System.Collections.Generic;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Component
{
    /// <summary>
    /// The base unit of dungeon building
    /// </summary>
    public partial class Room : MonoBehaviour
    {
        /// <summary>
        /// Set the bounds by hand
        /// </summary>
        public bool overwriteBounds = false;
        /// <summary>
        /// The manually set bounds
        /// </summary>
        public Bounds overwrittenBounds;
        /// <summary>
        /// Show debug information in the editor
        /// </summary>
        public bool debugMode;

        /// <summary>
        /// The dungeon structure element the room is built by
        /// </summary>
        public DungeonNode DungeonStructureNode { get; set; }
        
        /// <summary>
        /// Returns the computed or overwritten bounds of the room
        /// </summary>
        public Bounds GetBounds()
        {
            if (overwriteBounds)
                return overwrittenBounds;
            return GetActualBounds();
        }

        /// <summary>
        /// Computes the total bounds of the room and children objects
        /// </summary>
        public Bounds GetActualBounds()
        {
            Bounds bounds = new Bounds();
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            if (renderers.Length > 0)
            {
                //Find first enabled renderer to start encapsulate from it
                // ReSharper disable once LocalVariableHidesMember
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

        /// <summary>
        /// Collects the <see cref="RoomConnector"/> Components in child objects
        /// </summary>
        public IEnumerable<RoomConnector> GetConnections()
        {
            return GetComponentsInChildren<RoomConnector>();
        }

        /// <summary>
        /// Returns the base area of the room bounds
        /// </summary>
        public float GetArea()
        {
            var bounds = GetBounds();
            return bounds.size.x * bounds.size.z;
        }
        
        private void Reset()
        {
            debugMode = false;
        }

        // ReSharper disable once IdentifierTypo
        private void OnDrawGizmosSelected()
        {
            if (!debugMode)
                return;

            if (overwriteBounds)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(overwrittenBounds.center, overwrittenBounds.size);
            }
            else
            {
                Gizmos.color = Color.green;
                var bounds = GetBounds();
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }

    }
}