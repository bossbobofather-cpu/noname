using Noname.Core.Enums;
using Noname.Core.Primitives;

namespace Noname.Core.Entities
{
    public sealed class ResourceDropEntity
    {
        public ResourceDropEntity(int id, ResourceDropType type, Float2 position, float amount, float delay)
        {
            Id = id;
            Type = type;
            Position = position;
            Amount = amount;
            RemainingDelay = delay;
        }

        public int Id { get; }
        public ResourceDropType Type { get; }
        public Float2 Position { get; }
        public float Amount { get; }
        public float RemainingDelay { get; private set; }

        public bool Tick(float deltaTime)
        {
            RemainingDelay -= deltaTime;
            return RemainingDelay <= 0f;
        }
    }
}
