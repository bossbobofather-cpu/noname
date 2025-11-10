using System;
using Noname.Core.Enums;
using Noname.Core.Primitives;
using Noname.Core.ValueObjects;

namespace Noname.Core.Entities
{
    public sealed class EnemyEntity
    {
        /// <summary>
        /// 마지막 공격 이후 남아 있는 쿨다운(초).
        /// </summary>
        private float _cooldownRemaining;

        /// <summary>
        /// 전투에서 사용될 적 기본 스탯과 드롭 구성을 담은 생성자.
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
            DropDefinitions = drops ?? System.Array.Empty<EnemyDropDefinition>();
            _cooldownRemaining = 0f;
        }

        /// <summary>적 고유 식별자.</summary>
        public int Id { get; }

        /// <summary>현재 월드 상 좌표.</summary>
        public Float2 Position { get; private set; }

        /// <summary>초당 이동 속도.</summary>
        public float MoveSpeed { get; }

        /// <summary>최대 체력.</summary>
        public float MaxHealth { get; }

        /// <summary>실제 남은 체력.</summary>
        public float CurrentHealth { get; private set; }

        /// <summary>일반 공격 피해량.</summary>
        public float AttackDamage { get; }

        /// <summary>공격 사거리.</summary>
        public float AttackRange { get; }

        /// <summary>공격 간 최소 간격(초).</summary>
        public float AttackCooldown { get; }

        /// <summary>근접/원거리 등 전투 역할.</summary>
        public EnemyCombatRole Role { get; }

        /// <summary>원거리 적이 유지하려는 이상 거리.</summary>
        public float PreferredDistance { get; }

        /// <summary>사망 시 커스텀 드롭 테이블.</summary>
        public EnemyDropDefinition[] DropDefinitions { get; }

        /// <summary>현재 생존 여부.</summary>
        public bool IsAlive => CurrentHealth > 0f;

        /// <summary>
        /// 델타 타임만큼 공격 쿨다운을 감소시킨다.
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
        /// 지정된 목표를 향해 이동한다.
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
        /// 현재 위치가 공격에 적합한 거리인지 확인한다.
        /// </summary>
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

        /// <summary>
        /// 공격 가능하면 true를 반환하고 쿨다운을 재설정한다.
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
        /// 피해를 적용하고 실제로 감소한 체력을 반환한다.
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
        /// 외부 시스템이 강제로 위치를 갱신할 때 사용.
        /// </summary>
        public void SetPosition(Float2 position)
        {
            Position = position;
        }
    }
}
