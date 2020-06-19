using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.GenerationModel
{
    internal enum ConnectionState
    {
        FREE,
        FAILED,
        CONNECTED,
        CLOSED
    }

    internal class RoomPrototypeConnection
    {
        public RoomPrototypeConnection(RoomPrototype parentRoomPrototype, RoomConnector parentConnection)
        {
            ParentConnection = parentConnection;
            ParentRoomPrototype = parentRoomPrototype;
            State = ConnectionState.FREE;
        }

        internal ConnectionState State { get; private set; }

        internal readonly RoomConnector ParentConnection;
        internal RoomConnector ChildConnection;

        internal readonly RoomPrototype ParentRoomPrototype;
        internal RoomPrototype ChildRoomPrototype;

        public void SetChild(RoomPrototype childRoomPrototype, RoomConnector childConnection)
        {
            ChildConnection = childConnection;
            ChildRoomPrototype = childRoomPrototype;
            State = ConnectionState.CONNECTED;
        }

        public void ClearChild()
        {
            ChildConnection = null;
            ChildRoomPrototype = null;
            State = ConnectionState.FREE;
        }

        public void Close() => State = ConnectionState.CLOSED;
        
        public void Fail() => State = ConnectionState.FAILED;
    }
}