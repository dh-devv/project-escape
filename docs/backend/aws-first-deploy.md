# 처음 하는 AWS 배포: 백엔드·MySQL·Unity WebGL

이 안내는 Ubuntu 24.04 x86_64 EC2 한 대에서 Docker Compose로 실행하는 **공동 테스트 환경**을 기준으로 합니다.
본인은 백엔드·DB·웹서버를 준비하고, 친구는 Unity WebGL 빌드를 제작합니다.
게임과 API를 같은 HTTPS 주소에서 제공하므로 별도의 CORS 설정은 필요하지 않습니다.
친구가 다른 호스팅 사이트에 게임을 올린다면 이 전제가 달라지므로 해당 사이트의 origin을 허용하는 설정이 추가로 필요합니다.

```text
브라우저 → HTTPS 443 → Caddy ┬→ /api/* → Java 백엔드 → MySQL
                            └→ 나머지 주소 → 친구의 WebGL 파일
```

현재 점수 API에는 사용자 인증과 점수 위변조 방지가 없습니다. 실명·실제 개인정보 대신 테스트 닉네임을 사용합니다.
공모전 공개 서비스 수준의 계정 보안·부정 점수 방지는 별도 작업입니다.

## 1. AWS에서 서버 만들기 — 브라우저

1. AWS 콘솔에 로그인합니다. Billing에서 Free plan, 남은 크레딧, 종료일을 확인합니다.
2. 우측 상단 리전을 **아시아 태평양(서울), ap-northeast-2**로 정합니다.
3. 검색창에서 **EC2 → 인스턴스 → 인스턴스 시작**으로 이동합니다.
4. 이름은 `last-spark-test`로 입력합니다.
5. AMI는 **Ubuntu Server 24.04 LTS, 64-bit (x86)**를 선택합니다. 유료 Marketplace 이미지는 사용하지 않습니다.
6. 인스턴스 유형은 **t3.small (2 GiB)**을 선택합니다. 현재 신규 Free plan 적격 목록에 있지만 계정 화면의 적격 여부를 다시 확인합니다. 작은 서버에서 Java 빌드까지 하므로 아래 swap 설정을 함께 사용합니다.
7. **새 키 페어 생성 → last-spark-test → RSA → .pem**으로 다운로드합니다. 파일은 프로젝트 밖의 Downloads에 보관하며 공유하거나 Git에 넣지 않습니다.
8. 네트워크는 기본 VPC/퍼블릭 서브넷, **퍼블릭 IP 자동 할당 활성화**를 선택합니다.
9. 새 보안 그룹 `last-spark-test-sg`에 다음 인바운드 규칙을 만듭니다.

| 유형 | 포트 | 소스 | 목적 |
|---|---:|---|---|
| SSH | 22 | 내 IP | 본인 PC에서 관리 |
| HTTP | 80 | 0.0.0.0/0 | 인증서 발급·HTTPS 전환 |
| HTTPS | 443 | 0.0.0.0/0 | 게임·API 접속 |

**3306과 8080은 열지 않습니다.** 기존 채팅의 HTTP 8080 안내 대신 이 HTTPS 구성을 사용합니다.
아웃바운드는 기본 허용을 유지합니다. 다운로드와 인증서 발급에 필요합니다.

10. 스토리지는 **20 GiB gp3**로 설정합니다. 저장 공간·공인 IPv4 등도 크레딧 사용에 포함될 수 있습니다.
11. 인스턴스 수 **1**을 확인하고 시작합니다. Paid plan 전환을 요구하면 진행하지 말고 계정 조건을 확인합니다.
12. 상태 검사가 모두 통과하면 인스턴스 세부 정보의 **퍼블릭 IPv4 주소**를 기록합니다.

## 2. 서버 접속 — 본인 PC PowerShell

아래 `실제공인IPv4`를 방금 확인한 주소로 바꿉니다.

```powershell
ssh -i "$env:USERPROFILE\Downloads\last-spark-test.pem" ubuntu@실제공인IPv4
```

처음 접속할 때 대상 IP가 맞는지 확인하고 호스트 키 확인에 `yes`를 입력합니다.
`ubuntu@...:~$` 같은 표시가 나오면 AWS 서버에 들어온 것입니다.
이후 `exit`를 입력하기 전까지 명령은 본인 PC가 아닌 AWS에서 실행됩니다.
접속이 안 되면 서버 실행 상태, 현재 공인 IP, SSH 소스가 현재 내 IP인지 먼저 확인합니다.

## 3. Docker 설치 — AWS 터미널, 최초 1회

새 Ubuntu 24.04 서버를 기준으로 합니다. 기존 Docker 설치가 있는 서버에는 설치 명령을 반복하지 않습니다.
AWS에는 Docker Desktop 대신 Docker Engine을 설치합니다.

```bash
sudo apt-get update
sudo apt-get install -y ca-certificates curl git openssl unzip
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc
sudo tee /etc/apt/sources.list.d/docker.sources > /dev/null <<'EOF'
Types: deb
URIs: https://download.docker.com/linux/ubuntu
Suites: noble
Components: stable
Architectures: amd64
Signed-By: /etc/apt/keyrings/docker.asc
EOF
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo systemctl enable --now docker
sudo docker compose version
```

버전이 표시되면 설치된 것입니다. Java·Gradle·MySQL은 Docker가 준비하므로 따로 설치하지 않습니다.

2 GiB 서버의 빌드 중 메모리 부족을 완화하기 위해 swap을 한 번 준비합니다.
이미 `/swapfile`이 있으면 아래 블록은 변경하지 않습니다.

```bash
if [ ! -e /swapfile ]; then
  sudo fallocate -l 2G /swapfile &&
  sudo chmod 600 /swapfile &&
  sudo mkswap /swapfile &&
  sudo swapon /swapfile &&
  printf '/swapfile none swap sw 0 0\n' | sudo tee -a /etc/fstab > /dev/null
fi
free -h
```

## 4. 프로젝트 가져오기 — AWS 터미널

먼저 본인 PC의 AWS용 파일들이 GitHub에 commit/push되어 있어야 합니다.
아래 파일을 포함합니다: `deployment/docker-compose.aws.yml`, `deployment/Caddyfile`, `deployment/web/.gitkeep`, `.env.example`과 관련 문서·ignore 변경.
`.env`, `.pem`, 테스트 임시 파일은 올리지 않습니다.

```bash
git clone --filter=blob:none --sparse https://github.com/dh-devv/project-escape.git
cd project-escape
git sparse-checkout set backend database deployment docs/backend
```

서버에 필요 없는 Unity 소스와 에셋은 받지 않는 방식입니다. 루트의 `.dockerignore` 등은 함께 받습니다.
저장소가 비공개라서 인증 오류가 나면 GitHub 접근 설정이 먼저 필요합니다. AWS 키를 GitHub 비밀번호로 사용하지 않습니다.

## 5. 접속 주소·비밀번호 만들기 — AWS 터미널

도메인이 없으면 테스트용 `last-spark.실제공인IPv4.sslip.io`를 사용할 수 있습니다.
예시의 IP를 그대로 쓰지 말고 EC2에 표시된 공인 IPv4를 넣습니다.
이미 보유한 도메인을 사용한다면 A 레코드를 EC2 공인 IPv4로 지정하고 그 도메인을 입력합니다.

```bash
SITE_ADDRESS='last-spark.실제공인IPv4.sslip.io'
if [ -e deployment/.env ]; then
  echo '기존 .env를 유지합니다. 주소 변경은 nano deployment/.env에서 하세요.'
else
  (
    umask 077
    printf 'DB_USERNAME=last_spark_app\nDB_PASSWORD=%s\nMYSQL_ROOT_PASSWORD=%s\nSITE_ADDRESS=%s\n' \
      "$(openssl rand -hex 24)" "$(openssl rand -hex 24)" "$SITE_ADDRESS" > deployment/.env
  )
fi
```

비밀번호 두 개는 자동 생성되어 서버의 `.env`에만 저장됩니다. 비밀번호를 채팅에 보내지 않습니다.
`SITE_ADDRESS`에는 `https://`나 `/api/v1`을 넣지 않습니다.
한 번 DB를 만든 후 `.env` 비밀번호만 바꾸면 기존 MySQL 비밀번호와 어긋납니다. 이후에는 임의로 재생성하지 않습니다.
이 설정은 신규 AWS DB를 만듭니다. 로컬 PC의 기존 데이터가 자동 이전되지는 않습니다.

sslip.io는 외부 무료 DNS 서비스입니다. 장애·DNS 차단·공유 인증서 발급 한도가 생기면 주소 발급이 지연될 수 있으며, 최종 공개 서비스에는 보유 도메인을 권장합니다.

## 6. 함께 배포 — AWS 터미널

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml config --quiet
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml up -d --build
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml ps
```

첫 명령이 실패하면 설정을 고친 후 다음 명령으로 넘어갑니다.
처음에는 다운로드·Java 빌드·DB 초기화·인증서 발급이 필요합니다.
DB는 `healthy`, backend와 web은 `Up` 상태여야 합니다. `Up`만으로 API 성공이 증명되지는 않습니다.

자신의 브라우저에서 아래 주소를 엽니다.

```text
https://last-spark.실제공인IPv4.sslip.io/api/v1/ranks
```

`[]` 또는 점수 목록이 나오면 **HTTPS → 웹서버 → 백엔드 → MySQL 조회**가 연결된 것입니다.
인증서 경고를 무시하고 진행하지 않습니다. 아래 로그에서 원인을 먼저 확인합니다.
게임 파일을 아직 올리지 않았을 때 사이트 첫 화면 `/`이 404인 것은 정상입니다.

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml logs --tail 100 web backend db
```

## 7. 친구에게 전달할 작업 — Unity

1. 프로젝트에 맞는 Unity Editor와 Web Build Support를 설치합니다. 저장소의 `unity/ProjectSettings/ProjectVersion.txt`를 확인합니다.
2. Build Profiles에서 Web/WebGL을 선택하고 실제 게임 씬을 포함합니다.
3. 씬·프리팹의 `LastSparkApiClient`에서 Base Url을 `https://서버주소/api/v1`로 설정합니다. 코드 기본값뿐 아니라 Inspector 값도 확인합니다.
4. 첫 테스트의 Publishing Settings에서 **Compression Format = Disabled**로 설정합니다. Caddy가 전송 시 gzip 압축을 처리합니다. 사전 압축 `.gz`/`.br` 빌드는 현재 설정의 지원 대상이 아닙니다.
5. 멀티스레딩 옵션을 사용한다면 첫 테스트에서는 끕니다. 스레드용 브라우저 격리 헤더는 이 기본 구성에 포함하지 않았습니다.
6. 빌드 후 `index.html`, `Build/`, `TemplateData/`, 그리고 생성된 경우 `StreamingAssets/`를 포함해 **출력 폴더 전체**를 ZIP으로 묶습니다.
7. ZIP 최상위에 `index.html`이 있어야 합니다. `WebBuild/index.html`처럼 상위 폴더로 한 겹 감싸지 않습니다.

Unity 빌드 도구는 친구의 PC에서 실행합니다. AWS에 Unity Editor를 설치하지 않습니다.
친구가 별도 웹 호스팅을 사용할 계획이라면 먼저 정확한 주소를 공유받아 CORS와 HTTPS 구성을 맞춰야 합니다.

## 8. 게임 파일 올리기 — 본인 PC → AWS

친구가 보낸 ZIP을 본인 PC Downloads의 `LastSparkWeb.zip`으로 저장했다고 가정합니다.
AWS 키를 친구에게 전달하지 않아도 본인이 업로드할 수 있습니다.
**새 로컬 PowerShell 창**에서 다음을 실행합니다.

```powershell
scp -i "$env:USERPROFILE\Downloads\last-spark-test.pem" "$env:USERPROFILE\Downloads\LastSparkWeb.zip" ubuntu@실제공인IPv4:~/LastSparkWeb.zip
```

이어서 **AWS SSH 터미널**에서 실행합니다.

```bash
cd ~/project-escape
unzip -l ~/LastSparkWeb.zip
unzip -o ~/LastSparkWeb.zip -d deployment/web
test -f deployment/web/index.html && echo 'WebGL index.html 확인 완료'
```

출처가 확인된 친구의 빌드만 압축 해제합니다. 새 빌드 업로드 중에는 게임 테스트를 잠시 멈춥니다.
아래 주소에서 게임을 실행합니다. `index.html`을 PC에서 더블클릭하는 방식으로 테스트하지 않습니다.

```text
https://last-spark.실제공인IPv4.sslip.io/
```

서버 재빌드는 필요 없습니다. 변경이 안 보이면 브라우저 강력 새로고침 또는 사이트 캐시 삭제 후 다시 엽니다.
실제 WebGL의 실행·그래픽·메모리 문제는 친구가 만든 빌드로 별도 검증해야 합니다.

## 9. 공동 테스트 순서

| 검사 | 성공 기준 |
|---|---|
| 랭킹 주소 직접 열기 | HTTPS 경고 없이 JSON 응답 |
| 게임 첫 화면 | 브라우저에서 정상 로드 |
| 새 테스트 닉네임 생성 | 새 사용자 ID 수신 |
| 게임 결과 저장 | 서버에서 기록 ID 수신 |
| 랭킹·최고 기록 조회 | 방금 저장한 값과 일치 |
| 백엔드 재시작 후 재조회 | 이전 기록 유지 |

브라우저 개발자 도구 Network에서 요청 주소가 localhost나 HTTP가 아닌지 확인합니다.
같은 닉네임 중복 등록은 현재 API에서 400을 반환하므로 테스트마다 다른 이름을 사용합니다.
점수 저장 응답을 받기 전에 사용자 생성이 완료되어 있어야 합니다.

## 10. 수정·재배포·종료

본인 PC에서 수정하고 테스트한 뒤 GitHub의 해당 브랜치에 commit/push합니다.
그다음 **AWS 터미널**에서 실행합니다. `git pull`이 실패하면 멈추고 원인을 해결합니다.

```bash
cd ~/project-escape
git pull --ff-only
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml up -d --build
```

배포 후 랭킹 주소를 다시 확인합니다. backend 교체 중에는 짧은 연결 중단이 있을 수 있습니다.
재빌드는 게임 빌드 파일과 DB 볼륨을 지우지 않습니다.
테이블 변경은 별도 SQL로 반영해야 하며 `schema.sql`은 빈 DB 볼륨 초기화 때만 자동 실행됩니다.

테스트를 마쳤다면 AWS 콘솔에서 **인스턴스 상태 → 인스턴스 중지**를 선택합니다.
**인스턴스 종료(삭제)**를 선택하면 루트 디스크와 DB가 사라질 수 있습니다.
중지 중에도 저장 공간 등 일부 자원은 크레딧을 사용할 수 있습니다.
다음에 다시 시작하면 공인 IP가 바뀔 수 있습니다. 그때는:

1. 새 IP로 SSH 접속합니다.
2. `nano deployment/.env`에서 SITE_ADDRESS만 바꿉니다. 저장은 Ctrl+O, Enter, 종료는 Ctrl+X입니다.
3. 같은 Compose 명령을 `up -d`로 실행해 웹서버에 새 주소를 반영합니다.
4. Unity에 절대 주소를 넣었다면 친구도 새 주소로 바꾸고 다시 빌드합니다.

자주 바뀌는 주소가 불편하면 고정 IP·보유 도메인을 검토하되 비용과 Free plan 허용 여부를 먼저 확인합니다.
`down -v`는 DB·인증서 볼륨을 지우므로 평소 종료에 사용하지 않습니다.
볼륨은 백업이 아닙니다. 제출용 데이터가 쌓이면 별도 백업이 필요합니다.

## 자주 만나는 문제

| 증상 | 확인할 것 |
|---|---|
| SSH 시간 초과 | 공인 IP, 실행 상태, SSH 내 IP 규칙 |
| Docker 명령 권한 거부 | 이 안내대로 `sudo docker ...` 사용 |
| 빌드가 killed / exit 137 | `free -h`, swap, 메모리 부족 여부 |
| web이 502 응답 | 백엔드 준비 시간·backend 로그·DB healthy 여부 |
| TLS/인증서 오류 | SITE_ADDRESS 오타, DNS, 80/443 보안 그룹, web 로그 |
| DB Access denied | 최초 생성 당시 비밀번호와 `.env` 불일치 여부; 볼륨 삭제로 해결하지 않기 |
| 랭킹은 되지만 게임 첫 화면 404 | `deployment/web/index.html` 경로 |
| `.gz`/`.br` 구문·압축 오류 | 친구가 Compression Format Disabled로 다시 빌드 |
| Mixed Content / CORS | 게임과 API가 같은 HTTPS 호스트인지, Inspector 주소 확인 |
| WebAssembly 로드·메모리 오류 | Unity 브라우저 콘솔과 빌드 설정 확인; API 성공과 별개 |

## 참고 문서

### 2026-10-15 로컬 검증 결과

- 기존 Dockerfile로 Linux amd64 이미지 빌드 성공.
- Gradle `test bootJar` 성공: 일반 테스트 7개 통과, 환경변수로 활성화하는 MySQL 전용 테스트 1개는 생략.
- 별도의 테스트 Compose 프로젝트에서 MySQL 8.0 초기화와 앱 계정 healthcheck 통과.
- Caddy를 통과한 사용자 생성, 점수 저장, 최고 기록·랭킹 조회, 중복 사용자 400 응답 확인.
- MySQL 직접 조회로 저장된 점수 확인, backend 재시작 후 같은 기록 조회 성공.
- 정적 파일 제공, `.wasm` MIME, API `Cache-Control: no-store`, placeholder 비노출 확인.
- 도메인용 Caddy 설정 검증 통과. 공개 TLS 인증서 발급과 AWS 외부 접속은 EC2 생성 후 확인해야 함.
- 실제 Unity WebGL 빌드는 아직 제공되지 않아 게임 실행 검증은 미실시.
- 기존 Dockerfile과 로컬 Compose 설정에는 수정이 필요한 빌드 오류가 재현되지 않음.

### 공식 안내

- [AWS EC2 시작](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/EC2_GetStarted.html)
- [AWS 신규 Free Tier 적격 인스턴스](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/ec2-free-tier-usage.html)
- [Docker Ubuntu 설치](https://docs.docker.com/engine/install/ubuntu/)
- [Caddy 자동 HTTPS](https://caddyserver.com/docs/automatic-https)
- [sslip.io / nip.io 안내와 인증서 제한](https://sslip.io/)
- [Unity Web 배포](https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-deploying.html)
