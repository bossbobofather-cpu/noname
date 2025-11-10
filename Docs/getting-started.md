# Getting Started

이 문서는 프로젝트 구조를 빠르게 이해하기 위한 요약입니다.

## 계층 개요

| 계층 | 핵심 책임 | 주요 요소 | 상호작용 |
| --- | --- | --- | --- |
| **Core** | 게임 규칙, 도메인 모델 | `PlayerEntity`, `EnemyEntity`, `GameplayAbilityDefinition` | 다른 계층에서 Core 타입을 직접 사용(불변 법칙) |
| **Application** | UseCase, 서비스, 포트 | `StartGameUseCase`, `MovePlayerUseCase`, `DefenseSimulationService`, `IGameInputReader` | Core 엔티티를 조작하며, 외부 입력/출력을 포트로 추상화 |
| **Infrastructure** | Adapter, 데이터 접근 및 입력 | `DefenseInputAdapter`, `InMemoryGameStateRepository`, Definition Importer 툴 | Application 포트를 구현, Excel→JSON→SO 파이프라인 포함 |
| **Presentation** | UI-ViewModel-View | `GameViewModel`, `DefenseGameBootstrapper`, `PlayerView`, UI/FX/Sound 매니저 | Application을 호출해 게임을 구동하고 사용자와 상호작용 |
