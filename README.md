# Noname Defense Prototype (Unity)

클린 아키텍처(Core / Application / Infrastructure / Presentation) 구조로 제작한 격자 디펜스 프로토타입입니다. DocFX 문서와 Definition Importer 배치를 함께 제공합니다.

![Gameplay](images/gamePlayDemo.gif)
![Gameplay](images/gamePlayDemo2.gif)

---

## 주요 특징

- **격자 전투 루프** – `DefenseGameSettings`로 웨이브·격자·레벨 곡선을 정의하고, `DefenseSimulationService`가 소환/전진/공격/드랍 로직을 수행합니다.
- **능력 & 드랍 시스템** – `GameplayAbilityDefinition`이 플레이어 스탯(공격력, 쿨다운, 사거리 등)을 조정하고, 다양한 리소스 드랍이 성장 루프를 구성합니다.
- **클린 아키텍처** – Core 엔티티는 Unity API와 분리, Application은 규칙 실행, Infrastructure는 입출력 어댑터, Presentation은 ViewModel + Unity View/FX를 담당합니다.

---

## 계층 개요

| Layer | Folder | Responsibility | Representative Code |
| --- | --- | --- | --- |
| **Core** | `Assets/Scripts/Core` | 엔티티 · ScriptableObject (`PlayerEntity`, `EnemyDefinition`, `DefenseGameSettings` 등) | [EnemyDefinition](Assets/Scripts/Core/ValueObjects/EnemyDefinition.cs) |
| **Application** | `Assets/Scripts/Application` | Use case, 시뮬레이션 서비스, DTO | [DefenseSimulationService](Assets/Scripts/Application/Services/DefenseSimulationService.cs) |
| **Infrastructure** | `Assets/Scripts/Infrastructure` | 입력/저장 어댑터 | [DefenseInputAdapter](Assets/Scripts/Infrastructure/Input/DefenseInputAdapter.cs) |
| **Presentation** | `Assets/Scripts/Presentation` | ViewModel, Unity View, FX/사운드 매니저 | [GameViewModel](Assets/Scripts/Presentation/ViewModels/GameViewModel.cs) |
| **Docs** | `Docs/`, `_site/` | DocFX 문서/가이드/TODO | [Docs/todo/index.md](Docs/todo/index.md) |

---

## 게임플레이 흐름

```
Unity Update()
  -> DefenseGameBootstrapper.Update()
       -> GameViewModel.Tick(deltaTime)
            -> 입력 읽기 (DefenseInputAdapter)
            -> MovePlayerUseCase.Execute()
            -> DefenseSimulationService.Tick()
                 -> 격자 소환/전진
                 -> 공격 및 드랍 처리
            -> 이벤트 브로드캐스트 (EnemySpawned, ResourceDropSpawned, ...)
                 -> View/FX가 프리팹 생성, SFX/VFX 재생
```

---

## 문서 링크

- [Landing / Docs Home](https://bossbobofather-cpu.github.io/noname/index.html)
- [Getting Started](https://bossbobofather-cpu.github.io/noname/Docs/getting-started.html)
- [Layers Overview](https://bossbobofather-cpu.github.io/noname/Docs/layers/index.html)
- [Tools (Definition Importer)](https://bossbobofather-cpu.github.io/noname/Docs/tools/index.html)
- [TODO List](https://bossbobofather-cpu.github.io/noname/Docs/todo/index.html)
- [API Reference](https://bossbobofather-cpu.github.io/noname/api/index.html)

---

## Definition Import Workflow

- `Tools → Definition Importer` 배치 메뉴:
  1. **Run Enemy Definitions** – `enemyDefinitions.xlsx` → `enemyDefinitions.json` → `Assets/Resources/Enemies/*.asset`
  2. **Run Player Definitions** – `playerDefinitions.xlsx` → `playerDefinitions.json` → `Assets/Resources/Players/*.asset`
  3. **Run Stage Definitions** – `Assets/Table/Stage/*.xlsx` → 동명 JSON + `Assets/Resources/Stages/*.asset`
  4. **Run All Definitions** – 위 3개 단계를 순차 실행
- Excel의 스탯/드랍 수량/확률은 **만분율 정수**로 저장 (예: 8.25 → 82500). 런타임에서는 `FixedPointScaling`이 소수 넷째 자리까지 실수로 환산합니다.
- Stage 시트의 `monsterWeights`, `monsterWeightTable` 등에 `(EnemyCode:10001;weight:1)` 형태로 작성하면 ScriptableObject에 자동 반영됩니다.

---

## TODO Snapshot

자세한 TODO는 `Docs/todo/index.md`에서 확인하세요.

---

## Repository

GitHub: [bossbobofather-cpu/noname](https://github.com/bossbobofather-cpu/noname)
