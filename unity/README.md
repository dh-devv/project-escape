# Unity Client

Unity 게임 클라이언트입니다.

## 주요 위치

- `Assets/Carten/Script/Player/`: 플레이어 로직
- `Assets/Carten/Script/Enemy/`: 일반 적 로직
- `Assets/Carten/Script/Boss/`: 보스 로직
- `Assets/Carten/Script/GameManager/`: 스테이지와 게임 진행
- `Assets/Carten/Script/Network/`: 백엔드 API 통신
- `Assets/Scenes/`: Unity 씬
- `Assets/Carten/Menu/`: 시작 버튼과 게임 HUD
- `Assets/Carten/Script/UI/`: 별도 메뉴/플레이어 UI가 없는 씬용 보조 UI

`Assets/Scenes/` 아래에 저장한 씬은 Editor의 Scene List에 자동 등록됩니다.
`UI/Main UI`를 첫 번째로 두고 스테이지 순서로 정렬합니다. Build Profile의
`Override Global Scene List`는 끄고 공통 목록을 사용합니다.
UI 이미지와 프리팹은 씬에서 참조하면 빌드에 포함됩니다.
수동으로 갱신하려면 `LAST SPARK > Sync Build Scenes`를 선택합니다.
씬 변경 후에는 Web 빌드와 서버의 빌드 파일 교체가 필요합니다.

`Main UI`의 시작 버튼으로 첫 맵에 진입합니다.
각 맵의 플레이어·카메라·HUD를 사용하며, `MapTransitionManager`는
도착 맵에 별도 플레이어가 없는 경우 기존 플레이어를 유지합니다.
빌드 시 비활성화된 전투 구역·보스와 필수 UI/프리팹 누락을 검사합니다.
저장된 씬 구성 검사는 `python unity/Tools/test_build_content.py`로 실행합니다.
HUD는 HP 바가 필수이며, 숫자·페이즈·스킬 텍스트는 선택 항목입니다.
Unity의 `Window > General > Test Runner`에서 EditMode의 `BuildSceneTests`로
저장된 9개 씬을, PlayMode의 `GameplayBehaviorTests`로 전투·진행 로직을 검사할 수 있습니다.

API 통신 설정과 호출 방법은
`../docs/backend/unity-api-integration.md`를 참고합니다.

피격 스턴, 상호작용 오브젝트, 2·3페이즈 보스와 별도 데모 씬의
실행 방법·연결 설정·검증 결과는
[`gameplay-implementation.md`](../docs/game/gameplay-implementation.md)를 참고합니다.

C# 문법·에셋 정적 검사와 Unity에서 실행할 회귀 테스트 절차는
[프로젝트 점검 결과](../docs/project-review.md)를 참고합니다. 정적 검사가 Unity 컴파일을 대신하지는 않습니다.
