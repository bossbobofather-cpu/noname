using System;

namespace Noname.Core.Entities
{
    /// <summary>
    /// 플레이어가 방어해야 하는 성벽(거점)의 체력 상태를 관리합니다.
    /// </summary>
    public sealed class FortressEntity
    {
        public FortressEntity(float maxHealth)
        {
            MaxHealth = MathF.Max(1f, maxHealth);
            CurrentHealth = MaxHealth;
        }

        public float MaxHealth { get; }
        public float CurrentHealth { get; private set; }
        public bool IsDestroyed => CurrentHealth <= 0f;

        public void Reset()
        {
            CurrentHealth = MaxHealth;
        }

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
