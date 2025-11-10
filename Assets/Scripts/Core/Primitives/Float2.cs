using System;

namespace Noname.Core.Primitives
{
    /// <summary>
    /// Unity 의존성이 없는 2차원 부동소수점 벡터 구조체입니다.
    /// Core 계층에서 좌표·방향 정보를 다룰 때 사용합니다.
    /// </summary>
    [Serializable]
    public struct Float2 : IEquatable<Float2>
    {
        /// <summary>
        /// X 성분.
        /// </summary>
        public float x;

        /// <summary>
        /// Y 성분.
        /// </summary>
        public float y;

        /// <summary>
        /// 새 좌표를 생성합니다.
        /// </summary>
        /// <param name="x">X 좌표.</param>
        /// <param name="y">Y 좌표.</param>
        public Float2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// X 좌표를 반환합니다.
        /// </summary>
        public float X => x;

        /// <summary>
        /// Y 좌표를 반환합니다.
        /// </summary>
        public float Y => y;

        /// <summary>
        /// (0,0)을 나타내는 상수입니다.
        /// </summary>
        public static Float2 Zero => new Float2(0f, 0f);

        /// <summary>
        /// 위쪽 단위 벡터입니다.
        /// </summary>
        public static Float2 Up => new Float2(0f, 1f);

        /// <summary>
        /// 벡터 크기의 제곱값입니다.
        /// </summary>
        public float SqrMagnitude => x * x + y * y;

        /// <summary>
        /// 벡터 크기입니다.
        /// </summary>
        public float Magnitude => MathF.Sqrt(SqrMagnitude);

        /// <summary>
        /// 정규화된 벡터를 반환합니다.
        /// </summary>
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

        /// <summary>
        /// 두 벡터를 더합니다.
        /// </summary>
        public static Float2 operator +(Float2 left, Float2 right)
        {
            return new Float2(left.x + right.x, left.y + right.y);
        }

        /// <summary>
        /// 두 벡터의 차를 구합니다.
        /// </summary>
        public static Float2 operator -(Float2 left, Float2 right)
        {
            return new Float2(left.x - right.x, left.y - right.y);
        }

        /// <summary>
        /// 벡터의 부호를 반전합니다.
        /// </summary>
        public static Float2 operator -(Float2 value)
        {
            return new Float2(-value.x, -value.y);
        }

        /// <summary>
        /// 벡터에 스칼라를 곱합니다.
        /// </summary>
        public static Float2 operator *(Float2 value, float scalar)
        {
            return new Float2(value.x * scalar, value.y * scalar);
        }

        /// <summary>
        /// 스칼라에 벡터를 곱합니다.
        /// </summary>
        public static Float2 operator *(float scalar, Float2 value)
        {
            return new Float2(value.x * scalar, value.y * scalar);
        }

        /// <summary>
        /// 벡터를 스칼라로 나눕니다.
        /// </summary>
        public static Float2 operator /(Float2 value, float scalar)
        {
            if (MathF.Abs(scalar) <= 1e-12f)
            {
                return Zero;
            }

            var inv = 1f / scalar;
            return new Float2(value.x * inv, value.y * inv);
        }

        /// <summary>
        /// 두 벡터의 내적을 계산합니다.
        /// </summary>
        public static float Dot(Float2 left, Float2 right)
        {
            return left.x * right.x + left.y * right.y;
        }

        /// <summary>
        /// 주어진 법선을 기준으로 벡터를 반사합니다.
        /// </summary>
        public static Float2 Reflect(Float2 vector, Float2 normal)
        {
            var n = normal.Normalized;
            var dot = Dot(vector, n);
            return vector - 2f * dot * n;
        }

        /// <summary>
        /// 새 X 값을 적용한 복사본을 반환합니다.
        /// </summary>
        public Float2 WithX(float newX) => new Float2(newX, y);

        /// <summary>
        /// 새 Y 값을 적용한 복사본을 반환합니다.
        /// </summary>
        public Float2 WithY(float newY) => new Float2(x, newY);

        /// <summary>
        /// 튜플 분해를 지원합니다.
        /// </summary>
        public void Deconstruct(out float dx, out float dy)
        {
            dx = x;
            dy = y;
        }

        /// <inheritdoc />
        public bool Equals(Float2 other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Float2 other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({x:0.###}, {y:0.###})";
        }
    }
}
