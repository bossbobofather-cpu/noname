using Noname.Application.Ports;
using Noname.Core.Entities;

namespace Noname.Infrastructure.Repositories
{
    /// <summary>
    /// 단일 GameState 인스턴스를 보관하는 단순 저장소 구현입니다.
    /// </summary>
    public sealed class InMemoryGameStateRepository : IGameStateRepository
    {
        public InMemoryGameStateRepository(GameState state)
        {
            State = state;
        }

        /// <inheritdoc />
        public GameState State { get; private set; }
    }
}
