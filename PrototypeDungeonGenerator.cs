#if false
#define TAURUS_DEBUG_LOG
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Component;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.GenerationHelper;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.GenerationModel;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
using UnityEngine;
using Random = System.Random;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator
{
    /// <summary>
    /// The main dungeon generation class
    /// </summary>
    public class PrototypeDungeonGenerator
    {
        // ReSharper disable once InconsistentNaming
        public static string MAIN_TAG => "#MAIN";

        // ReSharper disable once InconsistentNaming
        public static string BRANCH_TAG => "#BRANCH";

        private BoundsOctree<RoomPrototype> _virtualSpace;

        private readonly DungeonStructure _loadedStructure;

        private readonly Dictionary<string, RoomCollection> _loadedRooms;

        private readonly GenerationParameters _generationParameters;

        private uint _retryNum = 10;

        private const uint DefaultMaxSteps = 512;

        private StepBackCounter _stepBackCounter;

        private Random _random;
        private int _seed = 0;

        private float _marginHalf = 0f;

        public PrototypeDungeonGenerator(AbstractDungeonStructure structure, int seed, GenerationParameters generationParameters = null)
        {
            _seed = seed;
            _random = new Random(seed);
            _generationParameters = generationParameters;
            _loadedStructure = DungeonStructureConcretizer.ConcretizeStructure(structure, _random);
            _loadedRooms = RoomResourceLoader.LoadRoomPrototypes(_loadedStructure);
            _marginHalf = structure.StructureMetaData.MarginUnit / 2f;
            CollectMetaData(_loadedStructure);

            if (_generationParameters != null)
                ParameterizeDungeon();

            if (_stepBackCounter == null)
                _stepBackCounter = new StepBackCounter(DefaultMaxSteps);
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

            TryCreateDungeonStructure(firstRoomWrapper);

            firstRoomWrapper.ActualGraphElement.MetaData.AddTag("ROOT");
            firstRoomWrapper.ActualGraphElement.TraverseDepthFirst().ForEach(n => n.MetaData.AddTag(MAIN_TAG));

            CreateBranches(firstRoomWrapper);

            CloseOpenConnections(firstRoomWrapper);

            return new PrototypeDungeon(firstRoomWrapper, _loadedStructure);
        }

        private void InitVirtualSpace()
        {
            _virtualSpace = new BoundsOctree<RoomPrototype>(15, Vector3.zero, 1, 1.25f);
        }

        private void ParameterizeDungeon()
        {
            if (_generationParameters.RequiredOptionalEndpointNumber.HasValue)
                PrepareOptionalRoutes();

            if (_generationParameters.GenerationMaxDeadEnds.HasValue)
                _stepBackCounter = new StepBackCounter(_generationParameters.GenerationMaxDeadEnds.Value);
            if (_generationParameters.GenerationRetryNum.HasValue)
                _retryNum = _generationParameters.GenerationRetryNum.Value;
        }

        private void PrepareOptionalRoutes()
        {
            System.Diagnostics.Debug.Assert(_generationParameters.RequiredOptionalEndpointNumber != null, "_generationParameters.RequiredOptionalEndpointNumber != null");
            uint requiredEndpointNum = _generationParameters.RequiredOptionalEndpointNumber.Value;

            var maxEndpointNum = GetGenMetaData(_loadedStructure.NodeMetaData).OptionalEndpointNum;
            var minEndpointNum = maxEndpointNum - _loadedStructure.NodeMetaData.ChildOptionalNodes.Sum(x => GetGenMetaData(x.MetaData).OptionalEndpointNum);

            if (maxEndpointNum < requiredEndpointNum)
            {
                throw new Exception($"Dungeon cannot be built with parameters. RequiredOptionalEndpointNum({requiredEndpointNum}) > MaxEndpointNum({maxEndpointNum})");
            }

            if (minEndpointNum > requiredEndpointNum)
            {
                throw new Exception($"Dungeon cannot be built with parameters. RequiredOptionalEndpointNum({requiredEndpointNum}) > MinEndpointNum({maxEndpointNum})");
            }

            // var remainingOptionalsToDisable = maxEndpointNum - (requiredEndpointNum - minEndpointNum);
            var remainingOptionalsToDisable = maxEndpointNum - requiredEndpointNum;

            //todo: this algorithm will not work with multiple level options, only top level
            var topLevelOptionalNodes = _loadedStructure.NodeMetaData.ChildOptionalNodes.ToList().OrderByDescending(x => GetGenMetaData(x.MetaData).OptionalEndpointNum).ToList();

            foreach (var optionalNode in topLevelOptionalNodes)
            {
                if (remainingOptionalsToDisable > 0 && GetGenMetaData(optionalNode.MetaData).OptionalEndpointNum <= remainingOptionalsToDisable)
                {
                    GetGenMetaData(optionalNode.MetaData).NodeRequired = false;
                    remainingOptionalsToDisable -= GetGenMetaData(optionalNode.MetaData).OptionalEndpointNum;
                }
            }

            if (remainingOptionalsToDisable != 0)
            {
                throw new Exception("Dungeon cannot be built with parameters. Optional structure cannot be optimized.");
            }

            //remove not required optional nodes
            RemoveUnneededOptionals();
        }

        private void RemoveUnneededOptionals()
        {
            List<Tuple<DungeonNode, DungeonNode>> nodesToRemove = new List<Tuple<DungeonNode, DungeonNode>>();

            foreach (var dungeonNode in _loadedStructure.StartElement.TraverseDepthFirst())
            {
                foreach (var dungeonNodeChild in dungeonNode.ChildNodes)
                {
                    if (!GetGenMetaData(dungeonNodeChild.Value.MetaData).NodeRequired)
                    {
                        nodesToRemove.Add(new Tuple<DungeonNode, DungeonNode>(dungeonNode.Value, dungeonNodeChild.Value));
                    }
                }
            }

            foreach (var tuple in nodesToRemove)
            {
                tuple.Item1.RemoveSubElement(tuple.Item2);
            }
        }

        private void TryCreateDungeonStructure(RoomPrototype firstRoomWrapper)
        {
            uint i = _retryNum;
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

                firstRoomWrapper.ChildRoomConnections.Where(x => x.ChildRoomPrototype != null).ForEach(connectionToRemove =>
                {
                    RemoveRoomAndChildrenRecur(connectionToRemove.ChildRoomPrototype);
                    connectionToRemove.ClearChild();
                });
                _seed += 37 * (1 + _seed);
                _random = new Random(_seed);
                _stepBackCounter.ResetMain();
                Debug.Log($"Retrying with seed {_seed}");

                i--;
            } while (i > 0);

            throw new Exception("Dungeon building failed!");
        }

        private void CreateBranches(RoomPrototype firstRoomWrapper)
        {
            var branchDataWrapper = _loadedStructure.NodeMetaData.BranchDataWrapper;

            if (branchDataWrapper == null)
                return;

            var branchPrototypeNames = branchDataWrapper.BranchPrototypeNames;
            var openConnections = new Stack<RoomPrototypeConnection>(CollectOpenConnections(firstRoomWrapper).Shuffle(_random));

            uint remainingBranchNum;

            if (branchDataWrapper.BranchCount.HasValue)
            {
                remainingBranchNum = branchDataWrapper.BranchCount.Value;
            }
            else if (branchDataWrapper.BranchPercentage.HasValue)
            {
                remainingBranchNum = (uint) (branchDataWrapper.BranchPercentage.Value * openConnections.Count / 100);
            }
            else return;

            int extremeCntr = 100;
            var embeddedDungeons = new ReadOnlyDictionary<string, AbstractDungeonStructure>(_loadedStructure.AbstractStructure.EmbeddedDungeons);

            while (openConnections.Count > 0 && remainingBranchNum > 0 && extremeCntr > 0)
            {
                var connection = openConnections.Pop();
                foreach (var selectedBranchType in branchPrototypeNames.Shuffle(_random))
                {
                    AbstractDungeonStructure embeddedBranchDungeon = _loadedStructure.AbstractStructure.EmbeddedDungeons[selectedBranchType];
                    DungeonNode concretizedDungeonBranch = DungeonStructureConcretizer.ConcretizeDungeonTree(
                        embeddedBranchDungeon.StartElement,
                        _random,
                        embeddedDungeons);

                    _stepBackCounter.ResetBranch();

                    try
                    {
                        if (GetPossibleRoomsForConnection(connection, concretizedDungeonBranch).Any(BuildPrototypeRoomRecur))
                        {
                            remainingBranchNum--;
                            connection.ParentRoomPrototype.ActualGraphElement.AddSubElement(concretizedDungeonBranch);
                            concretizedDungeonBranch.TraverseDepthFirst().ForEach(n => n.MetaData.AddTag(BRANCH_TAG));
                            break;
                        }
                    }
                    catch (MaxStepsReachedException e)
                    {
                        Debug.LogWarning(e.Message);
                    }
                }

                extremeCntr--;
            }
        }

        private bool BuildPrototypeRoomRecur(RoomPrototype room)
        {
#if TAURUS_DEBUG_LOG
            Debug.Log($"Room {room.RoomResource} is built");
#endif
            DungeonNode actualGraphElement = room.ActualGraphElement;

            if (actualGraphElement.IsEndNode)
                return true;

            var subElements = actualGraphElement.SubElements.Where(sub => GetGenMetaData(sub.MetaData).NodeRequired).ToList();

            if (room.ChildRoomConnections.Count < actualGraphElement.ChildNodes.Count())
            {
                throw new Exception($"Room {room.RoomResource.name} has {room.ChildRoomConnections.Count} connections, yet the graph element {actualGraphElement.Style} requires {actualGraphElement.ChildNodes.Count()}");
            }

            for (int connectionsToMake = subElements.Count; connectionsToMake > 0; connectionsToMake--)
            {
                IList<RoomPrototypeConnection> availableConnections = room.ChildRoomConnections
                    .Where(x => x.State == PrototypeConnectionState.FREE)
                    .ToList()
                    .Shuffle(_random);

                DungeonNode nextStructureElement = subElements[connectionsToMake - 1];

                bool failed = availableConnections.Count < connectionsToMake;
                RoomPrototypeConnection successfulConnection = null;
                if (!failed)
                {
                    foreach (var selectedConnection in availableConnections)
                    {
                        if (GetPossibleRoomsForConnection(selectedConnection, nextStructureElement).Any(BuildPrototypeRoomRecur))
                        {
                            successfulConnection = selectedConnection;
                            break;
                        }
                    }

                    if (successfulConnection == null)
                    {
                        failed = true;
                    }
                }

                // building cannot be successful
                if (failed)
                {
#if TAURUS_DEBUG_LOG
                    Debug.LogWarning($"Failed to make room {room} of element {actualGraphElement.Style}");
#endif
                    RemoveRoomAndChildrenRecur(room);
                    room.ParentRoomConnection?.ClearChild();
                    _stepBackCounter.StepBack();
                    return false;
                }
                else
                {
                    availableConnections.Remove(successfulConnection);
                }
            }

            return true;
        }

        private IEnumerable<RoomPrototype> GetPossibleRoomsForConnection(RoomPrototypeConnection baseConnection, DungeonNode nextStructureElement)
        {
            var baseRoom = baseConnection.ParentRoomPrototype;

            foreach (var newRoom in GetRandomOrderedRooms(nextStructureElement))
            {
                IEnumerable<RoomConnector> possibleNextConnections = newRoom
                    .GetConnections()
                    .Where(x => x.size.Equals(baseConnection.ParentConnection.size) && x.type == baseConnection.ParentConnection.type)
                    .ToList().Shuffle(_random);

                foreach (var nextRoomConnection in possibleNextConnections)
                {
                    var nextRoomConnectionTransform = nextRoomConnection.transform;
                    var selectedConnectionTransform = baseConnection.ParentConnection.transform;

                    GetNewRoomPosAndRot(baseRoom.GlobalPosition, baseRoom.Rotation, selectedConnectionTransform, nextRoomConnectionTransform, out var newRoomPosition, out var rotationDiff);

                    var nextRoomWrapper = new RoomPrototype(nextStructureElement, newRoom, newRoomPosition, rotationDiff);

                    if (!TryAddRoomToVirtualSpace(nextRoomWrapper, baseRoom))
                    {
                        continue;
                    }

                    nextRoomWrapper.ConnectToParent(nextRoomConnection, baseConnection);

                    yield return nextRoomWrapper;
                }
            }
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
                rotationDiff =
                    rotationDiff
                    * Quaternion.AngleAxis(180, selectedConnectionTransform.right)
                    // * Quaternion.AngleAxis(180, selectedConnectionTransform.forward)
                    // * Quaternion.AngleAxis(180, selectedConnectionTransform.up)
                    ;
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


        private void RemoveRoomAndChildrenRecur(RoomPrototype roomToDelete)
        {
            _virtualSpace.Remove(roomToDelete);
            foreach (var childRoom in roomToDelete.ChildRoomConnections.Where(x => x.ChildRoomPrototype != null).Select(x => x.ChildRoomPrototype))
            {
                RemoveRoomAndChildrenRecur(childRoom);
            }
#if DEBUG_LOG
            Debug.LogWarning($"Removed room {roomToDelete.RoomResource.name}");
#endif
        }


        private bool TryAddRoomToVirtualSpace(RoomPrototype room, RoomPrototype parentRoom)
        {
            var bounds = room.GetRotatedBounds();
            bounds.extents += new Vector3(_marginHalf, _marginHalf, _marginHalf);

            List<RoomPrototype> collidingWith = new List<RoomPrototype>();
            _virtualSpace.GetColliding(collidingWith, bounds);
            if (_marginHalf <= 0.01)
            {
                if (collidingWith.Count > 1 || collidingWith.Count == 1 && collidingWith[0] != parentRoom)
                {
                    return false;
                }
            }
            else
            {
                if (collidingWith.Count > 2)
                    return false;

                //trying without margins
                bounds = room.GetRotatedBounds();
                collidingWith.Clear();
                _virtualSpace.GetColliding(collidingWith, bounds);
                if (collidingWith.Count > 1 || collidingWith.Count == 1 && collidingWith[0] != parentRoom)
                {
                    return false;
                }
            }

            _virtualSpace.Add(room, bounds);

            return true;
        }

        private Room GetRandomRoom(DungeonNode element)
        {
            var roomCollection = _loadedRooms[element.Style];
            //todo weighting
            return roomCollection.rooms[_random.Next(roomCollection.rooms.Count)];
        }

        private IEnumerable<Room> GetRandomOrderedRooms(DungeonNode element) => _loadedRooms[element.Style].rooms.ToList().Shuffle(_random);

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

        private static void CollectOpenConnectionsRecursively(RoomPrototype roomPrototype, List<RoomPrototypeConnection> resultCollector)
        {
            foreach (var childRoomConnection in roomPrototype.ChildRoomConnections)
            {
                if (childRoomConnection.State == PrototypeConnectionState.FREE)
                {
                    resultCollector.Add(childRoomConnection);
                }
                else if (childRoomConnection.State == PrototypeConnectionState.CONNECTED)
                {
                    CollectOpenConnectionsRecursively(childRoomConnection.ChildRoomPrototype, resultCollector);
                }
                else
                {
                    throw new Exception("This should not happen");
                }
            }
        }

        private static void CollectMetaData(DungeonStructure dungeonStructure)
        {
            CollectMetaData(dungeonStructure.StartElement);
        }

        private static NodeMetaData CollectMetaData(DungeonNode dungeonElement)
        {
            IList<NodeMetaData> subMetaDataList = dungeonElement.SubElements.Select(CollectMetaData).ToList();

            GetGenMetaData(dungeonElement.MetaData).OptionalEndpointNum =
                subMetaDataList.Aggregate(dungeonElement.MetaData.OptionalEndpoint ? 1 : 0, (sum, e) => sum + GetGenMetaData(e).OptionalEndpointNum);
            dungeonElement.MetaData.ChildOptionalNodes = dungeonElement.SubElements.SelectMany(e =>
            {
                var m = e.MetaData;
                return m.OptionalNode ? new List<DungeonNode> {e} : m.ChildOptionalNodes;
            }).ToList();
            return dungeonElement.MetaData;
        }


        private static GenerationMetaData GetGenMetaData(IPropertyHolder holder)
        {
            if (!holder.HasProperty("#GENERATION_META_DATA"))
            {
                holder.AddProperty("#GENERATION_META_DATA", new GenerationMetaData());
            }

            return holder.GetPropertyAs<GenerationMetaData>("#GENERATION_META_DATA");
        }

        private class GenerationMetaData
        {
            public int OptionalEndpointNum { get; set; } = 0;
            public bool NodeRequired { get; set; } = true;
        }

        /// <inheritdoc />
        private class MaxStepsReachedException : Exception
        {
            public MaxStepsReachedException(string message) : base(message)
            {
            }
        }

        private class StepBackCounter
        {
            private readonly uint _maxStep;
            private uint _actualStepCntr;
            private bool _main = true;

            public StepBackCounter(uint maxStep)
            {
                _maxStep = maxStep;
                ResetMain();
            }

            public void ResetMain()
            {
                _actualStepCntr = _maxStep;
                _main = true;
            }

            public void ResetBranch()
            {
                _actualStepCntr = _maxStep / 2;
                _main = false;
            }

            public void StepBack()
            {
                if (_actualStepCntr == 0)
                {
                    string msg = _main ? $"Main path generation took too many steps {_maxStep}" : $"Branch generation took too many steps {_maxStep}";
                    throw new MaxStepsReachedException(msg);
                }

                _actualStepCntr--;
            }
        }

        /// <summary>
        /// Extra dungeon generation settings
        /// </summary>
        public class GenerationParameters
        {
            /// <summary>
            /// The number of optional endpoints this dungeon should built with
            /// Unset if null
            /// </summary>
            public uint? RequiredOptionalEndpointNumber { get; set; } = null;

            public Dictionary<string, int> RequiredOptionals { get; set; } = null;

            /// <summary>
            /// If the generation fails with the initial seed, the generation process will be retried this many times with a new seed (calculated from the original one)
            /// </summary>
            public uint? GenerationRetryNum { get; set; }

            /// <summary>
            /// The maximum number of dead end back steps before failing the generation
            /// </summary>
            public uint? GenerationMaxDeadEnds { get; set; }
        }
    }
}