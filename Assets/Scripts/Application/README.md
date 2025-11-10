# Application 레이어

핵심 방어 게임 규칙을 애플리케이션 계층에서 실행하는 유즈케이스와 시뮬레이션 서비스를 모아 둡니다.

- **Ports**
  - `IGameStateRepository` : 방어 `GameState`를 상위 계층(Presentation/Infrastructure)에 노출합니다.
  - `IGameInputReader` : 새 Input System 기반으로 플레이어의 좌우 이동 입력을 제공합니다.

- **UseCases**
  - `StartGameUseCase` : 플레이어, 성벽, 적, 투사체 상태를 재설정하여 새 라운드를 시작합니다.
  - `MovePlayerUseCase` : 입력과 `deltaTime`을 바탕으로 플레이어 위치·공격 쿨다운을 갱신합니다.

- **Services**
  - `DefenseSimulationService` : 적 소환/이동, 플레이어·적 투사체, 성벽 피해, 드랍 생성 등을 처리하고 틱 결과 이벤트(스폰, 제거, 탄환 발사/충돌, 성벽 피해 등)를 반환합니다.

Presentation/Infrastructure 레이어는 위 추상화를 주입받아 씬 표시와 입력 처리를 수행합니다.
