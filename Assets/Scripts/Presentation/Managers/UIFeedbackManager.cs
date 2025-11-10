using Noname.Core.ValueObjects;
using UnityEngine;

namespace Noname.Presentation.Managers
{
    public sealed class UIFeedbackManager : MonoBehaviour
    {
        public void ShowGoldGain(float amount, Vector3 worldPosition)
        {
            Debug.Log($"[UI] 골드 +{amount} at {worldPosition}");
        }

        public void ShowExperienceGain(float amount, Vector3 worldPosition)
        {
            Debug.Log($"[UI] 경험치 +{amount} at {worldPosition}");
        }

        public void ShowAbilitySelectionOpened()
        {
            Debug.Log("[UI] Ability 선택 UI 활성화");
        }

        public void ShowAbilitySelectionClosed()
        {
            Debug.Log("[UI] Ability 선택 UI 닫힘");
        }
    }
}
