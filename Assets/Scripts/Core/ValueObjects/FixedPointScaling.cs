using UnityEngine;

namespace Noname.Core.ValueObjects
{
    /// <summary>
    /// 만분율(1/10,000) 단위로 저장된 고정소수 값 변환을 담당합니다.
    /// </summary>
    public static class FixedPointScaling
    {
        /// <summary>만분율 분모 값.</summary>
        public const int Denominator = 10000;

        /// <summary>
        /// 원시 정수 값을 실수 스탯 값으로 변환합니다.
        /// </summary>
        public static float ToFloat(int rawValue)
        {
            var value = rawValue / (float)Denominator;
            return Mathf.Floor(value * Denominator) / Denominator;
        }

        /// <summary>
        /// 실수 스탯 값을 만분율 원시 정수로 변환합니다.
        /// </summary>
        public static int FromFloat(float value)
        {
            return Mathf.Max(0, Mathf.RoundToInt(value * Denominator));
        }
    }
}
