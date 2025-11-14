using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Noname.Presentation.Views
{
    /// <summary>
    /// 로비 UI 버튼 이벤트만 처리하고, 실제 UI는 에디터에서 구성한다.
    /// </summary>
    public class LobbySceneController : MonoBehaviour
    {
        [SerializeField] private string battleSceneName = "BattleScene";
        [Header("UI References")]
        [SerializeField] private Button enterBattleButton;
        [SerializeField] private Button equipmentButton;
        [SerializeField] private Button heroButton;
        [SerializeField] private Button traitsButton;

        private void Start()
        {
            BindButton(enterBattleButton, LoadBattleScene);
            BindButton(equipmentButton, () => Debug.Log("장비 UI는 아직 준비 중입니다."));
            BindButton(heroButton, () => Debug.Log("영웅 UI는 아직 준비 중입니다."));
            BindButton(traitsButton, () => Debug.Log("특성 UI는 아직 준비 중입니다."));
        }

        private void LoadBattleScene()
        {
            if (string.IsNullOrWhiteSpace(battleSceneName))
            {
                Debug.LogError("Battle scene name is not set.");
                return;
            }

            SceneManager.LoadScene(battleSceneName);
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction handler)
        {
            if (button == null)
            {
                Debug.LogWarning("LobbySceneController: 버튼 참조가 설정되지 않았습니다.");
                return;
            }

            button.onClick.RemoveAllListeners();
            if (handler != null)
            {
                button.onClick.AddListener(handler);
            }
        }
    }
}
