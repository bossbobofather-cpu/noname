# Core Layer

Unity 엔진 구현과 분리된 순수 게임 규칙·도메인 모델을 담는 계층입니다.

## Entities
- `PlayerEntity`: 위치, 이동/공격 스탯, 경험치·레벨·체력·럭·골드, 어빌리티/드롭 효과 적용.
- `FortressEntity`: 성벽의 최대/현재 체력을 추적하고 피해·회복·리셋 처리.
- `EnemyEntity`: 적 이동 속도, 체력, 공격력, 역할(근접/원거리), 드롭 테이블.
- `ProjectileEntity`: 투사체 위치·속도·폭발 반경·목표 좌표.
- `ResourceDropEntity`: 드롭 ID/종류/양/지연 시간.
- `GameState`: 플레이어/거점/적/투사체/드롭 컬렉션, 스폰 영역, 경과 시간, 폭격 지점 등 전체 상태.

## Primitives
- `Float2`: Core 전용 경량 2D 벡터.

## Enums
- `EnemyCombatRole`, `ProjectileFaction` 등 도메인 상수.

## Value Objects
- `DefenseGameSettings`: 플레이어 기본 스탯, 경험치 성장, 성벽/스폰/어빌리티 설정.
- `EnemyDefinition`, `EnemySpawnEntry`: ScriptableObject 기반 적 정의와 스폰 가중치.
- `GameplayAbilityDefinition`, `GameplayEffectDefinition`: Ability/Modifier 구성을 담당.

Application·Infrastructure·Presentation 계층은 이 Core 모델을 기반으로 동작합니다.
