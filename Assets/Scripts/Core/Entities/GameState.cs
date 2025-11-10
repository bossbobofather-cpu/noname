using System.Collections.Generic;
using Noname.Core.Enums;
using Noname.Core.Primitives;

namespace Noname.Core.Entities
{
    public sealed class GameState
    {
        private readonly List<EnemyEntity> _enemies = new List<EnemyEntity>();
        private readonly List<ProjectileEntity> _playerProjectiles = new List<ProjectileEntity>();
        private readonly List<ProjectileEntity> _enemyProjectiles = new List<ProjectileEntity>();
        private readonly List<ResourceDropEntity> _resourceDrops = new List<ResourceDropEntity>();
        private int _enemyIdCounter;
        private int _projectileIdCounter;
        private int _resourceDropIdCounter;
        private bool _hasFixedBombardment;
        private Float2 _fixedBombardmentPosition;

        public GameState(
            PlayerEntity player,
            FortressEntity fortress,
            Float2 enemySpawnMin,
            Float2 enemySpawnMax)
        {
            Player = player;
            Fortress = fortress;
            EnemySpawnMin = enemySpawnMin;
            EnemySpawnMax = enemySpawnMax;
        }

        public PlayerEntity Player { get; }
        public FortressEntity Fortress { get; }
        public IReadOnlyList<EnemyEntity> Enemies => _enemies;
        public IReadOnlyList<ProjectileEntity> PlayerProjectiles => _playerProjectiles;
        public IReadOnlyList<ProjectileEntity> EnemyProjectiles => _enemyProjectiles;
        public IReadOnlyList<ResourceDropEntity> ResourceDrops => _resourceDrops;
        public Float2 EnemySpawnMin { get; }
        public Float2 EnemySpawnMax { get; }
        public float ElapsedTime { get; private set; }
        public bool IsGameOver => Fortress.IsDestroyed;
        public bool HasFixedBombardment => _hasFixedBombardment;
        public Float2 FixedBombardmentPosition => _fixedBombardmentPosition;

        public void Reset()
        {
            Player.Reset();
            Fortress.Reset();
            _enemies.Clear();
            _enemyIdCounter = 0;
            _playerProjectiles.Clear();
            _enemyProjectiles.Clear();
            _projectileIdCounter = 0;
            _resourceDrops.Clear();
            _resourceDropIdCounter = 0;
            ElapsedTime = 0f;
            _hasFixedBombardment = false;
            _fixedBombardmentPosition = Float2.Zero;
        }

        public void AdvanceTime(float deltaTime)
        {
            ElapsedTime += deltaTime;
        }

        public int GetNextEnemyId()
        {
            return _enemyIdCounter++;
        }

        public void AddEnemy(EnemyEntity enemy)
        {
            if (enemy == null)
            {
                return;
            }

            _enemies.Add(enemy);
        }

        public void RemoveDeadEnemies()
        {
            _enemies.RemoveAll(e => !e.IsAlive);
        }

        public void ClearEnemies()
        {
            _enemies.Clear();
        }

        public ProjectileEntity AddPlayerProjectile(Float2 position, Float2 velocity, float damage, float explosionRadius, Float2 targetPosition)
        {
            var projectile = new ProjectileEntity(
                _projectileIdCounter++,
                -1,
                Enums.ProjectileFaction.Player,
                position,
                velocity,
                damage,
                explosionRadius,
                targetPosition);
            _playerProjectiles.Add(projectile);
            return projectile;
        }

        public ProjectileEntity AddEnemyProjectile(int enemyId, Float2 position, Float2 velocity, float damage, Float2 targetPosition)
        {
            var projectile = new ProjectileEntity(
                _projectileIdCounter++,
                enemyId,
                Enums.ProjectileFaction.Enemy,
                position,
                velocity,
                damage,
                0f,
                targetPosition);
            _enemyProjectiles.Add(projectile);
            return projectile;
        }

        public ResourceDropEntity QueueResourceDrop(ResourceDropType type, Float2 position, float amount, float delay)
        {
            var drop = new ResourceDropEntity(_resourceDropIdCounter++, type, position, amount, delay);
            _resourceDrops.Add(drop);
            return drop;
        }

        public int CollectReadyResourceDrops(float deltaTime, List<ResourceDropEntity> readyList)
        {
            if (readyList == null)
            {
                return 0;
            }

            readyList.Clear();
            for (int i = _resourceDrops.Count - 1; i >= 0; i--)
            {
                var drop = _resourceDrops[i];
                if (drop.Tick(deltaTime))
                {
                    readyList.Add(drop);
                    _resourceDrops.RemoveAt(i);
                }
            }

            return readyList.Count;
        }

        public void SetFixedBombardment(Float2 position)
        {
            _fixedBombardmentPosition = position;
            _hasFixedBombardment = true;
        }

        public void ClearFixedBombardment()
        {
            _hasFixedBombardment = false;
            _fixedBombardmentPosition = Float2.Zero;
        }

        public void RemovePlayerProjectileAt(int index)
        {
            if (index >= 0 && index < _playerProjectiles.Count)
            {
                _playerProjectiles.RemoveAt(index);
            }
        }

        public void RemoveEnemyProjectileAt(int index)
        {
            if (index >= 0 && index < _enemyProjectiles.Count)
            {
                _enemyProjectiles.RemoveAt(index);
            }
        }
    }
}
