# Core 계층

Unity 엔진 구현과 분리된 순수 게임 규칙과 상태를 보관합니다.

- **Entities**
  - `PlayerEntity` : 위치, 이동/공격 스탯, 경험치·레벨, 체력, 행운, 골드를 관리하며 게임 어빌리티가 적용되면 스탯을 갱신합니다.
  - `FortressEntity` : 성벽의 최대/현재 체력을 추적합니다.
  - `EnemyEntity` : 적의 이동 속도·체력·공격 속성, 전투 역할(근접/원거리), 선호 거리, 경험치 보상 값을 제공합니다.
  - `ProjectileEntity` : 투사체의 위치·속도·폭발 반경·목표 지점을 추적합니다.
  - `ResourceDropEntity` : 경험치/골드/HP/추가 증강 등 드랍 오브젝트와 자동 픽업까지의 지연 시간을 표현합니다.
  - `GameState` : 플레이어·성벽·적·투사체·경험치 픽업 목록과 스폰 범위, 경과 시간, 포격 고정 지점을 보관하며 리셋을 담당합니다.

- **Primitives**
  - `Float2` : Core 전역에서 사용하는 경량 2D 벡터 구조체입니다.

- **Enums**
  - `EnemyCombatRole` : 근접/원거리 전투 유형을 구분합니다.
  - `ProjectileFaction` : 투사체가 플레이어/적 중 어느 진영에 속하는지 표현합니다.

- **ValueObjects**
  - `DefenseGameSettings` : 플레이어 기본 스탯(체력/행운 포함)과 경험치 성장, 성벽 구성, 적 스폰 범위, 투사체 속도, 게임 어빌리티 풀, 행운 가중치를 통합합니다.
  - `EnemyDefinition` : ScriptableObject 형태로 적 스탯과 경험치/골드/HP/증강 드랍 정보를 정의하며 `EnemyEntity`를 생성합니다.
  - `EnemySpawnEntry` : 특정 적 정의와 스폰 확률(가중치)을 묶어 둡니다.
  - `GameplayAbilityDefinition` / `GameplayEffectDefinition` : 게임 어빌리티 시스템을 구성하는 ScriptableObject로, 여러 효과(Modifier)의 집합을 표현합니다.

Application, Infrastructure, Presentation 계층은 위 도메인 모델을 기반으로 상위 정책을 구현합니다.
