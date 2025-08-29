# 튜토리얼 / 대사 / 덱빌딩 / 로딩창 / 타이틀씬

---

## 📌 목차

- [소개](#소개)
- [주요 기능](#주요-기능)
- [시스템 구성](#시스템-구성)
  - [1. 튜토리얼 시스템 (FSM 기반)](#1-튜토리얼-시스템-fsm-기반)
  - [2. 대사 시스템 (CSV--ScriptableObject)](#2-대사-시스템-csv--scriptableobject)
  - [3. 덱빌딩 시스템](#3-덱빌딩-시스템)
  - [4. 로딩 창 시스템](#4-로딩창창-시스템)
  - [5. 타이틀 씬 캐릭터 연출](#5-타이틀-씬-캐릭터-연출)

---

## 주요 기능

- 튜토리얼 진행 상태 저장 및 재시작
- 대사 스크립트 자동 변환 및 UI 연출
- 캐릭터 장비/스킬 기반 덱 구성
- UX를 고려한 부드러운 씬 전환
- 타이틀 화면에서의 캐릭터 등장/걷기 연출

---

## 시스템 구성

### 1. 튜토리얼 시스템 (FSM 기반)

- `TutorialManager`: FSM 구조 기반으로 `TutorialStepSO`를 순차 실행
- `TutorialActionExecutor`: 기능별 모듈 분리 (예: UI 강조, 대사 출력, 트리거 대기, 보상 지급 등)
- 저장/불러오기: Step ID 기반으로 중단 지점부터 재시작 가능
- 씬 진입 시 특정 단계부터 시작 가능

**특징:**
- FSM 기반 흐름 제어
- 연출 모듈을 Executor로 분리 → 유지보수 용이
- UI 강조, 차단, 대사 출력 등 유연한 연출 지원

---

### 2. 대사 시스템 (CSV → ScriptableObject)

- CSV 포맷으로 작성된 대사 파일을 CSV → JSON → ScriptableObject로 자동 변환
- `DialogueManager`가 SO 파일을 로드하여 컷신/대화 실행
- 캐릭터 이름, 말투, 초상화, 배경 등이 자동 적용
- 디자이너가 직접 대사 수정 가능

**특징:**
- 스토리 연출 자동화
- 디자이너 친화적 파이프라인
- 데이터 기반 컷신 구성

---

### 3. 덱빌딩 시스템

- `EntryDeckData`: 캐릭터, 장비, 스킬 정보를 구조화하여 전달
- `DeckSelectManager`: 장착/해제 로직 처리
- `PlayerDeckContainer`: 전투 씬으로 선택 덱 전달
- 장비: `Dictionary<EquipmentType, EquipmentItem>`로 관리
- 스킬: 고정된 4개 슬롯 (패시브 1, 액티브 3)

**특징:**
- UI와 로직 분리된 구조
- 슬롯 기반 덱 UI
- 구조화된 데이터 전달로 확장 용이

---

### 4. 로딩 창 시스템 (비동기 + UX 보장)

- `LoadSceneManager`: `AsyncOperation` 기반 비동기 로딩
- 진행률 0~0.9를 이벤트로 전달 → `LoadingScreenController`에서 UI 표시
- `Mathf.MoveTowards`로 슬라이더 값 부드럽게 보간
- UX 타이밍 제어: 최소 로딩 시간(minLoadTime), 로딩 완료 후 추가 지연(extraDelayAfterReady)
- 검은 화면 페이드 아웃 후 로딩 UI 비활성화

**특징:**
- UX를 고려한 부드러운 씬 전환
- 이벤트 기반 진행률 표시
- 프리팹 기반 로딩 UI 구성

---

### 5. 타이틀 씬 캐릭터 연출

- DOTween을 활용한 캐릭터 등장 및 걷기 연출 시스템
- 캐릭터가 순차적으로 등장 → 대열 정렬 → 걷기 시작 → UI 등장

**주요 구성 요소:**
| 구성 요소 | 설명 |
|-----------|------|
| `CharacterIntroManager` | 전체 연출 컨트롤 |
| `characterObjects[]` | 등장 캐릭터 |
| `formationPositions[]` | 대열 정렬 위치 |
| `walkDirection` | 걷는 방향 기준 오브젝트 |
| `overlay` | 페이드용 이미지 |
| `startButton` | 연출 종료 후 나타날 버튼 UI |

**기능:**
- 화면 페이드 인/아웃
- 캐릭터 개별 달리기 + Idle 전환
- 무한 걷기 연출 (`SetSpeedBased(true)`)
- 자연스러운 버튼 스케일 등장 연출




