# Definition Importer

`Tools ? Definition Importer` 창에서 Excel/JSON 데이터를 기반으로 `EnemyDefinition`, `PlayerDefinition` ScriptableObject를 관리합니다.

## 엑셀 → JSON
1. `[enemyInfo]`, `[playerInfo]` 섹션이 포함된 `.xlsx` 파일을 지정합니다.
2. Definition Type(Enemy/Player)을 선택하고 `Convert` 버튼을 누르면 해당 섹션이 JSON으로 직렬화됩니다.

## JSON → ScriptableObject
1. Definition Type을 선택한 뒤 생성된 JSON 파일을 지정합니다.
2. `Import JSON -> ScriptableObjects` 버튼을 누르면
   - `Assets/Resources/Enemies` 또는 `Assets/Resources/Players` 폴더에 코드명 기반 SO가 생성/갱신됩니다.
   - JSON에 존재하지 않는 코드의 기존 SO는 자동으로 삭제됩니다.
   - 동일한 코드 SO의 값이 바뀌면 변경된 필드가 콘솔 경고로 출력됩니다.

이 툴을 통해 데이터 팀이 엑셀만 수정해도 게임 내 정의를 일괄 갱신할 수 있습니다.
