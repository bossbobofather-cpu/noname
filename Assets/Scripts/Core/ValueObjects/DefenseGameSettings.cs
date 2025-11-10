using Noname.Core.Primitives;
using UnityEngine;
using UnityEngine.Serialization;

namespace Noname.Core.ValueObjects
{
    [System.Serializable]
    public struct DefenseGameSettings
    {
        [Header("Player")]
        public Float2 playerSpawnPosition;
        [Tooltip("플레이어 스탯을 담은 ScriptableObject 목록")]
        public PlayerDefinition[] playerDefinitions;
        [Tooltip("playerDefinitions 배열에서 시작 시 활용할 인덱스")]
        public int defaultPlayerIndex;
        public float movementMinX;
        public float movementMaxX;
        public float playerProjectileSpeed;
        public float playerExplosionRadius;

        [Header("Player Progression")]
        [Tooltip("1레벨에서 2레벨로 올라갈 때 필요한 기본 경험치")]
        public float baseExperienceToLevel;
        [Tooltip("레벨이 오를 때마다 요구 경험치에 곱해지는 계수")]
        public float experienceGrowthFactor;
        [Tooltip("드랍된 경험치가 자동으로 수거되기까지의 지연 시간(초)")]
        public float experiencePickupDelay;
        [Tooltip("행운 1포인트당 드랍 확률에 더해지는 값")]
        public float luckBonusPerPoint;
        [Tooltip("레벨업 시 표시할 게임 어빌리티 선택지 수")]
        public int abilityChoicesPerLevel;
        [Tooltip("게임 어빌리티 풀 (ScriptableObject)")]
        [FormerlySerializedAs("augmentPool")]
        public GameplayAbilityDefinition[] abilityPool;

        [Header("Fortress")]
        public Float2 fortressPosition;
        public Float2 fortressHalfExtents;
        public float fortressMaxHealth;

        [Header("Enemy Spawning")]
        public Float2 enemySpawnMin;
        public Float2 enemySpawnMax;
        public float initialSpawnDelay;
        public float spawnInterval;
        [Tooltip("각 적 정의와 스폰 가중치 목록")]
        [FormerlySerializedAs("enemyArchetypes")]
        public EnemySpawnEntry[] enemySpawnEntries;

        [Header("Enemy Projectiles")]
        public float enemyProjectileSpeed;

        public DefenseGameSettings(
            Float2 playerSpawnPosition,
            PlayerDefinition[] playerDefinitions,
            int defaultPlayerIndex,
            float movementMinX,
            float movementMaxX,
            float playerProjectileSpeed,
            float playerExplosionRadius,
            float baseExperienceToLevel,
            float experienceGrowthFactor,
            float experiencePickupDelay,
            float luckBonusPerPoint,
            int abilityChoicesPerLevel,
            GameplayAbilityDefinition[] abilityPool,
            Float2 fortressPosition,
            Float2 fortressHalfExtents,
            float fortressMaxHealth,
            Float2 enemySpawnMin,
            Float2 enemySpawnMax,
            float initialSpawnDelay,
            float spawnInterval,
            EnemySpawnEntry[] enemySpawnEntries,
            float enemyProjectileSpeed)
        {
            this.playerSpawnPosition = playerSpawnPosition;
            this.playerDefinitions = playerDefinitions;
            this.defaultPlayerIndex = defaultPlayerIndex;
            this.movementMinX = movementMinX;
            this.movementMaxX = movementMaxX;
            this.playerProjectileSpeed = playerProjectileSpeed;
            this.playerExplosionRadius = playerExplosionRadius;
            this.baseExperienceToLevel = baseExperienceToLevel;
            this.experienceGrowthFactor = experienceGrowthFactor;
            this.experiencePickupDelay = experiencePickupDelay;
            this.luckBonusPerPoint = luckBonusPerPoint;
            this.abilityChoicesPerLevel = abilityChoicesPerLevel;
            this.abilityPool = abilityPool;
            this.fortressPosition = fortressPosition;
            this.fortressHalfExtents = fortressHalfExtents;
            this.fortressMaxHealth = fortressMaxHealth;
            this.enemySpawnMin = enemySpawnMin;
            this.enemySpawnMax = enemySpawnMax;
            this.initialSpawnDelay = initialSpawnDelay;
            this.spawnInterval = spawnInterval;
            this.enemySpawnEntries = enemySpawnEntries;
            this.enemyProjectileSpeed = enemyProjectileSpeed;
        }

        public PlayerDefinition GetPlayerDefinitionOrDefault(int index)
        {
            if (playerDefinitions == null || playerDefinitions.Length == 0)
            {
                return null;
            }

            index = Mathf.Clamp(index, 0, playerDefinitions.Length - 1);
            return playerDefinitions[index];
        }

        public PlayerDefinition GetDefaultPlayerDefinition()
        {
            return GetPlayerDefinitionOrDefault(defaultPlayerIndex);
        }
    }
}
