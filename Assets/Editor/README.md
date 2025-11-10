# Definition Importer

`Tools > Definition Importer` 메뉴에서 Excel/JSON 데이터를 불러와 `EnemyDefinition`, `PlayerDefinition` ScriptableObject를 생성/갱신합니다.

## Excel → JSON
1. `[enemyInfo]`, `[playerInfo]` 섹션을 포함한 `.xlsx` 파일을 선택합니다.
2. Definition Type(Enemy/Player)을 지정한 뒤 `Convert` 버튼을 누르면 해당 시트가 JSON으로 변환됩니다.

## JSON → ScriptableObject
1. Definition Type을 고르고 변환할 JSON 파일을 선택합니다.
2. `Import JSON -> ScriptableObjects` 버튼을 누르면
   - `Assets/Resources/Enemies` 또는 `Assets/Resources/Players` 폴더에 코드(ID) 기반 SO가 생성/갱신됩니다.
   - JSON에 없는 SO는 자동 삭제됩니다.
   - 기존 SO와 값이 다르면 어떤 필드가 바뀌었는지 경고 로그가 출력됩니다.

Excel/JSON 구조가 바뀔 경우 DefinitionImporterWindow.cs를 함께 수정해 주세요.
