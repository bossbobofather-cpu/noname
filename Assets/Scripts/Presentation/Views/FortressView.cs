using Noname.Presentation.ViewModels;
using UnityEngine;

namespace Noname.Presentation.Views
{
    public sealed class FortressView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Slider healthSlider;
        [SerializeField] private UnityEngine.UI.Slider experienceSlider;
        [SerializeField] private UnityEngine.UI.Text levelLabel;

        private GameViewModel _viewModel;

        private void OnDestroy()
        {
            Unbind();
        }

        public void Bind(GameViewModel viewModel)
        {
            Unbind();
            _viewModel = viewModel;
            _viewModel.FortressHealthChanged += HandleHealthChanged;
            _viewModel.PlayerExperienceChanged += HandleExperienceChanged;
        }

        public void Unbind()
        {
            if (_viewModel == null)
            {
                return;
            }

            _viewModel.FortressHealthChanged -= HandleHealthChanged;
            _viewModel.PlayerExperienceChanged -= HandleExperienceChanged;
            _viewModel = null;
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (healthSlider == null)
            {
                return;
            }

            healthSlider.maxValue = Mathf.Max(1f, max);
            healthSlider.value = Mathf.Clamp(current, 0f, healthSlider.maxValue);
        }

        private void HandleExperienceChanged(float current, float required, int level)
        {
            if (experienceSlider != null)
            {
                experienceSlider.maxValue = Mathf.Max(1f, required);
                experienceSlider.value = Mathf.Clamp(current, 0f, experienceSlider.maxValue);
            }

            if (levelLabel != null)
            {
                levelLabel.text = $"Lv.{level}";
            }
        }
    }
}
