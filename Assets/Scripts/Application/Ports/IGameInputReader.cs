using Noname.Core.Primitives;

namespace Noname.Application.Ports
{
    /// <summary>
    /// 게임 내 이동/폭격 등 입력을 추상화하는 포트입니다.
    /// </summary>
    public interface IGameInputReader
    {
        /// <summary>수평 이동 입력(-1~1)을 반환합니다.</summary>
        float ReadMovement();

        /// <summary>
        /// 폭격 지점을 읽어오고 성공 여부를 반환합니다.
        /// </summary>
        bool TryReadBombardmentPoint(out Float2 worldPosition);
    }
}
