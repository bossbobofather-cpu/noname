using Noname.Core.ValueObjects;
using Noname.Presentation.ViewModels;
using UnityEngine;

namespace Noname.Presentation.Views
{
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

