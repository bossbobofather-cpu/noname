using Noname.Core.Enums;
using Noname.Core.Primitives;

namespace Noname.Core.Entities
{
    /// <summary>
    /// 드롭 타입, 수량, 스폰 지연 시간을 보관하는 자원 드롭 모델입니다.
    /// </summary>
    public sealed class ResourceDropEntity
    {
        /// <summary>
        /// 드롭 정보를 초기화합니다.
        /// </summary>
        public ResourceDropEntity(int id, ResourceDropType type, Float2 position, float amount, float delay)
        {
            Id = id;
            Type = type;
            Position = position;
            Amount = amount;
            RemainingDelay = delay;
        }

        /// <summary>드롭 고유 ID입니다.</summary>
        public int Id { get; }

        /// <summary>드롭 종류입니다.</summary>
        public ResourceDropType Type { get; }

        /// <summary>생성 위치입니다.</summary>
        public Float2 Position { get; }

        /// <summary>지급량 혹은 개수입니다.</summary>
        public float Amount { get; }

        /// <summary>드롭이 활성화되기까지 남은 시간입니다.</summary>
        public float RemainingDelay { get; private set; }

        /// <summary>
        /// 지연 시간을 감소시키고, 0 이하가 되면 true를 반환합니다.
        /// </summary>
        public bool Tick(float deltaTime)
        {
            RemainingDelay -= deltaTime;
            return RemainingDelay <= 0f;
        }
    }
}
