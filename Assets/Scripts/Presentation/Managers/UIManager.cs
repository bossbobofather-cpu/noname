using UnityEngine;

namespace Noname.Presentation.Managers
{
    /// <summary>
    /// UI 표시/전환을 관리한다. 현재는 호출 로그만 남긴다.
    /// </summary>
    public sealed class UIManager : MonoBehaviour
    {
        public void OpenUI(string uiId)
        {
            Debug.Log($"[UI] Open {uiId}");
        }

        public void CloseUI(string uiId)
        {
            Debug.Log($"[UI] Close {uiId}");
        }

        public void ShowUI(string uiElementId)
        {
            Debug.Log($"[UI] Show {uiElementId}");
        }

        public void ShowUI(string uiElementId, Vector3 worldPosition)
        {
            Debug.Log($"[UI] Show {uiElementId} at {worldPosition}");
        }

        public void HideUI(string uiElementId)
        {
            Debug.Log($"[UI] Hide {uiElementId}");
        }

        public void HideUI(string uiElementId, Vector3 worldPosition)
        {
            Debug.Log($"[UI] Hide {uiElementId} at {worldPosition}");
        }
    }
}
