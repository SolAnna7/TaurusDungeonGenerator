using System;
using System.Collections.Generic;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using UnityEngine;
using Room = SnowFlakeGamesAssets.TaurusDungeonGenerator.Component.Room;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.GenerationModel
{
    /// <summary>
    /// A room wrapper used in the generation process
    /// </summary>
    internal class RoomPrototype
    {
        public RoomPrototype(
            DungeonNode actualGraphElement,
            Room roomResource,
            Vector3 globalPosition,
            Quaternion rotation)
        {
            GlobalPosition = globalPosition;
            Rotation = rotation;
            RoomResource = roomResource;
            ActualGraphElement = actualGraphElement;
        }

        internal readonly Vector3 GlobalPosition;
        internal readonly Quaternion Rotation;
        internal List<RoomPrototypeConnection> ChildRoomConnections { get; private set; }
        internal RoomPrototypeConnection ParentRoomConnection { get; private set; }
        internal readonly Room RoomResource;
        internal readonly DungeonNode ActualGraphElement;

        public void ConnectToParent(RoomConnector connectionToParentRoom, RoomPrototypeConnection parentRoomConnection)
        {
            if (parentRoomConnection != null)
            {
                if (parentRoomConnection.State != PrototypeConnectionState.FREE)
                {
                    throw new Exception("parent connection already filled");
                }

                ParentRoomConnection = parentRoomConnection;
                ParentRoomConnection.SetChild(this, connectionToParentRoom);
            }

            ChildRoomConnections = RoomResource.GetConnections()
                .Where(x => x != connectionToParentRoom)
                .Select(x => new RoomPrototypeConnection(this, x))
                .ToList();
        }

        public Bounds GetRotatedBounds()
        {
            var roomResourceBounds = RoomResource.GetBounds();
            var size = Rotation * roomResourceBounds.size;
            size.x = Math.Abs(size.x);
            size.y = Math.Abs(size.y);
            size.z = Math.Abs(size.z);
            return new Bounds(GlobalPosition + Rotation * roomResourceBounds.center, size);
        }
    }
}