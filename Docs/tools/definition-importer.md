# Definition Importer Tool

Excel 시트에서 적/플레이어 정의를 읽어 JSON과 ScriptableObject로 변환하는 Unity Editor 창입니다.

## 열기
Unity 상단 메뉴의 `Tools > Definition Importer`를 선택합니다.

## Excel → JSON
1. Definition Type(Enemy/Player)을 고릅니다.
2. `[enemyInfo]`, `[playerInfo]` 섹션을 포함하는 `.xlsx` 파일을 선택합니다.
3. Output JSON 경로를 지정한 뒤 `Convert` 버튼을 누르면 JSON 파일이 생성됩니다.

## JSON → ScriptableObject
1. JSON Import Path에 변환된 파일을 지정합니다.
2. `Import JSON -> ScriptableObjects` 버튼을 누르면
   - `Assets/Resources/Enemies` 또는 `Assets/Resources/Players`에 코드(ID) 기반 SO가 생성/갱신됩니다.
   - JSON에 없는 SO는 자동 삭제되고, 값이 변경된 필드는 경고 로그로 출력됩니다.

## 주의 사항
- Excel 셀 안에서 드롭 리스트는 `(type:Gold;amount:5;probability:1)` 형식으로 작성합니다.
- 행 시작이 `;`인 경우 주석으로 간주되어 무시됩니다.
- DefinitionImporterWindow.cs에 XML 주석이 포함되어 있으므로 DocFX API 문서에서도 인터페이스를 확인할 수 있습니다.
