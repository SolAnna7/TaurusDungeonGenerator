using System;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.GenerationModel;
using UnityEngine;
using Object = UnityEngine.Object;
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
                    throw new Exception("No other state should be possible");
                }
            }

            return instantiatedRoom;
        }

        private static void CreateOpenReplacement(RoomConnector conn)
        {
            RemoveOriginalChildrenIfNeeded(conn);

            if (conn.openPrefab != null)
            {
                if (conn.openPrefab.scene.name == null)
                {
                    Object.Instantiate(conn.openPrefab, conn.transform);
                }
                else
                {
                    conn.openPrefab.SetActive(true);
                }
            }

            if (conn.closedPrefab != null)
            {
                if (conn.closedPrefab.scene.name != null)
                {
                    Object.Destroy(conn.closedPrefab);
                }
            }
        }

        private static void CreateClosedReplacement(RoomConnector conn)
        {
            RemoveOriginalChildrenIfNeeded(conn);
            
            if (conn.closedPrefab != null)
            {
                if (conn.closedPrefab.scene.name == null)
                {
                    Object.Instantiate(conn.closedPrefab, conn.transform);
                }
                else
                {
                    conn.closedPrefab.SetActive(true);
                }
            }

            if (conn.openPrefab != null)
            {
                if (conn.openPrefab.scene.name != null)
                {
                    Object.Destroy(conn.openPrefab);
                }
            }

        }

        private static void RemoveOriginalChildrenIfNeeded(RoomConnector conn)
        {
            if (conn.removeOriginal)
            {
                // Object.Destroy(conn.gameObject);
                foreach (Transform child in conn.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
    }
}