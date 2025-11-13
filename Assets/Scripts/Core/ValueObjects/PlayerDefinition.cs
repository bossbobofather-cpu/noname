using Noname.Core.Entities;
using Noname.Core.Primitives;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 플레이어 캐릭터의 기본 능력치를 정의하는 ScriptableObject 입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Defense/Player Definition", fileName = "PlayerDefinition")]
    public sealed class PlayerDefinition : ScriptableObject
    {
        [Header("Combat Stats (만분율)")]
        [Tooltip("이동 속도 (만분율). 예: 8.0 -> 80000")]
        [Min(0)]
        public int moveSpeedRaw = 8 * FixedPointScaling.Denominator;

        [Tooltip("기본 공격력 (만분율). 예: 25 -> 250000")]
        [Min(0)]
        public int attackDamageRaw = 25 * FixedPointScaling.Denominator;

        [Tooltip("공격 사거리 (만분율). 예: 2.5 -> 25000")]
        [Min(0)]
        public int attackRangeRaw = (int)(2.5f * FixedPointScaling.Denominator);

        [Tooltip("공격 쿨다운 (만분율). 예: 0.75 -> 7500")]
        [Min(0)]
        public int attackCooldownRaw = (int)(0.75f * FixedPointScaling.Denominator);

        [Tooltip("최대 체력 (만분율). 예: 100 -> 1000000")]
        [Min(0)]
        public int maxHealthRaw = 100 * FixedPointScaling.Denominator;

        [Tooltip("기본 Luck 수치 (만분율). 예: 1 -> 10000")]
        [Min(0)]
        public int luckRaw = 0;

        /// <summary>이동 속도.</summary>
        public float MoveSpeed => FixedPointScaling.ToFloat(moveSpeedRaw);

        /// <summary>기본 공격력.</summary>
        public float AttackDamage => FixedPointScaling.ToFloat(attackDamageRaw);

        /// <summary>공격 사거리.</summary>
        public float AttackRange => FixedPointScaling.ToFloat(attackRangeRaw);

        /// <summary>공격 쿨다운.</summary>
        public float AttackCooldown => FixedPointScaling.ToFloat(attackCooldownRaw);

        /// <summary>최대 체력.</summary>
        public float MaxHealth => FixedPointScaling.ToFloat(maxHealthRaw);

        /// <summary>기본 Luck 수치.</summary>
        public float Luck => FixedPointScaling.ToFloat(luckRaw);

        /// <summary>
        /// 능력치 기반으로 PlayerEntity를 생성합니다.
        /// </summary>
        public PlayerEntity CreateEntity(Float2 spawnPosition)
        {
            return new PlayerEntity(
                spawnPosition,
                MoveSpeed,
                AttackDamage,
                AttackRange,
                AttackCooldown,
                MaxHealth,
                Luck);
        }
    }
}
