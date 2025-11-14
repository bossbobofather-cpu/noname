# Work List
> 하루 단위로 “무엇을 했고 / 왜 했고 / 애로사항은 무엇이었는지, 어떻게 해결 했는지”를 간단히 기록하는 페이지입니다. 

## 작성 규칙
- **최신순(최근 날짜가 위)** 으로 항목을 추가합니다. 새로운 날의 기록은 항상 문서 상단에 삽입됩니다.

## Work Entries

### 2025-01-01
- **작업 내용**
- 작업 내용
- **작업 이유**
- 작업 이유
- **애로사항 및 해결 방안**
- 애로사항 및 해결 방안

> 위는 예시 입니다.

### 2025-11-15

### 2025-11-14

- **작업 내용** 
- CoreRuntime 클래스 설계 및 CoreRuntime Prefab 제작
- RuntimeInitialization.cs 제작 및 Initialize 함수를[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] 어트리뷰트를 부여했다.

- **이유** 
- 씬 단위 테스트가 용이하도록 하고 싶었고, 게임 코어 모듈에 포함 될 만 한 GameSceneManager, UIManager,FXManager, SoundManager등의 레퍼런스를 물고 있는 CoreRuntime Prefab을 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] 부여 된 함수를 통해 Instantiate하도록 하여 어떤 씬을 실행하던지 관련 기능들을 문제 없이 접근하여 사용 할 수 있게끔 했다.
- **애로사항 및 해결 방안**
- 싱글턴 패턴을 사용하지 않은 이유는 싱글턴 패턴을 안쓰고 싶었다. 명확하게 매니저 등 공용 모듈 객체들의 인스턴스 생성 지점을 정하고 싶었다. 
또한 씬 초기화(Awake) 보다 이전 시점에 호출 됨을 이용해서 게임 전반적인 영향을 주는 코드를 넣을 지점으로 쓰기 좋을 거 같았다. 

--

- **작업 내용** 
- Load 방식 제거 및 Addressable로의 전환
- **이유** 
- 장기적 관점에서 원격 패치까지 고려하고 있어서
- **애로사항 및 해결 방안**
- 없음

--

- **작업 내용** 
- 기존 Definition Importer 툴을 개선하여 메뉴버튼 클릭으로 xlxs -> json -> so 가 진행되고 의도 된 경로에 작성 되도록 수정했다.
- stage Definition을 추가 하였다.
- **이유** 
- 게임 Stage 별 웨이브 관리를 위해 데이타를 추가했고 해당 데이타는 적 웨이브의 Column 수, 난이도, 행 당 스폰 되는 몬스터의 수, 스폰 되는 몬스터의 종류와 가중치를 입력 할 수 있다.
- 컨버팅하는 과정이 수고스러워서 잡을 압축하기 위해서 각 데이타별 컨버팅 버튼들과 전체 데이타 컨버팅 버튼으로 분리했다.
- **애로사항 및 해결 방안**
- 아직 해당 Stage 데이타를 기반하여 BattleScene 이 구성되진 않고 이후 로비 씬 작업 이후 Stage 진입 시점 작업을 진행하며 대응 할 예정

--

- **작업 내용** 
- CommponentPool.cs 및 ComponentPoolRegistry.cs 작성
- **이유** 
- 생성 된 오브젝트들의 재활용을 위해 만들었다.
- 프리팹 name을 key로 풀을 만들어서 사용한다.
- **애로사항 및 해결 방안**
- 없음