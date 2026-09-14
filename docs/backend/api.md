# LAST SPARK API

## API 개요

- 로컬 개발 Base URL: `http://localhost:8080`
- API Prefix: `/api/v1`
- Content-Type: `application/json`
- 인증: 현재 미적용

## 엔드포인트 요약

| Method | Endpoint | 설명 |
|---|---|---|
| `POST` | `/api/v1/users` | 플레이어 생성 |
| `POST` | `/api/v1/scores` | 게임 결과 저장 |
| `GET` | `/api/v1/ranks` | 랭킹 조회 |
| `GET` | `/api/v1/users/{userId}/best` | 개인 최고 기록 조회 |

## API 상세

### 1. 플레이어 생성

플레이어를 생성합니다.

#### Request

```http
POST /api/v1/users
Content-Type: application/json
```

```json
{
	"username": "player1"
}
```

#### Response: `200 OK`

```json
{
	"userId": 1,
	"username": "player1"
}
```

`username`이 없거나 빈 문자열이면 `400 Bad Request`를 반환합니다.
이미 사용 중인 사용자 이름도 `400 Bad Request`를 반환합니다.

### 2. 게임 결과 저장

플레이어의 보스 클리어 결과를 저장합니다.

#### Request

```http
POST /api/v1/scores
Content-Type: application/json
```

```json
{
	"userId": 1,
	"bossId": 1,
	"clearTime": 125.5,
	"score": 8500,
	"maxPhase": 3
}
```

| Field | Type | 설명 |
|---|---|---|
| `userId` | `int` | 결과를 기록한 플레이어 ID |
| `bossId` | `int` | 처치한 보스 ID |
| `clearTime` | `double` | 클리어 시간. 단위는 현재 코드에서 검증하거나 변환하지 않음 |
| `score` | `int` | 게임 점수 |
| `maxPhase` | `int` | 게임 중 도달한 최대 Phase |

#### Response: `200 OK`

```json
{
	"recordId": 1,
	"userId": 1,
	"bossId": 1,
	"clearTime": 125.5,
	"score": 8500,
	"maxPhase": 3
}
```

존재하지 않는 `userId`로 요청하면 `404 Not Found`를 반환합니다.

### 3. 랭킹 조회

점수가 높은 순서로 게임 결과를 조회합니다.

#### Request

```http
GET /api/v1/ranks?limit=10
```

`limit`을 생략하면 기본값은 `10`입니다. `limit`이 `1`보다 작으면 `10`으로 처리되고,
`100`보다 크면 최대 `100`으로 제한됩니다.

#### Response: `200 OK`

```json
[
	{
		"recordId": 1,
		"userId": 1,
		"bossId": 1,
		"clearTime": 125.5,
		"score": 8500,
		"maxPhase": 3
	}
]
```

정렬 기준은 다음과 같습니다.

1. `score` 내림차순
2. 점수가 같으면 `clearTime` 오름차순

### 4. 개인 최고 기록 조회

특정 플레이어의 최고 점수 기록을 조회합니다.

#### Request

```http
GET /api/v1/users/{userId}/best
```

#### Response: `200 OK`

```json
{
	"recordId": 1,
	"userId": 1,
	"bossId": 1,
	"clearTime": 125.5,
	"score": 8500,
	"maxPhase": 3
}
```

해당 플레이어의 게임 기록이 없으면 `404 Not Found`를 반환합니다.

## 공통 오류

오류 응답은 다음 형식을 사용합니다.

| 상황 | 현재 동작 |
|---|---|
| 존재하지 않는 사용자에게 결과 저장 | `404 Not Found` |
| 빈 사용자 이름, 중복 사용자 이름 또는 검증 실패 | `400 Bad Request` |
| 기록이 없는 플레이어의 최고 기록 조회 | `404 Not Found` |
| 잘못된 JSON 형식 | `400 Bad Request` |

오류 응답 예시:

```json
{
	"status": 400,
	"code": "VALIDATION_ERROR",
	"message": "score: score must not be negative"
}
```

## 현재 구현상의 제한사항

- 사용자와 게임 결과는 MySQL 데이터베이스에 저장됩니다.
- 서버를 재시작해도 DB가 유지되는 한 사용자와 게임 결과는 유지됩니다.
- DB 연결 정보는 `DB_URL`, `DB_USERNAME`, `DB_PASSWORD` 환경변수로 설정합니다.
- 인증 기능은 아직 없습니다. 사용자 이름은 중복으로 생성할 수 없습니다.
- 기본 요청 검증이 적용되어 음수 점수와 `1~3` 범위를 벗어난 `maxPhase`를 거부합니다.
- `clearTime`의 단위는 현재 확정되거나 강제되지 않았으므로 Unity 클라이언트와 단위를 먼저 합의해야 합니다.
- 랭킹 조회는 한 번에 최대 100개까지 반환합니다.
- Unity 통신 클라이언트와 호출 예시는 [unity-api-integration.md](unity-api-integration.md)를 참고합니다.

