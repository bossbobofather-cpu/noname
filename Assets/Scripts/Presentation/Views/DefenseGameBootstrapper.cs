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
        [SerializeField] private EnemyView meleeEnemyViewPrefab;
        [SerializeField] private EnemyView rangedEnemyViewPrefab;
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
            enemySpawnMin: new Float2(-6f, 6f),
            enemySpawnMax: new Float2(6f, 7.5f),
            initialSpawnDelay: 1.5f,
            spawnInterval: 2.5f,
            enemySpawnEntries: System.Array.Empty<EnemySpawnEntry>(),
            enemyProjectileSpeed: 8f);

        private GameViewModel _gameViewModel;
        private InMemoryGameStateRepository _repository;
        private readonly Dictionary<int, EnemyView> _enemyViews = new Dictionary<int, EnemyView>();
        private readonly Dictionary<int, ProjectileView> _playerProjectileViews = new Dictionary<int, ProjectileView>();
        private readonly Dictionary<int, ProjectileView> _enemyProjectileViews = new Dictionary<int, ProjectileView>();
        private readonly Dictionary<int, GameObject> _resourceDropViews = new Dictionary<int, GameObject>();
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

            var gameState = new GameState(
                playerEntity,
                fortressEntity,
                settings.enemySpawnMin,
                settings.enemySpawnMax);

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
            EnemyView prefab = null;
            switch (entity.Role)
            {
                case EnemyCombatRole.Melee:
                    prefab = meleeEnemyViewPrefab;
                    break;
                case EnemyCombatRole.Ranged:
                    prefab = rangedEnemyViewPrefab != null ? rangedEnemyViewPrefab : meleeEnemyViewPrefab;
                    break;
            }

            if (prefab == null)
            {
                Debug.LogWarning($"Enemy prefab for role {entity.Role} is not assigned.");
                return;
            }

            var container = enemyContainer != null ? enemyContainer : transform;
            var view = Instantiate(prefab, container);
            view.gameObject.SetActive(true);
            view.transform.position = new Vector3(entity.Position.X, entity.Position.Y, view.transform.position.z);
            view.Bind(_gameViewModel, entity);
            _enemyViews[entity.Id] = view;
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
            var prefab = GetResourceDropPrefab(evt.Type);
            if (prefab == null)
            {
                return;
            }

            var parent = resourceDropParent != null ? resourceDropParent : transform;
            var instance = Instantiate(prefab, parent);
            instance.transform.position = new Vector3(evt.Position.X, evt.Position.Y, instance.transform.position.z) + GetDropScatterOffset();

            var view = instance.GetComponent<ResourceDropView>();
            if (view != null)
            {
                view.Initialize(evt.DropId, evt.Type);
            }

            _resourceDropViews[evt.DropId] = instance;
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

            var dropView = view.GetComponent<ResourceDropView>();
            var pickupAnchor = _playerViewInstance != null ? _playerViewInstance.PickupAnchor : null;
            var pickupOrigin = view.transform.position;
            PlayResourcePickupFeedback(evt, pickupOrigin);
            if (dropView != null && pickupAnchor != null)
            {
                dropView.BeginPickupTravel(pickupAnchor, HandleDropTravelCompleted);
            }
            else
            {
                Destroy(view);
                _resourceDropViews.Remove(evt.DropId);
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

        private void DestroyResourceDropViews()
        {
            foreach (var pair in _resourceDropViews)
            {
                if (pair.Value != null)
                {
                    Destroy(pair.Value);
                }
            }

            _resourceDropViews.Clear();
            _pendingDropEffects.Clear();
        }

        private void HandleDropTravelCompleted(int dropId)
        {
            if (_resourceDropViews.TryGetValue(dropId, out var view) && view != null)
            {
                Destroy(view);
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
                    pair.Value.Unbind();
                    Destroy(pair.Value.gameObject);
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
                    Destroy(pair.Value.gameObject);
                }
            }

            _playerProjectileViews.Clear();

            foreach (var pair in _enemyProjectileViews)
            {
                if (pair.Value != null)
                {
                    Destroy(pair.Value.gameObject);
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
                view.Unbind();
                Destroy(view.gameObject);
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

        private void HandlePlayerProjectileFired(PlayerProjectileFiredEvent evt)
        {
            if (playerProjectilePrefab == null)
            {
                return;
            }

            var parent = projectileParent != null ? projectileParent : transform;
            var instance = Instantiate(playerProjectilePrefab, parent);
            instance.Launch(
                new Vector3(evt.Origin.X, evt.Origin.Y, instance.transform.position.z),
                new Vector3(evt.Target.X, evt.Target.Y, instance.transform.position.z),
                evt.Speed);
            _playerProjectileViews[evt.ProjectileId] = instance;
        }

        private void HandleEnemyProjectileFired(EnemyProjectileFiredEvent evt)
        {
            if (enemyProjectilePrefab == null)
            {
                return;
            }

            var parent = projectileParent != null ? projectileParent : transform;
            var instance = Instantiate(enemyProjectilePrefab, parent);
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












