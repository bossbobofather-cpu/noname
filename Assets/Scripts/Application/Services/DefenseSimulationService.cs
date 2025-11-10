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
    public sealed class DefenseSimulationService
    {
        private readonly IGameStateRepository _repository;
        private readonly DefenseGameSettings _settings;
        private readonly Random _random = new Random();
        private float _spawnTimer;

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

        public DefenseSimulationService(IGameStateRepository repository, DefenseGameSettings settings)
        {
            _repository = repository;
            _settings = settings;
            _spawnTimer = MathF.Max(0f, settings.initialSpawnDelay);
        }

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

            HandleSpawning(state, deltaTime);
            HandlePlayerAutoAttack(state);
            UpdatePlayerProjectiles(state, deltaTime);
            UpdateEnemies(state, deltaTime);
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

        private void HandleSpawning(GameState state, float deltaTime)
        {
            if (_settings.enemySpawnEntries == null || _settings.enemySpawnEntries.Length == 0)
            {
                return;
            }

            _spawnTimer -= deltaTime;
            while (_spawnTimer <= 0f)
            {
                SpawnEnemy(state);
                _spawnTimer += MathF.Max(0.1f, _settings.spawnInterval);
            }
        }

        private void SpawnEnemy(GameState state)
        {
            var definition = SelectEnemyDefinition();
            if (definition == null)
            {
                return;
            }

            var x = Lerp(_settings.enemySpawnMin.X, _settings.enemySpawnMax.X, (float)_random.NextDouble());
            var y = Lerp(_settings.enemySpawnMin.Y, _settings.enemySpawnMax.Y, (float)_random.NextDouble());
            var spawnPosition = new Float2(x, y);
            var id = state.GetNextEnemyId();
            var enemy = definition.CreateEntity(id, spawnPosition);
            state.AddEnemy(enemy);
            _spawned.Add(enemy);
        }

        private void HandlePlayerAutoAttack(GameState state)
        {
            var player = state.Player;
            if (!player.CanAttack)
            {
                return;
            }

            var rangeSq = player.AttackRange * player.AttackRange;
            Float2 targetPosition;
            Float2 direction;

            if (state.HasFixedBombardment)
            {
                targetPosition = state.FixedBombardmentPosition;
                direction = targetPosition - player.Position;
                var distanceSq = direction.SqrMagnitude;
                if (distanceSq > rangeSq)
                {
                    return;
                }

                if (distanceSq <= 1e-6f)
                {
                    direction = Float2.Up;
                }
            }
            else
            {
                EnemyEntity closestEnemy = null;
                var bestDistanceSq = float.MaxValue;

                for (int i = 0; i < state.Enemies.Count; i++)
                {
                    var enemy = state.Enemies[i];
                    if (!enemy.IsAlive)
                    {
                        continue;
                    }

                    var delta = enemy.Position - player.Position;
                    var distanceSq = delta.SqrMagnitude;
                    if (distanceSq > rangeSq)
                    {
                        continue;
                    }

                    if (distanceSq < bestDistanceSq)
                    {
                        bestDistanceSq = distanceSq;
                        closestEnemy = enemy;
                    }
                }

                if (closestEnemy == null)
                {
                    return;
                }

                targetPosition = closestEnemy.Position;
                direction = closestEnemy.Position - player.Position;
                if (direction.SqrMagnitude <= 1e-6f)
                {
                    direction = Float2.Up;
                }
            }

            var projectileSpeed = MathF.Max(0.1f, _settings.playerProjectileSpeed);
            var velocity = direction.Normalized * projectileSpeed;
            var projectile = state.AddPlayerProjectile(
                player.Position,
                velocity,
                player.AttackDamage,
                MathF.Max(0f, _settings.playerExplosionRadius),
                targetPosition);

            _playerProjectileFired.Add(new PlayerProjectileFiredEvent(
                projectile.Id,
                projectile.Position,
                projectile.TargetPosition,
                projectileSpeed,
                projectile.ExplosionRadius));

            player.StartAttack();
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
            var player = state.Player;
            var fortress = state.Fortress;
            var fortressPosition = _settings.fortressPosition;
            var playerPosition = player.Position;
            var halfExtents = _settings.fortressHalfExtents;

            for (int i = 0; i < state.Enemies.Count; i++)
            {
                var enemy = state.Enemies[i];
                if (!enemy.IsAlive)
                {
                    HandleEnemyDestroyed(state, enemy);
                    continue;
                }

                enemy.UpdateCooldown(deltaTime);

                if (enemy.Role == EnemyCombatRole.Ranged)
                {
                    var horizontalClampX = ClampAxis(playerPosition.X, fortressPosition.X, halfExtents.X);
                    var desiredDistance = enemy.PreferredDistance > 0f ? enemy.PreferredDistance : enemy.AttackRange;
                    if (desiredDistance <= 0f)
                    {
                        desiredDistance = 1.5f;
                    }

                    var desiredY = fortressPosition.Y + desiredDistance;
                    var anchor = new Float2(horizontalClampX, desiredY);
                    if (!enemy.IsInPreferredRange(anchor))
                    {
                        enemy.MoveTowards(anchor, deltaTime);
                        continue;
                    }

                    if (enemy.TryAttack())
                    {
                        SpawnEnemyProjectile(state, enemy, fortressPosition);
                    }

                    continue;
                }

                var contactThreshold = enemy.AttackRange > 0f ? enemy.AttackRange : 0.75f;
                var nearestPoint = GetNearestPointOnFortress(enemy.Position);
                var distanceToFortress = (enemy.Position - nearestPoint).Magnitude;
                if (distanceToFortress <= contactThreshold)
                {
                    var dealt = fortress.ApplyDamage(enemy.AttackDamage);
                    if (dealt > 0f)
                    {
                        _attacks.Add(new EnemyAttackEvent(enemy.Id, dealt, fortress.CurrentHealth));
                    }

                    enemy.ApplyDamage(enemy.CurrentHealth);
                    HandleEnemyDestroyed(state, enemy);
                    continue;
                }

                enemy.MoveTowards(nearestPoint, deltaTime);
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

        private void HandleEnemyDestroyed(GameState state, EnemyEntity enemy)
        {
            AddRemovedEnemy(enemy.Id);
            var playerLuck = state.Player.Luck;

            var drops = enemy.DropDefinitions;
            if (drops == null || drops.Length == 0)
            {
                return;
            }

            for (int i = 0; i < drops.Length; i++)
            {
                var drop = drops[i];
                var chance = drop.guaranteed ? 1f : drop.probability;
                if (!drop.guaranteed && !RollChance(chance, playerLuck))
                {
                    continue;
                }

                SpawnResourceDrop(state, drop.type, enemy.Position, drop.amount);
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

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
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

        private static float ClampAxis(float value, float center, float halfExtent)
        {
            if (halfExtent <= 0f)
            {
                return value;
            }

            var min = center - halfExtent;
            var max = center + halfExtent;
            return MathF.Max(min, MathF.Min(value, max));
        }
    }

    public readonly struct SimulationStepResult
    {
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
        public int[] RemovedEnemyIds { get; }
        public EnemyAttackEvent[] EnemyAttacks { get; }
        public EnemyHitInfo[] PlayerProjectileHits { get; }
        public PlayerProjectileFiredEvent[] PlayerProjectilesFired { get; }
        public EnemyProjectileFiredEvent[] EnemyProjectilesFired { get; }
        public ProjectileImpactEvent[] ProjectileImpacts { get; }
        public ResourceDropSpawnedEvent[] ResourceDropsSpawned { get; }
        public ResourceDropCollectedEvent[] ResourceDropsCollected { get; }
        public PlayerLevelUpEvent[] PlayerLevelUps { get; }
    }

    public readonly struct EnemyAttackEvent
    {
        public EnemyAttackEvent(int enemyId, float damage, float fortressRemainingHealth)
        {
            EnemyId = enemyId;
            Damage = damage;
            FortressRemainingHealth = fortressRemainingHealth;
        }

        public int EnemyId { get; }
        public float Damage { get; }
        public float FortressRemainingHealth { get; }
    }
}
