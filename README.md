# project-escape
Unity 기반 로봇 탈출 액션 게임 프로젝트

# LAST SPARK

> **2D Risk Management Action Game**

플레이어의 체력이 감소할수록 더욱 강해지는 **리스크 관리형 2D 액션 게임**입니다.

---

## About

**LAST SPARK**는 플레이어의 HP에 따라 전투 방식이 변화하는 2D 액션 게임입니다.

플레이어는 체력을 잃을수록 이동 속도와 공격 능력이 강해지지만, 동시에 생존에 대한 위험도 높아집니다.

각 Phase의 특징을 활용하여 전투를 진행하고 보스를 처치하는 것이 핵심 게임 플레이입니다.

### Phase System

| Phase | HP | 특징 |
|:---:|:---:|---|
| **Phase 1** | 70~100% | 높은 방어력, 느린 이동 |
| **Phase 2** | 30~70% | 일반적인 전투, 대시 사용 |
| **Phase 3** | 1~30% | 빠른 이동, 강화된 공격 능력 |

---

## Project Goal

> **실제 사용자가 플레이할 수 있는 게임을 완성하는 것**

Unity로 게임을 개발하고 Spring Boot 기반 백엔드와 MySQL을 연동한 후, AWS 환경에 배포하여 실제로 구동되는 게임을 만드는 것을 목표로 합니다.

---

## Key Features

- 2D 액션 게임
- HP에 따른 Phase 변화
- 보스 전투
- 게임 결과 데이터 저장
- Unity ↔ Backend API 통신
- AWS 서버 배포

> 랭킹 기능은 팀 협의 후 개발 여부를 결정할 예정입니다.

---

## Tech Stack

| 기능 | 기술 |
|---|---|
| Client | Unity |
| Backend | Spring Boot |
| Database | MySQL |
| Server | AWS EC2 / Linux |
| Deployment | Docker |
| Version Control | Git / GitHub |

---

## Architecture

```text
Unity Client
     │
     │ HTTP / REST API
     ▼
Spring Boot
     │
     ▼
   MySQL

AWS EC2
└── Docker
    └── Backend / Database
```

---

## Team

| Member | Role |
|---|---|
| **김승운** | Unity Game Development · Game Planning |
| **변동헌** | Backend · Database · Infrastructure · PM · Game Planning |

### 김승운

- Unity 기반 게임 클라이언트 개발
- 플레이어 및 보스 구현
- 전투 및 게임플레이 로직 개발
- 게임 UI 개발
- 게임 기획

### 변동헌

- Spring Boot 백엔드 개발
- MySQL 데이터베이스 설계 및 연동
- 게임 결과 API 개발
- Unity ↔ Backend API 연동
- Docker 환경 구성
- AWS EC2 배포 및 서버 관리
- GitHub 프로젝트 관리
- 프로젝트 일정 및 진행 관리 (PM)
- 게임 기획

---

## AI Usage

본 프로젝트는 **AI를 개발 과정에 활용하여 제작**합니다.

AI를 활용하여 코드 작성 및 수정, 오류 해결, 기술 학습, 아이디어 정리 등의 작업을 진행합니다.

AI가 생성한 결과물을 그대로 사용하는 것이 아니라, 프로젝트에 맞게 검토하고 수정하여 적용하는 것을 목표로 합니다.

---

## Project Structure

```text
project-escape/
├── backend/
├── database/
├── docs/
├── unity/
└── README.md
```

---

## Development Goal

Unity 게임과 백엔드 서버를 완성하고 AWS에 배포하여,

**실제 사용자가 게임을 실행하고 플레이할 수 있는 프로젝트를 만드는 것**을 최종 목표로 합니다.