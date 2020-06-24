using UnityEngine;

namespace TaurusDungeonGenerator.Example.Scripts
{
    public class DungeonTestApplication : MonoBehaviour
    {
        public void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}