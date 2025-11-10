# Application Layer

핵심 방어 게임 규칙을 수행하는 UseCase, 서비스, 포트를 모은 계층입니다.  
Core 모델을 조작하지만 Unity나 UI에 직접 의존하지 않습니다.

## Ports
- `IGameStateRepository`: 현재 `GameState`를 상위 계층에서 주입받기 위한 인터페이스.
- `IGameInputReader`: Unity Input System 등에서 읽은 입력(좌우 이동, 폭격 위치)을 전달.

## UseCases
- `StartGameUseCase`: 플레이어·거점·적 상태를 초기화하고 턴 진행을 리셋.
- `MovePlayerUseCase`: 입력값과 `deltaTime`을 기반으로 플레이어 이동/쿨다운 갱신.

## Services
- `DefenseSimulationService`: 적 스폰, 이동, 투사체, 자원 드롭, 레벨업 등을 한 프레임 단위로 계산하고 결과 이벤트를 반환.

Presentation/Infrastructure 계층은 이 인터페이스와 유즈케이스를 주입받아 입력 처리와 화면 갱신을 수행합니다.
