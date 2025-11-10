using System;

namespace Noname.Core.Entities
{
    /// <summary>
    /// 플레이어가 지켜야 하는 거점(성벽)의 체력 상태를 관리합니다.
    /// </summary>
    public sealed class FortressEntity
    {
        /// <summary>
        /// 거점을 지정된 최대 체력으로 초기화합니다.
        /// </summary>
        /// <param name="maxHealth">거점 최대 체력.</param>
        public FortressEntity(float maxHealth)
        {
            MaxHealth = MathF.Max(1f, maxHealth);
            CurrentHealth = MaxHealth;
        }

        /// <summary>
        /// 거점이 가질 수 있는 최대 체력입니다.
        /// </summary>
        public float MaxHealth { get; }

        /// <summary>
        /// 현재 체력입니다.
        /// </summary>
        public float CurrentHealth { get; private set; }

        /// <summary>
        /// 체력이 0 이하인지 여부를 반환합니다.
        /// </summary>
        public bool IsDestroyed => CurrentHealth <= 0f;

        /// <summary>
        /// 체력을 최대치로 되돌립니다.
        /// </summary>
        public void Reset()
        {
            CurrentHealth = MaxHealth;
        }

        /// <summary>
        /// 피해를 적용하고 실제 감소한 양을 돌려줍니다.
        /// </summary>
        public float ApplyDamage(float amount)
        {
            if (amount <= 0f || IsDestroyed)
            {
                return 0f;
            }

            var previous = CurrentHealth;
            CurrentHealth = MathF.Max(0f, CurrentHealth - amount);
            return previous - CurrentHealth;
        }

        /// <summary>
        /// 회복량을 적용하고 실제 회복된 양을 돌려줍니다.
        /// </summary>
        public float Heal(float amount)
        {
            if (amount <= 0f || IsDestroyed)
            {
                return 0f;
            }

            var previous = CurrentHealth;
            CurrentHealth = MathF.Min(MaxHealth, CurrentHealth + amount);
            return CurrentHealth - previous;
        }
    }
}
