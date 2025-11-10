using System;
using Noname.Core.Primitives;
using Noname.Core.ValueObjects;

namespace Noname.Core.Entities
{
    /// <summary>
    /// 플레이어 캐릭터의 위치, 전투 스탯, 성장 상태를 관리합니다.
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

        /// <summary>
        /// 플레이어를 기본 스탯과 스폰 위치로 초기화합니다.
        /// </summary>
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

        /// <summary>스폰 위치입니다.</summary>
        public Float2 SpawnPosition { get; }

        /// <summary>현재 위치입니다.</summary>
        public Float2 Position { get; private set; }

        /// <summary>이동 속도입니다.</summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary>기본 공격력입니다.</summary>
        public float AttackDamage => _attackDamage;

        /// <summary>공격 사거리입니다.</summary>
        public float AttackRange => _attackRange;

        /// <summary>공격 쿨다운(초)입니다.</summary>
        public float AttackCooldown => _attackCooldown;

        /// <summary>현재 레벨입니다.</summary>
        public int Level { get; private set; }

        /// <summary>누적 경험치입니다.</summary>
        public float CurrentExperience { get; private set; }

        /// <summary>다음 레벨업까지 필요한 경험치입니다.</summary>
        public float ExperienceForNextLevel { get; private set; }

        /// <summary>즉시 공격 가능 여부입니다.</summary>
        public bool CanAttack => _cooldownRemaining <= 0f;

        /// <summary>최대 체력입니다.</summary>
        public float MaxHealth => _maxHealth;

        /// <summary>현재 체력입니다.</summary>
        public float CurrentHealth => _currentHealth;

        /// <summary>운(Luck) 수치입니다.</summary>
        public float Luck => _luck;

        /// <summary>보유 골드입니다.</summary>
        public float Gold => _gold;

        /// <summary>
        /// 좌우 이동 한계를 설정합니다.
        /// </summary>
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

        /// <summary>
        /// 위치/쿨다운/경험치/골드를 초기화합니다.
        /// </summary>
        public void Reset()
        {
            Position = SpawnPosition;
            _cooldownRemaining = 0f;
            CurrentExperience = 0f;
            Level = 1;
            _currentHealth = _maxHealth;
            _gold = 0f;
        }

        /// <summary>
        /// 공격 쿨다운을 감소시킵니다.
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            if (_cooldownRemaining <= 0f)
            {
                return;
            }

            _cooldownRemaining = MathF.Max(0f, _cooldownRemaining - deltaTime);
        }

        /// <summary>
        /// 수평 입력에 따라 이동합니다.
        /// </summary>
        public void Move(float horizontalInput, float deltaTime)
        {
            var displacement = _moveSpeed * horizontalInput * deltaTime;
            var nextX = Position.X + displacement;
            nextX = MathF.Max(_minX, MathF.Min(_maxX, nextX));
            Position = new Float2(nextX, Position.Y);
        }

        /// <summary>
        /// 공격을 시작하고 쿨다운을 설정합니다.
        /// </summary>
        public void StartAttack()
        {
            _cooldownRemaining = _attackCooldown;
        }

        /// <summary>
        /// 공격이 발사되는 위치를 반환합니다.
        /// </summary>
        public Float2 GetAttackOrigin()
        {
            return Position;
        }

        /// <summary>
        /// 레벨업 곡선을 초기화합니다.
        /// </summary>
        public void ConfigureExperienceProgression(float startingThreshold, float growthFactor, int startingLevel = 1)
        {
            Level = Math.Max(1, startingLevel);
            CurrentExperience = 0f;
            ExperienceForNextLevel = MathF.Max(1f, startingThreshold);
            _experienceGrowthFactor = MathF.Max(1f, growthFactor);
        }

        /// <summary>
        /// 경험치를 더하고, 레벨업 여부를 반환합니다.
        /// </summary>
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

        /// <summary>
        /// 게임 플레이 어빌리티를 적용합니다.
        /// </summary>
        public void ApplyAbility(GameplayAbilityDefinition ability)
        {
            ability?.Apply(this);
        }

        /// <summary>
        /// 개별 속성에 대한 수치 변화를 적용합니다.
        /// </summary>
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

        /// <summary>
        /// 운 수치를 설정합니다.
        /// </summary>
        public void SetLuck(float luck)
        {
            _luck = MathF.Max(0f, luck);
        }

        /// <summary>
        /// 골드를 추가합니다.
        /// </summary>
        public void AddGold(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            _gold += amount;
        }

        /// <summary>
        /// 체력을 회복시키고 회복량을 반환합니다.
        /// </summary>
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

        /// <summary>
        /// 피해를 적용하고 실제 감소량을 반환합니다.
        /// </summary>
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
