using System;
using Noname.Core.Primitives;
using Noname.Core.ValueObjects;

namespace Noname.Core.Entities
{
    /// <summary>
    /// 단일 적의 전투 상태를 관리하고 그리드 상태를 나타냅니다.
    /// </summary>
    public sealed class EnemyEntity
    {
        private float _cooldownRemaining;
        private bool _dropsDisabled;

        public EnemyEntity(
            int id,
            int gridRow,
            int gridColumn,
            Float2 spawnPosition,
            float maxHealth,
            float attackDamage,
            float attackRange,
            float attackCooldown,
            EnemyDropDefinition[] drops)
        {
            Id = id;
            GridRow = gridRow;
            GridColumn = Math.Max(0, gridColumn);
            Position = spawnPosition;
            MaxHealth = MathF.Max(0.1f, maxHealth);
            CurrentHealth = MaxHealth;
            AttackDamage = MathF.Max(0f, attackDamage);
            AttackRange = MathF.Max(0f, attackRange);
            AttackCooldown = MathF.Max(0f, attackCooldown);
            DropDefinitions = drops ?? Array.Empty<EnemyDropDefinition>();
            _dropsDisabled = false;
        }

        public int Id { get; }
        public int GridRow { get; private set; }
        public int GridColumn { get; }
        public Float2 Position { get; private set; }
        public float MaxHealth { get; }
        public float CurrentHealth { get; private set; }
        public float AttackDamage { get; }
        public float AttackRange { get; }
        public float AttackCooldown { get; }
        public EnemyDropDefinition[] DropDefinitions { get; }
        public bool IsAlive => CurrentHealth > 0f;
        public bool DropsDisabled => _dropsDisabled;

        public void AdvanceRow()
        {
            GridRow++;
        }

        public void SetGridPosition(int row, Float2 worldPosition)
        {
            GridRow = row;
            Position = worldPosition;
        }

        public void DisableDrops()
        {
            _dropsDisabled = true;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (_cooldownRemaining <= 0f)
            {
                return;
            }

            _cooldownRemaining = MathF.Max(0f, _cooldownRemaining - deltaTime);
        }

        public bool TryAttack()
        {
            if (_cooldownRemaining > 0f)
            {
                return false;
            }

            _cooldownRemaining = AttackCooldown;
            return true;
        }

        public float ApplyDamage(float amount)
        {
            if (amount <= 0f || !IsAlive)
            {
                return 0f;
            }

            var previous = CurrentHealth;
            CurrentHealth = MathF.Max(0f, CurrentHealth - amount);
            return previous - CurrentHealth;
        }

        public void SetPosition(Float2 position)
        {
            Position = position;
        }
    }
}
