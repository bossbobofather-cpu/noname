using Noname.Application.Managers;
using Noname.Presentation.Managers;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Noname.Application.Runtime
{
    /// <summary>
    /// Core 시스템에서 공유 매니저에 접근하기 위한 정적 진입점.
    /// </summary>
    public static class CoreRuntime
    {
        private const string HubAddress = "Assets/Prefabs/Runtime/CoreRuntimeHub.prefab";

        private static CoreRuntimeHub hub;
        private static bool isInitializing;

        public static GameSceneManager GameSceneManager => EnsureHub()?.SceneManager;
        public static UIManager UIManager => EnsureHub()?.UIManager;
        public static FXManager FXManager => EnsureHub()?.FXManager;
        public static SoundManager SoundManager => EnsureHub()?.SoundManager;

        public static CoreRuntimeHub Initialize() => EnsureHub();

        private static CoreRuntimeHub EnsureHub()
        {
            if (hub != null || isInitializing)
            {
                return hub;
            }

            isInitializing = true;

            hub = FindExistingHub();
            if (hub == null)
            {
                hub = CreateHub();
            }

            if (hub != null)
            {
                Object.DontDestroyOnLoad(hub.gameObject);
                hub.InitializeManagers();
            }

            isInitializing = false;
            return hub;
        }

        private static CoreRuntimeHub CreateHub()
        {
            var handle = AddressableAssetLoader.LoadAssetHandleSync<GameObject>(HubAddress);
            CoreRuntimeHub instance = null;

            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                var prefab = handle.Result.GetComponent<CoreRuntimeHub>();
                if (prefab != null)
                {
                    instance = Object.Instantiate(prefab);
                }
                else
                {
                    Debug.LogError($"CoreRuntimeHub prefab at '{HubAddress}' does not contain CoreRuntimeHub component.");
                }
            }

            if (instance == null)
            {
                Debug.LogError($"CoreRuntimeHub prefab not found or invalid for address '{HubAddress}'. Creating fallback hub.");
                var go = new GameObject("[CoreRuntimeHub]");
                instance = go.AddComponent<CoreRuntimeHub>();
            }

            AddressableAssetLoader.ReleaseHandle(ref handle);
            return instance;
        }

        private static CoreRuntimeHub FindExistingHub()
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindFirstObjectByType<CoreRuntimeHub>(FindObjectsInactive.Include);
#else
            return Object.FindObjectOfType<CoreRuntimeHub>();
#endif
        }
    }
}
