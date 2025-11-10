using System;
using Noname.Core.ValueObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Noname.Presentation.Views
{
    /// <summary>
    /// 단일 어빌리티 선택 버튼을 표현하는 뷰입니다.
    /// </summary>
    public sealed class AugmentSelectionOptionView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;

        private void OnDisable()
        {
            button?.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// 어빌리티 정보를 표시하고 클릭 콜백을 연결합니다.
        /// </summary>
        public void Show(GameplayAbilityDefinition ability, Action onClick)
        {
            if (button == null)
            {
                return;
            }

            button.gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick.Invoke());
            }

            if (titleText != null)
            {
                titleText.text = string.IsNullOrWhiteSpace(ability.Title)
                    ? name
                    : ability.Title;
            }

            if (descriptionText != null)
            {
                descriptionText.text = ability.Description ?? string.Empty;
            }
        }

        /// <summary>
        /// 버튼을 숨기고 리스너를 제거합니다.
        /// </summary>
        public void Hide()
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }
}
