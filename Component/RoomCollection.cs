using System.Collections.Generic;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Component
{
    /// <summary>
    /// A list of rooms to save and load as prefab
    /// </summary>
    public class RoomCollection : ScriptableObject
    {
        public List<Room> rooms = new List<Room>();
    }
}