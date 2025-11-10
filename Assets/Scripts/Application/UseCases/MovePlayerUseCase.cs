using Noname.Application.Ports;

namespace Noname.Application.UseCases
{
    public sealed class MovePlayerUseCase
    {
        private readonly IGameStateRepository _repository;

        public MovePlayerUseCase(IGameStateRepository repository)
        {
            _repository = repository;
        }

        public void Execute(float movement, float deltaTime)
        {
            var player = _repository.State.Player;
            player.UpdateCooldown(deltaTime);
            player.Move(movement, deltaTime);
        }
    }
}
