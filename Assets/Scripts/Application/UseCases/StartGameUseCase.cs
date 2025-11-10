using Noname.Application.Ports;

namespace Noname.Application.UseCases
{
    public sealed class StartGameUseCase
    {
        private readonly IGameStateRepository _repository;

        public StartGameUseCase(IGameStateRepository repository)
        {
            _repository = repository;
        }

        public void Execute()
        {
            _repository.State.Reset();
        }
    }
}
