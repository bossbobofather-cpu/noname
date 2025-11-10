using System;
using Noname.Core.Primitives;
using Noname.Core.ValueObjects;

namespace Noname.Core.Entities
{
    /// <summary>
    /// 플레이어 캐릭터의 위치, 전투 스탯, 성장 정보를 관리합니다.
    /// </summary>
    public sealed class PlayerEntity
    {
        private float _cooldownRemaining;
        private float _minX;
        private float _maxX;
        private float _moveSpeed;
        private float _attackDamage;
        private float _attackRange;
        private float _attackCooldown;
        private float _experienceGrowthFactor;
        private float _maxHealth;
        private float _currentHealth;
        private float _luck;
        private float _gold;

        public PlayerEntity(Float2 spawnPosition, float moveSpeed, float attackDamage, float attackRange, float attackCooldown, float maxHealth, float luck)
        {
            SpawnPosition = spawnPosition;
            Position = spawnPosition;
            _moveSpeed = MathF.Max(0f, moveSpeed);
            _attackDamage = MathF.Max(0f, attackDamage);
            _attackRange = MathF.Max(0f, attackRange);
            _attackCooldown = MathF.Max(0f, attackCooldown);
            _cooldownRemaining = 0f;
            _maxHealth = MathF.Max(1f, maxHealth);
            _currentHealth = _maxHealth;
            _luck = MathF.Max(0f, luck);
            _gold = 0f;

            Level = 1;
            CurrentExperience = 0f;
            ExperienceForNextLevel = 100f;
            _experienceGrowthFactor = 1.25f;
        }

        public Float2 SpawnPosition { get; }
        public Float2 Position { get; private set; }
        public float MoveSpeed => _moveSpeed;
        public float AttackDamage => _attackDamage;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public int Level { get; private set; }
        public float CurrentExperience { get; private set; }
        public float ExperienceForNextLevel { get; private set; }
        public bool CanAttack => _cooldownRemaining <= 0f;
        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public float Luck => _luck;
        public float Gold => _gold;

        public void SetHorizontalBounds(float minX, float maxX)
        {
            if (minX > maxX)
            {
                (_minX, _maxX) = (maxX, minX);
            }
            else
            {
                _minX = minX;
                _maxX = maxX;
            }
        }

        public void Reset()
        {
            Position = SpawnPosition;
            _cooldownRemaining = 0f;
            CurrentExperience = 0f;
            Level = 1;
            _currentHealth = _maxHealth;
            _gold = 0f;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (_cooldownRemaining <= 0f)
            {
                return;
            }

            _cooldownRemaining = MathF.Max(0f, _cooldownRemaining - deltaTime);
        }

        public void Move(float horizontalInput, float deltaTime)
        {
            var displacement = _moveSpeed * horizontalInput * deltaTime;
            var nextX = Position.X + displacement;
            nextX = MathF.Max(_minX, MathF.Min(_maxX, nextX));
            Position = new Float2(nextX, Position.Y);
        }

        public void StartAttack()
        {
            _cooldownRemaining = _attackCooldown;
        }

        public Float2 GetAttackOrigin()
        {
            return Position;
        }

        public void ConfigureExperienceProgression(float startingThreshold, float growthFactor, int startingLevel = 1)
        {
            Level = Math.Max(1, startingLevel);
            CurrentExperience = 0f;
            ExperienceForNextLevel = MathF.Max(1f, startingThreshold);
            _experienceGrowthFactor = MathF.Max(1f, growthFactor);
        }

        public bool AddExperience(float amount)
        {
            if (amount <= 0f)
            {
                return false;
            }

            CurrentExperience += amount;
            var leveledUp = false;

            while (CurrentExperience >= ExperienceForNextLevel)
            {
                CurrentExperience -= ExperienceForNextLevel;
                Level++;
                ExperienceForNextLevel *= _experienceGrowthFactor;
                leveledUp = true;
            }

            return leveledUp;
        }

        public void ApplyAbility(GameplayAbilityDefinition ability)
        {
            ability?.Apply(this);
        }

        public void ApplyModifier(GameplayAttribute attribute, ModifierOperation operation, float value)
        {
            switch (attribute)
            {
                case GameplayAttribute.AttackDamage:
                    _attackDamage = ApplyOperation(_attackDamage, operation, value, minValue: 0f);
                    break;
                case GameplayAttribute.AttackCooldown:
                    _attackCooldown = ApplyOperation(_attackCooldown, operation, value, minValue: 0.05f);
                    break;
                case GameplayAttribute.MoveSpeed:
                    _moveSpeed = ApplyOperation(_moveSpeed, operation, value, minValue: 0f);
                    break;
                case GameplayAttribute.AttackRange:
                    _attackRange = ApplyOperation(_attackRange, operation, value, minValue: 0f);
                    break;
            }
        }

        private static float ApplyOperation(float current, ModifierOperation operation, float value, float minValue)
        {
            switch (operation)
            {
                case ModifierOperation.Add:
                    return MathF.Max(minValue, current + value);
                case ModifierOperation.Multiply:
                    return MathF.Max(minValue, current * MathF.Max(0.01f, value));
                default:
                    return current;
            }
        }

        public void SetLuck(float luck)
        {
            _luck = MathF.Max(0f, luck);
        }

        public void AddGold(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            _gold += amount;
        }

        public float Heal(float amount)
        {
            if (amount <= 0f)
            {
                return 0f;
            }

            var previous = _currentHealth;
            _currentHealth = MathF.Min(_maxHealth, _currentHealth + amount);
            return _currentHealth - previous;
        }

        public float ApplyDamage(float amount)
        {
            if (amount <= 0f)
            {
                return 0f;
            }

            var previous = _currentHealth;
            _currentHealth = MathF.Max(0f, _currentHealth - amount);
            return previous - _currentHealth;
        }
    }
}
