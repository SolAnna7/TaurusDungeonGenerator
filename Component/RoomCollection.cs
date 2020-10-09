using System;
using System.Collections.Generic;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Component
{
    /// <summary>
    /// A list of rooms to save and load as prefab
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Room Collection", menuName = "TaurusDungeonGenerator/RoomCollection", order = 1)]
    public class RoomCollection : ScriptableObject
    {
        public List<Room> rooms = new List<Room>();
    }
}