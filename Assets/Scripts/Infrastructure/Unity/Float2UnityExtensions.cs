using Noname.Core.Primitives;
using UnityEngine;

namespace Noname.Infrastructure.Unity
{
    public static class Float2UnityExtensions
    {
        public static Vector2 ToUnityVector2(this Float2 value)
        {
            return new Vector2(value.X, value.Y);
        }

        public static Float2 ToFloat2(this Vector2 value)
        {
            return new Float2(value.x, value.y);
        }

        public static Float2 ToFloat2(this Vector3 value)
        {
            return new Float2(value.x, value.y);
        }
    }
}
