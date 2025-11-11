# Getting Started

프로젝트 구조를 빠르게 이해하기 위한 요약입니다.

- **왜 계층을 나눴나요?**
  - Unity/Unreal 등 실시간 엔진에서 핵심 로직을 테스트하기 어렵기 때문에 Core/Application 계층을 분리해 재사용성을 확보했습니다.
  - 콘텐츠 팀이 Core 로직을 직접 수정하지 않도록 책임을 분리하고, Definition Importer로 데이터 자산을 주입할 수 있게 했습니다.

## 계층 개요

| 계층 | 핵심 책임 | 주요 요소 | 참고 |
| --- | --- | --- | --- |
| **Core** | 게임 규칙·도메인 모델 | `PlayerEntity`, `EnemyEntity`, `DefenseGameSettings` | 상위 계층에서 직접 사용 |
| **Application** | UseCase·서비스·포트 | `StartGameUseCase`, `DefenseSimulationService`, `IGameInputReader` | Core 모델 조작 + 추상화 |
| **Infrastructure** | Adapter, I/O 구현 | `DefenseInputAdapter`, `InMemoryGameStateRepository`, Definition Importer | Application 포트 구현 |
| **Presentation** | ViewModel·View | `GameViewModel`, `DefenseGameBootstrapper`, Player/Enemy/Fortress View | 씬/FX/UI 연결 |

## 격자 기반 플레이 흐름
- `DefenseGameSettings`는 `spawnOriginX`와 `spawnColumnSpacing`으로 열을, `firstRowY`와 `rowSpacing`으로 행을 정의하고 숨겨진 -1행에서 몬스터 대기열을 채웁니다.
- `enemyRowAdvanceInterval`마다 모든 몬스터가 한 행씩 내려오며, 행 개수에 상관없이 계속 진행하다가 플레이어 위치나 성벽 위치의 트리거와 충돌하면 제거됩니다(성벽을 맞추면 피해가 발생).
- 플레이어는 좌우 이동 후 포인터 클릭으로 조준 셀(row/column)을 잠그고, 해당 셀이 비면 자동으로 사거리 내 가장 가까운 적을 다시 조준합니다.
- `attackRange`가 0보다 크면 성벽 기준으로 몇 행 위까지 공격 가능한지 계산해 원거리 공격을 수행합니다.
- 드롭을 수거하면 경험치·골드·체력·증강을 획득하며, 게임 오버 시 남아 있는 드롭은 연출 없이 제거되고 골드 보상만 즉시 지급됩니다(증강 드롭은 소멸).
- 플레이어나 성벽에 충돌해 피해를 준 뒤 제거된 몬스터는 드롭 아이템을 생성하지 않습니다.

