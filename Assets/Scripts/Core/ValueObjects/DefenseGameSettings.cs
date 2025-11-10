using Noname.Core.Primitives;
using UnityEngine;
using UnityEngine.Serialization;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 디펜스 게임 모드에 필요한 각종 밸런스 수치를 묶은 구조체입니다.
    /// </summary>
    [System.Serializable]
    public struct DefenseGameSettings
    {
        [Header("Player")]
        /// <summary>플레이어 스폰 위치.</summary>
        public Float2 playerSpawnPosition;

        [Tooltip("플레이어 스탯을 담은 ScriptableObject 목록")]
        /// <summary>선택 가능한 플레이어 정의 목록.</summary>
        public PlayerDefinition[] playerDefinitions;

        [Tooltip("playerDefinitions 배열에서 기본으로 사용할 인덱스")]
        /// <summary>기본 플레이어 인덱스.</summary>
        public int defaultPlayerIndex;

        /// <summary>좌측 이동 제한.</summary>
        public float movementMinX;

        /// <summary>우측 이동 제한.</summary>
        public float movementMaxX;

        /// <summary>플레이어 투사체 속도.</summary>
        public float playerProjectileSpeed;

        /// <summary>플레이어 폭발 반경.</summary>
        public float playerExplosionRadius;

        [Header("Player Progression")]
        [Tooltip("1레벨에서 2레벨로 가기까지 필요한 기본 경험치")]
        /// <summary>기본 레벨업 요구 경험치.</summary>
        public float baseExperienceToLevel;

        [Tooltip("레벨마다 누적 경험치에 곱해지는 계수")]
        /// <summary>레벨업 요구량 증가 계수.</summary>
        public float experienceGrowthFactor;

        [Tooltip("드랍된 경험치가 자동으로 주워지기까지의 지연 시간(초)")]
        /// <summary>경험치 드롭 지연.</summary>
        public float experiencePickupDelay;

        [Tooltip("운 1포인트당 드랍 확률이 올라가는 양")]
        /// <summary>운 스탯 당 보너스 확률.</summary>
        public float luckBonusPerPoint;

        [Tooltip("레벨업 시 제공할 게임 어빌리티 선택지 수")]
        /// <summary>레벨업 당 어빌리티 선택지 수.</summary>
        public int abilityChoicesPerLevel;

        [Tooltip("게임 어빌리티 풀 (ScriptableObject)")]
        [FormerlySerializedAs("augmentPool")]
        /// <summary>랜덤 추첨에 사용할 어빌리티 풀.</summary>
        public GameplayAbilityDefinition[] abilityPool;

        [Header("Fortress")]
        /// <summary>거점 위치.</summary>
        public Float2 fortressPosition;

        /// <summary>거점 바운딩 박스 절반 크기.</summary>
        public Float2 fortressHalfExtents;

        /// <summary>거점 최대 체력.</summary>
        public float fortressMaxHealth;

        [Header("Enemy Spawning")]
        /// <summary>적 스폰 영역 최소 좌표.</summary>
        public Float2 enemySpawnMin;

        /// <summary>적 스폰 영역 최대 좌표.</summary>
        public Float2 enemySpawnMax;

        /// <summary>첫 스폰까지의 지연.</summary>
        public float initialSpawnDelay;

        /// <summary>스폰 간격.</summary>
        public float spawnInterval;

        [Tooltip("웨이브별 스폰 가중치 목록")]
        [FormerlySerializedAs("enemyArchetypes")]
        /// <summary>스폰 후보와 가중치 목록.</summary>
        public EnemySpawnEntry[] enemySpawnEntries;

        [Header("Enemy Projectiles")]
        /// <summary>적 발사체 속도.</summary>
        public float enemyProjectileSpeed;

        /// <summary>
        /// 모든 설정 값을 한 번에 채우는 생성자입니다.
        /// </summary>
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

        /// <summary>
        /// 인덱스로 플레이어 정의를 안전하게 가져옵니다.
        /// </summary>
        public PlayerDefinition GetPlayerDefinitionOrDefault(int index)
        {
            if (playerDefinitions == null || playerDefinitions.Length == 0)
            {
                return null;
            }

            index = Mathf.Clamp(index, 0, playerDefinitions.Length - 1);
            return playerDefinitions[index];
        }

        /// <summary>
        /// 기본 인덱스로 설정된 플레이어 정의를 반환합니다.
        /// </summary>
        public PlayerDefinition GetDefaultPlayerDefinition()
        {
            return GetPlayerDefinitionOrDefault(defaultPlayerIndex);
        }
    }
}
