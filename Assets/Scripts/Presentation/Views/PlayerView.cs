using Noname.Core.Primitives;
using Noname.Presentation.ViewModels;
using UnityEngine;

namespace Noname.Presentation.Views
{
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;

        private GameViewModel _viewModel;

        public Transform PickupAnchor => visualRoot != null ? visualRoot : transform;

        private void Awake()
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }

        public void Bind(GameViewModel viewModel)
        {
            Unbind();
            _viewModel = viewModel;
            _viewModel.PlayerPositionChanged += OnPlayerPositionChanged;
        }

        public void Unbind()
        {
            if (_viewModel == null)
            {
                return;
            }

            _viewModel.PlayerPositionChanged -= OnPlayerPositionChanged;
            _viewModel = null;
        }

        private void OnPlayerPositionChanged(Float2 position)
        {
            var anchor = PickupAnchor;
            var current = anchor.position;
            current.x = position.X;
            current.y = position.Y;
            anchor.position = current;
        }
    }
}
