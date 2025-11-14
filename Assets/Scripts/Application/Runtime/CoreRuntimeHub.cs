using Noname.Application.Managers;
using Noname.Presentation.Managers;
using UnityEngine;

namespace Noname.Application.Runtime
{
    /// <summary>
    /// CoreRuntime에서 사용하는 매니저 모음을 담는 허브.
    /// </summary>
    public sealed class CoreRuntimeHub : MonoBehaviour
    {
        [Header("Manager References")]
        [SerializeField] private GameSceneManager gameSceneManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private FXManager fxManager;
        [SerializeField] private SoundManager soundManager;

        public GameSceneManager SceneManager => EnsureManager(ref gameSceneManager);
        public UIManager UIManager => EnsureManager(ref uiManager);
        public FXManager FXManager => EnsureManager(ref fxManager);
        public SoundManager SoundManager => EnsureManager(ref soundManager);

        internal void InitializeManagers()
        {
            EnsureManager(ref gameSceneManager);
            EnsureManager(ref uiManager);
            EnsureManager(ref fxManager);
            EnsureManager(ref soundManager);
        }

        private T EnsureManager<T>(ref T field) where T : Component
        {
            if (field != null)
            {
                return field;
            }

            field = GetComponentInChildren<T>(true);
            if (field != null)
            {
                return field;
            }

            field = gameObject.AddComponent<T>();
            return field;
        }
    }
}
