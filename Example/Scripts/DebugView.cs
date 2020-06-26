using System;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
using UnityEngine;

namespace TaurusDungeonGenerator.Example.Scripts
{
    public class DebugView : MonoBehaviour
    {
        public bool drawDebugView = false;

        public DungeonDemoRoot dungeonDemoRoot;

        private GameObject _debugStructure;

        private void Awake()
        {
            dungeonDemoRoot.OnDungeonRebuilt += GenerateDebugStructure;
        }

        public void ToggleDebugView()
        {
            drawDebugView = !drawDebugView;
            _debugStructure.SetActive(drawDebugView);
        }

        private void GenerateDebugStructure(DungeonStructure dungeonStructure)
        {
            Destroy(_debugStructure);
            _debugStructure = new GameObject("DebugStructure");
            DungeonDebugger.GenerateDungeonDebugStructure(dungeonStructure, _debugStructure.transform);
            _debugStructure.SetActive(drawDebugView);
        }
    }
}