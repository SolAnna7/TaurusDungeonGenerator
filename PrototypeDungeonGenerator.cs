#if true
//#define DEBUG_LOG
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.GenerationModel;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
using UnityEngine;
using Random = System.Random;
using Room = SnowFlakeGamesAssets.TaurusDungeonGenerator.Component.Room;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator
{
    public class PrototypeDungeonGenerator
    {
        private BoundsOctree<RoomPrototype> _virtualSpace;
//        private readonly List<RoomWrapper> _connactableRooms = new List<RoomWrapper>();

        private readonly DungeonStructure _loadedStructure;

        private readonly Dictionary<string, RoomCollection> _loadedRooms;

        private readonly Stack<GenerationStackItem> _connectionStack;

        private readonly GenerationParameters _generationParameters;

        public int _retryNum = 10;

        private const int maxSteps = 10000;
        private int _stepsMade;

        private Random _random;
        private int _seed = 0;

        public PrototypeDungeonGenerator(AbstractDungeonStructure structure, int seed, GenerationParameters generationParameters = null)
        {
            _seed = seed;
            _random = new Random(seed);
            _generationParameters = generationParameters;
            _loadedStructure = DungeonStructureConcretizer.ConcretizeStructure(structure, _random);
            _loadedRooms = RoomResourceLoader.LoadRoomPrototypes(_loadedStructure);
            CollectMetaData(_loadedStructure, _loadedRooms);

            if (_generationParameters != null)
                ParameterizeDungeon();
        }

        public PrototypeDungeon BuildPrototype()
        {
            InitVirtualSpace();

            DungeonNode firstElement = _loadedStructure.StartElement;
            var firstRoom = GetRandomRoom(firstElement);
            var firstRoomWrapper = new RoomPrototype(
                firstElement,
                firstRoom,
                Vector3.zero,
                Quaternion.identity);

            firstRoomWrapper.ConnectToParent(null, null);

            if (!TryAddRoomToVirtualSpace(firstRoomWrapper, null))
            {
                throw new Exception("Could not place first element!");
            }

//            if (!BuildPrototypeRoomRecur(firstRoomWrapper))
//            {
//                throw new Exception("Dungeon could not be built!");
//            }

            TryCreateDungeonStructure(firstRoomWrapper);

            CreateBranches(firstRoomWrapper);

            CloseOpenConnections(firstRoomWrapper);

            return new PrototypeDungeon(firstRoomWrapper, _loadedStructure);
        }

        private void CreateBranches(RoomPrototype firstRoomWrapper)
        {
            var branchDataWrapper = _loadedStructure.StructureMetaData.BranchDataWrapper;

            if (branchDataWrapper == null)
                return;

            var branchPrototypeNames = branchDataWrapper.BranchPrototypeNames;
            var openConnections = new Stack<RoomPrototypeConnection>(CollectOpenConnections(firstRoomWrapper).Shuffle(_random));

            int remainingBranchNum;

            if (branchDataWrapper.BranchCount.HasValue)
            {
                remainingBranchNum = branchDataWrapper.BranchCount.Value;
            }
            else if (branchDataWrapper.BranchPercentage.HasValue)
            {
                remainingBranchNum = (int) (branchDataWrapper.BranchPercentage.Value * openConnections.Count / 100);
            }
            else return;

            int extremeCntr = 100;

            while (openConnections.Count > 0 && remainingBranchNum > 0 && extremeCntr > 0)
            {
                var selectedBranchType = branchPrototypeNames.AnyRandom(_random);
                AbstractDungeonStructure embeddedBranchDungeon = _loadedStructure.AbstractStructure.EmbeddedDungeons[selectedBranchType];
                DungeonNode concretizedDungeonBranch = DungeonStructureConcretizer.ConcretizeDungeonTree(embeddedBranchDungeon.StartElement, _random,
                    new ReadOnlyDictionary<string, AbstractDungeonStructure>(_loadedStructure.AbstractStructure.EmbeddedDungeons));

                var connection = openConnections.Pop();

                if (TryAddRoomToConnection(connection.ParentRoomPrototype, concretizedDungeonBranch, connection, new List<RoomPrototypeConnection>()))
                {
                    remainingBranchNum--;
                    connection.ParentRoomPrototype.ActualGraphElement.AddSubElement(concretizedDungeonBranch);
                    concretizedDungeonBranch.TraverseTopToDown().ForEach(n => n.Tags.Add(new Tag("BRANCH")));
                }

                extremeCntr--;
            }
        }

        private void ParameterizeDungeon()
        {
            int requiredTransitNum = _generationParameters.RequiredTransitNumber;

            if (requiredTransitNum >= 0)
            {
                var maxTransitNum = _loadedStructure.StructureMetaData.SubTransitNum;
                var minTransitNum = maxTransitNum - _loadedStructure.StructureMetaData.ChildOptionalNodes.Sum(x => x.StructureMetaData.SubTransitNum);

                if (maxTransitNum < requiredTransitNum)
                {
                    throw new Exception($"Dungeon cannot be built with parameters. RequiredTransitNum({requiredTransitNum}) > MaxTransitNum({maxTransitNum})");
                }

                if (minTransitNum > requiredTransitNum)
                {
                    throw new Exception($"Dungeon cannot be built with parameters. RequiredTransitNum({requiredTransitNum}) > MinTransitNum({maxTransitNum})");
                }

                var remainingOptionalsToDisable = maxTransitNum - (requiredTransitNum - minTransitNum);

                //todo: ez az algoritmus több szintű optional-ok használatakor nem helyes, mert csak a felső szintet veszi figyelembe
                var topLevelOptionalNodes = _loadedStructure.StructureMetaData.ChildOptionalNodes.ToList().OrderByDescending(x => x.StructureMetaData.SubTransitNum).ToList();

                foreach (var optionalNode in topLevelOptionalNodes)
                {
                    if (remainingOptionalsToDisable > 0 && optionalNode.StructureMetaData.SubTransitNum <= remainingOptionalsToDisable)
                    {
                        optionalNode.StructureMetaData.OptionalNodeData.Required = false;
                        remainingOptionalsToDisable -= optionalNode.StructureMetaData.SubTransitNum;
                    }
                }

                if (remainingOptionalsToDisable != 0)
                {
                    throw new Exception("Dungeon cannot be built with parameters. Optional structure cannot be optimized.");
                }
            }
        }

        private void InitVirtualSpace()
        {
            _virtualSpace = new BoundsOctree<RoomPrototype>(15, Vector3.zero, 1, 1.25f);
        }

        private void TryCreateDungeonStructure(RoomPrototype firstRoomWrapper)
        {
            int i = _retryNum;
            do
            {
                try
                {
                    if (BuildPrototypeRoomRecur(firstRoomWrapper))
                        return;

                    Debug.LogWarning($"Dungeon building failed with seed {_seed}!");
                }
                catch (MaxStepsReachedException e)
                {
                    Debug.LogWarning(e.Message);
                }

                if (i == 0)
                    break;

                firstRoomWrapper.ChildRoomConnections.Where(x => x.ChildRoomPrototype != null).ForEach(x =>
                {
                    RecursiveRemoveRoomAndChildren(x.ChildRoomPrototype);
                    x.ClearChild();
                });
                _seed = _seed + 37 * (1 + _seed);
                _random = new Random(_seed);
                _stepsMade = 0;
                Debug.Log($"Retrying with seed {_seed}");

                i--;
            } while (i > 0);

            throw new Exception("Dungeon building failed!");
        }


        //nem rekurzív generálási kísérlet, teszteletlen
        [Obsolete]
        private void BuildConnection()
        {
            _stepsMade++;
            if (_stepsMade > maxSteps)
            {
                throw new MaxStepsReachedException(_stepsMade);
            }

            var generationItem = _connectionStack.Pop();
            var connection = generationItem.Connection;
            var nextNode = generationItem.Node;

            //már van szoba a végén
            if (connection.ChildRoomPrototype != null)
            {
                RoomPrototype baseRoom = connection.ChildRoomPrototype;

                var childConnections = baseRoom.ChildRoomConnections;

                //ha kevesebbet sikerült összekötni, mint amennyit kellett volna, újrakezdjük másik szobával
                if (childConnections.Count(c => c.State == ConnectionState.CONNECTED) < nextNode.SubElements.Count)
                {
                    connection.ClearChild();
                    _connectionStack.Push(generationItem);
                }

                return;
            }

            //új conn
            if (connection.ChildRoomPrototype == null)
            {
                if (generationItem.Rooms == null)
                {
                    generationItem.Rooms = new Stack<Room>(GetRandomRooms(nextNode));
                }

                var possibleRooms = generationItem.Rooms;

                //minden lehetőséget kimerítettünk, nem sikerült megépíteni a szobát
                if (possibleRooms.Count == 0)
                {
                    connection.Fail();
                    //todo: ha már nem lehet létrehozni megfelelő kapcsolatot, akkor a testvéreket kiírtani a stack-ről
                    return;
                }

                var selectedRoom = possibleRooms.Pop();
                RoomPrototype baseRoom = connection.ParentRoomPrototype;

                IEnumerable<RoomConnector> possibleNextConnections = selectedRoom
                    .GetConnections()
                    .Where(c => c.size.Equals(connection.ParentConnection.size) && c.type == connection.ParentConnection.type).ToList();


                foreach (var nextRoomConnection in possibleNextConnections)
                {
                    var nextRoomConnectionTransform = nextRoomConnection.transform;
                    var selectedConnectionTransform = connection.ParentConnection.transform;

                    Vector3 newRoomPosition;
                    Quaternion rotationDiff;
                    GetNewRoomPosAndRot(baseRoom.GlobalPosition, baseRoom.Rotation, selectedConnectionTransform, nextRoomConnectionTransform, out newRoomPosition, out rotationDiff);

                    var nextRoomWrapper = new RoomPrototype(
                        nextNode,
                        selectedRoom,
                        newRoomPosition,
                        rotationDiff
                    );

                    if (!TryAddRoomToVirtualSpace(nextRoomWrapper, baseRoom))
                    {
                        //nem sikerült elhelyezni az adott szobát az adott kapcsolattal
                    }
                    else
                    {
                        nextRoomWrapper.ConnectToParent(nextRoomConnection, connection);
                        _connectionStack.Push(generationItem);


                        var builtRoomChildConnections = nextRoomWrapper.ChildRoomConnections.ToList();
                        builtRoomChildConnections.Shuffle(_random);

                        for (var i = 0; i < nextNode.SubElements.Count; i++)
                        {
                            _connectionStack.Push(new GenerationStackItem(builtRoomChildConnections[i], nextNode.SubElements[i]));
                        }
                    }
                }
            }
        }

        private bool BuildPrototypeRoomRecur(RoomPrototype room)
        {
#if DEBUG_LOG
            Debug.Log($"Room {room.RoomResource} is built");
#endif

            DungeonNode actualGraphElement = room.ActualGraphElement;

            if (actualGraphElement.IsEndNode)
                return true;

            var subElements = actualGraphElement.SubElements.Where(s => s.StructureMetaData.OptionalNodeData == null || s.StructureMetaData.OptionalNodeData.Required).ToList();
            for (int connectionsToMake = subElements.Count; connectionsToMake > 0; connectionsToMake--)
            {
                List<RoomPrototypeConnection> availableConnections = room.ChildRoomConnections.Where(x => x.State == ConnectionState.FREE).ToList();
                availableConnections.Shuffle(_random);

                if (availableConnections.Count < connectionsToMake)
                {
                    // már nem lehet sikeres az építés
                    Debug.LogWarning($"Room {room.RoomResource} is failed");
                    RecursiveRemoveRoomAndChildren(room);
                    room.ParentRoomConnection?.ClearChild();
                    return RoomCreationFailed();
                }

                DungeonNode nextElement = subElements[connectionsToMake - 1];

                bool connectionMade = false;

                foreach (var selectedConnection in availableConnections)
                {
                    if (TryAddRoomToConnection(room, nextElement, selectedConnection, availableConnections))
                    {
                        connectionMade = true;
                        break;
                    }
                }

                if (!connectionMade)
                {
#if DEBUG_LOG
                    Debug.LogWarning($"Failed to make connection {nextElement}[{nextElement.Style}] in room {room.RoomResource} is failed");
#endif
                    RecursiveRemoveRoomAndChildren(room);
                    room.ParentRoomConnection?.ClearChild();
                    return RoomCreationFailed();
                }
            }

            return true;
        }

        private bool RoomCreationFailed()
        {
            _stepsMade++;
            if (_stepsMade > maxSteps)
            {
                throw new MaxStepsReachedException(_stepsMade);
            }

            return false;
        }


        private bool TryAddRoomToConnection(RoomPrototype room, DungeonNode nextElement, RoomPrototypeConnection selectedConnection, List<RoomPrototypeConnection> availableConnections)
        {
            var possibleRooms = GetRandomRooms(nextElement);

            foreach (var selectedRoom in possibleRooms)
            {
                IEnumerable<RoomConnector> possibleNextConnections = selectedRoom
                    .GetConnections()
                    .Where(x => x.size.Equals(selectedConnection.ParentConnection.size) && x.type == selectedConnection.ParentConnection.type).ToList();

                foreach (var nextRoomConnection in possibleNextConnections)
                {
                    var nextRoomConnectionTransform = nextRoomConnection.transform;
                    var selectedConnectionTransform = selectedConnection.ParentConnection.transform;

                    Vector3 newRoomPosition;
                    Quaternion rotationDiff;
                    GetNewRoomPosAndRot(room.GlobalPosition, room.Rotation, selectedConnectionTransform, nextRoomConnectionTransform, out newRoomPosition, out rotationDiff);

                    var nextRoomWrapper = new RoomPrototype(
                        nextElement,
                        selectedRoom,
                        newRoomPosition,
                        rotationDiff
                    );

                    if (!TryAddRoomToVirtualSpace(nextRoomWrapper, room))
                    {
                        continue;
                    }

                    nextRoomWrapper.ConnectToParent(nextRoomConnection, selectedConnection);

                    if (!BuildPrototypeRoomRecur(nextRoomWrapper)) continue;

                    availableConnections.Remove(selectedConnection);
                    return true;
                }
            }

            return false;
        }

        private static void GetNewRoomPosAndRot(
            Vector3 roomGlobalPosition,
            Quaternion roomRotation,
            Transform selectedConnectionTransform,
            Transform nextRoomConnectionTransform,
            out Vector3 newRoomPosition,
            out Quaternion rotationDiff)
        {
            var selectedConnectionTransformBackwards = selectedConnectionTransform.rotation * Quaternion.AngleAxis(180, selectedConnectionTransform.up);

            rotationDiff = roomRotation * Quaternion.Inverse(nextRoomConnectionTransform.rotation * Quaternion.Inverse(selectedConnectionTransformBackwards));

            // for vertical connections. This is pretty ugly this way...
            if (rotationDiff * nextRoomConnectionTransform.up != roomRotation * selectedConnectionTransform.up)
            {
                rotationDiff *= Quaternion.AngleAxis(180, selectedConnectionTransform.right) * Quaternion.AngleAxis(180, selectedConnectionTransform.forward);
            }

            var nextRoom = GetParentRoom(nextRoomConnectionTransform);
            var nextRoomPosition = nextRoom.transform.position;
            var newConnGlobalPos = nextRoomPosition - rotationDiff * nextRoom.transform.InverseTransformPoint(nextRoomConnectionTransform.position);
            var connPosDifference = roomRotation * selectedConnectionTransform.position + newConnGlobalPos;
            newRoomPosition = roomGlobalPosition + connPosDifference;
        }

        private static Room GetParentRoom(Transform nextRoomConnectionTransform)
        {
            Transform actualTransform = nextRoomConnectionTransform.parent;
            for (int i = 0; i < 10; i++)
            {
                var room = actualTransform.GetComponent<Room>();
                if (room != null)
                    return room;
                actualTransform = actualTransform.parent;
            }

            throw new Exception("No Room parent found in ancestors");
        }


        private void RecursiveRemoveRoomAndChildren(RoomPrototype roomToDelete)
        {
            _virtualSpace.Remove(roomToDelete);
            foreach (var childRoom in roomToDelete.ChildRoomConnections.Where(x => x.ChildRoomPrototype != null).Select(x => x.ChildRoomPrototype))
            {
                RecursiveRemoveRoomAndChildren(childRoom);
            }
#if DEBUG_LOG
            Debug.LogWarning($"Removed room {roomToDelete.RoomResource.name}");
#endif
        }


        private bool TryAddRoomToVirtualSpace(RoomPrototype room, RoomPrototype parentRoom)
        {
            var bounds = room.GetRotatedBounds();

            List<RoomPrototype> collidingWith = new List<RoomPrototype>();
            _virtualSpace.GetColliding(collidingWith, bounds);
            if (collidingWith.Count > 1 || collidingWith.Count == 1 && collidingWith[0] != parentRoom)
            {
                return false;
            }

            _virtualSpace.Add(room, bounds);

            return true;
        }

        private Room GetRandomRoom(DungeonNode element)
        {
            var roomCollection = _loadedRooms[element.Style];
            //todo súlyozás
            return roomCollection.rooms[_random.Next(roomCollection.rooms.Count)];
        }

        private List<Room> GetRandomRooms(DungeonNode element)
        {
            var rooms = _loadedRooms[element.Style].rooms.ToList();
            rooms.Shuffle(_random);
            return rooms;
        }

        void CloseOpenConnections(RoomPrototype roomPrototype)
        {
            foreach (var roomPrototypeConnection in CollectOpenConnections(roomPrototype))
            {
                roomPrototypeConnection.Close();
            }
        }

        private List<RoomPrototypeConnection> CollectOpenConnections(RoomPrototype roomPrototype)
        {
            List<RoomPrototypeConnection> result = new List<RoomPrototypeConnection>();
            CollectOpenConnectionsRecursively(roomPrototype, result);
            return result;
        }

        private void CollectOpenConnectionsRecursively(RoomPrototype roomPrototype, List<RoomPrototypeConnection> resultCollector)
        {
            foreach (var childRoomConnection in roomPrototype.ChildRoomConnections)
            {
                if (childRoomConnection.State == ConnectionState.FREE)
                {
                    resultCollector.Add(childRoomConnection);
                }
                else if (childRoomConnection.State == ConnectionState.CONNECTED)
                {
                    CollectOpenConnectionsRecursively(childRoomConnection.ChildRoomPrototype, resultCollector);
                }
                else
                {
                    throw new Exception("This should not happen");
                }
            }
        }

        public class MaxStepsReachedException : Exception
        {
            public MaxStepsReachedException(int stepNum) : base($"Generation took too many steps {stepNum}")
            {
            }
        }

        private struct GenerationStackItem
        {
            public GenerationStackItem(RoomPrototypeConnection connection, DungeonNode node)
            {
                Connection = connection;
                Node = node;
                Rooms = null;
            }

            public RoomPrototypeConnection Connection;
            public DungeonNode Node;
            public Stack<Room> Rooms;
        }

        public void Draw()
        {
//            _virtualSpace.DrawAllBounds();
            _virtualSpace.DrawAllObjects();
//            _virtualSpace.DrawCollisionChecks();
        }

        public static void CollectMetaData(DungeonStructure dungeonStructure, Dictionary<string, RoomCollection> roomsByPath)
        {
            CollectMetaData(dungeonStructure.StartElement, roomsByPath);
        }

        private static StructureMetaData CollectMetaData(DungeonNode dungeonElement, Dictionary<string, RoomCollection> roomsByPath)
        {
            IList<StructureMetaData> subMetaDataList = dungeonElement.SubElements.Select(s => CollectMetaData(s, roomsByPath)).ToList();

            dungeonElement.StructureMetaData.SubTransitNum = subMetaDataList.Aggregate(dungeonElement.StructureMetaData.IsTransit ? 1 : 0, (sum, e) => sum + e.SubTransitNum);
            dungeonElement.StructureMetaData.ChildOptionalNodes = dungeonElement.SubElements.SelectMany(e =>
            {
                var m = e.StructureMetaData;
                return m.OptionalNodeData != null ? new List<DungeonNode> {e} : m.ChildOptionalNodes;
            }).ToList();

            return dungeonElement.StructureMetaData;
        }

        public class GenerationParameters
        {
            public int RequiredTransitNumber { get; set; } = -1;
        }
    }
}