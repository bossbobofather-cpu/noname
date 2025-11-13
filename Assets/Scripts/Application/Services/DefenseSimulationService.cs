using System;
using System.Collections.Generic;
using Noname.Application.Ports;
using Noname.Application.ValueObjects;
using Noname.Core.Entities;
using Noname.Core.Enums;
using Noname.Core.Primitives;
using Noname.Core.ValueObjects;

namespace Noname.Application.Services
{
    /// <summary>
    /// 적 스폰, 투사체, 드롭 등 디펜스 게임 진행을 계산하는 핵심 시뮬레이션 서비스입니다.
    /// </summary>
    public sealed class DefenseSimulationService
    {
        private readonly IGameStateRepository _repository;
        private readonly DefenseGameSettings _settings;
        private readonly Random _random = new Random();
        private float _rowAdvanceTimer;

        private readonly List<EnemyEntity> _spawned = new List<EnemyEntity>();
        private readonly List<int> _removed = new List<int>();
        private readonly List<EnemyAttackEvent> _attacks = new List<EnemyAttackEvent>();
        private readonly List<EnemyHitInfo> _playerHits = new List<EnemyHitInfo>();
        private readonly List<PlayerProjectileFiredEvent> _playerProjectileFired = new List<PlayerProjectileFiredEvent>();
        private readonly List<EnemyProjectileFiredEvent> _enemyProjectileFired = new List<EnemyProjectileFiredEvent>();
        private readonly List<ProjectileImpactEvent> _projectileImpacts = new List<ProjectileImpactEvent>();
        private readonly List<ResourceDropSpawnedEvent> _resourceDropsSpawned = new List<ResourceDropSpawnedEvent>();
        private readonly List<ResourceDropCollectedEvent> _resourceDropsCollected = new List<ResourceDropCollectedEvent>();
        private readonly List<PlayerLevelUpEvent> _levelUps = new List<PlayerLevelUpEvent>();
        private readonly List<ResourceDropEntity> _resourceBuffer = new List<ResourceDropEntity>();
        private bool _hasStandbyRow;
        private bool _firstWaveSpawned;
        private readonly List<int> _columnSelectionBuffer = new List<int>();

        /// <summary>
        /// 시뮬레이션을 수행하기 위한 필수 의존성을 주입합니다.
        /// </summary>
        public DefenseSimulationService(IGameStateRepository repository, DefenseGameSettings settings)
        {
            _repository = repository;
            _settings = settings;
            _rowAdvanceTimer = MathF.Max(0f, settings.initialSpawnDelay);
        }

        /// <summary>
        /// 한 프레임 분량의 게임 로직을 진행하고 결과 이벤트 묶음을 반환합니다.
        /// </summary>
        public SimulationStepResult Tick(float deltaTime)
        {
            var state = _repository.State;
            state.AdvanceTime(deltaTime);
            _spawned.Clear();
            _removed.Clear();
            _attacks.Clear();
            _playerHits.Clear();
            _playerProjectileFired.Clear();
            _enemyProjectileFired.Clear();
            _projectileImpacts.Clear();
            _resourceDropsSpawned.Clear();
            _resourceDropsCollected.Clear();
            _levelUps.Clear();
            _resourceBuffer.Clear();

            HandleWaveProgression(state, deltaTime);
            HandlePlayerAutoAttack(state);
            UpdatePlayerProjectiles(state, deltaTime);
            UpdateEnemyProjectiles(state, deltaTime);
            UpdateResourceDrops(state, deltaTime);

            state.RemoveDeadEnemies();

            return new SimulationStepResult(
                _spawned.ToArray(),
                _removed.ToArray(),
                _attacks.ToArray(),
                _playerHits.ToArray(),
                _playerProjectileFired.ToArray(),
                _enemyProjectileFired.ToArray(),
                _projectileImpacts.ToArray(),
                _resourceDropsSpawned.ToArray(),
                _resourceDropsCollected.ToArray(),
                _levelUps.ToArray());
        }

        private void HandleWaveProgression(GameState state, float deltaTime)
        {
            if (!HasValidGridConfig() || _settings.enemySpawnEntries == null || _settings.enemySpawnEntries.Length == 0)
            {
                return;
            }

            if (!_hasStandbyRow && (!_settings.spawnOnlyFirstWave || !_firstWaveSpawned))
            {
                SpawnStandbyRow(state);
            }

            _rowAdvanceTimer -= deltaTime;
            while (_rowAdvanceTimer <= 0f)
            {
                AdvanceExistingEnemies(state);
                UpdateEnemies(state, deltaTime);
                if (!_settings.spawnOnlyFirstWave || !_firstWaveSpawned)
                {
                    SpawnStandbyRow(state);
                }
                _rowAdvanceTimer += MathF.Max(0.1f, _settings.enemyRowAdvanceInterval);
            }
        }

        private bool HasValidGridConfig()
        {
            return _settings.gridRows > 0 && _settings.gridColumns > 0;
        }

        private void AdvanceExistingEnemies(GameState state)
        {
            for (int i = 0; i < state.Enemies.Count; i++)
            {
                var enemy = state.Enemies[i];
                if (!enemy.IsAlive)
                {
                    HandleEnemyDestroyed(state, enemy);
                    continue;
                }

                enemy.AdvanceRow();
                var worldPosition = _settings.GetCellWorldPosition(enemy.GridRow, enemy.GridColumn);
                enemy.SetPosition(worldPosition);

                if (TryResolveTriggerCollision(state, enemy))
                {
                    continue;
                }
            }

            _hasStandbyRow = false;
        }

        private void SpawnStandbyRow(GameState state)
        {
            if (_settings.spawnOnlyFirstWave && _firstWaveSpawned)
            {
                return;
            }

            var availableColumns = SelectColumnsToSpawn();
            if (availableColumns.Count == 0)
            {
                return;
            }

            foreach (var column in availableColumns)
            {
                SpawnEnemyAtCell(state, -1, column);
            }

            _hasStandbyRow = true;
            _firstWaveSpawned = true;
        }

        private IReadOnlyList<int> SelectColumnsToSpawn()
        {
            _columnSelectionBuffer.Clear();
            for (int column = 0; column < _settings.gridColumns; column++)
            {
                _columnSelectionBuffer.Add(column);
            }

            if (_columnSelectionBuffer.Count == 0)
            {
                return Array.Empty<int>();
            }

            Shuffle(_columnSelectionBuffer);
            var ratio = _settings.waveColumnFillRatio;
            if (ratio <= 0f)
            {
                ratio = 0.4f;
            }

            ratio = MathF.Min(1f, ratio);
            var desiredCount = Math.Max(1, (int)MathF.Floor(_settings.gridColumns * ratio));
            desiredCount = Math.Min(desiredCount, _columnSelectionBuffer.Count);

            return _columnSelectionBuffer.GetRange(0, desiredCount);
        }

        private void SpawnEnemyAtCell(GameState state, int row, int column)
        {
            var definition = SelectEnemyDefinition();
            if (definition == null)
            {
                return;
            }

            var spawnPosition = _settings.GetCellWorldPosition(row, column);
            var id = state.GetNextEnemyId();
            var enemy = definition.CreateEntity(id, row, column, spawnPosition);
            state.AddEnemy(enemy);
            _spawned.Add(enemy);
        }

        private bool TryResolveTriggerCollision(GameState state, EnemyEntity enemy)
        {
            var playerY = state.Player.Position.Y;
            if (enemy.Position.Y <= playerY)
            {
                ResolvePlayerCollision(state, enemy);
                return true;
            }

            var fortressY = _settings.fortressPosition.Y;
            if (enemy.Position.Y <= fortressY)
            {
                ResolveFortressCollision(state, enemy);
                return true;
            }

            return false;
        }

        private void ResolveFortressCollision(GameState state, EnemyEntity enemy)
        {
            var dealt = state.Fortress.ApplyDamage(enemy.AttackDamage);
            if (dealt > 0f)
            {
                _attacks.Add(new EnemyAttackEvent(enemy.Id, dealt, state.Fortress.CurrentHealth));
            }

            enemy.ApplyDamage(enemy.CurrentHealth);
            enemy.DisableDrops();
            HandleEnemyDestroyed(state, enemy, allowDrops: false);
        }

        private void ResolvePlayerCollision(GameState state, EnemyEntity enemy)
        {
            enemy.ApplyDamage(enemy.CurrentHealth);
            enemy.DisableDrops();
            HandleEnemyDestroyed(state, enemy, allowDrops: false);
        }

        private void Shuffle(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                var swapIndex = _random.Next(i + 1);
                if (swapIndex == i)
                {
                    continue;
                }

                (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
            }
        }

        private void HandlePlayerAutoAttack(GameState state)
        {
            var player = state.Player;
            if (!player.CanAttack)
            {
                return;
            }

            var rangeSq = player.AttackRange * player.AttackRange;
            EnemyEntity target = null;

            if (state.TryGetTargetCell(out var targetRow, out var targetColumn))
            {
                target = FindEnemyInCell(state, targetRow, targetColumn);
            }

            if (target == null)
            {
                target = FindClosestEnemy(state, player.Position, rangeSq);
            }

            if (target == null)
            {
                return;
            }

            var direction = target.Position - player.Position;
            var distanceSq = direction.SqrMagnitude;
            if (distanceSq > rangeSq)
            {
                return;
            }

            if (distanceSq <= 1e-6f)
            {
                direction = Float2.Up;
            }

            var projectileSpeed = MathF.Max(0.1f, _settings.playerProjectileSpeed);
            var velocity = direction.Normalized * projectileSpeed;
            var projectile = state.AddPlayerProjectile(
                player.Position,
                velocity,
                player.AttackDamage,
                MathF.Max(0f, _settings.playerExplosionRadius),
                target.Position);

            _playerProjectileFired.Add(new PlayerProjectileFiredEvent(
                projectile.Id,
                projectile.Position,
                projectile.TargetPosition,
                projectileSpeed,
                projectile.ExplosionRadius));

            player.StartAttack();
        }

        private static EnemyEntity FindEnemyInCell(GameState state, int row, int column)
        {
            for (int i = 0; i < state.Enemies.Count; i++)
            {
                var enemy = state.Enemies[i];
                if (!enemy.IsAlive)
                {
                    continue;
                }

                if (enemy.GridRow == row && enemy.GridColumn == column)
                {
                    return enemy;
                }
            }

            return null;
        }

        private static EnemyEntity FindClosestEnemy(GameState state, Float2 origin, float maxRangeSq)
        {
            EnemyEntity closest = null;
            var bestDistanceSq = maxRangeSq;

            for (int i = 0; i < state.Enemies.Count; i++)
            {
                var enemy = state.Enemies[i];
                if (!enemy.IsAlive)
                {
                    continue;
                }

                var delta = enemy.Position - origin;
                var distanceSq = delta.SqrMagnitude;
                if (distanceSq > maxRangeSq)
                {
                    continue;
                }

                if (distanceSq < bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    closest = enemy;
                }
            }

            return closest;
        }

        private void UpdatePlayerProjectiles(GameState state, float deltaTime)
        {
            var projectiles = state.PlayerProjectiles;
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = projectiles[i];
                projectile.Advance(deltaTime);

                if (!projectile.HasReachedTarget())
                {
                    continue;
                }

                ResolvePlayerProjectileImpact(state, projectile);
                _projectileImpacts.Add(new ProjectileImpactEvent(
                    projectile.Id,
                    ProjectileFaction.Player,
                    projectile.TargetPosition,
                    projectile.ExplosionRadius));
                state.RemovePlayerProjectileAt(i);
            }
        }

        private void ResolvePlayerProjectileImpact(GameState state, ProjectileEntity projectile)
        {
            var radiusSq = projectile.ExplosionRadius * projectile.ExplosionRadius;

            for (int i = 0; i < state.Enemies.Count; i++)
            {
                var enemy = state.Enemies[i];
                if (!enemy.IsAlive)
                {
                    continue;
                }

                var distanceSq = (enemy.Position - projectile.Position).SqrMagnitude;
                if (distanceSq > radiusSq)
                {
                    continue;
                }

                var dealt = enemy.ApplyDamage(projectile.Damage);
                if (dealt <= 0f)
                {
                    continue;
                }

                _playerHits.Add(new EnemyHitInfo(enemy.Id, dealt, enemy.CurrentHealth));
                if (!enemy.IsAlive)
                {
                    HandleEnemyDestroyed(state, enemy);
                }
            }
        }

        private void UpdateEnemies(GameState state, float deltaTime)
        {
            if (!HasValidGridConfig())
            {
                return;
            }

            var fortressRow = Math.Max(0, _settings.gridRows - 1);
            var fortressPosition = _settings.fortressPosition;

            for (int i = 0; i < state.Enemies.Count; i++)
            {
                var enemy = state.Enemies[i];
                if (!enemy.IsAlive)
                {
                    HandleEnemyDestroyed(state, enemy);
                    continue;
                }

                enemy.UpdateCooldown(deltaTime);

                if (enemy.AttackRange <= 0f)
                {
                    continue;
                }

                var rowsFromFortress = fortressRow - enemy.GridRow;
                if (rowsFromFortress < 0f)
                {
                    continue;
                }

                if (rowsFromFortress <= enemy.AttackRange && enemy.TryAttack())
                {
                    SpawnEnemyProjectile(state, enemy, fortressPosition);
                }
            }
        }

        private void SpawnEnemyProjectile(GameState state, EnemyEntity enemy, Float2 fortressPosition)
        {
            var targetPoint = GetNearestPointOnFortress(enemy.Position);
            var direction = targetPoint - enemy.Position;
            if (direction.SqrMagnitude <= 1e-6f)
            {
                direction = new Float2(0f, -1f);
            }

            var velocity = direction.Normalized * MathF.Max(0.1f, _settings.enemyProjectileSpeed);
            var projectile = state.AddEnemyProjectile(enemy.Id, enemy.Position, velocity, enemy.AttackDamage, targetPoint);

            _enemyProjectileFired.Add(new EnemyProjectileFiredEvent(
                projectile.Id,
                enemy.Id,
                projectile.Position,
                projectile.TargetPosition,
                MathF.Max(0.1f, _settings.enemyProjectileSpeed)));
        }

        private void UpdateEnemyProjectiles(GameState state, float deltaTime)
        {
            var projectiles = state.EnemyProjectiles;
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = projectiles[i];
                projectile.Advance(deltaTime);

                if (!projectile.HasReachedTarget())
                {
                    continue;
                }

                var dealt = state.Fortress.ApplyDamage(projectile.Damage);
                if (dealt > 0f)
                {
                    _attacks.Add(new EnemyAttackEvent(projectile.SourceId, dealt, state.Fortress.CurrentHealth));
                }

                _projectileImpacts.Add(new ProjectileImpactEvent(
                    projectile.Id,
                    ProjectileFaction.Enemy,
                    projectile.TargetPosition,
                    0f));

                state.RemoveEnemyProjectileAt(i);
            }
        }

        private void UpdateResourceDrops(GameState state, float deltaTime)
        {
            var count = state.CollectReadyResourceDrops(deltaTime, _resourceBuffer);
            if (count <= 0)
            {
                return;
            }

            for (int i = 0; i < _resourceBuffer.Count; i++)
            {
                var drop = _resourceBuffer[i];
                _resourceDropsCollected.Add(new ResourceDropCollectedEvent(drop.Id, drop.Type, drop.Amount));
            }
        }

        private EnemyDefinition SelectEnemyDefinition()
        {
            var entries = _settings.enemySpawnEntries;
            if (entries == null || entries.Length == 0)
            {
                return null;
            }

            if (entries.Length == 1)
            {
                return entries[0].definition;
            }

            float totalWeight = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].definition == null)
                {
                    continue;
                }

                var weight = entries[i].weight;
                totalWeight += weight > 0f ? weight : 1f;
            }

            if (totalWeight <= 0f)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].definition != null)
                    {
                        return entries[i].definition;
                    }
                }
                return null;
            }

            var roll = (float)_random.NextDouble() * totalWeight;
            float cumulative = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].definition == null)
                {
                    continue;
                }

                var weight = entries[i].weight > 0f ? entries[i].weight : 1f;
                cumulative += weight;
                if (roll <= cumulative)
                {
                    return entries[i].definition;
                }
            }

            for (int i = entries.Length - 1; i >= 0; i--)
            {
                if (entries[i].definition != null)
                {
                    return entries[i].definition;
                }
            }

            return null;
        }

        private void HandleEnemyDestroyed(GameState state, EnemyEntity enemy, bool allowDrops = true)
        {
            AddRemovedEnemy(enemy.Id);
            if (!allowDrops || enemy.DropsDisabled)
            {
                return;
            }

            var playerLuck = state.Player.Luck;

            var drops = enemy.DropDefinitions;
            if (drops == null || drops.Length == 0)
            {
                return;
            }

            for (int i = 0; i < drops.Length; i++)
            {
                var drop = drops[i];
                var chance = drop.guaranteed ? 1f : drop.Probability;
                if (!drop.guaranteed && !RollChance(chance, playerLuck))
                {
                    continue;
                }

                SpawnResourceDrop(state, drop.type, enemy.Position, drop.Amount);
            }
        }

        private void SpawnResourceDrop(GameState state, ResourceDropType type, Float2 position, float amount)
        {
            var delay = MathF.Max(0f, _settings.experiencePickupDelay);
            var drop = state.QueueResourceDrop(type, position, amount, delay);
            _resourceDropsSpawned.Add(new ResourceDropSpawnedEvent(drop.Id, type, position, amount, delay));
        }

        private bool RollChance(float baseChance, float luck)
        {
            if (baseChance <= 0f)
            {
                return false;
            }

            var chance = baseChance + luck * _settings.luckBonusPerPoint;
            chance = MathF.Max(0f, MathF.Min(1f, chance));
            return _random.NextDouble() <= chance;
        }

        private void AddRemovedEnemy(int enemyId)
        {
            if (_removed.Contains(enemyId))
            {
                return;
            }

            _removed.Add(enemyId);
        }

        private Float2 GetNearestPointOnFortress(Float2 point)
        {
            var center = _settings.fortressPosition;
            var extents = _settings.fortressHalfExtents;
            if (extents.X <= 0f && extents.Y <= 0f)
            {
                return center;
            }

            var minX = center.X - extents.X;
            var maxX = center.X + extents.X;
            var minY = center.Y - extents.Y;
            var maxY = center.Y + extents.Y;

            var clampedX = MathF.Max(minX, MathF.Min(point.X, maxX));
            var clampedY = MathF.Max(minY, MathF.Min(point.Y, maxY));
            return new Float2(clampedX, clampedY);
        }
    }

    /// <summary>
    /// 한 번의 Tick 결과로 생성된 모든 이벤트와 상태 변화를 담습니다.
    /// </summary>
    public readonly struct SimulationStepResult
    {
        /// <summary>
        /// 시뮬레이션 결과를 초기화합니다.
        /// </summary>
        public SimulationStepResult(
            EnemyEntity[] spawnedEnemies,
            int[] removedEnemyIds,
            EnemyAttackEvent[] attackEvents,
            EnemyHitInfo[] playerHits,
            PlayerProjectileFiredEvent[] playerProjectiles,
            EnemyProjectileFiredEvent[] enemyProjectiles,
            ProjectileImpactEvent[] projectileImpacts,
            ResourceDropSpawnedEvent[] resourceDropsSpawned,
            ResourceDropCollectedEvent[] resourceDropsCollected,
            PlayerLevelUpEvent[] playerLevelUps)
        {
            SpawnedEnemies = spawnedEnemies ?? System.Array.Empty<EnemyEntity>();
            RemovedEnemyIds = removedEnemyIds ?? System.Array.Empty<int>();
            EnemyAttacks = attackEvents ?? System.Array.Empty<EnemyAttackEvent>();
            PlayerProjectileHits = playerHits ?? System.Array.Empty<EnemyHitInfo>();
            PlayerProjectilesFired = playerProjectiles ?? System.Array.Empty<PlayerProjectileFiredEvent>();
            EnemyProjectilesFired = enemyProjectiles ?? System.Array.Empty<EnemyProjectileFiredEvent>();
            ProjectileImpacts = projectileImpacts ?? System.Array.Empty<ProjectileImpactEvent>();
            ResourceDropsSpawned = resourceDropsSpawned ?? System.Array.Empty<ResourceDropSpawnedEvent>();
            ResourceDropsCollected = resourceDropsCollected ?? System.Array.Empty<ResourceDropCollectedEvent>();
            PlayerLevelUps = playerLevelUps ?? System.Array.Empty<PlayerLevelUpEvent>();
        }

        public EnemyEntity[] SpawnedEnemies { get; }
        /// <summary>제거된 적 ID.</summary>
        public int[] RemovedEnemyIds { get; }
        /// <summary>적 공격 이벤트.</summary>
        public EnemyAttackEvent[] EnemyAttacks { get; }
        /// <summary>플레이어 투사체가 적에게 준 피해.</summary>
        public EnemyHitInfo[] PlayerProjectileHits { get; }
        /// <summary>플레이어 발사체 생성 이벤트.</summary>
        public PlayerProjectileFiredEvent[] PlayerProjectilesFired { get; }
        /// <summary>적 발사체 생성 이벤트.</summary>
        public EnemyProjectileFiredEvent[] EnemyProjectilesFired { get; }
        /// <summary>발사체 충돌 이벤트.</summary>
        public ProjectileImpactEvent[] ProjectileImpacts { get; }
        /// <summary>새 드롭 스폰 이벤트.</summary>
        public ResourceDropSpawnedEvent[] ResourceDropsSpawned { get; }
        /// <summary>드롭 수집 이벤트.</summary>
        public ResourceDropCollectedEvent[] ResourceDropsCollected { get; }
        /// <summary>플레이어 레벨업 이벤트.</summary>
        public PlayerLevelUpEvent[] PlayerLevelUps { get; }
    }

    /// <summary>
    /// 특정 적이 거점을 공격했을 때 발생하는 이벤트입니다.
    /// </summary>
    public readonly struct EnemyAttackEvent
    {
        /// <summary>
        /// 적 공격 이벤트를 초기화합니다.
        /// </summary>
        public EnemyAttackEvent(int enemyId, float damage, float fortressRemainingHealth)
        {
            EnemyId = enemyId;
            Damage = damage;
            FortressRemainingHealth = fortressRemainingHealth;
        }

        /// <summary>공격한 적 ID.</summary>
        public int EnemyId { get; }
        /// <summary>입힌 피해량.</summary>
        public float Damage { get; }
        /// <summary>공격 이후 남은 거점 체력.</summary>
        public float FortressRemainingHealth { get; }
    }
}
