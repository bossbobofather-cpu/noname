using System;
using Noname.Core.Enums;
using Noname.Core.Primitives;

namespace Noname.Core.Entities
{
    /// <summary>
    /// 투사체의 위치, 속도, 피해량 등을 표현하는 도메인 모델입니다.
    /// </summary>
    public sealed class ProjectileEntity
    {
        public ProjectileEntity(
            int id,
            int sourceId,
            ProjectileFaction faction,
            Float2 position,
            Float2 velocity,
            float damage,
            float explosionRadius,
            Float2 targetPosition)
        {
            Id = id;
            SourceId = sourceId;
            Faction = faction;
            Position = position;
            Velocity = velocity;
            Damage = MathF.Max(0f, damage);
            ExplosionRadius = MathF.Max(0f, explosionRadius);
            TargetPosition = targetPosition;
        }

        /// <summary>투사체 고유 ID입니다.</summary>
        public int Id { get; }

        /// <summary>발사체를 생성한 주체(플레이어 혹은 적)의 ID입니다.</summary>
        public int SourceId { get; }

        /// <summary>플레이어/적 진영 정보입니다.</summary>
        public ProjectileFaction Faction { get; }

        /// <summary>현재 위치입니다.</summary>
        public Float2 Position { get; private set; }

        /// <summary>현재 속도입니다.</summary>
        public Float2 Velocity { get; private set; }

        /// <summary>기본 피해량입니다.</summary>
        public float Damage { get; }

        /// <summary>폭발 반경(0이면 직접 명중형)입니다.</summary>
        public float ExplosionRadius { get; }

        /// <summary>추적 중인 목표 좌표입니다.</summary>
        public Float2 TargetPosition { get; }

        /// <summary>
        /// 델타 타임만큼 이동합니다.
        /// </summary>
        public void Advance(float deltaTime)
        {
            Position += Velocity * deltaTime;
        }

        /// <summary>
        /// 목표 좌표를 지나쳤는지 여부를 반환합니다.
        /// </summary>
        public bool HasReachedTarget()
        {
            var toTarget = TargetPosition - Position;
            return Float2.Dot(toTarget, Velocity) <= 0f;
        }

        /// <summary>
        /// 속도를 갱신합니다.
        /// </summary>
        public void SetVelocity(Float2 velocity)
        {
            Velocity = velocity;
        }
    }
}
