using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.GenerationModel;
using UnityEngine;
using Room = SnowFlakeGamesAssets.TaurusDungeonGenerator.Component.Room;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure
{
    public class PrototypeDungeon
    {
        private RoomPrototype _firstRoom;
        private DungeonStructure _structure;

        internal PrototypeDungeon(RoomPrototype firstRoom, DungeonStructure structure)
        {
            _structure = structure;
            _firstRoom = firstRoom;
        }

        public DungeonStructure BuildDungeonInUnitySpace(Transform parent)
        {
            BuildDungeonInUnitySpaceRecur(_firstRoom, parent);
            return _structure;
        }

        private static Room BuildDungeonInUnitySpaceRecur(RoomPrototype actualElement, Transform parent)
        {
            var instantiatedRoom = Object.Instantiate(actualElement.RoomResource, actualElement.GlobalPosition, actualElement.Rotation, parent);

            actualElement.ActualGraphElement.Room = instantiatedRoom;
            instantiatedRoom.DungeonStructureNode = actualElement.ActualGraphElement;

            foreach (var prototypeConnection in actualElement.ChildRoomConnections)
            {
                var parentConn = instantiatedRoom.GetConnections().Single(c =>
                    c.name == prototypeConnection.ParentConnection.name &&
                    c.transform.localPosition == prototypeConnection.ParentConnection.transform.localPosition);

//                var x = PrefabUtility.GetCorrespondingObjectFromSource(instantiatedRoom.GetConnections().ToList()[0]);

                if (prototypeConnection.State == ConnectionState.CONNECTED)
                {
                    var nextRoom = BuildDungeonInUnitySpaceRecur(prototypeConnection.ChildRoomPrototype, parent);
                    var childConn = nextRoom.GetConnections().Single(c => c.name == prototypeConnection.ChildConnection.name &&
                                                                          c.transform.localPosition == prototypeConnection.ChildConnection.transform.localPosition);
                    RoomConnector.Connect(parentConn, childConn);

                    CreateOpenReplacement(parentConn);
                    CreateOpenReplacement(childConn);
                }
                else if (prototypeConnection.State == ConnectionState.CLOSED)
                {
                    parentConn.Close();
                    CreateClosedReplacement(parentConn);
                }
                else
                {
                    // todo branch létrehozás után exception!!
                }
            }

            return instantiatedRoom;
        }

        private static void CreateOpenReplacement(RoomConnector conn)
        {
            if (conn.OpenReplacement != null)
            {
                if (conn.OpenReplacement.scene.name == null)
                {
                    Object.Instantiate(conn.OpenReplacement, conn.transform);
                }
                else
                {
                    conn.OpenReplacement.SetActive(true);
                }
            }

            if (conn.ClosedReplacement != null)
            {
                if (conn.ClosedReplacement.scene.name != null)
                {
                    Object.Destroy(conn.ClosedReplacement);
                }
            }
        }

        private static void CreateClosedReplacement(RoomConnector conn)
        {
            if (conn.ClosedReplacement != null)
            {
                if (conn.ClosedReplacement.scene.name == null)
                {
                    Object.Instantiate(conn.ClosedReplacement, conn.transform);
                }
                else
                {
                    conn.ClosedReplacement.SetActive(true);
                }
            }

            if (conn.OpenReplacement != null)
            {
                if (conn.OpenReplacement.scene.name != null)
                {
                    Object.Destroy(conn.OpenReplacement);
                }
            }
        }
    }
}