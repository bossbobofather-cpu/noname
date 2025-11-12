# Noname Defense Prototype (Unity)

핀볼·벽돌깨기에서 영감을 얻은 격자 기반 방어 게임 프로토타입입니다. Clean Architecture를 적용해 **Core 도메인 로직 → Application UseCase/Service → Infrastructure Adapter → Presentation(ViewModel & View)**로 계층을 나누고, DocFX로 개발 문서를 자동화했습니다.

![Gameplay](images/gamePlayDemo.gif)

---

## ✨ Highlights

- **격자 웨이브 시뮬레이션**  
  - `DefenseGameSettings`로 열/행, 전진 간격, 드롭/경험치 파라미터를 데이터화.  
  - `DefenseSimulationService`가 숨겨진 -1행 대기열, 행 전진, 플레이어 자동 조준, 적/플레이어 투사체, 드롭 생성을 단일 `SimulationStepResult`로 전달.

- **증강(Ability) 시스템**  
  - `GameplayAbilityDefinition`/`GameplayEffectDefinition` ScriptableObject로 능력을 정의하고, `PlayerEntity.ApplyModifier`로 공격력·쿨다운·사거리 등을 조정.  
  - 게임 오버 시 증강 드롭은 소멸, 골드 드롭만 즉시 지급.

- **Clean Architecture 적용**  
  - Core 엔티티/ValueObject는 Unity API 없이 순수 C#.  
  - Application 레이어는 `StartGameUseCase`, `MovePlayerUseCase`, `DefenseSimulationService` 등으로 도메인 규칙을 실행.  
  - Infrastructure는 Input/RepositoryAdapter, Presentation은 `GameViewModel` + Unity View/FX가 담당.

---

## 🏗 Architecture

| Layer | Folder | Responsibility | Representative Code |
| --- | --- | --- | --- |
| **Core** | `Assets/Scripts/Core` | `PlayerEntity`, `EnemyEntity`, `GameState`, `DefenseGameSettings`, `EnemyDefinition` 등 도메인 모델과 ScriptableObject | [EnemyDefinition](Assets/Scripts/Core/ValueObjects/EnemyDefinition.cs) |
| **Application** | `Assets/Scripts/Application` | UseCase, SimulationService, 이벤트 DTO (`SimulationStepResult`, `PlayerProjectileFiredEvent` 등) | [DefenseSimulationService](Assets/Scripts/Application/Services/DefenseSimulationService.cs) |
| **Infrastructure** | `Assets/Scripts/Infrastructure` | Input/Repository/Tool 구현 (예: `DefenseInputAdapter`) | [DefenseInputAdapter](Assets/Scripts/Infrastructure/Input/DefenseInputAdapter.cs) |
| **Presentation** | `Assets/Scripts/Presentation` | `GameViewModel` + View/FX/Sound, Bootstrapper | [GameViewModel](Assets/Scripts/Presentation/ViewModels/GameViewModel.cs) |
| **Docs** | `Docs/`, `_site/` | DocFX 문서, TODO, Tools | [Docs/todo/index.md](Docs/todo/index.md) |

---

## 🔁 Gameplay Flow

```markdown
```mermaid
flowchart TD
    U[Unity Update] --> B[DefenseGameBootstrapper.Update]
    B --> V[GameViewModel.Tick]
    V -->|Input| I[Read movement/target (DefenseInputAdapter)]
    V -->|UseCase| M[MovePlayerUseCase.Execute]
    V -->|Simulation| S[DefenseSimulationService.Tick]
    S --> G[격자 스폰/전진]
    S --> P[플레이어/적 투사체]
    S --> D[드롭·레벨업 이벤트]
    V -->|Broadcast| E[EnemySpawned / ResourceDropSpawned ...]
    E --> F[Views/FX respond]
             
---

## 📚 Documentation

- [Landing / Docs Home](https://bossbobofather-cpu.github.io/noname/index.html)
- [Getting Started](https://bossbobofather-cpu.github.io/noname/Docs/getting-started.html)
- [Layers Overview](https://bossbobofather-cpu.github.io/noname/Docs/layers/index.html)
- [Tools (Definition Importer)](https://bossbobofather-cpu.github.io/noname/Docs/tools/index.html)
- [TODO List](https://bossbobofather-cpu.github.io/noname/Docs/todo/index.html)
- [API Reference](https://bossbobofather-cpu.github.io/noname/api/index.html)

---

✅ TODO Snapshot

 Docs/todo/index.md를 확인하세요.

---

📁 Repository
GitHub: bossbobofather-cpu/noname
필요한 정보나 코드 하이라이트 스크린샷이 더 필요하면 언제든 말씀 주세요!
