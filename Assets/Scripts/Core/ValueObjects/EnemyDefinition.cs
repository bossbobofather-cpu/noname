using System;
using Noname.Core.Entities;
using Noname.Core.Enums;
using Noname.Core.Primitives;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 각 적 유닛의 능력치를 정의하는 ScriptableObject 입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Defense/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Combat (만분율)")]
        /// <summary>최대 체력(만분율).</summary>
        [Min(0)]
        public int maxHealthRaw = 50 * FixedPointScaling.Denominator;

        /// <summary>기본 공격력(만분율).</summary>
        [Min(0)]
        public int attackDamageRaw = 10 * FixedPointScaling.Denominator;

        [Tooltip("플레이어와의 기준 거리(만분율). 0이면 근접 공격으로 처리됩니다.")]
        /// <summary>공격 사거리(만분율).</summary>
        [Min(0)]
        public int attackRangeRaw = 0;

        /// <summary>공격 쿨다운(만분율).</summary>
        [Min(0)]
        public int attackCooldownRaw = 1 * FixedPointScaling.Denominator;

        [Header("Drop Table")]
        /// <summary>적 처치 시 드롭될 리소스 구성.</summary>
        public EnemyDropDefinition[] drops =
        {
            new EnemyDropDefinition { type = ResourceDropType.Experience, amountRaw = FixedPointScaling.FromFloat(10f), probabilityRaw = FixedPointScaling.FromFloat(1f), guaranteed = true },
            new EnemyDropDefinition { type = ResourceDropType.Gold, amountRaw = FixedPointScaling.FromFloat(5f), probabilityRaw = FixedPointScaling.FromFloat(1f), guaranteed = true },
            new EnemyDropDefinition { type = ResourceDropType.Health, amountRaw = FixedPointScaling.FromFloat(5f), probabilityRaw = FixedPointScaling.FromFloat(0.1f), guaranteed = false },
            new EnemyDropDefinition { type = ResourceDropType.Ability, amountRaw = FixedPointScaling.FromFloat(1f), probabilityRaw = FixedPointScaling.FromFloat(0.05f), guaranteed = false }
        };

        /// <summary>최대 체력.</summary>
        public float MaxHealth => FixedPointScaling.ToFloat(maxHealthRaw);

        /// <summary>기본 공격력.</summary>
        public float AttackDamage => FixedPointScaling.ToFloat(attackDamageRaw);

        /// <summary>공격 사거리.</summary>
        public float AttackRange => FixedPointScaling.ToFloat(attackRangeRaw);

        /// <summary>공격 쿨다운.</summary>
        public float AttackCooldown => FixedPointScaling.ToFloat(attackCooldownRaw);

        /// <summary>
        /// 정의된 능력치로 EnemyEntity를 생성합니다.
        /// </summary>
        /// <param name="row">격자 행 인덱스(0 = 최상단)</param>
        /// <param name="column">격자 열 인덱스</param>
        public EnemyEntity CreateEntity(int id, int row, int column, Float2 spawnPosition)
        {
            var effectiveDrops = drops ?? Array.Empty<EnemyDropDefinition>();

            return new EnemyEntity(
                id,
                row,
                column,
                spawnPosition,
                MaxHealth,
                AttackDamage,
                AttackRange,
                AttackCooldown,
                effectiveDrops);
        }
    }
}
