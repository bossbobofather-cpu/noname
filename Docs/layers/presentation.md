# Presentation Layer

Unity 씬에서 GameState를 `GameViewModel` 이벤트로 바인딩하고 UI/뷰를 구성합니다.

## 주요 View
- **DefenseGameBootstrapper**: 모든 View/Prefab/Adapter를 주입하고 `GameViewModel`을 생성.
- **PlayerView / EnemyView / FortressView**: 위치·체력 등 상태 변화를 반영.
- **ProjectileView / ResourceDropView**: 이동·수집 애니메이션 처리.
- **AugmentSelectionView**: 어빌리티 선택 UI.
- **DefenseDebugPanel**: 개발용 치트/디버그 UI.

## ViewModel
- `GameViewModel`: 입력을 읽고 `DefenseSimulationService`를 호출하여 턴 진행, 어빌리티 선택, 폭격 입력 등을 관리합니다.

## Managers
- `UIFeedbackManager`, `FXManager`, `SoundManager`: UI/FX/Sound 이벤트를 중앙에서 처리(현재는 로그 기반 스텁).

씬에서는 `DefenseGameBootstrapper` 하나가 ViewModel/UseCase/Repository를 조립하고 Inspector 노출 필드에 View/Prefabs/Adapters를 연결합니다.
