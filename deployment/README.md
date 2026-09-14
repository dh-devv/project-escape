# Deployment

Docker와 운영 배포에 필요한 파일을 모아 둔 폴더입니다.

- `docker-compose.yml`: MySQL과 Spring Boot 실행
- `Dockerfile`: 백엔드 Docker 이미지 생성
- `.env.example`: 배포 환경변수 예시

프로젝트 루트에서 실행합니다.

```powershell
Copy-Item deployment/.env.example deployment/.env
docker compose --env-file deployment/.env -f deployment/docker-compose.yml up --build
```

자세한 내용은 `../docs/backend/deployment.md`를 참고합니다.
