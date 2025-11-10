using Noname.Core.ValueObjects;
using Noname.Presentation.ViewModels;
using UnityEngine;

namespace Noname.Presentation.Views
{
    /// <summary>
    /// 어빌리티 선택 UI 전체를 관리하는 뷰입니다.
    /// </summary>
    public sealed class AugmentSelectionView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private AugmentSelectionOptionView[] optionViews;

        private GameViewModel _viewModel;

        private void Awake()
        {
            SetVisible(false);
        }

        private void OnDestroy()
        {
            Unbind();
        }

        /// <summary>
        /// GameViewModel 이벤트에 구독합니다.
        /// </summary>
        public void Bind(GameViewModel viewModel)
        {
            Unbind();
            _viewModel = viewModel;
            if (_viewModel == null)
            {
                return;
            }

            _viewModel.AbilityChoicesPresented += HandleChoicesPresented;
            _viewModel.AbilitySelectionCleared += HandleSelectionCleared;
        }

        /// <summary>
        /// 구독 중인 모든 이벤트를 해제합니다.
        /// </summary>
        public void Unbind()
        {
            if (_viewModel == null)
            {
                return;
            }

            _viewModel.AbilityChoicesPresented -= HandleChoicesPresented;
            _viewModel.AbilitySelectionCleared -= HandleSelectionCleared;
            _viewModel = null;
        }

        private void HandleChoicesPresented(GameplayAbilityDefinition[] choices)
        {
            if (choices == null || choices.Length == 0)
            {
                HandleSelectionCleared();
                return;
            }

            SetVisible(true);
            var length = optionViews?.Length ?? 0;
            for (int i = 0; i < length; i++)
            {
                var view = optionViews[i];
                if (view == null)
                {
                    continue;
                }

                if (i >= choices.Length)
                {
                    view.Hide();
                    continue;
                }

                var ability = choices[i];
                var index = i;
                view.Show(ability, () => _viewModel?.SelectAbility(index));
            }
        }

        private void HandleSelectionCleared()
        {
            SetVisible(false);
            if (optionViews == null)
            {
                return;
            }

            foreach (var view in optionViews)
            {
                view?.Hide();
            }
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = visible;
                canvasGroup.interactable = visible;
            }

            if (!visible && optionViews != null)
            {
                foreach (var view in optionViews)
                {
                    view?.Hide();
                }
            }
        }
    }
}
