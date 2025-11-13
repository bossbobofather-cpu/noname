using System;
using System.Collections.Generic;
using Noname.Application.Ports;
using Noname.Application.Services;
using Noname.Application.UseCases;
using Noname.Application.ValueObjects;
using Noname.Core.Entities;
using Noname.Core.Enums;
using Noname.Core.Primitives;
using Noname.Core.ValueObjects;
using Noname.Infrastructure.Input;
using Noname.Infrastructure.Repositories;
using Noname.Presentation.Managers;
using Noname.Presentation.Utilities;
using Noname.Presentation.ViewModels;
using UnityEngine;

namespace Noname.Presentation.Views
{
    /// <summary>
    /// Scene 내 의존성을 구성하고 GameViewModel을 초기화/갱신하는 엔트리 포인트입니다.
    /// </summary>
    public sealed class DefenseGameBootstrapper : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DefenseInputAdapter inputAdapter;
        [SerializeField] private PlayerView playerViewPrefab;
        [SerializeField] private Transform playerParent;
        [SerializeField] private FortressView fortressViewPrefab;
        [SerializeField] private Transform fortressParent;
        [SerializeField] private EnemyView fallbackEnemyViewPrefab;
        [SerializeField] private EnemyViewVariant[] enemyViewVariants;
        [SerializeField] private Transform enemyContainer;
        [SerializeField] private ProjectileView playerProjectilePrefab;
        [SerializeField] private ProjectileView enemyProjectilePrefab;
        [SerializeField] private Transform projectileParent;
        [SerializeField] private AugmentSelectionView augmentSelectionViewPrefab;
        [SerializeField] private Transform augmentSelectionParent;
        [SerializeField] private Transform resourceDropParent;
        [SerializeField] private ResourceDropPrefabEntry[] resourceDropPrefabs;
        [SerializeField] private Vector2 dropScatterRange = new Vector2(0.75f, 0.35f);
        [Header("Player Movement Bounds")]
        [SerializeField] private Collider2D leftBoundaryCollider;
        [SerializeField] private Collider2D rightBoundaryCollider;
        [Header("Feedback Managers")]
        [SerializeField] private UIFeedbackManager uiFeedbackManager;
        [SerializeField] private FXManager fxManager;
        [SerializeField] private SoundManager soundManager;

        [Header("Settings")]
        [SerializeField] private DefenseGameSettings settings = new DefenseGameSettings(
            playerSpawnPosition: new Float2(0f, -4f),
            playerDefinitions: System.Array.Empty<PlayerDefinition>(),
            defaultPlayerIndex: 0,
            movementMinX: -6f,
            movementMaxX: 6f,
            playerProjectileSpeed: 12f,
            playerExplosionRadius: 1.5f,
            baseExperienceToLevel: 100f,
            experienceGrowthFactor: 1.25f,
            experiencePickupDelay: 1.0f,
            luckBonusPerPoint: 0.02f,
            abilityChoicesPerLevel: 3,
            abilityPool: System.Array.Empty<GameplayAbilityDefinition>(),
            fortressPosition: new Float2(0f, -5f),
            fortressHalfExtents: new Float2(1.5f, 0.75f),
            fortressMaxHealth: 500f,
            gridRows: 8,
            gridColumns: 6,
            enemyRowAdvanceInterval: 1.25f,
            spawnOriginX: -6f,
            spawnColumnSpacing: 2.4f,
            firstRowY: 7.5f,
            rowSpacing: 0.4f,
            waveColumnFillRatio: 0.4f,
            spawnOnlyFirstWave: false,
            initialSpawnDelay: 1.5f,
            enemySpawnEntries: System.Array.Empty<EnemySpawnEntry>(),
            enemyProjectileSpeed: 8f);

        private GameViewModel _gameViewModel;
        private InMemoryGameStateRepository _repository;
        private readonly Dictionary<int, EnemyView> _enemyViews = new Dictionary<int, EnemyView>();
        private readonly ComponentPoolRegistry<EnemyView> _enemyViewPoolRegistry = new ComponentPoolRegistry<EnemyView>();
        private readonly Dictionary<int, ProjectileView> _playerProjectileViews = new Dictionary<int, ProjectileView>();
        private readonly Dictionary<int, ProjectileView> _enemyProjectileViews = new Dictionary<int, ProjectileView>();
        private readonly Dictionary<int, ResourceDropView> _resourceDropViews = new Dictionary<int, ResourceDropView>();
        private readonly ComponentPoolRegistry<ProjectileView> _playerProjectilePoolRegistry = new ComponentPoolRegistry<ProjectileView>();
        private readonly ComponentPoolRegistry<ProjectileView> _enemyProjectilePoolRegistry = new ComponentPoolRegistry<ProjectileView>();
        private readonly ComponentPoolRegistry<ResourceDropView> _resourceDropPoolRegistry = new ComponentPoolRegistry<ResourceDropView>();
        private readonly Dictionary<int, ResourceDropCollectedEvent> _pendingDropEffects = new Dictionary<int, ResourceDropCollectedEvent>();
        private PlayerView _playerViewInstance;
        private FortressView _fortressViewInstance;
        private AugmentSelectionView _augmentSelectionInstance;

        /// <summary>현재 활성화된 GameViewModel 인스턴스.</summary>
        public GameViewModel ViewModel => _gameViewModel;
        [Serializable]
        private struct ResourceDropPrefabEntry
        {
            public ResourceDropType type;
            public GameObject prefab;
        }

        [Serializable]
        private struct EnemyViewVariant
        {
            public EnemyView prefab;
            public float weight;
        }

        private void Awake()
        {
            ComposeGame();
        }

        private void Update()
        {
            _gameViewModel?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            UnsubscribeGameViewModelEvents();
            DestroyEnemyViews();
            DestroyPlayerView();
            DestroyFortressView();
            DestroyProjectileViews();
            DestroyAugmentSelectionView();
            DestroyResourceDropViews();
            ClearPools();
            _gameViewModel = null;
        }

        private void ComposeGame()
        {
            UnsubscribeGameViewModelEvents();
            DestroyEnemyViews();
            DestroyProjectileViews();
            DestroyPlayerView();
            DestroyFortressView();
            DestroyResourceDropViews();
            ClearPools();

            ApplyMovementBoundsFromColliders();

            if (settings.enemySpawnEntries == null)
            {
                settings.enemySpawnEntries = System.Array.Empty<EnemySpawnEntry>();
            }

            if (settings.abilityPool == null)
            {
                settings.abilityPool = System.Array.Empty<GameplayAbilityDefinition>();
            }

            if (settings.abilityChoicesPerLevel <= 0)
            {
                settings.abilityChoicesPerLevel = 3;
            }

            _playerViewInstance = InstantiatePlayerView(settings);
            var measuredExtents = InstantiateFortressView(settings);
            if (measuredExtents.SqrMagnitude > 0f)
            {
                settings.fortressHalfExtents = measuredExtents;
            }

            if (settings.playerDefinitions == null || settings.playerDefinitions.Length == 0)
            {
                Debug.LogError("DefenseGameSettings needs at least one PlayerDefinition assigned.");
                enabled = false;
                return;
            }

            var selectedPlayerDefinition = settings.GetDefaultPlayerDefinition();
            if (selectedPlayerDefinition == null)
            {
                Debug.LogError("Invalid default player index for DefenseGameSettings.");
                enabled = false;
                return;
            }

            var playerEntity = selectedPlayerDefinition.CreateEntity(settings.playerSpawnPosition);
            playerEntity.ConfigureExperienceProgression(settings.baseExperienceToLevel, settings.experienceGrowthFactor);

            var fortressEntity = new FortressEntity(settings.fortressMaxHealth);

            var gameState = new GameState(playerEntity, fortressEntity);

            _repository = new InMemoryGameStateRepository(gameState);

            var adapter = inputAdapter != null ? inputAdapter : GetComponent<DefenseInputAdapter>();
            if (adapter == null)
            {
                Debug.LogError("DefenseGameBootstrapper requires a DefenseInputAdapter reference.");
                enabled = false;
                return;
            }

            var startGame = new StartGameUseCase(_repository);
            var movePlayer = new MovePlayerUseCase(_repository);
            var simulationService = new DefenseSimulationService(_repository, settings);

        _gameViewModel = new GameViewModel(
            startGame,
            movePlayer,
            simulationService,
            adapter,
            _repository);
        InstantiateAugmentSelectionView();

            if (_playerViewInstance != null)
            {
                _playerViewInstance.Bind(_gameViewModel);
            }
            else
            {
                Debug.LogError("PlayerView prefab is not assigned.");
            }

            if (_fortressViewInstance != null)
            {
                _fortressViewInstance.Bind(_gameViewModel);
            }
            else
            {
                Debug.LogError("FortressView prefab is not assigned.");
            }

            SubscribeGameViewModelEvents();
            _gameViewModel.Initialize(settings);
        }

        private void HandleEnemySpawned(EnemyEntity entity)
        {
            var prefab = ResolveEnemyViewPrefab();
            var view = GetEnemyViewInstance(prefab);
            if (view == null)
            {
                Debug.LogWarning("Enemy view prefab is not assigned.");
                return;
            }

            view.transform.position = new Vector3(entity.Position.X, entity.Position.Y, view.transform.position.z);
            view.Bind(_gameViewModel, entity);
            _enemyViews[entity.Id] = view;
        }

        private EnemyView ResolveEnemyViewPrefab()
        {
            float totalWeight = 0f;
            if (enemyViewVariants != null && enemyViewVariants.Length > 0)
            {
                foreach (var option in enemyViewVariants)
                {
                    if (option.prefab == null)
                    {
                        continue;
                    }

                    var weight = option.weight > 0f ? option.weight : 1f;
                    totalWeight += weight;
                }

                if (totalWeight > 0f)
                {
                    var roll = UnityEngine.Random.value * totalWeight;
                    float cumulative = 0f;
                    foreach (var option in enemyViewVariants)
                    {
                        if (option.prefab == null)
                        {
                            continue;
                        }

                        var weight = option.weight > 0f ? option.weight : 1f;
                        cumulative += weight;
                        if (roll <= cumulative)
                        {
                            return option.prefab;
                        }
                    }
                }
            }

            return fallbackEnemyViewPrefab;
        }

        private PlayerView InstantiatePlayerView(DefenseGameSettings cfg)
        {
            if (playerViewPrefab == null)
            {
                Debug.LogError("PlayerView prefab is not assigned.");
                return null;
            }

            var parent = playerParent != null ? playerParent : transform;
            var instance = Instantiate(playerViewPrefab, parent);
            var t = instance.transform;
            t.position = new Vector3(cfg.playerSpawnPosition.X, cfg.playerSpawnPosition.Y, t.position.z);
            instance.gameObject.SetActive(true);
            return instance;
        }

        private Float2 InstantiateFortressView(DefenseGameSettings cfg)
        {
            if (fortressViewPrefab == null)
            {
                Debug.LogError("FortressView prefab is not assigned.");
                return Float2.Zero;
            }

            var parent = fortressParent != null ? fortressParent : transform;
            _fortressViewInstance = Instantiate(fortressViewPrefab, parent);
            var t = _fortressViewInstance.transform;
            t.position = new Vector3(cfg.fortressPosition.X, cfg.fortressPosition.Y, t.position.z);
            _fortressViewInstance.gameObject.SetActive(true);
            return MeasureFortressHalfExtents(_fortressViewInstance);
        }

        private void InstantiateAugmentSelectionView()
        {
            DestroyAugmentSelectionView();
            if (augmentSelectionViewPrefab == null || _gameViewModel == null)
            {
                return;
            }

            var parent = augmentSelectionParent != null ? augmentSelectionParent : transform;
            _augmentSelectionInstance = Instantiate(augmentSelectionViewPrefab, parent);
            _augmentSelectionInstance.Bind(_gameViewModel);
        }

        private void DestroyAugmentSelectionView()
        {
            if (_augmentSelectionInstance == null)
            {
                return;
            }

            _augmentSelectionInstance.Unbind();
            Destroy(_augmentSelectionInstance.gameObject);
            _augmentSelectionInstance = null;
        }

        private void HandleResourceDropSpawned(ResourceDropSpawnedEvent evt)
        {
            var view = GetResourceDropView(evt.Type);
            if (view == null)
            {
                return;
            }

            var instance = view.gameObject;
            instance.transform.position = new Vector3(evt.Position.X, evt.Position.Y, instance.transform.position.z) + GetDropScatterOffset();
            view.Initialize(evt.DropId, evt.Type);
            _resourceDropViews[evt.DropId] = view;
        }

        private void HandleResourceDropCollected(ResourceDropCollectedEvent evt)
        {
            _pendingDropEffects[evt.DropId] = evt;

            if (!_resourceDropViews.TryGetValue(evt.DropId, out var view) || view == null)
            {
                _resourceDropViews.Remove(evt.DropId);
                
                var fallbackPos = _playerViewInstance != null ? _playerViewInstance.PickupAnchor.position : Vector3.zero;
                PlayResourcePickupFeedback(evt, fallbackPos);
                ApplyResourceDropEffect(evt);
                return;
            }

            var pickupAnchor = _playerViewInstance != null ? _playerViewInstance.PickupAnchor : null;
            var pickupOrigin = view.transform.position;
            PlayResourcePickupFeedback(evt, pickupOrigin);
            if (pickupAnchor != null)
            {
                view.BeginPickupTravel(pickupAnchor, HandleDropTravelCompleted);
            }
            else
            {
                _resourceDropViews.Remove(evt.DropId);
                ReleaseResourceDropView(view);
                ApplyResourceDropEffect(evt);
            }
        }

        private GameObject GetResourceDropPrefab(ResourceDropType type)
        {
            if (resourceDropPrefabs == null)
            {
                return null;
            }

            for (int i = 0; i < resourceDropPrefabs.Length; i++)
            {
                if (resourceDropPrefabs[i].type == type)
                {
                    return resourceDropPrefabs[i].prefab;
                }
            }

            return null;
        }

        private void DestroyResourceDropViews(bool clearPending = true)
        {
            foreach (var pair in _resourceDropViews)
            {
                if (pair.Value != null)
                {
                    ReleaseResourceDropView(pair.Value);
                }
            }

            _resourceDropViews.Clear();
            if (clearPending)
            {
                _pendingDropEffects.Clear();
            }
        }

        private void HandleDropTravelCompleted(int dropId)
        {
            if (_resourceDropViews.TryGetValue(dropId, out var view) && view != null)
            {
                ReleaseResourceDropView(view);
            }

            _resourceDropViews.Remove(dropId);

            if (_pendingDropEffects.TryGetValue(dropId, out var evt))
            {
                ApplyResourceDropEffect(evt);
            }
        }

        private void ApplyResourceDropEffect(ResourceDropCollectedEvent evt)
        {
            _pendingDropEffects.Remove(evt.DropId);
            _gameViewModel?.ApplyResourceDropEffect(evt);
        }

        private void PlayResourcePickupFeedback(ResourceDropCollectedEvent evt, Vector3 worldPosition)
        {
            switch (evt.Type)
            {
                case ResourceDropType.Gold:
                    uiFeedbackManager?.ShowGoldGain(evt.Amount, worldPosition);
                    break;
                case ResourceDropType.Experience:
                    uiFeedbackManager?.ShowExperienceGain(evt.Amount, worldPosition);
                    break;
            }

            fxManager?.PlayResourcePickupFx(worldPosition, evt.Type);
            soundManager?.PlayResourcePickupSound(evt.Type);
        }

        private Vector3 GetDropScatterOffset()
        {
            if (dropScatterRange == Vector2.zero)
            {
                return Vector3.zero;
            }

            var offset = new Vector3(
                UnityEngine.Random.Range(-dropScatterRange.x, dropScatterRange.x),
                UnityEngine.Random.Range(0f, dropScatterRange.y),
                0f);
            return offset;
        }

        private void ApplyMovementBoundsFromColliders()
        {
            var minX = settings.movementMinX;
            var maxX = settings.movementMaxX;

            if (leftBoundaryCollider != null)
            {
                minX = leftBoundaryCollider.bounds.max.x;
            }

            if (rightBoundaryCollider != null)
            {
                maxX = rightBoundaryCollider.bounds.min.x;
            }

            if (minX > maxX)
            {
                var temp = minX;
                minX = maxX;
                maxX = temp;
            }

            settings.movementMinX = minX;
            settings.movementMaxX = maxX;
        }

        private static Float2 MeasureFortressHalfExtents(FortressView fortressView)
        {
            if (fortressView == null)
            {
                return Float2.Zero;
            }

            var box = fortressView.GetComponentInChildren<BoxCollider2D>();
            if (box != null)
            {
                var extents = box.bounds.extents;
                return new Float2(extents.x, extents.y);
            }

            var collider = fortressView.GetComponentInChildren<Collider2D>();
            if (collider != null)
            {
                var extents = collider.bounds.extents;
                return new Float2(extents.x, extents.y);
            }

            return Float2.Zero;
        }

        private void DestroyPlayerView()
        {
            if (_playerViewInstance == null)
            {
                return;
            }

            _playerViewInstance.Unbind();
            Destroy(_playerViewInstance.gameObject);
            _playerViewInstance = null;
        }

        private void DestroyFortressView()
        {
            if (_fortressViewInstance == null)
            {
                return;
            }

            _fortressViewInstance.Unbind();
            Destroy(_fortressViewInstance.gameObject);
            _fortressViewInstance = null;
        }

        private void DestroyEnemyViews()
        {
            foreach (var pair in _enemyViews)
            {
                if (pair.Value != null)
                {
                    ReleaseEnemyView(pair.Value);
                }
            }

            _enemyViews.Clear();
        }

        private void DestroyProjectileViews()
        {
            foreach (var pair in _playerProjectileViews)
            {
                if (pair.Value != null)
                {
                    ReleasePlayerProjectileView(pair.Value);
                }
            }

            _playerProjectileViews.Clear();

            foreach (var pair in _enemyProjectileViews)
            {
                if (pair.Value != null)
                {
                    ReleaseEnemyProjectileView(pair.Value);
                }
            }

            _enemyProjectileViews.Clear();
        }

        private void SubscribeGameViewModelEvents()
        {
            if (_gameViewModel == null)
            {
                return;
            }

            _gameViewModel.EnemySpawned += HandleEnemySpawned;
            _gameViewModel.EnemyRemoved += HandleEnemyRemoved;
            _gameViewModel.FortressDamaged += HandleFortressDamaged;
            _gameViewModel.GameOver += HandleGameOver;
            _gameViewModel.PlayerProjectileFired += HandlePlayerProjectileFired;
            _gameViewModel.EnemyProjectileFired += HandleEnemyProjectileFired;
            _gameViewModel.ProjectileImpactOccurred += HandleProjectileImpact;
            _gameViewModel.ResourceDropSpawned += HandleResourceDropSpawned;
            _gameViewModel.ResourceDropCollected += HandleResourceDropCollected;
            _gameViewModel.AbilityChoicesPresented += HandleAbilityChoicesPresented;
            _gameViewModel.AbilitySelectionCleared += HandleAbilitySelectionCleared;
        }

        private void UnsubscribeGameViewModelEvents()
        {
            if (_gameViewModel == null)
            {
                return;
            }

            _gameViewModel.EnemySpawned -= HandleEnemySpawned;
            _gameViewModel.EnemyRemoved -= HandleEnemyRemoved;
            _gameViewModel.FortressDamaged -= HandleFortressDamaged;
            _gameViewModel.GameOver -= HandleGameOver;
            _gameViewModel.PlayerProjectileFired -= HandlePlayerProjectileFired;
            _gameViewModel.EnemyProjectileFired -= HandleEnemyProjectileFired;
            _gameViewModel.ProjectileImpactOccurred -= HandleProjectileImpact;
            _gameViewModel.ResourceDropSpawned -= HandleResourceDropSpawned;
            _gameViewModel.ResourceDropCollected -= HandleResourceDropCollected;
            _gameViewModel.AbilityChoicesPresented -= HandleAbilityChoicesPresented;
            _gameViewModel.AbilitySelectionCleared -= HandleAbilitySelectionCleared;
        }

        private void HandleEnemyRemoved(int enemyId)
        {
            if (_enemyViews.TryGetValue(enemyId, out var view))
            {
                ReleaseEnemyView(view);
                _enemyViews.Remove(enemyId);
            }
        }

        private void HandleFortressDamaged(float damage)
        {
            Debug.Log($"Fortress damaged: -{damage:F1} HP");
        }

        private void HandleGameOver()
        {
            Debug.Log("Fortress destroyed. Game Over.");
            DestroyEnemyViews();
            DestroyProjectileViews();
            ForceCompleteResourceDropsOnGameOver();
        }

        private void HandleAbilityChoicesPresented(GameplayAbilityDefinition[] choices)
        {
            uiFeedbackManager?.ShowAbilitySelectionOpened();
            fxManager?.PlayAbilitySelectionFx(true);
            soundManager?.PlayAbilitySelectionSound(true);
        }

        private void HandleAbilitySelectionCleared()
        {
            uiFeedbackManager?.ShowAbilitySelectionClosed();
            fxManager?.PlayAbilitySelectionFx(false);
            soundManager?.PlayAbilitySelectionSound(false);
        }

        private EnemyView GetEnemyViewInstance(EnemyView prefab)
        {
            var reference = prefab != null ? prefab : fallbackEnemyViewPrefab;
            if (reference == null)
            {
                return null;
            }

            var parent = enemyContainer != null ? enemyContainer : transform;
            return _enemyViewPoolRegistry.GetInstance(reference, parent, reference.gameObject.name);
        }

        private void ReleaseEnemyView(EnemyView view)
        {
            if (view == null)
            {
                return;
            }

            view.Unbind();
            _enemyViewPoolRegistry.ReleaseInstance(view);
        }

        private ProjectileView GetPlayerProjectileView()
        {
            if (playerProjectilePrefab == null)
            {
                return null;
            }

            var parent = projectileParent != null ? projectileParent : transform;
            return _playerProjectilePoolRegistry.GetInstance(playerProjectilePrefab, parent, playerProjectilePrefab.gameObject.name);
        }

        private void ReleasePlayerProjectileView(ProjectileView view)
        {
            if (view == null)
            {
                return;
            }

            _playerProjectilePoolRegistry.ReleaseInstance(view);
        }

        private ProjectileView GetEnemyProjectileView()
        {
            if (enemyProjectilePrefab == null)
            {
                return null;
            }

            var parent = projectileParent != null ? projectileParent : transform;
            return _enemyProjectilePoolRegistry.GetInstance(enemyProjectilePrefab, parent, enemyProjectilePrefab.gameObject.name);
        }

        private void ReleaseEnemyProjectileView(ProjectileView view)
        {
            if (view == null)
            {
                return;
            }

            _enemyProjectilePoolRegistry.ReleaseInstance(view);
        }

        private ResourceDropView GetResourceDropView(ResourceDropType type)
        {
            var prefab = GetResourceDropPrefab(type);
            if (prefab == null)
            {
                return null;
            }

            var prefabView = prefab.GetComponent<ResourceDropView>();
            if (prefabView == null)
            {
                Debug.LogWarning($"Resource drop prefab for {type} does not contain ResourceDropView.");
                return null;
            }

            var parent = resourceDropParent != null ? resourceDropParent : transform;
            return _resourceDropPoolRegistry.GetInstance(prefabView, parent, type.ToString());
        }

        private void ReleaseResourceDropView(ResourceDropView view)
        {
            if (view == null)
            {
                return;
            }

            view.ResetIdleState();
            _resourceDropPoolRegistry.ReleaseInstance(view);
        }

        private void ClearPools()
        {
            _enemyViewPoolRegistry.Clear();
            _playerProjectilePoolRegistry.Clear();
            _enemyProjectilePoolRegistry.Clear();
            _resourceDropPoolRegistry.Clear();
        }

        private void ForceCompleteResourceDropsOnGameOver()
        {
            if (_pendingDropEffects.Count > 0)
            {
                foreach (var evt in _pendingDropEffects.Values)
                {
                    if (evt.Type == ResourceDropType.Gold)
                    {
                        ApplyResourceDropEffect(evt);
                    }
                }

                _pendingDropEffects.Clear();
            }

            DestroyResourceDropViews(clearPending: false);
        }

        private void HandlePlayerProjectileFired(PlayerProjectileFiredEvent evt)
        {
            var instance = GetPlayerProjectileView();
            if (instance == null)
            {
                return;
            }

            instance.Launch(
                new Vector3(evt.Origin.X, evt.Origin.Y, instance.transform.position.z),
                new Vector3(evt.Target.X, evt.Target.Y, instance.transform.position.z),
                evt.Speed);
            _playerProjectileViews[evt.ProjectileId] = instance;
        }

        private void HandleEnemyProjectileFired(EnemyProjectileFiredEvent evt)
        {
            var instance = GetEnemyProjectileView();
            if (instance == null)
            {
                return;
            }

            instance.Launch(
                new Vector3(evt.Origin.X, evt.Origin.Y, instance.transform.position.z),
                new Vector3(evt.Target.X, evt.Target.Y, instance.transform.position.z),
                evt.Speed);
            _enemyProjectileViews[evt.ProjectileId] = instance;
        }

        private void HandleProjectileImpact(ProjectileImpactEvent evt)
        {
            switch (evt.Faction)
            {
                case ProjectileFaction.Player:
                    if (_playerProjectileViews.TryGetValue(evt.ProjectileId, out var playerProjectile))
                    {
                        playerProjectile.SnapTo(new Vector3(evt.Position.X, evt.Position.Y, playerProjectile.transform.position.z));
                        playerProjectile.Complete(evt.ExplosionRadius);
                        _playerProjectileViews.Remove(evt.ProjectileId);
                        ReleasePlayerProjectileView(playerProjectile);
                    }
                    else
                    {
                        SpawnImpactOnly(playerProjectilePrefab, evt);
                    }
                    break;
                case ProjectileFaction.Enemy:
                    if (_enemyProjectileViews.TryGetValue(evt.ProjectileId, out var enemyProjectile))
                    {
                        enemyProjectile.SnapTo(new Vector3(evt.Position.X, evt.Position.Y, enemyProjectile.transform.position.z));
                        enemyProjectile.Complete(evt.ExplosionRadius);
                        _enemyProjectileViews.Remove(evt.ProjectileId);
                        ReleaseEnemyProjectileView(enemyProjectile);
                    }
                    else
                    {
                        SpawnImpactOnly(enemyProjectilePrefab, evt);
                    }
                    break;
            }
        }

        private void SpawnImpactOnly(ProjectileView prefab, ProjectileImpactEvent evt)
        {
            if (prefab == null)
            {
                return;
            }

            var parent = projectileParent != null ? projectileParent : transform;
            var temp = Instantiate(prefab, parent);
            temp.SnapTo(new Vector3(evt.Position.X, evt.Position.Y, temp.transform.position.z));
            temp.Complete(evt.ExplosionRadius);
        }
    }
}












