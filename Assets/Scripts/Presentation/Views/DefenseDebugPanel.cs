using Noname.Core.Enums;
using Noname.Infrastructure.Input;
using Noname.Presentation.ViewModels;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Noname.Presentation.Views
{
    public sealed class DefenseDebugPanel : MonoBehaviour
    {
        [SerializeField] private DefenseGameBootstrapper bootstrapper;

        [Header("Cheat Console")]
        [SerializeField] private GameObject cheatPanelRoot;
        [SerializeField] private InputField cheatInputField;

        private GameViewModel _viewModel;
        private bool _cheatPanelVisible;
        private bool _cheatInputCaptured;

        private GameViewModel ViewModel
        {
            get
            {
                if (_viewModel == null && bootstrapper != null)
                {
                    _viewModel = bootstrapper.ViewModel;
                }

                return _viewModel;
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            if (_cheatPanelVisible)
            {
                InputBlocker.IsBlocked = false;
            }
        }

        private void Update()
        {
            if (!Debug.isDebugBuild)
            {
                if (_cheatPanelVisible)
                {
                    ToggleCheatPanel(false);
                }
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.backquoteKey.wasPressedThisFrame)
            {
                ToggleCheatPanel(!_cheatPanelVisible);
            }
        }

        private void Initialize()
        {
            if (cheatInputField != null)
            {
                cheatInputField.onEndEdit.AddListener(HandleCheatEntered);
            }

            ToggleCheatPanel(false);
        }

        private void ToggleCheatPanel(bool visible)
        {
            if (cheatPanelRoot != null)
            {
                cheatPanelRoot.SetActive(visible && Debug.isDebugBuild);
            }

            _cheatPanelVisible = visible && Debug.isDebugBuild;
            InputBlocker.IsBlocked = _cheatPanelVisible;

            if (_cheatPanelVisible && cheatInputField != null)
            {
                cheatInputField.text = string.Empty;
                EventSystem.current?.SetSelectedGameObject(cheatInputField.gameObject);
            }
            else if (!_cheatPanelVisible && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == cheatInputField?.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void HandleCheatEntered(string command)
        {
            if (!_cheatPanelVisible || string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            command = command.Trim().ToLowerInvariant();
            switch (command)
            {
                case "forceability":
                case "fa":
                    HandleForceAbility();
                    break;
                default:
                    Debug.Log($"Unknown cheat command: {command}");
                    break;
            }

            if (cheatInputField != null)
            {
                cheatInputField.text = string.Empty;
                EventSystem.current?.SetSelectedGameObject(cheatInputField.gameObject);
            }

            ToggleCheatPanel(false);
        }

        private void HandleForceAbility()
        {
            if (!Debug.isDebugBuild)
            {
                return;
            }

            var viewModel = ViewModel;
            if (viewModel == null)
            {
                Debug.LogWarning("DebugPanel: GameViewModel reference is missing.");
                return;
            }

            if (!viewModel.ForceAbilitySelection())
            {
                Debug.Log("DebugPanel: Ability selection could not be shown (maybe already active or pool insufficient).");
            }
        }
    }
}
