using Noname.Core.Entities;
using Noname.Core.Primitives;
using Noname.Presentation.ViewModels;
using UnityEngine;

namespace Noname.Presentation.Views
{
    public sealed class EnemyView : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;

        private GameViewModel _viewModel;
        private int _enemyId = -1;
        private bool _isActive;

        public int EnemyId => _enemyId;

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

        public void Bind(GameViewModel viewModel, EnemyEntity entity)
        {
            Unbind();
            _viewModel = viewModel;
            _enemyId = entity.Id;
            _isActive = true;

            _viewModel.EnemyPositionChanged += HandleEnemyPositionChanged;
            _viewModel.EnemyHealthChanged += HandleEnemyHealthChanged;
            _viewModel.EnemyRemoved += HandleEnemyRemoved;

            ApplyPosition(entity.Position);
        }

        public void Unbind()
        {
            if (_viewModel == null)
            {
                return;
            }

            _viewModel.EnemyPositionChanged -= HandleEnemyPositionChanged;
            _viewModel.EnemyHealthChanged -= HandleEnemyHealthChanged;
            _viewModel.EnemyRemoved -= HandleEnemyRemoved;
            _viewModel = null;
            _enemyId = -1;
            _isActive = false;
        }

        private void HandleEnemyPositionChanged(int id, Float2 position)
        {
            if (!_isActive || id != _enemyId)
            {
                return;
            }

            ApplyPosition(position);
        }

        private void HandleEnemyHealthChanged(int id, float remainingHealth)
        {
            if (!_isActive || id != _enemyId)
            {
                return;
            }

            if (remainingHealth <= 0f)
            {
                HandleEnemyRemoved(id);
            }
        }

        private void HandleEnemyRemoved(int id)
        {
            if (!_isActive || id != _enemyId)
            {
                return;
            }

            _isActive = false;
            gameObject.SetActive(false);
            Unbind();
        }

        private void ApplyPosition(Float2 position)
        {
            if (visualRoot == null)
            {
                return;
            }

            var world = visualRoot.position;
            world.x = position.X;
            world.y = position.Y;
            visualRoot.position = world;
        }
    }
}
