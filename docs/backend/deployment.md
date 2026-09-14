# LAST SPARK 백엔드 배포

## Docker로 로컬 실행

Docker Desktop을 설치한 뒤 프로젝트 루트에서 환경변수를 준비합니다.

```powershell
Copy-Item deployment/.env.example deployment/.env
```

`deployment/.env`의 세 비밀번호와 애플리케이션 DB 사용자 값을 실제 값으로 변경합니다.
`deployment/.env`는 비밀번호를 포함하므로 Git에 커밋하지 않습니다.

```powershell
docker compose --env-file deployment/.env -f deployment/docker-compose.yml up --build
```

처음 실행하면 MySQL 컨테이너가 `database/schema.sql`을 적용하고,
백엔드는 `http://localhost:8080`에서 실행됩니다. DB 데이터는
`mysql_data` Docker 볼륨에 저장되므로 컨테이너를 재시작해도 유지됩니다.

```powershell
docker compose --env-file deployment/.env -f deployment/docker-compose.yml down
```

데이터까지 삭제해야 하는 개발 초기화 작업에서만 다음 명령을 사용합니다.

```powershell
docker compose --env-file deployment/.env -f deployment/docker-compose.yml down -v
```

## AWS EC2 배포 개요

1. Ubuntu EC2 인스턴스에 Docker와 Docker Compose를 설치합니다.
2. 저장소를 EC2에 clone합니다.
3. `deployment/.env.example`을 `deployment/.env`로 복사하고 운영용 비밀번호를 설정합니다.
4. `docker compose --env-file deployment/.env -f deployment/docker-compose.yml up -d --build`로 백엔드와 MySQL을 시작합니다.
5. 보안 그룹에서 필요한 포트만 허용합니다. 개발 중에는 `8080`을 사용할 수 있지만,
   운영에서는 reverse proxy와 HTTPS를 사용하는 구성이 권장됩니다.

운영 환경에서는 MySQL root 계정을 백엔드에 사용하지 않고,
`deployment/.env`를 소스 저장소에 올리지 않아야 합니다. 백업과 모니터링도 별도로 구성해야 합니다.
