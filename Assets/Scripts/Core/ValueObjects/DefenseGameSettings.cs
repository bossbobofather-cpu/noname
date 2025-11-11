using Noname.Core.Primitives;
using UnityEngine;
using UnityEngine.Serialization;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 방어 모드 밸런스 파라미터를 모두 보관하는 구조체입니다.
    /// </summary>
    [System.Serializable]
    public struct DefenseGameSettings
    {
        [Header("Player")]
        public Float2 playerSpawnPosition;

        [Tooltip("사용 가능한 플레이어 정의(ScriptableObject) 목록")]
        public PlayerDefinition[] playerDefinitions;

        [Tooltip("playerDefinitions 배열에서 기본으로 사용할 인덱스")]
        public int defaultPlayerIndex;

        public float movementMinX;
        public float movementMaxX;
        public float playerProjectileSpeed;
        public float playerExplosionRadius;

        [Header("Player Progression")]
        [Tooltip("1레벨 → 2레벨에 필요한 기본 경험치")]
        public float baseExperienceToLevel;

        [Tooltip("레벨마다 적용되는 경험치 배수")]
        public float experienceGrowthFactor;

        [Tooltip("드롭이 자동으로 줍히기까지의 지연 시간(초)")]
        public float experiencePickupDelay;

        [Tooltip("Luck 1포인트당 확률 가중치")]
        public float luckBonusPerPoint;

        [Tooltip("레벨업 시 제공할 능력 선택지 수")]
        public int abilityChoicesPerLevel;

        [Tooltip("플레이어에게 부여할 능력 풀")]
        [FormerlySerializedAs("augmentPool")]
        public GameplayAbilityDefinition[] abilityPool;

        [Header("Fortress")]
        public Float2 fortressPosition;
        public Float2 fortressHalfExtents;
        public float fortressMaxHealth;

        [Header("Enemy Grid & Waves")]
        [Tooltip("격자 행 수")]
        public int gridRows;

        [Tooltip("격자 열 수")]
        public int gridColumns;

        [Tooltip("한 번에 한 행씩 전진하는 간격(초)")]
        public float enemyRowAdvanceInterval;

        [Tooltip("왼쪽 기준 스폰 X 좌표")]
        public float spawnOriginX;

        [Tooltip("열 간격(단위: 월드 좌표)")]
        public float spawnColumnSpacing;

        [Tooltip("0행(최상단) Y 좌표")]
        public float firstRowY;

        [Tooltip("행 간격(월드 좌표)")]
        public float rowSpacing;

        [Tooltip("라운드마다 채울 열 비율 (0~1)")]
        public float waveColumnFillRatio;

        [Tooltip("디버그용: 첫 웨이브만 스폰할지 여부")]
        public bool spawnOnlyFirstWave;

        [Tooltip("첫 전진까지 대기 시간")]
        public float initialSpawnDelay;

        [Tooltip("웨이브 구성에 사용할 적 정의/가중치")]
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
            int gridRows,
            int gridColumns,
            float enemyRowAdvanceInterval,
            float spawnOriginX,
            float spawnColumnSpacing,
            float firstRowY,
            float rowSpacing,
            float waveColumnFillRatio,
            bool spawnOnlyFirstWave,
            float initialSpawnDelay,
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
            this.gridRows = gridRows;
            this.gridColumns = gridColumns;
            this.enemyRowAdvanceInterval = enemyRowAdvanceInterval;
            this.spawnOriginX = spawnOriginX;
            this.spawnColumnSpacing = spawnColumnSpacing;
            this.firstRowY = firstRowY;
            this.rowSpacing = rowSpacing;
            this.waveColumnFillRatio = waveColumnFillRatio;
            this.spawnOnlyFirstWave = spawnOnlyFirstWave;
            this.initialSpawnDelay = initialSpawnDelay;
            this.enemySpawnEntries = enemySpawnEntries;
            this.enemyProjectileSpeed = enemyProjectileSpeed;
        }

        private float ResolveRowSpacing()
        {
            if (rowSpacing > 0f)
            {
                return rowSpacing;
            }

            var fallbackBottom = fortressPosition.Y - fortressHalfExtents.Y - 1f;
            var span = Mathf.Abs(firstRowY - fallbackBottom);
            return gridRows <= 1 ? Mathf.Max(1f, span) : Mathf.Max(0.1f, span / Mathf.Max(1, gridRows - 1));
        }

        private float ResolveColumnSpacing()
        {
            return spawnColumnSpacing > 0f ? spawnColumnSpacing : 1f;
        }

        public Float2 GetCellWorldPosition(int row, int column)
        {
            var columns = Mathf.Max(1, gridColumns);
            var spacingX = ResolveColumnSpacing();
            var clampedColumn = Mathf.Clamp(column, 0, columns - 1);
            var x = spawnOriginX + clampedColumn * spacingX;

            var spacingY = ResolveRowSpacing();
            // row 0 == firstRowY, 양수는 아래쪽(성벽 방향)으로 진행
            var y = firstRowY - spacingY * row;
            return new Float2(x, y);
        }

        public bool TryGetCellIndices(Float2 worldPosition, out int row, out int column)
        {
            row = 0;
            column = 0;

            if (gridColumns <= 0 || gridRows <= 0)
            {
                return false;
            }

            var spacingX = ResolveColumnSpacing();
            var spacingY = ResolveRowSpacing();
            var minX = spawnOriginX - spacingX * 0.5f;
            var maxX = spawnOriginX + spacingX * (Mathf.Max(1, gridColumns) - 0.5f);
            if (worldPosition.X < minX || worldPosition.X > maxX)
            {
                return false;
            }

            var topY = firstRowY;
            var minY = topY - spacingY * (Mathf.Max(1, gridRows) - 0.5f);
            var maxY = topY + spacingY * 0.5f;
            if (worldPosition.Y > maxY || worldPosition.Y < minY)
            {
                return false;
            }

            var normalizedColumn = (worldPosition.X - spawnOriginX) / spacingX;
            column = Mathf.Clamp(Mathf.RoundToInt(normalizedColumn), 0, gridColumns - 1);

            var normalizedRow = (topY - worldPosition.Y) / spacingY;
            row = Mathf.Clamp(Mathf.RoundToInt(normalizedRow), 0, gridRows - 1);
            return true;
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
