using UnityEditor;
using UnityEngine;

namespace SnowFlakeGamesAssets.TaurusDungeonGenerator.Component.Editor
{
    /// <summary>
    /// Handles the inspector drawing of the <see cref="Room"/> component
    /// </summary>
    [CustomEditor(typeof(Room))]
    public class RoomDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var room = (Room) serializedObject.targetObject;
            // DrawDefaultInspector();

            room.overwriteBounds = EditorGUILayout.Toggle("Overwrite Bounds", room.overwriteBounds);
            if (room.overwriteBounds)
            {
                if (room.overwrittenBounds == default)
                {
                    room.overwrittenBounds = room.GetActualBounds();
                }

                room.overwrittenBounds = EditorGUILayout.BoundsField("Overwrite",room.overwrittenBounds);
            }

            // ReSharper disable once AssignmentInConditionalExpression
            if (room.debugMode = EditorGUILayout.Foldout(room.debugMode, "Debug", true))
            {
                GUILayout.Label("Bounds:", EditorStyles.boldLabel);
                EditorGUILayout.BoundsField(room.GetBounds());
            }
        }
    }
}