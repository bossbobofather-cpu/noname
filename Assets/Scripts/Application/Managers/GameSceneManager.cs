using UnityEngine;
using UnityEngine.SceneManagement;

namespace Noname.Application.Managers
{
    /// <summary>
    /// 씬 전환을 총괄하는 싱글턴 MonoBehaviour.
    /// </summary>
    public sealed class GameSceneManager : MonoBehaviour
    {
        [SerializeField] private string lobbySceneName = "LobbyScene";
        [SerializeField] private string battleSceneName = "BattleScene";

        public string LobbySceneName => lobbySceneName;
        public string BattleSceneName => battleSceneName;

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

        public void LoadLobbyScene()
        {
            LoadSceneByName(lobbySceneName);
        }

        public void LoadBattleScene()
        {
            LoadSceneByName(battleSceneName);
        }

        private void LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("Scene name is not configured.");
                return;
            }

            var current = SceneManager.GetActiveScene().name;
            if (current == sceneName)
            {
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
