using Noname.Core.Enums;

namespace Noname.Application.ValueObjects
{
    public readonly struct ResourceDropCollectedEvent
    {
        public ResourceDropCollectedEvent(int dropId, ResourceDropType type, float amount)
        {
            DropId = dropId;
            Type = type;
            Amount = amount;
        }

        public int DropId { get; }
        public ResourceDropType Type { get; }
        public float Amount { get; }
    }
}
