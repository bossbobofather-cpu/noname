# Application Layer

핵심 방어 규칙을 실행하는 UseCase·서비스·포트 계층입니다. Core 모델만 조작하며 Unity 구체 구현과는 분리되어 동작합니다.

## Ports
- `IGameStateRepository`: `GameState`를 보관하고 상위 계층이 현재 상태를 주입/조회할 수 있도록 해 주는 저장소 포트입니다.
- `IGameInputReader`: 최신 Input System 입력(좌우 이동, 격자 셀을 지정하는 포인터 클릭 등)을 읽어 도메인 친화적인 값으로 변환합니다.

## UseCases
- `StartGameUseCase`: 플레이어·거점·게임 상태를 초기화하고 진행 상황을 리셋합니다.
- `MovePlayerUseCase`: 입력값과 `deltaTime`을 기반으로 플레이어 이동과 공격 쿨다운을 갱신합니다.

## Services
- `DefenseSimulationService`
  - `DefenseGameSettings`가 정의한 격자(`spawnOriginX`, `spawnColumnSpacing`, `firstRowY`, `rowSpacing`, `gridRows`, `gridColumns`, `enemyRowAdvanceInterval`)를 바탕으로 -1행 대기열을 만들고 주기적으로 한 행씩 전진시킵니다. 행 개수와 무관하게 계속 내려오며, 플레이어 위치나 성벽 위치 트리거에 닿으면 제거됩니다.
  - 플레이어 자동 공격은 `GameState`에 기록된 셀(row/column)을 우선 조준하고, 해당 셀이 비면 사거리 안에서 가장 가까운 적을 즉시 재조준합니다.
  - 플레이어나 성벽에 충돌해 피해를 입힌 몬스터는 드롭 아이템을 생성하지 않습니다.
  - 드롭 스폰/수거, 적 공격, 투사체 충돌, 레벨업 이벤트를 묶어 `SimulationStepResult`로 반환하여 Presentation 계층이 뷰를 갱신하도록 합니다.

Presentation/Infrastructure 계층은 위 포트를 주입받아 입력 처리와 화면 갱신을 담당합니다.
