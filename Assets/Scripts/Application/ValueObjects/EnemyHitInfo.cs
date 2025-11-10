namespace Noname.Application.ValueObjects
{
    /// <summary>
    /// 플레이어 투사체가 적에게 준 피해 정보를 나타냅니다.
    /// </summary>
    public readonly struct EnemyHitInfo
    {
        public EnemyHitInfo(int enemyId, float damageDealt, float remainingHealth)
        {
            EnemyId = enemyId;
            DamageDealt = damageDealt;
            RemainingHealth = remainingHealth;
        }

        /// <summary>피해를 받은 적 ID.</summary>
        public int EnemyId { get; }

        /// <summary>이번 타격으로 준 피해량.</summary>
        public float DamageDealt { get; }

        /// <summary>타격 후 남은 체력.</summary>
        public float RemainingHealth { get; }
    }
}
