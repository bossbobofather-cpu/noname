using Noname.Application.Ports;

namespace Noname.Application.UseCases
{
    /// <summary>
    /// 게임 상태를 초기화하여 새 세션을 시작하는 유즈케이스입니다.
    /// </summary>
    public sealed class StartGameUseCase
    {
        private readonly IGameStateRepository _repository;

        public StartGameUseCase(IGameStateRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 현재 GameState를 초기화합니다.
        /// </summary>
        public void Execute()
        {
            _repository.State.Reset();
        }
    }
}
