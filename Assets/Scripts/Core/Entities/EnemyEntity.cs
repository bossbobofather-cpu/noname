using System;
using Noname.Core.Enums;
using Noname.Core.Primitives;
using Noname.Core.ValueObjects;

namespace Noname.Core.Entities
{
    /// <summary>
    /// 단일 적 개체의 전투 스탯과 상태를 표현하는 도메인 모델입니다.
    /// </summary>
    public sealed class EnemyEntity
    {
        /// <summary>
        /// 다음 공격까지 남은 쿨다운 시간(초).
        /// </summary>
        private float _cooldownRemaining;

        /// <summary>
        /// 적 기본 스탯과 드롭 구성을 지정합니다.
        /// </summary>
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
            DropDefinitions = drops ?? Array.Empty<EnemyDropDefinition>();
            _cooldownRemaining = 0f;
        }

        /// <summary>적 고유 ID.</summary>
        public int Id { get; }

        /// <summary>현재 위치.</summary>
        public Float2 Position { get; private set; }

        /// <summary>초당 이동 속도.</summary>
        public float MoveSpeed { get; }

        /// <summary>최대 체력.</summary>
        public float MaxHealth { get; }

        /// <summary>현재 체력.</summary>
        public float CurrentHealth { get; private set; }

        /// <summary>기본 공격력.</summary>
        public float AttackDamage { get; }

        /// <summary>공격 사거리.</summary>
        public float AttackRange { get; }

        /// <summary>공격 간 최소 시간.</summary>
        public float AttackCooldown { get; }

        /// <summary>전투 역할(근접/원거리).</summary>
        public EnemyCombatRole Role { get; }

        /// <summary>원거리 적이 유지하고 싶은 이상 거리.</summary>
        public float PreferredDistance { get; }

        /// <summary>드롭 테이블.</summary>
        public EnemyDropDefinition[] DropDefinitions { get; }

        /// <summary>생존 여부.</summary>
        public bool IsAlive => CurrentHealth > 0f;

        /// <summary>
        /// 델타 타임만큼 공격 쿨다운을 감소시킵니다.
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            if (_cooldownRemaining <= 0f)
            {
                return;
            }

            _cooldownRemaining = MathF.Max(0f, _cooldownRemaining - deltaTime);
        }

        /// <summary>
        /// 목표를 향해 이동합니다.
        /// </summary>
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

        /// <summary>
        /// 현재 위치가 공격하기에 적합한 거리인지 확인합니다.
        /// </summary>
        public bool IsInPreferredRange(Float2 target)
        {
            var distance = (target - Position).Magnitude;
            const float tolerance = 0.25f;

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

        /// <summary>
        /// 쿨다운이 끝났다면 공격을 시작합니다.
        /// </summary>
        public bool TryAttack()
        {
            if (_cooldownRemaining > 0f)
            {
                return false;
            }

            _cooldownRemaining = AttackCooldown;
            return true;
        }

        /// <summary>
        /// 피해를 적용하고 실제 감소한 체력을 반환합니다.
        /// </summary>
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

        /// <summary>
        /// 외부에서 강제로 위치를 갱신할 때 사용합니다.
        /// </summary>
        public void SetPosition(Float2 position)
        {
            Position = position;
        }
    }
}
