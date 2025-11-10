# Infrastructure 계층

입력 장치와 Unity 구성 요소가 Application/Core 계층의 포트를 구현합니다.

- **Input**
  - `DefenseInputAdapter` : 최신 Input System에서 좌우 이동 입력과 공격 트리거(좌클릭 등)를 읽어 `IGameInputReader`에 전달합니다.
- **Unity Helpers**
  - `Float2UnityExtensions` : Core에서 사용하는 `Float2`와 `UnityEngine.Vector2` 사이 변환 유틸리티입니다.

새로운 인프라 구현은 동일한 디렉터리 구조에 배치한 뒤 Application 포트와 연결해 사용합니다.
