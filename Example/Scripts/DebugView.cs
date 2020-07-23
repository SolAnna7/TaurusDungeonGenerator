using System;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Structure;
using SnowFlakeGamesAssets.TaurusDungeonGenerator.Utils;
using UnityEditor;
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

        private void OnGUI()
        {
            if (drawDebugView)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
                {
                    Transform objectHit = hit.transform;

                    string text = DungeonDebugger.TryGetSummaryTextForObject(objectHit.gameObject);

                    if (text != null)
                    {
                        var mousePosition = Input.mousePosition;
                        GUI.Label(new Rect(mousePosition.x + 15, Screen.height - mousePosition.y + 10, 500, 500), text);
                    }
                }
            }
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