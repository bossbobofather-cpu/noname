# Application 레이어

핵심 방어 게임 규칙을 수행하는 유즈케이스와 서비스를 모아 둔 계층입니다.  
도메인(Core) 모델을 조작하지만 Unity나 UI에 직접 의존하지 않도록 구성되어 있습니다.

## Ports
- `IGameStateRepository` : 현재 `GameState`를 상위 계층에서 주입받기 위한 인터페이스입니다.
- `IGameInputReader` : Unity Input System 등을 통해 읽은 입력(좌우 이동, 폭격 위치)을 애플리케이션 계층에 전달합니다.

## UseCases
- `StartGameUseCase` : 플레이어·거점·적 상태를 초기화하고 단계 진행을 리셋합니다.
- `MovePlayerUseCase` : 입력값과 `deltaTime`을 기반으로 플레이어 이동/쿨다운을 갱신합니다.

## Services
- `DefenseSimulationService` : 적 스폰, 이동, 투사체, 자원 드롭, 레벨업 등을 한 프레임 단위로 계산하고 결과 이벤트 묶음을 반환합니다.

Presentation/Infrastructure 계층은 이 인터페이스/서비스를 주입받아 입력 처리와 화면 갱신을 수행합니다.
