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
using UnityEngine;

namespace Noname.Presentation.ViewModels
{
    public sealed class GameViewModel
    {
        private const int AbilityChoicesCount = 3;
        private const float InitialAbilityDelaySeconds = 1f;

        private readonly StartGameUseCase _startGame;
        private readonly MovePlayerUseCase _movePlayer;
        private readonly DefenseSimulationService _simulationService;
        private readonly IGameInputReader _inputReader;
        private readonly IGameStateRepository _repository;
        private readonly System.Random _random = new System.Random();

        private DefenseGameSettings _settings;
        private float _bombardmentReleaseRadiusSq;
        private bool _isPausedForAbilitySelection;
        private GameplayAbilityDefinition[] _currentAbilityChoices;
        private bool _initialAbilityPresented;
        private float _initialAbilityDelayRemaining;
        private int _pendingAbilitySelections;

        public GameViewModel(
            StartGameUseCase startGame,
            MovePlayerUseCase movePlayer,
            DefenseSimulationService simulationService,
            IGameInputReader inputReader,
            IGameStateRepository repository)
        {
            _startGame = startGame;
            _movePlayer = movePlayer;
            _simulationService = simulationService;
            _inputReader = inputReader;
            _repository = repository;
        }

        public event Action<Float2> PlayerPositionChanged;
        public event Action<float, float> FortressHealthChanged;
        public event Action<EnemyEntity> EnemySpawned;
        public event Action<int> EnemyRemoved;
        public event Action<int, Float2> EnemyPositionChanged;
        public event Action<int, float> EnemyHealthChanged;
        public event Action<float> FortressDamaged;
        public event Action<PlayerProjectileFiredEvent> PlayerProjectileFired;
        public event Action<EnemyProjectileFiredEvent> EnemyProjectileFired;
        public event Action<ProjectileImpactEvent> ProjectileImpactOccurred;
        public event Action<ResourceDropSpawnedEvent> ResourceDropSpawned;
        public event Action<ResourceDropCollectedEvent> ResourceDropCollected;
        public event Action<float, float, int> PlayerExperienceChanged;
        public event Action<GameplayAbilityDefinition[]> AbilityChoicesPresented;
        public event Action AbilitySelectionCleared;
        public event Action<GameViewPauseReason> GamePaused;
        public event Action<GameViewPauseReason> GameResumed;
        public event Action GameOver;
        public event Action<Float2?> BombardmentPointChanged;

        public void Initialize(DefenseGameSettings settings)
        {
            _settings = settings;

            _startGame.Execute();
            var state = _repository.State;
            state.Player.SetHorizontalBounds(settings.movementMinX, settings.movementMaxX);
            state.Player.Reset();
            state.Fortress.Reset();
            state.ClearEnemies();
            state.ClearFixedBombardment();

            var releaseRadius = MathF.Max(0.25f, settings.playerExplosionRadius * 0.5f);
            _bombardmentReleaseRadiusSq = releaseRadius * releaseRadius;

            _initialAbilityDelayRemaining = InitialAbilityDelaySeconds;
            _initialAbilityPresented = false;
            _isPausedForAbilitySelection = false;
            _currentAbilityChoices = null;
            _pendingAbilitySelections = 0;

            PlayerPositionChanged?.Invoke(state.Player.Position);
            FortressHealthChanged?.Invoke(state.Fortress.CurrentHealth, state.Fortress.MaxHealth);
            PlayerExperienceChanged?.Invoke(
                state.Player.CurrentExperience,
                state.Player.ExperienceForNextLevel,
                state.Player.Level);
            BombardmentPointChanged?.Invoke(null);
        }

        public void Tick(float deltaTime)
        {
            var state = _repository.State;
            if (state.IsGameOver)
            {
                return;
            }

            UpdateInitialAbilityOffer(deltaTime);

            if (_isPausedForAbilitySelection)
            {
                return;
            }

            var movement = _inputReader.ReadMovement();
            _movePlayer.Execute(movement, deltaTime);
            PlayerPositionChanged?.Invoke(state.Player.Position);

            ProcessBombardmentInput(state);

            var result = _simulationService.Tick(deltaTime);

            foreach (var enemy in result.SpawnedEnemies)
            {
                EnemySpawned?.Invoke(enemy);
            }

            foreach (var id in result.RemovedEnemyIds)
            {
                EnemyRemoved?.Invoke(id);
            }

            foreach (var attack in result.EnemyAttacks)
            {
                FortressDamaged?.Invoke(attack.Damage);
                FortressHealthChanged?.Invoke(attack.FortressRemainingHealth, state.Fortress.MaxHealth);
            }

            foreach (var hit in result.PlayerProjectileHits)
            {
                EnemyHealthChanged?.Invoke(hit.EnemyId, hit.RemainingHealth);
            }

            foreach (var evt in result.PlayerProjectilesFired)
            {
                PlayerProjectileFired?.Invoke(evt);
            }

            foreach (var evt in result.EnemyProjectilesFired)
            {
                EnemyProjectileFired?.Invoke(evt);
            }

            foreach (var impact in result.ProjectileImpacts)
            {
                ProjectileImpactOccurred?.Invoke(impact);
            }

            foreach (var spawn in result.ResourceDropsSpawned)
            {
                ResourceDropSpawned?.Invoke(spawn);
            }

            foreach (var collected in result.ResourceDropsCollected)
            {
                ResourceDropCollected?.Invoke(collected);
            }

            foreach (var enemy in state.Enemies)
            {
                EnemyPositionChanged?.Invoke(enemy.Id, enemy.Position);
            }

            if (state.Fortress.IsDestroyed)
            {
                GameOver?.Invoke();
                return;
            }
        }

        public void SelectAbility(int index)
        {
            if (!_isPausedForAbilitySelection || _currentAbilityChoices == null || index < 0 || index >= _currentAbilityChoices.Length)
            {
                return;
            }

            var choice = _currentAbilityChoices[index];
            ApplyAbility(_repository.State.Player, choice);
            _currentAbilityChoices = null;
            _isPausedForAbilitySelection = false;
            AbilitySelectionCleared?.Invoke();
            GameResumed?.Invoke(GameViewPauseReason.AwaitAbilitySelection);

            var player = _repository.State.Player;
            PlayerExperienceChanged?.Invoke(player.CurrentExperience, player.ExperienceForNextLevel, player.Level);

            if (_pendingAbilitySelections > 0)
            {
                _pendingAbilitySelections = Math.Max(0, _pendingAbilitySelections - 1);
                TryPresentAbilityChoices();
            }
        }

        public bool ForceAbilitySelection()
        {
            if (_pendingAbilitySelections <= 0)
            {
                _pendingAbilitySelections = 1;
            }

            return TryPresentAbilityChoices();
        }

        public void ApplyResourceDropEffect(ResourceDropCollectedEvent evt)
        {
            var player = _repository.State.Player;
            switch (evt.Type)
            {
                case ResourceDropType.Experience:
                    var leveledUp = player.AddExperience(evt.Amount);
                    PlayerExperienceChanged?.Invoke(player.CurrentExperience, player.ExperienceForNextLevel, player.Level);
                    if (leveledUp)
                    {
                        _pendingAbilitySelections++;
                        TryPresentAbilityChoices();
                    }
                    break;
                case ResourceDropType.Gold:
                    player.AddGold(evt.Amount);
                    break;
                case ResourceDropType.Health:
                    player.Heal(evt.Amount);
                    break;
                case ResourceDropType.Ability:
                    _pendingAbilitySelections++;
                    TryPresentAbilityChoices();
                    break;
            }
        }

        private void UpdateInitialAbilityOffer(float deltaTime)
        {
            if (_initialAbilityPresented)
            {
                return;
            }

            _initialAbilityDelayRemaining -= deltaTime;
            if (_initialAbilityDelayRemaining > 0f)
            {
                return;
            }

            _initialAbilityPresented = true;
            _pendingAbilitySelections = Math.Max(1, _pendingAbilitySelections);
            TryPresentAbilityChoices();
        }

        private bool TryPresentAbilityChoices()
        {
            if (_isPausedForAbilitySelection || _pendingAbilitySelections <= 0)
            {
                return false;
            }

            var pool = _settings.abilityPool;
            if (pool == null || pool.Length < AbilityChoicesCount)
            {
                Debug.LogWarning($"Ability pool은 최소 {AbilityChoicesCount}개의 어빌리티가 등록되어야 합니다.");
                return false;
            }

            var choices = new List<GameplayAbilityDefinition>(AbilityChoicesCount);
            var usedIndices = new HashSet<int>();

            while (choices.Count < AbilityChoicesCount)
            {
                var index = _random.Next(pool.Length);
                if (usedIndices.Add(index))
                {
                    choices.Add(pool[index]);
                }
            }

            _currentAbilityChoices = choices.ToArray();
            _isPausedForAbilitySelection = true;
            GamePaused?.Invoke(GameViewPauseReason.AwaitAbilitySelection);
            AbilityChoicesPresented?.Invoke(_currentAbilityChoices);
            return true;
        }

        private static void ApplyAbility(PlayerEntity player, GameplayAbilityDefinition ability)
        {
            player.ApplyAbility(ability);
        }

        private void ProcessBombardmentInput(GameState state)
        {
            if (!_inputReader.TryReadBombardmentPoint(out var pointerPosition))
            {
                return;
            }

            var playerPosition = state.Player.Position;
            var rangeSq = state.Player.AttackRange * state.Player.AttackRange;

            if (!state.HasFixedBombardment)
            {
                if ((pointerPosition - playerPosition).SqrMagnitude > rangeSq)
                {
                    return;
                }

                state.SetFixedBombardment(pointerPosition);
                BombardmentPointChanged?.Invoke(pointerPosition);
                return;
            }

            var delta = pointerPosition - state.FixedBombardmentPosition;
            if (delta.SqrMagnitude <= _bombardmentReleaseRadiusSq)
            {
                state.ClearFixedBombardment();
                BombardmentPointChanged?.Invoke(null);
            }
        }
    }

    public enum GameViewPauseReason
    {
        AwaitAbilitySelection
    }
}
