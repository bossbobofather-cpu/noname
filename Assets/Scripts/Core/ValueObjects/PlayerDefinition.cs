using Noname.Core.Entities;
using Noname.Core.Primitives;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    [CreateAssetMenu(menuName = "Defense/Player Definition", fileName = "PlayerDefinition")]
    public sealed class PlayerDefinition : ScriptableObject
    {
        [Header("Combat Stats")]
        [Min(0f)] public float moveSpeed = 8f;
        [Min(0f)] public float attackDamage = 25f;
        [Min(0f)] public float attackRange = 2.5f;
        [Min(0.01f)] public float attackCooldown = 0.75f;
        [Min(1f)] public float maxHealth = 100f;
        [Min(0f)] public float luck = 0f;

        public PlayerEntity CreateEntity(Float2 spawnPosition)
        {
            return new PlayerEntity(
                spawnPosition,
                moveSpeed,
                attackDamage,
                attackRange,
                attackCooldown,
                maxHealth,
                luck);
        }
    }
}
