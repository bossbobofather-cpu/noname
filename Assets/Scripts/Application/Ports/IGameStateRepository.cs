using Noname.Core.Entities;

namespace Noname.Application.Ports
{
    /// <summary>
    /// 게임 상태 저장소를 나타내는 포트입니다.
    /// </summary>
    public interface IGameStateRepository
    {
        /// <summary>현재 게임 상태 루트 객체.</summary>
        GameState State { get; }
    }
}
