using UnityEngine;
using UnityEngine.UI;

namespace TaurusDungeonGenerator.Example.Scripts
{
    /// <summary>
    /// Source: https://forum.unity.com/threads/fps-counter.505495/#post-5287614
    /// </summary>
    public class FpsCounter : MonoBehaviour
    {
        [SerializeField] private Text fpsText;
        [SerializeField] private float hudRefreshRate = 1f;

        private float _timer;

        private void Update()
        {
            if (Time.unscaledTime > _timer)
            {
                int fps = (int) (1f / Time.unscaledDeltaTime);
                fpsText.text = "FPS: " + fps;
                _timer = Time.unscaledTime + hudRefreshRate;
            }
        }
    }
}