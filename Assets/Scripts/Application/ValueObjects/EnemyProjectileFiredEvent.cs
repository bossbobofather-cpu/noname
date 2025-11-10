using Noname.Core.Primitives;

namespace Noname.Application.ValueObjects
{
    public readonly struct EnemyProjectileFiredEvent
    {
        public EnemyProjectileFiredEvent(int projectileId, int sourceEnemyId, Float2 origin, Float2 target, float speed)
        {
            ProjectileId = projectileId;
            SourceEnemyId = sourceEnemyId;
            Origin = origin;
            Target = target;
            Speed = speed;
        }

        public int ProjectileId { get; }
        public int SourceEnemyId { get; }
        public Float2 Origin { get; }
        public Float2 Target { get; }
        public float Speed { get; }
    }
}
