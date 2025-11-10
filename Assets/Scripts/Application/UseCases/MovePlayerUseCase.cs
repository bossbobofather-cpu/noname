using Noname.Application.Ports;

namespace Noname.Application.UseCases
{
    /// <summary>
    /// 플레이어 이동 및 쿨다운 갱신을 수행하는 유즈케이스입니다.
    /// </summary>
    public sealed class MovePlayerUseCase
    {
        private readonly IGameStateRepository _repository;

        public MovePlayerUseCase(IGameStateRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 주어진 입력 값으로 플레이어를 이동시킵니다.
        /// </summary>
        public void Execute(float movement, float deltaTime)
        {
            var player = _repository.State.Player;
            player.UpdateCooldown(deltaTime);
            player.Move(movement, deltaTime);
        }
    }
}
