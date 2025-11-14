# Work List
> 하루 단위로 “무엇을 했고 / 왜 했고 / 애로사항은 무엇이었는지”를 기록하는 페이지입니다. 최신 기록을 최상단에 추가하세요.

## 작성 규칙
- **최신순(최근 날짜가 위)** 으로 섹션을 추가합니다.
- 각 항목은 `작업 내용`, `이유`, `애로사항·해결` 최소 세 줄로 구성합니다.
- 애로사항이 없다면 `애로사항: 없음`이라고 명시합니다.
- 필요 시 하단에 참고 링크 또는 스크린샷을 첨부해도 됩니다.

## Work Entries

### 2025-11-15
- **작업 내용:** Addressables 그룹 구성 및 CoreRuntime/DefenseGameBootstrapper Addressables 전환
- **이유:** 씬 독립 실행, 패치 배포 대비, Resources 제거
- **애로사항·해결:** 허브 프리팹이 GameObject 타입으로만 등록되어 InvalidKeyException 발생 → GameObject 로드 후 `GetComponent<CoreRuntimeHub>()`로 해결

### 2025-11-14
- **작업 내용:** Resource.Load 기반 Definition/Prefab 로드를 Addressables 로 이관
- **이유:** 런타임 리소스 관리 일원화, 빌드 사이즈/패치 효율 개선
- **애로사항·해결:** 일부 Prefab이 Sprite/Material을 잃어버려 보이지 않는 문제 → Prefab 참조 재확인, 새 머티리얼 생성 후 SpriteRenderer에 재할당

> 새 기록을 추가할 때는 위 형식을 복사해 사용하세요.
