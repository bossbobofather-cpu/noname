using UnityEngine;

namespace Noname.Presentation.Managers
{
    /// <summary>
    /// UI 토스트나 텍스트 피드백을 표시하는 간단한 매니저입니다.
    /// </summary>
    public sealed class UIFeedbackManager : MonoBehaviour
    {
        /// <summary>
        /// 골드 획득 메시지를 표시합니다.
        /// </summary>
        public void ShowGoldGain(float amount, Vector3 worldPosition)
        {
            Debug.Log($"[UI] 골드 +{amount} at {worldPosition}");
        }

        /// <summary>
        /// 경험치 획득 메시지를 표시합니다.
        /// </summary>
        public void ShowExperienceGain(float amount, Vector3 worldPosition)
        {
            Debug.Log($"[UI] 경험치 +{amount} at {worldPosition}");
        }

        /// <summary>
        /// 어빌리티 선택 UI가 열렸음을 알립니다.
        /// </summary>
        public void ShowAbilitySelectionOpened()
        {
            Debug.Log("[UI] Ability 선택 UI 활성화");
        }

        /// <summary>
        /// 어빌리티 선택 UI가 닫혔음을 알립니다.
        /// </summary>
        public void ShowAbilitySelectionClosed()
        {
            Debug.Log("[UI] Ability 선택 UI 비활성화");
        }
    }
}
