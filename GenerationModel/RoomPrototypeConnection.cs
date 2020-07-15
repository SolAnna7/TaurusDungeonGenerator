using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.GenerationModel
{
    internal enum PrototypeConnectionState
    {
        FREE,
        CONNECTED,
        CLOSED
    }

    /// <summary>
    /// A connection between RoomPrototypes
    /// </summary>
    internal class RoomPrototypeConnection
    {
        public RoomPrototypeConnection(RoomPrototype parentRoomPrototype, RoomConnector parentConnection)
        {
            ParentConnection = parentConnection;
            ParentRoomPrototype = parentRoomPrototype;
            State = PrototypeConnectionState.FREE;
        }

        internal PrototypeConnectionState State { get; private set; }

        internal readonly RoomConnector ParentConnection;
        internal RoomConnector ChildConnection;

        internal readonly RoomPrototype ParentRoomPrototype;
        internal RoomPrototype ChildRoomPrototype;

        public void SetChild(RoomPrototype childRoomPrototype, RoomConnector childConnection)
        {
            ChildConnection = childConnection;
            ChildRoomPrototype = childRoomPrototype;
            State = PrototypeConnectionState.CONNECTED;
        }

        public void ClearChild()
        {
            ChildConnection = null;
            ChildRoomPrototype = null;
            State = PrototypeConnectionState.FREE;
        }

        public void Close() => State = PrototypeConnectionState.CLOSED;
    }
}