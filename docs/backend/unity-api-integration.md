# Unity API 연동

Unity API 통신 코드는 `unity/Assets/Carten/Script/Network/LastSparkApiClient.cs`에 있습니다.
전투 결과 연동은 별도 `LastSparkSession`과 `BossResultReporter` 컴포넌트가 담당합니다.
보스 처치 이벤트로 저장하는 데모 씬과 설정은
[`gameplay-implementation.md`](../game/gameplay-implementation.md)를 참고합니다.

## Unity에서 준비할 것

1. Spring Boot 백엔드를 먼저 실행합니다.
2. 빈 GameObject를 만들고 `LastSparkApiClient` 컴포넌트를 추가합니다.
3. 로컬 PC에서 실행할 때 `Base Url`은 기본값인
   `http://localhost:8080/api/v1`을 사용합니다.
4. 실제 기기에서 실행할 때는 `localhost` 대신 백엔드 PC의 로컬 IP를 사용합니다.
5. 현재 `Allow downloads over HTTP`는 `Allowed in Development Builds`입니다.
   로컬 HTTP 서버를 검사할 때는 Editor 또는 Development Build를 사용하고,
   일반 배포 빌드는 `Base Url`을 실제 HTTPS 서버 주소로 바꿉니다.

## 호출 예시

```csharp
using UnityEngine;

namespace Carten
{
    public class ApiExample : MonoBehaviour
    {
        [SerializeField] private LastSparkApiClient apiClient;

        private void Start()
        {
            apiClient.CreateUser(
                "player1",
                response => Debug.Log("Created user: " + response.userId),
                error => Debug.LogError(error)
            );
        }

        public void SaveGameResult(
            int userId,
            int bossId,
            double clearTime,
            int score,
            int maxPhase)
        {
            apiClient.SaveScore(
                userId,
                bossId,
                clearTime,
                score,
                maxPhase,
                response => Debug.Log("Saved record: " + response.recordId),
                error => Debug.LogError(error)
            );
        }

        public void LoadRanking()
        {
            apiClient.GetRanking(
                10,
                responses => Debug.Log("Ranking count: " + responses.Length),
                error => Debug.LogError(error)
            );
        }
    }
}
```

## 기존 맵에 결과 저장 연결하기

씬의 루트 GameObject 하나에 `LastSparkSession`을 추가합니다. 필요한 `LastSparkApiClient`도 함께 추가되며
세션은 맵 이동 중 유지됩니다. 보스에는 `BossResultReporter`를 추가하고 보스 ID·클리어 점수를 설정합니다.
데모 씬에는 이 연결이 이미 되어 있습니다. 다른 종료 조건은 위 예시처럼 `SaveScore`를 직접 호출할 수 있습니다.

사용자 생성 오류와 저장 실패는 콜백으로 전달됩니다. 연결 전 보스 처치 기록은 세션이 메모리에 보관합니다.
응답을 잃었을 때 서버에 이미 저장되었을 수 있어 자동 재전송은 하지 않습니다.

서버 요청에 필요한 값은 다음과 같습니다.

- `userId`: 사용자 생성 응답의 `userId`
- `bossId`: 처치한 보스 번호
- `clearTime`: 초 단위 클리어 시간
- `score`: 최종 점수
- `maxPhase`: 1부터 3 사이의 최고 페이즈
