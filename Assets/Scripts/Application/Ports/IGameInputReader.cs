using Noname.Core.Primitives;

namespace Noname.Application.Ports
{
    public interface IGameInputReader
    {
        float ReadMovement();
        bool TryReadBombardmentPoint(out Float2 worldPosition);
    }
}
