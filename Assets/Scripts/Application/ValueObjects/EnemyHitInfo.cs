namespace Noname.Application.ValueObjects
{
    public readonly struct EnemyHitInfo
    {
        public EnemyHitInfo(int enemyId, float damageDealt, float remainingHealth)
        {
            EnemyId = enemyId;
            DamageDealt = damageDealt;
            RemainingHealth = remainingHealth;
        }

        public int EnemyId { get; }
        public float DamageDealt { get; }
        public float RemainingHealth { get; }
    }
}
