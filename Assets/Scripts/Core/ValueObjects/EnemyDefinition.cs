using Noname.Core.Entities;
using Noname.Core.Enums;
using Noname.Core.Primitives;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 적 전투 스탯과 드롭 구성을 정의하는 ScriptableObject 입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Defense/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Combat")]
        /// <summary>근접/원거리 전투 역할.</summary>
        public EnemyCombatRole role = EnemyCombatRole.Melee;

        /// <summary>이동 속도.</summary>
        public float moveSpeed = 2f;

        /// <summary>최대 체력.</summary>
        public float maxHealth = 50f;

        /// <summary>기본 공격력.</summary>
        public float attackDamage = 10f;

        /// <summary>공격 사거리.</summary>
        public float attackRange = 1.5f;

        /// <summary>공격 쿨다운.</summary>
        public float attackCooldown = 1f;

        [Tooltip("원거리 적이 유지하고 싶은 거리 (0이면 attackRange 사용)")]
        /// <summary>선호 거리.</summary>
        public float preferredDistance;

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
        public EnemyEntity CreateEntity(int id, Float2 spawnPosition)
        {
            return new EnemyEntity(
                id,
                spawnPosition,
                moveSpeed,
                maxHealth,
                attackDamage,
                attackRange,
                attackCooldown,
                role,
                preferredDistance,
                drops);
        }
    }
}
