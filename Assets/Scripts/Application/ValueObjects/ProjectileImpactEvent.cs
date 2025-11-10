using Noname.Core.Enums;
using Noname.Core.Primitives;

namespace Noname.Application.ValueObjects
{
    /// <summary>
    /// 투사체가 목표 지점에 도달하거나 충돌했음을 나타냅니다.
    /// </summary>
    public readonly struct ProjectileImpactEvent
    {
        public ProjectileImpactEvent(int projectileId, ProjectileFaction faction, Float2 position, float explosionRadius)
        {
            ProjectileId = projectileId;
            Faction = faction;
            Position = position;
            ExplosionRadius = explosionRadius;
        }

        /// <summary>충돌한 투사체 ID.</summary>
        public int ProjectileId { get; }

        /// <summary>투사체 소속.</summary>
        public ProjectileFaction Faction { get; }

        /// <summary>충돌 위치.</summary>
        public Float2 Position { get; }

        /// <summary>폭발 반경.</summary>
        public float ExplosionRadius { get; }
    }
}
