using Noname.Core.Enums;
using Noname.Core.Primitives;

namespace Noname.Application.ValueObjects
{
    public readonly struct ProjectileImpactEvent
    {
        public ProjectileImpactEvent(int projectileId, ProjectileFaction faction, Float2 position, float explosionRadius)
        {
            ProjectileId = projectileId;
            Faction = faction;
            Position = position;
            ExplosionRadius = explosionRadius;
        }

        public int ProjectileId { get; }
        public ProjectileFaction Faction { get; }
        public Float2 Position { get; }
        public float ExplosionRadius { get; }
    }
}
