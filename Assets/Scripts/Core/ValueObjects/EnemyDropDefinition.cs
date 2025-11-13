using Noname.Core.Enums;
using UnityEngine;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 적이 드롭할 자원 종류와 확률 정보를 표현합니다.
    /// </summary>
    [System.Serializable]
    public struct EnemyDropDefinition
    {
        /// <summary>드롭 종류.</summary>
        public ResourceDropType type;

        [Tooltip("드롭 수량 (만분율). 예: 10 -> 100000")]
        [Min(0)]
        public int amountRaw;

        [Tooltip("드롭 확률 (만분율). 예: 50% -> 5000")]
        [Min(0)]
        public int probabilityRaw;

        [Tooltip("확률을 무시하고 항상 지급할지 여부")]
        /// <summary>항상 지급 여부.</summary>
        public bool guaranteed;

        /// <summary>실제 드롭 수량.</summary>
        public float Amount => FixedPointScaling.ToFloat(amountRaw);

        /// <summary>실제 드롭 확률(0~1).</summary>
        public float Probability => Mathf.Clamp01(FixedPointScaling.ToFloat(probabilityRaw));
    }
}
