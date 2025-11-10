using System;
using Noname.Core.Enums;
using Noname.Core.Primitives;
using Noname.Core.ValueObjects;

namespace Noname.Core.Entities
{
    public sealed class EnemyEntity
    {
        private float _cooldownRemaining;

        public EnemyEntity(
            int id,
            Float2 spawnPosition,
            float moveSpeed,
            float maxHealth,
            float attackDamage,
            float attackRange,
            float attackCooldown,
            EnemyCombatRole role,
            float preferredDistance,
            EnemyDropDefinition[] drops)
        {
            Id = id;
            Position = spawnPosition;
            MoveSpeed = MathF.Max(0f, moveSpeed);
            MaxHealth = MathF.Max(0.1f, maxHealth);
            CurrentHealth = MaxHealth;
            AttackDamage = MathF.Max(0f, attackDamage);
            AttackRange = MathF.Max(0f, attackRange);
            AttackCooldown = MathF.Max(0f, attackCooldown);
            Role = role;
            PreferredDistance = MathF.Max(0f, preferredDistance);
            DropDefinitions = drops ?? System.Array.Empty<EnemyDropDefinition>();
            _cooldownRemaining = 0f;
        }

        public int Id { get; }
        public Float2 Position { get; private set; }
        public float MoveSpeed { get; }
        public float MaxHealth { get; }
        public float CurrentHealth { get; private set; }
        public float AttackDamage { get; }
        public float AttackRange { get; }
        public float AttackCooldown { get; }
        public EnemyCombatRole Role { get; }
        public float PreferredDistance { get; }
        public EnemyDropDefinition[] DropDefinitions { get; }
        public bool IsAlive => CurrentHealth > 0f;

        public void UpdateCooldown(float deltaTime)
        {
            if (_cooldownRemaining <= 0f)
            {
                return;
            }

            _cooldownRemaining = MathF.Max(0f, _cooldownRemaining - deltaTime);
        }

        public void MoveTowards(Float2 target, float deltaTime)
        {
            if (!IsAlive || MoveSpeed <= 0f)
            {
                return;
            }

            var direction = target - Position;
            if (direction.SqrMagnitude <= 1e-6f)
            {
                return;
            }

            var step = direction.Normalized * MoveSpeed * deltaTime;
            Position += step;
        }

        public bool IsInPreferredRange(Float2 target)
        {
            var distance = (target - Position).Magnitude;
            var tolerance = 0.25f;

            if (Role == EnemyCombatRole.Ranged)
            {
                var desired = PreferredDistance > 0f ? PreferredDistance : AttackRange;
                desired = MathF.Max(0f, desired);
                var min = MathF.Max(0f, desired - tolerance);
                var max = desired + tolerance;
                return distance >= min && distance <= max;
            }

            return distance <= AttackRange + tolerance;
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
