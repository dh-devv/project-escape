# project-escape
Unity로 만든 로봇 탈출 액션 게임 프로젝트입니다.

# LAST SPARK

> 체력이 줄어들수록 강해지는 2D 액션 게임입니다.

플레이어의 체력이 감소할수록 더욱 강해지는 **리스크 관리형 2D 액션 게임**입니다.

---

## 게임 소개

**LAST SPARK**는 플레이어의 HP에 따라 전투 방식이 변화하는 2D 액션 게임입니다.

플레이어는 체력을 잃을수록 이동 속도와 공격 능력이 강해지지만, 동시에 생존에 대한 위험도 높아집니다.

각 페이즈의 특징을 활용하여 전투를 진행하고 보스를 처치하는 것이 핵심 게임 플레이입니다.

### 페이즈 시스템

| 페이즈 | 체력 | 특징 |
|:---:|:---:|---|
| **페이즈 1** | 70% 초과~100% | 높은 방어력, 느린 이동, 대시 불가 |
| **페이즈 2** | 30% 초과~70% | 일반적인 전투, 대시 사용 |
| **페이즈 3** | 0% 초과~30% | 빠른 이동, 강화된 전투 능력 |

---

## 프로젝트 목표

> **Unity로 게임을 개발하고 Spring Boot 기반 백엔드와 MySQL을 연동한 후, AWS 환경에 배포하여 실제 사용자가 플레이할 수 있는 게임을 만드는 것을 목표로 합니다.**

---

## 주요 기능

- 2D 액션 게임
- HP에 따른 Phase 변화
- 보스 전투
- 게임 결과 데이터 저장
- Unity와 백엔드 API 통신
- Docker 실행 설정 및 AWS 배포 절차
- 랭킹 조회


---

## 사용 기술

| 구분 | 사용 기술 |
|---|---|
| 클라이언트 | Unity |
| 백엔드 | Spring Boot |
| 데이터베이스 | MySQL |
| 서버 목표 환경 | AWS EC2 / Linux |
| 배포 설정 | Docker Compose |
| 버전 관리 | Git / GitHub |

---

## 구조

### 개발할 때의 구조
```text
Unity 게임
     │
     │ HTTP / REST API
     ▼
Spring Boot 백엔드
     │
     ▼
   MySQL
```

### 배포 후의 구조
```text
Unity 게임
     │
     │ HTTP / REST API
     ▼
AWS EC2
└── Docker
     ├── Spring Boot 백엔드
     └── MySQL 데이터베이스
```

---

## 팀

| 팀원 | 담당 업무 |
|---|---|
| **김승운** | Unity 게임 개발 · 게임 기획 |
| **변동헌** | Unity 게임 개발 · 백엔드 · 데이터베이스 · 서버 환경 · 일정 관리 · 게임 기획 |

### 김승운

- Unity 게임 클라이언트 개발
- 플레이어와 보스 구현
- 전투와 게임플레이 로직 개발
- 게임 화면 개발
- 게임 기획

### 변동헌

- 게임 로직 개발
- Spring Boot 백엔드 개발
- MySQL 데이터베이스 설계 및 연결
- 게임 결과 API 개발
- Unity와 백엔드 API 연결
- Docker 환경 구성
- AWS EC2 배포 및 서버 관리
- GitHub 프로젝트 관리
- 프로젝트 일정 및 진행 관리
- 게임 기획

---

## AI 사용

이 프로젝트는 개발 과정에서 **AI를 활용하여 제작**합니다.

AI를 사용해서 코드 작성과 수정, 오류 해결, 기술 공부, 아이디어 정리 등을 진행합니다.

AI가 만든 결과를 그대로 사용하지 않고, 프로젝트에 맞는지 직접 확인한 뒤 수정해서 사용합니다.

---

## 프로젝트 구조

```text
project-escape/
├── backend/             # Spring Boot 백엔드와 테스트
├── database/            # MySQL 스키마와 DB 설정
├── docs/                # 백엔드, 배포, 게임 기획 문서
├── unity/               # Unity 클라이언트
├── deployment/          # Docker와 배포 설정
└── README.md            # 프로젝트 시작 안내
```

각 주요 폴더의 상세 내용은 `backend/README.md`, `database/README.md`,
`docs/README.md`, `unity/README.md`에서 확인할 수 있습니다.

---

## 현재 개발 상황

| 항목 | 상태 |
|---|:---:|
| Spring Boot API · 랭킹 · 요청 검증 | 구현 · 자동 테스트 통과 |
| MySQL 연결 · 결과 저장/조회 | 로컬 MySQL에서 검증 |
| 피격 경직 · 상호작용 · 2/3페이즈 보스 | 구현 · 정적 검사 통과, Unity 실행 검증 필요 |
| Unity와 백엔드 API 연결 | 데모 연동 구현, Unity 실행 검증 필요 |
| Docker 환경 구성 | 설정 작성, 컨테이너 실행 검증 필요 |
| AWS EC2 배포 | 절차 작성, 실제 배포 예정 |
| 최종 게임 테스트 | Unity Editor에서 진행 필요 |

2026-10-08 기준 백엔드 테스트 8개(H2 7개, 실제 MySQL 1개)와 실행 JAR 빌드가 통과했습니다.
이 PC에는 Unity Editor와 Docker가 없어 게임 실행과 컨테이너 동작은 아직 확인하지 못했습니다.
점검 범위와 남은 확인 항목은 [프로젝트 점검 결과](docs/project-review.md),
게임 실행 방법은 [전투·상호작용 구현 안내](docs/game/gameplay-implementation.md)에 정리했습니다.

---

## API 목록

| 방식 | 주소 | 설명 |
|---|---|---|
| POST | `/api/v1/users` | 플레이어 생성 |
| POST | `/api/v1/scores` | 게임 결과 저장 |
| GET | `/api/v1/ranks` | 랭킹 조회 |
| GET | `/api/v1/users/{userId}/best` | 개인 최고 기록 조회 |

## 백엔드 실행

로컬 MySQL을 사용하는 경우에는 [database/README.md](database/README.md)의 환경변수 설정 후
다음 명령으로 실행합니다.

```powershell
.\backend\gradlew.bat -p backend bootRun
```

Docker 실행과 AWS 배포 절차는
[docs/backend/deployment.md](docs/backend/deployment.md)를 참고합니다.
