using Noname.Core.Primitives;
using Noname.Presentation.ViewModels;
using UnityEngine;

namespace Noname.Presentation.Views
{
    /// <summary>
    /// 플레이어 위치를 표시하고 드롭 픽업 앵커를 제공하는 뷰입니다.
    /// </summary>
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;

        private GameViewModel _viewModel;

        /// <summary>드롭이 도달해야 할 기준 위치.</summary>
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

        /// <summary>
        /// 플레이어 위치 이벤트에 구독합니다.
        /// </summary>
        public void Bind(GameViewModel viewModel)
        {
            Unbind();
            _viewModel = viewModel;
            _viewModel.PlayerPositionChanged += OnPlayerPositionChanged;
        }

        /// <summary>
        /// 모든 이벤트 구독을 해제합니다.
        /// </summary>
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
