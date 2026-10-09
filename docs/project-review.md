# 프로젝트 점검 결과

점검일: 2026-10-08. 기존 Unity–Spring Boot–MySQL 구조와 API 주소를 유지했습니다.
AI 사용 고지도 유지했습니다. 검증이 필요한 항목은 완료로 표시하지 않습니다.

2026-10-09 재검사에서도 백엔드 8개 테스트·빌드와 Unity 정적 검사가 통과했습니다.
HTTP 허용 정책·데모 공격 키 불일치를 추가로 수정했고, 커밋 안전성과 게임 완성도 평가는
[공모전 제출 준비도](contest-readiness.md)에 정리했습니다. Unity 실행은 여전히 미검증입니다.

## 검증 범위

| 대상 | 결과 | 확인 내용 |
|---|---|---|
| 백엔드 | 통과 | Java 21에서 clean · test · bootJar |
| H2 자동 테스트 | 7개 통과 | 실제 HTTP/DB 저장·조회, 입력 오류, 중복 이름, 시간 검증, 동점 정렬 |
| 로컬 MySQL 8.0.46 | 1개 통과 | 기존 스키마 validate, 임시 사용자·보스 기록 저장/조회, 테스트 행 삭제 |
| Unity 소스 | 정적 검사 통과 | C# 38개 UTF-8·구문 파싱, 어셈블리·입력·패키지 JSON |
| Unity 에셋 | 정적 검사 통과 | YAML 45개, 메타데이터, 로컬 fileID/GUID, Build Settings 씬 4개 |
| Unity 실행 | 미검증 | Editor가 없어 컴파일·PlayMode·화면 확인 불가 |
| Docker/AWS | 실행 미검증 | Compose YAML·경로·환경변수 연결만 점검. Docker 설치와 실제 배포 없음 |

Unity 정적 검사는 패키지 복원이나 타입 검사를 수행하지 않습니다. 공식 패키지 아카이브에서 대조한
GUID 목록은 식별용이며 프로젝트의 패키지 버전을 바꾸지 않습니다. 현재 Editor 전용 렌더링 설정의
외부 참조 99개는 설치된 Unity에서 확인해야 합니다. 이를 누락된 프로젝트 에셋으로 단정하여 교체하지 않았습니다.

## 수정 사항

- 맵 이동 시 체력을 전달하여 페이즈가 초기화되지 않도록 수정했습니다. 목적지 스폰 위치를 스테이지 초기화가 덮어쓰지 않도록 했습니다.
- 연결 전 보스 처치 기록을 지속되는 세션에 보관하여 보스 제거·맵 이동 시 유실되지 않도록 했습니다. 기록은 메모리에만 보관합니다.
- 중복 전투 시작으로 적이 두 번 생성되는 경우를 차단하고, 비어 있는 스폰 설정을 검사합니다. 벽에 닿아 소모된 탄환의 후속 피해도 차단했습니다.
- API 클라이언트의 GET/POST 전송 중복을 통합하고, 잘못된 URL·응답 JSON·서버 오류를 오류 콜백으로 전달하도록 했습니다.
- 랭킹과 최고 기록을 DB에서 개수 제한하여 조회하고, 점수·시간이 같으면 기록 ID로 순서를 고정했습니다.
- 사용자 이름 공백 정규화, 동시 생성 시 고유키 충돌 처리, 무한대 클리어 시간 거부, 숫자 파라미터 오류 응답을 보완했습니다.
- H2 테스트 설정을 `src/test/resources`로 옮겨 운영 JAR에 포함되지 않도록 했습니다. MySQL 테스트는 명시적으로 활성화할 때만 실행합니다.
- 샘플 씬의 삭제된 `WaveManager` 컴포넌트 참조를 제거하고 Volume을 기존 `DefaultVolumeProfile`에 연결했습니다. 사용하지 않는 폴더의 고아 메타데이터와 Network 폴더의 누락된 메타데이터를 정리했습니다.
- Unity 소스 인코딩을 UTF-8로 통일하고 반복 표제 주석·불필요한 줄바꿈을 정리했습니다. 서식 정리 전후 C# 구문 토큰이 같음을 대조했습니다.
- 저장소의 Unity 캐시 제외 경로를 `unity/` 구조에 맞췄습니다. 비어 있던 게임·페이즈 문서를 채우고 API 단위·페이즈 경계·개발 현황을 실제 구현에 맞췄습니다.
- `.editorconfig`로 소스 인코딩과 들여쓰기 기준을 추가했습니다. Compose의 DB 준비 검사는 TCP 접속을 사용하고 비밀번호를 셸에서 따옴표로 감싸도록 수정했습니다. 컨테이너 실행 검증은 별도로 필요합니다.

## 다시 검사하기

프로젝트 루트에서 기본 백엔드 테스트를 실행합니다. MySQL이 없어도 H2 테스트는 실행됩니다.

```powershell
.\backend\gradlew.bat -p backend clean test bootJar --console=plain
```

MySQL 검사는 [DB 설정](../database/README.md)의 접속 환경변수를 준비한 후 실행합니다.
임시 사용자 한 명과 결과 한 건을 생성하며 테스트 종료 시 해당 행만 삭제합니다.
스키마는 `validate`를 사용합니다. AUTO_INCREMENT 번호는 테스트 중 소비될 수 있습니다.

```powershell
$env:VERIFY_MYSQL = "true"
try {
    .\backend\gradlew.bat -p backend test --rerun-tasks --console=plain
} finally {
    Remove-Item Env:VERIFY_MYSQL
}
```

Unity 정적 검사에는 Python 3.12 이상을 사용합니다.

```powershell
python -m venv .venv
.\.venv\Scripts\python.exe -m pip install -r unity/Tools/requirements.txt
.\.venv\Scripts\python.exe unity/Tools/verify_gameplay_assets.py
```

외부 설정 참조 목록이 필요하면 `--verbose`를 붙입니다. 도구의 PASS는 정적 검사 범위에만 적용됩니다.

## 제출 전 남은 실행 확인

1. Unity 6000.5.0f1에서 프로젝트를 열고 패키지 복원 후 Console의 컴파일 오류와 Missing Script를 확인합니다.
2. Test Runner의 PlayMode에서 작성된 회귀 테스트 17개를 실행합니다.
3. 두 보스 데모에서 경직·페이즈 전환·필드 예고·문 열기·체력 유지 맵 이동을 직접 플레이합니다.
4. 백엔드 실행 상태에서 연결 전 처치 → 맵 이동 → 사용자 연결 → 기록 저장을 확인합니다. Unity 클라이언트의 실제 HTTP 실행은 아직 미검증입니다.
5. Docker에서 컨테이너를 시작하고 API 저장/조회를 확인한 뒤 재시작 시 데이터 유지 여부를 확인합니다.

데모 외형과 HUD는 테스트용입니다. 참가자용 시작·결과 화면, 최종 아트와 난이도는 별도 완성 작업이 필요합니다.
인증과 서버 측 점수 검증도 현재 구현에는 없습니다. 제출 설명에서 현재 기능과 향후 계획을 구분해야 합니다.
