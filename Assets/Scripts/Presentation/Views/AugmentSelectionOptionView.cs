using System;
using Noname.Core.ValueObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Noname.Presentation.Views
{
    public sealed class AugmentSelectionOptionView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;

        private void OnDisable()
        {
            button?.onClick.RemoveAllListeners();
        }

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
