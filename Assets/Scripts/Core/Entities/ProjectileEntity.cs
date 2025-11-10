using System;
using Noname.Core.Enums;
using Noname.Core.Primitives;

namespace Noname.Core.Entities
{
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

        public int Id { get; }
        public int SourceId { get; }
        public ProjectileFaction Faction { get; }
        public Float2 Position { get; private set; }
        public Float2 Velocity { get; private set; }
        public float Damage { get; }
        public float ExplosionRadius { get; }
        public Float2 TargetPosition { get; }

        public void Advance(float deltaTime)
        {
            Position += Velocity * deltaTime;
        }

        public bool HasReachedTarget()
        {
            var toTarget = TargetPosition - Position;
            return Float2.Dot(toTarget, Velocity) <= 0f;
        }

        public void SetVelocity(Float2 velocity)
        {
            Velocity = velocity;
        }
    }
}
