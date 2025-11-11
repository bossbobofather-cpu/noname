# Core Layer

Unity 구현과 분리된 게임 규칙·도메인 모델을 정의합니다.

## Entities
- `PlayerEntity`: 위치, 이동/공격 스탯, 경험치·레벨·체력·럭·골드, 증강 적용 효과를 관리합니다.
- `FortressEntity`: 성벽의 최대/현재 체력과 피해·회복 로직을 캡슐화합니다.
- `EnemyEntity`: 격자 행/열 좌표, 체력/공격력/사거리/쿨다운, 드롭 테이블을 보관합니다.
- `ProjectileEntity`: 투사체 위치·속도·폭발 반경·목표 좌표를 추적합니다.
- `ResourceDropEntity`: 드롭 ID/종류/지연 시간을 기록해 수집 시점을 계산합니다.
- `GameState`: 플레이어·성벽·적·투사체·드롭 컬렉션을 묶는 루트 상태이며, 플레이어가 조준 중인 격자 셀(row/column)도 함께 보관합니다.

## Primitives & Enums
- `Float2`: Core 전용 경량 2D 벡터.
- `ProjectileFaction`, `ResourceDropType` 등 게임 진행에 필요한 상수들을 모았습니다.

## Value Objects
- `DefenseGameSettings`: 플레이어/성벽 기본값과 격자 스폰 파라미터를 정의하며, `GetCellWorldPosition`/`TryGetCellIndices`로 행·열 ↔ 월드 좌표 변환을 제공합니다. 행 개수에 관계없이 계속 내려오는 적을 위한 트리거 위치(플레이어/성벽) 판단 기준도 제공합니다.
- `EnemyDefinition`, `EnemySpawnEntry`: ScriptableObject 기반 적 스펙/가중치 테이블.
- `GameplayAbilityDefinition`, `GameplayEffectDefinition`: 증강(Modifier) 집합을 정의합니다.

상위 계층(Application/Infrastructure/Presentation)은 이 모델을 직접 사용해 UseCase, Adapter, View를 구성합니다.
