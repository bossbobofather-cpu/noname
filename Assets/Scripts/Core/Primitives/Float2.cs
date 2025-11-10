using System;

namespace Noname.Core.Primitives
{
    /// <summary>
    /// 2차원 부동 소수점 벡터를 표현하는 값 유형.
    /// UnityEngine.Vector2와 분리해 도메인 계층에서 독립적으로 사용한다.
    /// </summary>
    [Serializable]
    public struct Float2 : IEquatable<Float2>
    {
        public float x;
        public float y;

        public Float2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float X => x;
        public float Y => y;

        public static Float2 Zero => new Float2(0f, 0f);
        public static Float2 Up => new Float2(0f, 1f);

        public float SqrMagnitude => x * x + y * y;
        public float Magnitude => MathF.Sqrt(SqrMagnitude);

        public Float2 Normalized
        {
            get
            {
                var sqrMag = SqrMagnitude;
                if (sqrMag <= 1e-12f)
                {
                    return Zero;
                }

                var invMagnitude = 1f / MathF.Sqrt(sqrMag);
                return new Float2(x * invMagnitude, y * invMagnitude);
            }
        }

        public static Float2 operator +(Float2 left, Float2 right)
        {
            return new Float2(left.x + right.x, left.y + right.y);
        }

        public static Float2 operator -(Float2 left, Float2 right)
        {
            return new Float2(left.x - right.x, left.y - right.y);
        }

        public static Float2 operator -(Float2 value)
        {
            return new Float2(-value.x, -value.y);
        }

        public static Float2 operator *(Float2 value, float scalar)
        {
            return new Float2(value.x * scalar, value.y * scalar);
        }

        public static Float2 operator *(float scalar, Float2 value)
        {
            return new Float2(value.x * scalar, value.y * scalar);
        }

        public static Float2 operator /(Float2 value, float scalar)
        {
            if (MathF.Abs(scalar) <= 1e-12f)
            {
                return Zero;
            }

            var inv = 1f / scalar;
            return new Float2(value.x * inv, value.y * inv);
        }

        public static float Dot(Float2 left, Float2 right)
        {
            return left.x * right.x + left.y * right.y;
        }

        public static Float2 Reflect(Float2 vector, Float2 normal)
        {
            var n = normal.Normalized;
            var dot = Dot(vector, n);
            return vector - 2f * dot * n;
        }

        public Float2 WithX(float newX) => new Float2(newX, y);
        public Float2 WithY(float newY) => new Float2(x, newY);

        public void Deconstruct(out float dx, out float dy)
        {
            dx = x;
            dy = y;
        }

        public bool Equals(Float2 other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj)
        {
            return obj is Float2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public override string ToString()
        {
            return $"({x:0.###}, {y:0.###})";
        }
    }
}
