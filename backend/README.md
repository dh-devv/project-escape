# Backend

Spring Boot API 서버입니다.

## 주요 위치

- `src/main/java/backend/controller/`: HTTP API 엔드포인트
- `src/main/java/backend/service/`: 비즈니스 로직
- `src/main/java/backend/persistence/`: JPA 엔티티와 저장소
- `src/main/java/backend/dto/`: 요청/응답 형식
- `src/main/java/backend/exception/`: 공통 오류 응답
- `src/main/resources/`: 실행 환경 설정
- `src/test/`: H2 자동 테스트와 선택적으로 실행하는 MySQL 통합 테스트

## 실행

프로젝트 루트에서 실행합니다.

```powershell
.\backend\gradlew.bat -p backend bootRun
```

DB 환경변수 설정과 API 설명은 `database/README.md`와 `docs/backend/api.md`를 참고합니다.
Docker 실행은 `deployment/README.md`를 참고합니다.

## 검사

```powershell
.\backend\gradlew.bat -p backend clean test bootJar --console=plain
```

기본 테스트는 H2를 사용합니다. 실제 MySQL 검사에는 DB 접속 환경변수와 `VERIFY_MYSQL=true`가 필요합니다.
검사 범위와 실행 절차는 [프로젝트 점검 결과](../docs/project-review.md)에 정리했습니다.
