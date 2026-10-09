# 전투·상호작용 구현 및 검증

## 바로 실행하기

Unity **6000.5.0f1**에서 `unity` 폴더를 열고 아래 씬을 실행합니다. 기존 Stage1 맵은 수정하지 않았습니다.

- `Assets/Scenes/MiniBossDemo.unity`: 2페이즈 중간 보스, 원거리 적, 문을 여는 일회용 스위치.
- `Assets/Scenes/FinalBossDemo.unity`: 중앙 고정 대형 보스와 3페이즈 필드 공격.
- 이동 A/D 또는 방향키, 점프 Space, 대시 Shift, 기본 공격 숫자패드 3, 스킬 Q/W/E, 상호작용 F.
- 초록 스위치 근처에서 F를 누르면 파란 문이 열립니다. 맵 끝의 출구에서 F를 누르면 다른 데모 씬으로 이동합니다.
- 노란 테두리는 공격 예고, 빨간 테두리는 실제 피해 범위입니다. 판정은 예고 시 고정되므로 범위를 벗어나 회피할 수 있습니다.
- 데모의 보스 외형은 임시 사각형입니다. 기존 Enemy3보다 중간 보스는 1.35배, 최종 보스는 4배입니다. 데모 외형을 별도로 교체할 수 있도록 프로젝트 안에 임시 스프라이트를 포함했습니다. 원본 맵의 기본 도형과 재질은 Unity 패키지 참조를 유지합니다.

## 책임과 확장 지점

| 담당 | 컴포넌트 | 동작 |
|---|---|---|
| 피격 경직 | `PlayerHitReaction` | 시간 기반 스턴, 중첩 시 더 긴 남은 시간 유지, 비활성화 시 초기화 |
| 체력·행동 제어 | `PlayerController` | 기존 방어·페이즈 계산 유지, `TakeDamage(damage, stunDuration)` 추가 |
| 입력 제한 | `PlayerCombat`, `PlayerSkillController` | 스턴 중 공격·스킬 차단, 진행 중 연타·이동 스킬 중단; 쿨타임은 유지 |
| 상호작용 선택 | `PlayerInteractor` | F키로 가장 가까운 사용 가능한 오브젝트 하나 선택 |
| 맵 오브젝트 | `WorldInteractable`, `InteractiveObject` | 활성화·비활성화·토글, 일회용 설정, 쿨타임, `OnInteracted` UnityEvent |
| 맵 이동 | `MapExit`, `MapTransitionManager` | 동일 상호작용 경로, Build Settings 확인, 중복 이동 방지, 체력·페이즈 유지 |
| 보스 체력·페이즈 | `BossController` | 중간 보스는 2페이즈, 최종 보스는 3페이즈; 시작·전환·처치 이벤트 |
| 보스 이동 | `BossAI` | 플레이어 자동 탐색, 중간 보스 추적, 최종 보스 물리 위치 고정 |
| 패턴 실행 | `BossPattern` | 예고·공격·후딜, 페이즈 전환/죽음 시 실행 중 패턴 중단 |
| 피해 판정 | `BossCombat`, `BossFieldAttack` | 예고 후 박스 판정, 필드 하나가 플레이어의 여러 콜라이더를 중복 타격하지 않음 |
| API 연결 | `LastSparkSession`, `BossResultReporter` | 사용자 ID 관리, 전투 시작부터 시간 측정, 플레이어 최고 페이즈 기록, 보스 처치 후 결과 저장 |

기존 `TakeDamage(float)`는 일반 피해로 유지합니다. 원거리 탄환은 기본 0.25초, Enemy3 강공격은 0.45초, 보스 강공격·필드 공격은 0.5초 경직을 줍니다. Inspector에서 조정할 수 있습니다. 피해가 0이거나 방어로 전부 막았거나 사망한 경우 스턴을 걸지 않습니다. 경직은 이동·점프·대시·공격·스킬·상호작용을 막지만 중력과 쿨타임은 계속 적용됩니다.

`PlayerController`는 기존 씬에서도 `PlayerHitReaction`과 `PlayerInteractor`를 자동으로 준비합니다. Player가 `IDamageable`을 구현하도록 수정하여 Enemy3의 기존 피해 경로도 연결했습니다.

`InteractiveObject`의 `Targets`에 문·다리·오브젝트를 연결하고 `Mode`를 설정합니다. 상자나 이벤트 트리거는 `Single Use`와 `On Interacted`에 보상/연출 함수를 연결해 구성합니다. 인벤토리 시스템은 새로 추가하지 않았습니다.

## 보스 패턴 기본안

| 보스 | 페이즈 조건 | 패턴 |
|---|---|---|
| 중간 보스 | HP 100~50% | 추적 후 범위를 예고하는 근접 공격 |
| 중간 보스 | HP 50% 이하 | 빨라진 추적, 2연속 예고 근접 공격 또는 범위 강공격·경직 |
| 최종 보스 | HP 100~70% | 플레이어 위치에 예고 필드 공격, 일부 레인 공격 |
| 최종 보스 | HP 70~30% | 넓어진 조준 필드, 안전 레인을 남기는 필드 공격 |
| 최종 보스 | HP 30% 이하 | 넓어진 필드 및 지연 후속 조준 공격, 짧아진 공격 간격 |

`MiniBoss.prefab`과 `FinalBoss.prefab`을 기존 맵에 배치할 수 있습니다. 최종 보스의 위치를 맵 중앙에 두고 `BossCombat.Arena Center`와 `Arena Size`를 실제 전장 크기에 맞추세요. 데모에서는 전장 중심 `(15, 4.5)`, 크기 `(24, 8)`로 연결했습니다. 외형·애니메이션은 Sprite 자식 교체와 이벤트 구독으로 확장할 수 있습니다.

## 백엔드·DB 연결

1. `database/README.md`대로 `DB_URL`, `DB_USERNAME`, `DB_PASSWORD`를 설정하고 백엔드를 실행합니다.
   로컬 HTTP 연결은 Editor 또는 Development Build에서 검사합니다. 일반 배포 빌드는 HTTPS API 주소가 필요합니다.
2. 데모 HUD에 새로운 사용자 이름을 입력하고 `Connect new username`을 누릅니다. 기존 사용자라면 `LastSparkSession.Existing User Id`를 Inspector에서 설정합니다. 기본 API 주소는 `http://localhost:8080/api/v1`입니다.
3. 보스를 처치하면 결과를 전송하고 HUD에 `Saved record ...`를 표시합니다. 중간 보스 ID는 1, 최종 보스 ID는 2입니다. 점수는 프리팹/씬의 `Clear Score`로 지정하며 데모 기본값은 1000/3000입니다.
4. 세션과 API 오브젝트는 맵 이동 중 유지됩니다. 연결 전 처치한 결과는 세션의 메모리 큐에 보관되므로 보스 오브젝트가 사라지거나 다른 맵으로 이동해도 연결 후 전송합니다. 게임 종료 시 메모리의 대기 기록은 사라집니다.

저장 실패는 상태와 로그로 표시합니다. 네트워크 응답 유실 시 중복 기록이 생길 수 있으므로 자동 재전송은 하지 않습니다. 오프라인 기록의 디스크 저장, 로그인·인증, 점수의 서버 검증은 현재 프로젝트 범위에 포함하지 않았습니다.

### 검증 결과 (2026-10-08)

- 기본 백엔드 테스트: **H2 7개 통과**. 실제 HTTP 사용자 생성 → 점수 저장 → DB 확인 → 랭킹/최고 기록 조회, 잘못된 요청·존재하지 않는 사용자·중복 이름·유한한 시간 검증·동점 정렬을 확인했습니다.
- `backend/gradlew.bat -p backend bootJar --console=plain`: 통과.
- 로컬 **MySQL 8.0.46**의 `last_spark` 테이블·컬럼·사용자 고유키·결과 외래키·랭킹 인덱스를 확인했습니다.
- 실제 MySQL 통합 테스트 **1개 통과**. `ddl-auto=validate`로 기동하고 임시 사용자 생성·보스 결과 저장·DB 직접 확인·최고 기록/랭킹 조회·중복 이름 거부를 검증했습니다. 테스트가 만든 행은 삭제했고 기존 기록은 수정하지 않았습니다. 비밀번호는 파일에 저장하지 않았습니다.
- Unity YAML 45개와 C# 소스 38개, 메타데이터·로컬 참조·Build Settings 등록을 정적으로 검사했습니다. C# 구문 검사는 Unity 타입 검사나 엔진 컴파일을 대신하지 않습니다. Editor에 포함된 렌더링 패키지 참조는 Unity에서 복원·확인해야 합니다.
- Unity PlayMode 회귀 테스트 **17개**는 `Assets/Carten/Tests/PlayMode/GameplayBehaviorTests.cs`에 있습니다. 이 PC에는 Unity Editor가 없어 엔진 컴파일·PlayMode 테스트·화면 검증을 실행하지 못했습니다. Unity에서 `Window > General > Test Runner > PlayMode > Run All`로 실행하세요.

검사 명령과 제출 전 확인 순서는 [프로젝트 점검 결과](../project-review.md)를 참고합니다.

Unity API와 테스트 어셈블리 구조는 [Unity 6 Rigidbody2D 문서](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/rigidbody2d/linearvelocity)와 [테스트 어셈블리 문서](https://docs.unity.com/en-us/engine/6000.5/manual/scripting/test-framework-introduction/getting-started/workflow-create-test-assembly)를 참고했습니다.
