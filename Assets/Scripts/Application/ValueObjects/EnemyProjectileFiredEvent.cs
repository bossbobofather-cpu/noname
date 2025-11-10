using Noname.Core.Primitives;

namespace Noname.Application.ValueObjects
{
    /// <summary>
    /// 적이 투사체를 발사했을 때 전달되는 이벤트입니다.
    /// </summary>
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

        /// <summary>생성된 투사체 ID.</summary>
        public int ProjectileId { get; }

        /// <summary>발사한 적 ID.</summary>
        public int SourceEnemyId { get; }

        /// <summary>시작 위치.</summary>
        public Float2 Origin { get; }

        /// <summary>목표 위치.</summary>
        public Float2 Target { get; }

        /// <summary>발사 속도.</summary>
        public float Speed { get; }
    }
}
