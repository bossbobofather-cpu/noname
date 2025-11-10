using Noname.Core.Primitives;

namespace Noname.Application.ValueObjects
{
    public readonly struct PlayerProjectileFiredEvent
    {
        public PlayerProjectileFiredEvent(int projectileId, Float2 origin, Float2 target, float speed, float explosionRadius)
        {
            ProjectileId = projectileId;
            Origin = origin;
            Target = target;
            Speed = speed;
            ExplosionRadius = explosionRadius;
        }

        public int ProjectileId { get; }
        public Float2 Origin { get; }
        public Float2 Target { get; }
        public float Speed { get; }
        public float ExplosionRadius { get; }
    }
}
