using System.Collections.Generic;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Component
{
    public class RoomCollection : ScriptableObject
    {
        public List<TaurusDungeonGenerator.Component.Room> rooms = new List<TaurusDungeonGenerator.Component.Room>();
    }
}