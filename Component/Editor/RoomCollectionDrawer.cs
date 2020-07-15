using Malee.List;
using UnityEditor;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Component.Editor
{
    [CustomEditor(typeof(RoomCollection))]
    public class RoomCollectionDrawer : UnityEditor.Editor
    {
        private ReorderableList roomsList;
        
        public void OnEnable()
        {
            roomsList = new ReorderableList(serializedObject.FindProperty("rooms"));
            roomsList.elementNameProperty = "Rooms";
        }
        
        public override void OnInspectorGUI() {

            serializedObject.Update();

            //draw the list using GUILayout, you can of course specify your own position and label
            roomsList.DoLayoutList();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}