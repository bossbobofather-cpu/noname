using System;
using Noname.Core.Entities;
using Noname.Core.Enums;
using Noname.Core.Primitives;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 적 전투 스탯과 드롭 구성을 ScriptableObject 형태로 유지합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Defense/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Combat")]
        /// <summary>최대 체력.</summary>
        public float maxHealth = 50f;

        /// <summary>기본 공격력.</summary>
        public float attackDamage = 10f;

        [Tooltip("성벽으로부터 몇 행 떨어진 지점까지 공격 가능한지(행 단위). 0이면 충돌 시에만 공격합니다.")]
        /// <summary>사정거리(행 단위).</summary>
        public float attackRange = 0f;

        /// <summary>공격 쿨다운.</summary>
        public float attackCooldown = 1f;

        [Header("Drop Table")]
        /// <summary>사망 시 드롭되는 자원 목록.</summary>
        public EnemyDropDefinition[] drops =
        {
            new EnemyDropDefinition { type = ResourceDropType.Experience, amount = 10f, probability = 1f, guaranteed = true },
            new EnemyDropDefinition { type = ResourceDropType.Gold, amount = 5f, probability = 1f, guaranteed = true },
            new EnemyDropDefinition { type = ResourceDropType.Health, amount = 5f, probability = 0.1f, guaranteed = false },
            new EnemyDropDefinition { type = ResourceDropType.Ability, amount = 1f, probability = 0.05f, guaranteed = false }
        };

        /// <summary>
        /// 정의된 수치로 EnemyEntity를 생성합니다.
        /// </summary>
        /// <param name="row">행 인덱스(0 = 최상단)</param>
        /// <param name="column">열 인덱스</param>
        public EnemyEntity CreateEntity(int id, int row, int column, Float2 spawnPosition)
        {
            var effectiveDrops = drops ?? Array.Empty<EnemyDropDefinition>();

            return new EnemyEntity(
                id,
                row,
                column,
                spawnPosition,
                maxHealth,
                attackDamage,
                attackRange,
                attackCooldown,
                effectiveDrops);
        }
    }
}
