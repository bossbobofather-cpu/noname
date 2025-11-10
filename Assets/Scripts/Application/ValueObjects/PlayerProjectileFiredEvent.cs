using Noname.Core.Primitives;

namespace Noname.Application.ValueObjects
{
    /// <summary>
    /// 플레이어가 투사체를 발사했음을 나타내는 이벤트입니다.
    /// </summary>
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

        /// <summary>투사체 ID.</summary>
        public int ProjectileId { get; }

        /// <summary>발사 위치.</summary>
        public Float2 Origin { get; }

        /// <summary>목표 위치.</summary>
        public Float2 Target { get; }

        /// <summary>초당 이동 속도.</summary>
        public float Speed { get; }

        /// <summary>폭발 반경.</summary>
        public float ExplosionRadius { get; }
    }
}
