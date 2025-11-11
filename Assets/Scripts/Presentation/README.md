# Presentation Layer

Unity 씬에서 GameState를 View/ViewModel/Manager로 바인딩하는 계층입니다. `GameViewModel` 이벤트를 구독해 UI를 활성/비활성하고, Input·FX·사운드 매니저를 orchestrate 합니다.

## 주요 View
- **DefenseGameBootstrapper**
  - Inspector로 `DefenseInputAdapter`, Player/Fortress/Enemy/Projectile/Drop Prefab, `DefenseGameSettings`, `AugmentSelectionView`, 좌우 이동 제한 Collider를 주입받습니다.
  - `enemyViewVariants` 배열을 통해 적 View Prefab을 가중치 기반으로 선택하며, 없을 경우 `fallbackEnemyViewPrefab`을 사용합니다.
  - `resourceDropPrefabs`를 이용해 경험치/골드/체력/증강 드롭을 스폰하고, 게임 오버 시 남아 있는 드롭을 즉시 제거하며 골드·증강 보상은 바로 적용합니다.
- **PlayerView / EnemyView / FortressView**: `GameViewModel` 이벤트로 위치·체력·파괴 상태를 반영합니다.
- **ProjectileView / ResourceDropView**: 이동·충돌·수집 애니메이션을 담당합니다.
- **AugmentSelectionView**: 증강(어빌리티) 선택 UI를 열고, 선택 결과를 `GameViewModel`로 돌려줍니다.
- **DefenseDebugPanel**: 경험치 부스트, 시간 정지 등 개발용 헬퍼를 노출합니다.
- **DefenseInputAdapter**: 최신 Input System을 사용해 좌우 이동 값과 격자 셀 지정용 포인터 좌표를 읽어 `IGameInputReader`로 전달합니다.

## ViewModel
- **GameViewModel**
  - 입력을 처리하고 `MovePlayerUseCase`, `DefenseSimulationService`를 호출해 턴을 진행합니다.
  - `TargetCellChanged` 이벤트로 현재 조준 중인 격자 셀(`GridCellSelection`)을 브로드캐스트하며, 적이 제거되면 자동으로 선택을 해제하고 가장 가까운 적으로 재조준합니다.
  - 플레이어/적/투사체/드롭/어빌리티 선택 이벤트를 묶어 각 View/Manager가 화면을 갱신하도록 합니다.

## 흐름 요약
1. 플레이어가 좌우로 이동하고, 포인터 클릭으로 조준 셀을 선택합니다.
2. `DefenseSimulationService`가 몬스터 이동·공격·드롭/투사체 판정을 계산해 `SimulationStepResult`로 돌려줍니다.
3. ViewModel이 결과 이벤트를 전달하면 View가 위치/HP/이펙트를 갱신합니다.
4. 드롭을 수거하면 `ResourceDropView`가 곡선을 따라 플레이어에게 이동하고, 게임 오버 시에는 모든 드롭이 즉시 정리됩니다.
5. 성벽 HP가 0이 되면 `GameOver` 이벤트가 발생하고, Bootstrapper가 남은 비주얼과 이펙트를 정리합니다.
