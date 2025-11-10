# Infrastructure Layer

입력/저장소/툴 등 Unity 구성 요소가 Application/Core 계층과 상호작용하도록 중계합니다.

## Input
- `DefenseInputAdapter`: 최신 Input System으로 좌우 이동과 폭격 포인터를 읽어 `IGameInputReader`에 전달.

## Repositories & Tools
- `InMemoryGameStateRepository`: 단일 GameState 인스턴스를 보관.
- Definition Importer 툴: Excel `[enemyInfo]/[playerInfo]` → JSON → ScriptableObject 파이프라인.

## Unity Helpers
- `Float2UnityExtensions`: `Float2`와 `UnityEngine.Vector2/Vector3` 간 변환.

이 계층에서 Application 포트를 구현하고, 외부 데이터(Excel, JSON, SO)를 연결합니다.
