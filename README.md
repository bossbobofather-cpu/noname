# Noname Defense Prototype (Unity)

Unity 기반 탑다운 디펜스 프로토타입입니다. Clean Architecture 구조(Core / Application / Infrastructure / Presentation)를 따르며, DocFX 문서와 Definition Importer 툴을 함께 제공합니다.

![Gameplay](images/gamePlayDemo.gif)
![Gameplay](images/gamePlayDemo2.gif)

---

## 핵심 특징

- **데이터 주도 진행**: `DefenseGameSettings`와 다양한 Definition(ScriptableObject)을 통해 플레이어/적/스테이지 정보를 관리합니다. `DefenseSimulationService`가 모든 전투 로직을 책임집니다.
- **증강/어빌리티 시스템**: `GameplayAbilityDefinition`으로 여러 능력 조합을 만들 수 있으며, 런타임에서 어빌리티 선택 UI를 통해 적용합니다.
- **레이어 분리**: Core는 순수 로직, Application은 Use Case, Infrastructure는 입력/저장소, Presentation은 ViewModel과 Unity View로 명확히 나뉩니다.

---

## 레이어 개요

| Layer | Folder | Responsibility | Representative Code |
| --- | --- | --- | --- |
| **Core** | `Assets/Scripts/Core` | 순수 로직 & ScriptableObject (`PlayerEntity`, `EnemyDefinition` 등) | [EnemyDefinition](Assets/Scripts/Core/ValueObjects/EnemyDefinition.cs) |
| **Application** | `Assets/Scripts/Application` | Use case, 서비스, DTO | [DefenseSimulationService](Assets/Scripts/Application/Services/DefenseSimulationService.cs) |
| **Infrastructure** | `Assets/Scripts/Infrastructure` | 입력/저장소/어댑터 | [DefenseInputAdapter](Assets/Scripts/Infrastructure/Input/DefenseInputAdapter.cs) |
| **Presentation** | `Assets/Scripts/Presentation` | ViewModel, Unity View, FX/SFX | [GameViewModel](Assets/Scripts/Presentation/ViewModels/GameViewModel.cs) |
| **Docs** | `Docs/`, `_site/` | DocFX 문서/가이드/TODO/Work Log | [Docs/todo/index.md](Docs/todo/index.md), [Docs/work/index.md](Docs/work/index.md) |

---

## 런타임 흐름

```
Unity Update()
  -> DefenseGameBootstrapper.Update()
       -> GameViewModel.Tick(deltaTime)
            -> 입력 처리 (DefenseInputAdapter)
            -> MovePlayerUseCase.Execute()
            -> DefenseSimulationService.Tick()
                 -> 전투 상태 갱신
                 -> 리소스/드랍/어빌리티 이벤트 발생
            -> 이벤트를 바탕으로 View/FX/SFX 호출
```

---

## 빠른 링크

- [Landing / Docs Home](https://bossbobofather-cpu.github.io/noname/index.html)
- [Getting Started](https://bossbobofather-cpu.github.io/noname/Docs/getting-started.html)
- [Layers Overview](https://bossbobofather-cpu.github.io/noname/Docs/layers/index.html)
- [Tools (Definition Importer)](https://bossbobofather-cpu.github.io/noname/Docs/tools/index.html)
- [TODO List](https://bossbobofather-cpu.github.io/noname/Docs/todo/index.html)
- [Work List](https://bossbobofather-cpu.github.io/noname/Docs/work/index.html)
- [API Reference](https://bossbobofather-cpu.github.io/noname/api/index.html)

---

## Definition Import Workflow

1. **Run Enemy Definitions**  
   `enemyDefinitions.xlsx` → `enemyDefinitions.json` → `Assets/AddressableAssetsData/...` (ScriptableObject)
2. **Run Player Definitions**  
   `playerDefinitions.xlsx` → `playerDefinitions.json` → ScriptableObject
3. **Run Stage Definitions**  
   `Assets/Table/Stage/*.xlsx` → JSON → Stage ScriptableObject
4. **Run All Definitions**  
   위 세 단계를 한 번에 실행

> Excel 수치 입력 시 정수/퍼밀 단위(예: 8.25 → 82500) 규칙을 지키고, `FixedPointScaling`으로 소수 처리합니다. Stage의 `monsterWeights`는 `(EnemyCode:10001;weight:1)`과 같은 포맷을 사용합니다.

---

## TODO & Work Log

- TODO 상세: `Docs/todo/index.md`
- 일일 작업 로그(Work List): `Docs/work/index.md`

---

## Repository

GitHub: [bossbobofather-cpu/noname](https://github.com/bossbobofather-cpu/noname)
