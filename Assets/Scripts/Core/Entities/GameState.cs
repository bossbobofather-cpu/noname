using System.Collections.Generic;
using Noname.Core.Enums;
using Noname.Core.Primitives;

namespace Noname.Core.Entities
{
    /// <summary>
    /// 플레이어/적/투사체/드롭 등 런타임 진행 상황을 보관하는 루트 상태입니다.
    /// </summary>
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

        /// <summary>
        /// 게임 상태를 구성하는 핵심 엔티티와 스폰 영역을 지정합니다.
        /// </summary>
        /// <param name="player">플레이어 엔티티.</param>
        /// <param name="fortress">거점 엔티티.</param>
        /// <param name="enemySpawnMin">적 스폰 영역 최소 좌표.</param>
        /// <param name="enemySpawnMax">적 스폰 영역 최대 좌표.</param>
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

        /// <summary>플레이어 엔티티입니다.</summary>
        public PlayerEntity Player { get; }

        /// <summary>거점 엔티티입니다.</summary>
        public FortressEntity Fortress { get; }

        /// <summary>현재 존재하는 적 목록입니다.</summary>
        public IReadOnlyList<EnemyEntity> Enemies => _enemies;

        /// <summary>플레이어 발사체 목록입니다.</summary>
        public IReadOnlyList<ProjectileEntity> PlayerProjectiles => _playerProjectiles;

        /// <summary>적 발사체 목록입니다.</summary>
        public IReadOnlyList<ProjectileEntity> EnemyProjectiles => _enemyProjectiles;

        /// <summary>대기 중인 자원 드롭 목록입니다.</summary>
        public IReadOnlyList<ResourceDropEntity> ResourceDrops => _resourceDrops;

        /// <summary>적 스폰 영역 최소 좌표입니다.</summary>
        public Float2 EnemySpawnMin { get; }

        /// <summary>적 스폰 영역 최대 좌표입니다.</summary>
        public Float2 EnemySpawnMax { get; }

        /// <summary>게임이 시작된 이후 누적 시간입니다.</summary>
        public float ElapsedTime { get; private set; }

        /// <summary>거점 파괴 여부입니다.</summary>
        public bool IsGameOver => Fortress.IsDestroyed;

        /// <summary>고정 폭격 지점 설정 여부입니다.</summary>
        public bool HasFixedBombardment => _hasFixedBombardment;

        /// <summary>고정 폭격 지점 좌표입니다.</summary>
        public Float2 FixedBombardmentPosition => _fixedBombardmentPosition;

        /// <summary>
        /// 플레이어·거점·컬렉션을 모두 초기화합니다.
        /// </summary>
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

        /// <summary>
        /// 경과 시간을 누적합니다.
        /// </summary>
        public void AdvanceTime(float deltaTime)
        {
            ElapsedTime += deltaTime;
        }

        /// <summary>
        /// 적 ID 시퀀스를 증가시키고 값을 반환합니다.
        /// </summary>
        public int GetNextEnemyId()
        {
            return _enemyIdCounter++;
        }

        /// <summary>
        /// 적을 상태에 추가합니다.
        /// </summary>
        public void AddEnemy(EnemyEntity enemy)
        {
            if (enemy == null)
            {
                return;
            }

            _enemies.Add(enemy);
        }

        /// <summary>
        /// 사망한 적을 컬렉션에서 제거합니다.
        /// </summary>
        public void RemoveDeadEnemies()
        {
            _enemies.RemoveAll(e => !e.IsAlive);
        }

        /// <summary>
        /// 적 목록을 모두 비웁니다.
        /// </summary>
        public void ClearEnemies()
        {
            _enemies.Clear();
        }

        /// <summary>
        /// 플레이어 발사체를 생성합니다.
        /// </summary>
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

        /// <summary>
        /// 적 발사체를 생성합니다.
        /// </summary>
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

        /// <summary>
        /// 지연 시간 이후 등장할 자원 드롭을 등록합니다.
        /// </summary>
        public ResourceDropEntity QueueResourceDrop(ResourceDropType type, Float2 position, float amount, float delay)
        {
            var drop = new ResourceDropEntity(_resourceDropIdCounter++, type, position, amount, delay);
            _resourceDrops.Add(drop);
            return drop;
        }

        /// <summary>
        /// 드롭 타이머를 갱신하고, 준비 완료된 드롭을 readyList로 반환합니다.
        /// </summary>
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

        /// <summary>
        /// 폭격 지점을 설정합니다.
        /// </summary>
        public void SetFixedBombardment(Float2 position)
        {
            _fixedBombardmentPosition = position;
            _hasFixedBombardment = true;
        }

        /// <summary>
        /// 폭격 지점을 해제합니다.
        /// </summary>
        public void ClearFixedBombardment()
        {
            _hasFixedBombardment = false;
            _fixedBombardmentPosition = Float2.Zero;
        }

        /// <summary>
        /// 플레이어 발사체를 인덱스로 제거합니다.
        /// </summary>
        public void RemovePlayerProjectileAt(int index)
        {
            if (index >= 0 && index < _playerProjectiles.Count)
            {
                _playerProjectiles.RemoveAt(index);
            }
        }

        /// <summary>
        /// 적 발사체를 인덱스로 제거합니다.
        /// </summary>
        public void RemoveEnemyProjectileAt(int index)
        {
            if (index >= 0 && index < _enemyProjectiles.Count)
            {
                _enemyProjectiles.RemoveAt(index);
            }
        }
    }
}
