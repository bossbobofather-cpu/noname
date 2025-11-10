using Noname.Application.Ports;
using Noname.Core.Entities;

namespace Noname.Infrastructure.Repositories
{
    public sealed class InMemoryGameStateRepository : IGameStateRepository
    {
        public InMemoryGameStateRepository(GameState state)
        {
            State = state;
        }

        public GameState State { get; private set; }
    }
}
