using Noname.Core.Entities;

namespace Noname.Application.Ports
{
    public interface IGameStateRepository
    {
        GameState State { get; }
    }
}
