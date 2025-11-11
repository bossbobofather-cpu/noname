# Definition Importer Tool

Excel 시트를 기반으로 적/플레이어 정의를 JSON 및 ScriptableObject로 변환하는 Unity Editor 전용 창입니다.

## 열기
Unity 상단 메뉴에서 `Tools > Definition Importer`를 선택합니다.

## Excel → JSON
1. Definition Type(Enemy/Player)을 고릅니다.
2. `[enemyInfo]`, `[playerInfo]` 섹션이 포함된 `.xlsx` 파일을 선택합니다.
3. Output JSON 경로를 지정한 뒤 `Convert` 버튼을 누르면 JSON 파일이 생성됩니다.

## JSON → ScriptableObject
1. JSON Import Path에 변환된 파일을 지정합니다.
2. `Import JSON -> ScriptableObjects` 버튼을 누르면  
   - `Assets/Resources/Enemies` 또는 `Assets/Resources/Players` 아래에 코드(ID) 기반 SO가 생성/갱신됩니다.  
   - 기존 SO와 값이 달라진 항목은 경고 로그로 알려줍니다.

## 컬럼 가이드
- **EnemyInfo**
  - `enemyCode`
  - `maxHealth`
  - `attackDamage`
  - `attackRange` : 성벽으로부터 몇 행 떨어진 곳까지 공격 가능한지(행 단위)
  - `attackCooldown`
  - `dropTable`
- **PlayerInfo**
  - `playerCode`, `moveSpeed`, `attackDamage`, `attackRange`, `attackCooldown`, `maxHealth`, `luck`

## 주의 사항
- 드롭 리스트는 `(type:Gold;amount:5;probability:1)` 형태로 작성합니다.
- 행의 첫 문자가 `;`인 경우 주석으로 간주되어 무시됩니다.
- DefinitionImporterWindow.cs에 XML 주석이 포함되어 있으므로 DocFX API 문서에서도 동일 정보를 확인할 수 있습니다.
