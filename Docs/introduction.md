# Introduction

Noname Defense Prototype은 브릭브레이커 감성의 턴제 디펜스 게임 시제품입니다.  
포탑(Player)이 성벽을 지키고, 상단에서 내려오는 적을 제거해 웨이브를 버티는 플레이 루프를 중심으로 설계됐습니다.

## 게임 루프
- 화면 하단의 포탑이 좌우로 이동하며 공격 각도를 조준합니다.
- 발사 이후 모든 포탄이 회수될 때까지는 이동이 제한되고, 몬스터 턴이 진행됩니다.
- 적은 성벽에 도달하거나 포탄에 맞으면 각각 피해/제거 처리되며, 경험치·골드·증강 드롭을 남깁니다.
- 경험치를 모아 레벨업하면 3종 증강 중 하나를 선택하여 포탑 능력을 강화합니다.

## 아키텍처 하이라이트
- **Clean Architecture**: Core / Application / Infrastructure / Presentation 계층으로 분리해 도메인 모델을 보호했습니다.
- **Definition Importer**: Excel의 `[enemyInfo]`, `[playerInfo]` 섹션을 JSON과 ScriptableObject로 변환하는 Editor 툴.
- **DocFX + GitHub Pages**: XML 주석을 자동 문서화하고 Pages에 배포하여 항상 최신 API를 열람할 수 있습니다.

## 문서 탐색 가이드
- 빠른 요약과 테이블: [Getting Started](getting-started.html)
- 계층별 세부 설명: [Layers](layers/index.html) (Core/Application/Infrastructure/Presentation)
- API Reference: `api/` 경로에서 클래스별 XML 주석 기반 설명 확인
- 저장소 및 워크플로 개요는 [홈페이지](../index.html)에서 보실 수 있습니다.
