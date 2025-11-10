using Noname.Core.Entities;
using Noname.Core.Enums;
using Noname.Core.Primitives;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    [CreateAssetMenu(menuName = "Defense/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Combat")]
        public EnemyCombatRole role = EnemyCombatRole.Melee;
        public float moveSpeed = 2f;
        public float maxHealth = 50f;
        public float attackDamage = 10f;
        public float attackRange = 1.5f;
        public float attackCooldown = 1f;
        [Tooltip("원거리 적일 때 선호하는 거리 (0이면 공격 사거리를 사용)")]
        public float preferredDistance;

        [Header("Drop Table")]
        public EnemyDropDefinition[] drops =
        {
            new EnemyDropDefinition { type = ResourceDropType.Experience, amount = 10f, probability = 1f, guaranteed = true },
            new EnemyDropDefinition { type = ResourceDropType.Gold, amount = 5f, probability = 1f, guaranteed = true },
            new EnemyDropDefinition { type = ResourceDropType.Health, amount = 5f, probability = 0.1f, guaranteed = false },
            new EnemyDropDefinition { type = ResourceDropType.Ability, amount = 1f, probability = 0.05f, guaranteed = false }
        };

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
