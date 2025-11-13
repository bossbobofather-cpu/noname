# Definition Import Workflow (Editor)

DefinitionImportUtility 기반 배치 명령만 사용합니다. 기존 Definition Importer 창은 더 이상 제공되지 않습니다.

## XLSX 요구사항

- Enemy/Player 엑셀에는 [enemyInfo], [playerInfo] 섹션이 필수입니다.
- Stage 엑셀에는 [stageInfo]와 waveColumns, spawnCountPerRow, difficultyScale, monsterWeights/monsterWeightTable 중 하나 이상의 헤더가 필요합니다.
- Stage 몬스터 엔트리는 (EnemyCode:10001;weight:1) 형식을 따릅니다.
- 모든 실수형 스탯/드랍 값은 만분율 정수(값 × 10,000)로 입력합니다.

## 배치 메뉴

**Tools → Definition Importer** 메뉴에서 아래 항목을 실행하세요.

1. **Run Enemy Definitions** – Assets/Table/enemyDefinitions.xlsx → enemyDefinitions.json → Assets/Resources/Enemies/*.asset
2. **Run Player Definitions** – Assets/Table/playerDefinitions.xlsx → playerDefinitions.json → Assets/Resources/Players/*.asset
3. **Run Stage Definitions** – Assets/Table/Stage/*.xlsx → 동일한 이름의 JSON + Assets/Resources/Stages/*.asset
4. **Run All Definitions** – 위 세 단계를 순차 실행

각 단계가 완료되면 Console 로그에서 결과를 확인할 수 있습니다.

## 문제 해결 팁

- 배치를 실행하기 전에 관련 XLSX 파일을 모두 닫아 파일 잠금을 방지하세요.
- 몬스터 가중치가 비어 있으면 헤더 이름과 (EnemyCode:코드;weight:값) 문자열이 올바른지 확인하세요.
- XLSX를 수정한 뒤에는 반드시 해당 배치를 다시 실행해야 JSON과 ScriptableObject가 갱신됩니다.
