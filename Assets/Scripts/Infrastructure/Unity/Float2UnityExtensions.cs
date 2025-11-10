using Noname.Core.Primitives;
using UnityEngine;

namespace Noname.Infrastructure.Unity
{
    /// <summary>
    /// Float2와 Unity의 Vector 타입 간 변환 확장 메서드를 제공합니다.
    /// </summary>
    public static class Float2UnityExtensions
    {
        /// <summary>
        /// Float2를 UnityEngine.Vector2로 변환합니다.
        /// </summary>
        public static Vector2 ToUnityVector2(this Float2 value)
        {
            return new Vector2(value.X, value.Y);
        }

        /// <summary>
        /// Vector2를 Float2로 변환합니다.
        /// </summary>
        public static Float2 ToFloat2(this Vector2 value)
        {
            return new Float2(value.x, value.y);
        }

        /// <summary>
        /// Vector3의 x,y 성분만을 사용해 Float2를 생성합니다.
        /// </summary>
        public static Float2 ToFloat2(this Vector3 value)
        {
            return new Float2(value.x, value.y);
        }
    }
}
