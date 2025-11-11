# Infrastructure 계층

입력·저장소·툴 등 Unity 구성 요소가 Application/Core 계층과 상호 작용하도록 중계합니다.

- **Input**
  - `DefenseInputAdapter`: 최신 Input System으로 좌우 이동과 마우스/터치 포인터를 읽어 격자 셀 선택용 월드 좌표를 `IGameInputReader`에 전달합니다.
- **Repositories & Tools**
  - `InMemoryGameStateRepository`: 단일 `GameState` 인스턴스를 보관합니다.
  - Definition Importer: Excel → JSON → ScriptableObject 파이프라인으로 데이터 자산을 생성합니다.
- **Unity Helpers**
  - `Float2UnityExtensions`: Core에서 사용하는 `Float2`와 `UnityEngine.Vector2/Vector3` 간 변환을 제공합니다.

이 계층에서 Application 포트를 구현하고, 외부 데이터나 Unity 구성 요소를 연결합니다.
