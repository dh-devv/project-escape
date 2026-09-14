# Unity API 연동

Unity API 통신 코드는 `unity/Assets/Carten/Script/Network/LastSparkApiClient.cs`에 있습니다.
기존 플레이어, 전투, 보스 로직은 수정하지 않습니다.

## Unity에서 준비할 것

1. Spring Boot 백엔드를 먼저 실행합니다.
2. 빈 GameObject를 만들고 `LastSparkApiClient` 컴포넌트를 추가합니다.
3. 로컬 PC에서 실행할 때 `Base Url`은 기본값인
   `http://localhost:8080/api/v1`을 사용합니다.
4. 실제 기기에서 실행할 때는 `localhost` 대신 백엔드 PC의 로컬 IP를 사용합니다.

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

## 게임 로직과 연결할 때 필요한 수정

API 클라이언트를 사용하는 GameObject를 씬에 배치하고,
게임 종료를 판단하는 기존 코드에서 `SaveScore`를 호출해야 합니다.
점수 계산 방식이나 보스 전투 로직 자체를 바꿀 필요는 없습니다.

서버 요청에 필요한 값은 다음과 같습니다.

- `userId`: 사용자 생성 응답의 `userId`
- `bossId`: 처치한 보스 번호
- `clearTime`: 초 단위 클리어 시간
- `score`: 최종 점수
- `maxPhase`: 1부터 3 사이의 최고 페이즈
