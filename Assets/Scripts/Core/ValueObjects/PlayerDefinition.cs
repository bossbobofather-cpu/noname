using Noname.Core.Entities;
using Noname.Core.Primitives;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 플레이어 캐릭터의 기본 스탯을 정의하는 ScriptableObject 입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Defense/Player Definition", fileName = "PlayerDefinition")]
    public sealed class PlayerDefinition : ScriptableObject
    {
        [Header("Combat Stats")]
        [Min(0f)]
        /// <summary>이동 속도.</summary>
        public float moveSpeed = 8f;

        [Min(0f)]
        /// <summary>기본 공격력.</summary>
        public float attackDamage = 25f;

        [Min(0f)]
        /// <summary>공격 사거리.</summary>
        public float attackRange = 2.5f;

        [Min(0.01f)]
        /// <summary>공격 쿨다운.</summary>
        public float attackCooldown = 0.75f;

        [Min(1f)]
        /// <summary>최대 체력.</summary>
        public float maxHealth = 100f;

        [Min(0f)]
        /// <summary>운(Luck) 수치.</summary>
        public float luck = 0f;

        /// <summary>
        /// 스탯을 기반으로 PlayerEntity를 생성합니다.
        /// </summary>
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
