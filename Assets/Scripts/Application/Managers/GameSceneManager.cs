using UnityEngine;
using UnityEngine.SceneManagement;

namespace Noname.Application.Managers
{
    /// <summary>
    /// 씬 로드를 중앙에서 관리하는 공용 매니저.
    /// </summary>
    public sealed class GameSceneManager : MonoBehaviour
    {
        private static GameSceneManager cachedInstance;

        private void Awake()
        {
            if (cachedInstance != null && cachedInstance != this)
            {
                Destroy(gameObject);
                return;
            }

            cachedInstance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("GameSceneManager: scene name is empty.");
                return;
            }

            if (mode == LoadSceneMode.Single)
            {
                var current = SceneManager.GetActiveScene().name;
                if (current == sceneName)
                {
                    return;
                }
            }

            SceneManager.LoadScene(sceneName, mode);
        }

        public void ReloadCurrentScene()
        {
            var current = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(current))
            {
                SceneManager.LoadScene(current, LoadSceneMode.Single);
            }
        }
    }
}
