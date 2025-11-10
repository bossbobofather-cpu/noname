# Getting Started

이 문서는 프로젝트 구조를 빠르게 이해하기 위한 요약입니다.

- **내 프로젝트를 클린 아키텍처로 설계하려는 이유?**
  - Unity나 Unreal 개발 경험이 있고 핵심 로직을 서로 다른 엔진에서도 재사용하고 싶었기 때문입니다.
  - 콘텐츠 개발자가 필요에 의해 핵심 로직을 직접 건드리는 경우가 있었고, 그로 인해 크리티컬한 사이드 이펙트가 발생하기도 했습니다. 명확한 계층 분리가 책임 구분에 도움이 됩니다.
  - 테스트가 용이합니다. 엔진 없이도 핵심 로직 단위 테스트가 가능하며, 무거운 엔진 실행/특정 상황 재현에 드는 시간을 줄일 수 있습니다.

## 계층 개요

| 계층 | 핵심 책임 | 주요 요소 | 상호작용 |
| --- | --- | --- | --- |
| **Core** | 게임 규칙, 도메인 모델 | `PlayerEntity`, `EnemyEntity`, `GameplayAbilityDefinition` | 다른 계층에서 Core 타입을 직접 사용(불변 법칙) |
| **Application** | UseCase, 서비스, 포트 | `StartGameUseCase`, `MovePlayerUseCase`, `DefenseSimulationService`, `IGameInputReader` | Core 엔티티를 조작하며, 외부 입력/출력을 포트로 추상화 |
| **Infrastructure** | Adapter, 데이터 접근 및 입력 | `DefenseInputAdapter`, `InMemoryGameStateRepository`, Definition Importer 툴 | Application 포트를 구현, Excel→JSON→SO 파이프라인 포함 |
| **Presentation** | UI-ViewModel-View | `GameViewModel`, `DefenseGameBootstrapper`, `PlayerView`, UI/FX/Sound 매니저 | Application을 호출해 게임을 구동하고 사용자와 상호작용 |
