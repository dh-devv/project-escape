# LAST SPARK Database

## 데이터베이스 소개

이 폴더에는 LAST SPARK에서 사용하는 MySQL 데이터베이스 파일이 있습니다.
데이터베이스는 플레이어 정보와 게임 결과를 저장하는 곳입니다.

사용하는 데이터베이스는 다음과 같습니다.

- 사용 프로그램: MySQL 8.0 이상
- 데이터베이스 이름: `last_spark`
- 글자 설정: `utf8mb4`
- 테이블을 만드는 파일: `schema.sql`

## 처음 설정하는 방법

### 1. MySQL 설치하기

먼저 컴퓨터에 MySQL 8.0 이상을 설치해야 합니다.
설치할 때 설정한 비밀번호는 나중에 필요하므로 잊어버리지 않도록 합니다.

### 2. 데이터베이스 만들기

프로젝트의 가장 바깥쪽 폴더에서 아래 명령어를 실행합니다.

PowerShell에서는 `<` 입력 리다이렉션을 지원하지 않으므로 다음 명령어를 사용합니다.

```powershell
cmd /c "mysql -u root -p < database\schema.sql"
```

명령어를 실행하면 MySQL 비밀번호를 물어봅니다.
MySQL을 설치할 때 설정한 비밀번호를 입력하면 됩니다.

이 명령어는 `schema.sql` 파일을 읽어서 다음 작업을 합니다.

- `last_spark` 데이터베이스 만들기
- 플레이어를 저장하는 `users` 테이블 만들기
- 게임 결과를 저장하는 `game_results` 테이블 만들기
- 플레이어와 게임 결과를 연결하기

이미 데이터베이스나 테이블이 있어도 기존 데이터를 바로 지우지는 않습니다.

## 테이블 설명

### `users` 테이블

플레이어의 정보를 저장합니다.

| 컬럼 | 설명 |
|---|---|
| `user_id` | 플레이어의 고유 번호 |
| `username` | 플레이어 이름 |
| `created_at` | 플레이어가 만들어진 시간 |

플레이어 이름은 같은 이름으로 여러 번 만들 수 없습니다.

### `game_results` 테이블

플레이어가 보스를 클리어했을 때의 결과를 저장합니다.

| 컬럼 | 설명 |
|---|---|
| `record_id` | 게임 결과의 고유 번호 |
| `user_id` | 결과를 저장한 플레이어 번호 |
| `boss_id` | 처치한 보스 번호 |
| `clear_time` | 클리어하는 데 걸린 시간. 단위는 초입니다. |
| `score` | 게임 점수 |
| `max_phase` | 게임 중 도달한 가장 높은 페이즈 |
| `created_at` | 게임 결과가 저장된 시간 |

`user_id`는 `users` 테이블에 실제로 존재하는 플레이어만 사용할 수 있습니다.
그래서 존재하지 않는 플레이어의 게임 결과가 저장되는 것을 막을 수 있습니다.

## 백엔드 실행에 필요한 설정

Spring Boot 백엔드는 아래 환경변수로 MySQL 접속 정보를 읽습니다.

```text
DB_URL=jdbc:mysql://localhost:3306/last_spark?useSSL=false&serverTimezone=UTC&allowPublicKeyRetrieval=true
DB_USERNAME=root
DB_PASSWORD=MySQL_비밀번호
JPA_DDL_AUTO=validate
```

각 설정의 뜻은 다음과 같습니다.

- `DB_URL`: MySQL이 실행 중인 주소입니다.
- `DB_USERNAME`: MySQL 사용자 이름입니다.
- `DB_PASSWORD`: MySQL 비밀번호입니다.
- `JPA_DDL_AUTO`: 백엔드가 테이블 구조를 확인하는 방법입니다.

Windows PowerShell에서는 백엔드를 실행하기 전에 다음처럼 입력할 수 있습니다.

```powershell
$env:DB_URL="jdbc:mysql://localhost:3306/last_spark?useSSL=false&serverTimezone=UTC&allowPublicKeyRetrieval=true"
$env:DB_USERNAME="root"
$env:DB_PASSWORD="MySQL_비밀번호"
$env:JPA_DDL_AUTO="validate"
```

실제 비밀번호는 GitHub에 올리거나 코드에 직접 적으면 안 됩니다.

## `JPA_DDL_AUTO`에 대하여

현재 프로젝트는 기본값으로 `validate`를 사용합니다.
이 설정은 백엔드가 시작될 때 테이블 구조가 올바른지만 확인하고,
데이터베이스를 마음대로 바꾸지는 않습니다.

그래서 새로운 컬럼이나 테이블을 추가할 때는 먼저 SQL 파일을 수정하고,
데이터베이스에 직접 적용한 다음 백엔드를 실행해야 합니다.

개발 중에만 임시로 `update`를 사용할 수 있지만,
실제 서버나 AWS에서는 `validate`를 사용하는 것이 좋습니다.

## AWS에 배포할 때

AWS에 배포할 때도 데이터베이스 비밀번호를 소스 코드에 넣으면 안 됩니다.
AWS의 환경변수나 비밀 설정에 다음 값을 저장해야 합니다.

- `DB_URL`
- `DB_USERNAME`
- `DB_PASSWORD`
- `JPA_DDL_AUTO=validate`

처음 AWS 데이터베이스를 만들 때는 `schema.sql`을 한 번 실행하면 됩니다.
그 뒤에는 백엔드가 AWS의 MySQL 데이터베이스에 연결해서 데이터를 저장합니다.