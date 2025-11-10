# Presentation 계층

Unity 씬에서 게임 상태를 시각화하고 `GameViewModel` 이벤트를 UI/오브젝트와 연결합니다.

## 주요 View
- **DefenseGameBootstrapper**  
  - 필수 참조: `DefenseInputAdapter`, 플레이어/성벽/적/투사체 프리팹, `DefenseGameSettings`, `AugmentSelectionView` 프리팹, 좌우 이동 한계를 위한 콜라이더(`leftBoundaryCollider`, `rightBoundaryCollider`). 콜라이더의 `bounds.min/max.x` 값이 이동 최소/최대 경계로 자동 반영됩니다.  
  - `resourceDropPrefabs` 배열에는 경험치/골드/HP/증강 드랍용 프리팹을 타입별로 등록합니다. 드랍 프리팹에는 `ResourceDropView`가 붙어 있어야 하며, `resourceDropParent` 아래에서 생성·수거됩니다.
- **PlayerView**  
  - `PlayerPositionChanged` 이벤트를 받아 플레이어 프리팹 위치를 갱신합니다.
- **EnemyView**  
  - 생성된 `EnemyEntity`마다 인스턴스화되어 위치·체력 변화를 반영하고 제거 이벤트에서 비활성화됩니다.
- **FortressView**  
  - 성벽 HP 슬라이더와 경험치 슬라이더/레벨 라벨을 갱신합니다. 인스펙터에서 UI 참조를 연결해야 합니다.
- **ProjectileView**  
  - 플레이어/적 투사체 이동을 시각화하고 `ProjectileImpactOccurred` 이벤트 시 이펙트를 재생합니다.
- **AugmentSelectionView**  
  - 레벨업 또는 어빌리티 드랍 시 나타나는 증강 선택 UI입니다. `AbilityChoicesPresented` 이벤트로 제목·설명을 채우고, 버튼 클릭 시 `GameViewModel.SelectAbility`를 호출합니다.
- **ResourceDropView**  
  - 드랍 아이템을 약간의 진동 애니메이션으로 표시하고, 픽업 신호를 받으면 곡선을 그리며 플레이어에게 이동한 뒤 제거합니다.
- **DefenseDebugPanel**  
  - 테스트 편의를 위한 UI 스크립트입니다. 버튼으로 `GameViewModel.ForceAbilitySelection()`을 호출해 즉시 증강 선택창을 띄울 수 있으며, 슬라이더/토글로 `Time.timeScale`을 조절하거나 일시 정지할 수 있습니다.
- **DefenseInputAdapter**  
  - Unity Input System을 사용해 좌우 이동 입력과 포격 고정 지점을 읽어 `IGameInputReader`에 전달합니다.

## ViewModel
- **GameViewModel**  
  - 이동, 자동 공격, `DefenseSimulationService` 호출을 담당합니다.  
  - 적/투사체/드랍/경험치/레벨업 이벤트를 뷰에 전달하고, 레벨업 또는 어빌리티 드랍 시 게임을 일시 정지해 증강 선택 과정을 기다립니다.  
  - `ForceAbilitySelection()` 등 디버그 편의 메서드를 제공해 테스트 UI에서 호출할 수 있습니다.

## 흐름 요약
1. 플레이어는 설정된 좌우 경계 안에서만 이동합니다.  
2. 공격 범위 안에 적이 있거나 고정 포격 지점이 지정되어 있으면 자동으로 투사체를 발사합니다.  
3. 시뮬레이션 서비스가 적 스폰·이동·공격, 그리고 드랍 생성/자동 픽업을 처리합니다.  
4. 경험치나 어빌리티 드랍으로 레벨업 조건을 만족하면 증강 선택 UI가 열리고, 선택 후 게임이 다시 진행됩니다.  
5. 성벽 HP가 0이 되면 `GameOver` 이벤트가 발생하며 모든 뷰가 정리됩니다.

씬에서 Bootstrapper의 필드를 모두 채우고 실행하면 프리팹과 UI가 자동으로 연결됩니다. DebugPanel을 추가로 배치하면 테스트 중 증강 선택이나 타임 스케일을 쉽게 제어할 수 있습니다.
