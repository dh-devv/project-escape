# Unity Client

Unity 게임 클라이언트입니다.

## 주요 위치

- `Assets/Carten/Script/Player/`: 플레이어 로직
- `Assets/Carten/Script/Enemy/`: 일반 적 로직
- `Assets/Carten/Script/Boss/`: 보스 로직
- `Assets/Carten/Script/GameManager/`: 스테이지와 게임 진행
- `Assets/Carten/Script/Network/`: 백엔드 API 통신
- `Assets/Scenes/`: Unity 씬

API 통신 설정과 호출 방법은
`../docs/backend/unity-api-integration.md`를 참고합니다.

피격 스턴, 상호작용 오브젝트, 2·3페이즈 보스와 별도 데모 씬의
실행 방법·연결 설정·검증 결과는
[`gameplay-implementation.md`](../docs/game/gameplay-implementation.md)를 참고합니다.

C# 문법·에셋 정적 검사와 Unity에서 실행할 회귀 테스트 절차는
[프로젝트 점검 결과](../docs/project-review.md)를 참고합니다. 정적 검사가 Unity 컴파일을 대신하지는 않습니다.
