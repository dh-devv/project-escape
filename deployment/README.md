# Deployment

Docker와 운영 배포에 필요한 파일을 모아 둔 폴더입니다.

- `docker-compose.yml`: MySQL과 Spring Boot 실행
- `Dockerfile`: 백엔드 Docker 이미지 생성
- `.env.example`: 배포 환경변수 예시

Docker Desktop을 실행한 뒤 프로젝트 루트에서 다음 명령 하나로 시작합니다.
로컬 테스트용 DB 계정은 기본으로 설정되어 있어 `.env` 파일을 만들 필요가 없습니다.

```text
docker compose -f .\deployment\docker-compose.yml up --build -d
```

종료할 때는 다음 명령을 실행합니다. DB 데이터는 유지됩니다.

```text
docker compose -f .\deployment\docker-compose.yml down
```

이 기본 계정은 로컬 테스트 전용입니다. 운영 배포에서는 `.env.example`을 복사해
강력한 비밀번호를 설정하고, `--env-file`로 지정해야 합니다.

자세한 내용은 `../docs/backend/deployment.md`를 참고합니다.

AWS에서 WebGL과 API를 같은 HTTPS 주소로 제공하려면
[`../docs/backend/aws-first-deploy.md`](../docs/backend/aws-first-deploy.md)를 따라갑니다.
`docker-compose.aws.yml`은 AWS 전용 독립 구성입니다. 기존 Compose 파일과 합치지 않습니다.
이 구성은 80/443만 공개하고 백엔드 8080과 DB 3306은 내부에서만 사용합니다.
친구가 제작한 WebGL 빌드 전체는 `deployment/web/`에 업로드합니다.
