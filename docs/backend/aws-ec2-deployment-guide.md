# EC2 한 대로 배포하기: Unity WebGL · Spring Boot · MySQL

Windows PC에서 Ubuntu EC2 한 대에 게임, 백엔드, 데이터베이스를 배포하는 안내입니다.
별도의 RDS나 다른 서버는 사용하지 않습니다. EC2의 디스크는 EBS 볼륨입니다.

> **대상: 공동 테스트 환경.** 현재 API에는 사용자 인증과 점수 위변조 방지가 없습니다.
> 실제 개인정보를 입력하거나 공개 서비스로 사용하기 전에 보안, 백업, 모니터링을 별도로 준비해야 합니다.

## 먼저 읽기

- 처음 배포한다면 **1~9번 순서**로 진행합니다. 기존 서버를 업데이트한다면 [10번](#10-다시-배포하기)부터 확인합니다.
- 각 명령 위에 적힌 **내 PC PowerShell**과 **EC2 터미널**을 구분합니다.
  - `PS C:\...>`가 보이면 내 PC입니다.
  - `ubuntu@...:~$`가 보이면 EC2입니다.
- **명령을 한 줄씩 복사하고 Enter를 누릅니다.** 앞의 `PS ...>`나 `ubuntu@...$`는 입력하지 않습니다.
- `[EC2 공인 IP]`처럼 대괄호로 표시한 값은 설명용입니다. 실제 값으로 바꾸고 **대괄호를 지웁니다**.
- `.pem` 키, `.env` 파일, DB 비밀번호는 공유하거나 Git에 올리지 않습니다.

## 구성과 주소

```text
내 브라우저
   │ HTTPS :443
   ▼
EC2 한 대
   └─ Docker Compose
      ├─ Caddy       /        → Unity WebGL 파일
      │              /api/*   → Spring Boot
      ├─ Spring Boot          → MySQL
      └─ MySQL                → Docker 볼륨에 데이터 저장
```

| 구분 | 주소 또는 포트 | 설명 |
|---|---|---|
| 게임 | `https://[도메인]/` | Unity 게임 화면 |
| 랭킹 API | `https://[도메인]/api/v1/ranks` | 게임 화면이 아닌 JSON 데이터 |
| SSH | 22 | 내 PC에서 서버 관리 |
| HTTP / HTTPS | 80 / 443 | 웹 접속 및 인증서 발급 |
| 백엔드 / DB | 8080 / 3306 | Docker 내부에서만 사용 |

EC2의 **퍼블릭 IPv4**는 인터넷에서 서버에 접속할 때 사용합니다.
`172.31...`처럼 표시되는 **프라이빗 IPv4**는 AWS 내부 주소입니다.
보안 그룹에서 SSH 소스로 선택하는 **내 IP**는 EC2가 아니라 내 PC의 공인 IP입니다.

## 1. EC2와 보안 그룹 준비

### AWS 콘솔

기존 EC2가 있다면 중복으로 만들기 전에 기존 인스턴스와 데이터를 먼저 확인합니다.
새로 만든다면 다음 설정을 기준으로 합니다.

1. **EC2 → 인스턴스 → 인스턴스 시작**을 선택합니다.
2. 리전은 예를 들어 **아시아 태평양(서울), ap-northeast-2**를 선택합니다.
3. AMI는 **Ubuntu Server 24.04 LTS, 64-bit x86**을 선택합니다.
4. 인스턴스 유형은 예를 들어 **t3.small (2 GiB)**을 선택합니다. Docker 이미지 빌드에는 메모리와 디스크 여유가 필요합니다.
5. 키 페어를 만들고 `.pem`을 PC에 보관합니다. 이 안내는 `Downloads\last-spark-test.pem`을 예로 사용합니다.
6. 퍼블릭 IP 자동 할당을 활성화하고 스토리지는 예를 들어 **30 GiB gp3**로 설정합니다.
7. 보안 그룹의 인바운드 규칙을 설정합니다.

| 유형 | 포트 | 소스 | 이유 |
|---|---:|---|---|
| SSH | 22 | **내 IP** | 내 PC에서만 관리 |
| HTTP | 80 | `0.0.0.0/0` | 인증서 발급과 HTTPS 전환 |
| HTTPS | 443 | `0.0.0.0/0` | 게임과 API 접속 |

SSH를 전체 인터넷에 공개하지 않습니다. **8080과 3306은 열지 않습니다.**
기본 아웃바운드 허용은 이미지 다운로드와 인증서 발급을 위해 유지합니다.
SSH 접속 PC의 공인 IP가 바뀌면 보안 그룹의 `내 IP`도 갱신해야 합니다.

인스턴스 시작 후 상태 검사를 확인하고 **퍼블릭 IPv4**를 기록합니다. 인스턴스,
EBS, 공인 IPv4 등의 비용과 Free Tier 또는 크레딧 적용 여부는 생성 전에 AWS Billing에서 확인합니다.
중지한 인스턴스도 스토리지 등의 비용이 발생할 수 있습니다.

## 2. EC2 접속

### 내 PC PowerShell

`[EC2 공인 IP]`를 실제 공인 IPv4로 바꾸고 실행합니다. 키 위치가 다르면 경로도 바꿉니다.

```powershell
ssh -i "$env:USERPROFILE\Downloads\last-spark-test.pem" ubuntu@[EC2 공인 IP]
```

첫 연결이라면 대상과 호스트 키를 확인한 후 안내에 따라 접속합니다.
성공하면 `ubuntu@...`가 표시됩니다. 아래 **EC2 터미널** 명령은 여기에서 실행합니다.
접속이 안 된다면 인스턴스 상태, 공인 IP, SSH 보안 그룹 및 키 경로를 확인합니다.

## 3. Docker 설치와 디스크 준비

### EC2 터미널 — 새 Ubuntu 서버에서 최초 1회

Docker가 이미 설치된 기존 서버에서는 설치를 반복하지 않습니다. 먼저 확인합니다.

```bash
sudo docker compose version
```

새 Ubuntu 24.04 x86 서버에 Docker가 없다면 다음을 실행합니다.

```bash
sudo apt-get update
sudo apt-get install -y ca-certificates curl git openssl unzip
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc
```

다음 블록은 여러 줄이 하나의 명령입니다. `EOF`까지 **전체를 복사**해 붙여 넣습니다.

```bash
sudo tee /etc/apt/sources.list.d/docker.sources > /dev/null <<'EOF'
Types: deb
URIs: https://download.docker.com/linux/ubuntu
Suites: noble
Components: stable
Architectures: amd64
Signed-By: /etc/apt/keyrings/docker.asc
EOF
```

이어서 한 줄씩 실행합니다.

```bash
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo systemctl enable --now docker
sudo docker compose version
```

백엔드 빌드 도구와 MySQL은 컨테이너에서 실행하므로 EC2에 따로 설치하지 않습니다.
현재 설치 방법은 [Docker 공식 Ubuntu 설치 문서](https://docs.docker.com/engine/install/ubuntu/)도 참고하세요.

### EC2 터미널 — 메모리와 디스크 확인

```bash
free -h
df -h /
```

2 GiB 인스턴스에서 빌드 메모리가 부족할 수 있습니다. 새 서버에 `/swapfile`이 없을 때만
다음 블록을 전체 실행해 2 GiB swap을 준비합니다. swap도 디스크 공간을 사용합니다.

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

디스크가 부족하다면 [11번 문제 해결](#11-문제-해결)의 EBS 확장 절차를 먼저 진행합니다.

## 4. 프로젝트 가져오기

### EC2 터미널

이미 `~/project-escape`에 저장소가 있다면 다시 clone하지 말고 그 폴더로 이동합니다.
새 서버라면 배포할 파일들이 GitHub의 사용할 브랜치에 push되어 있는지 확인한 후:

```bash
git clone --filter=blob:none --sparse https://github.com/dh-devv/project-escape.git
```

```bash
cd ~/project-escape
```

```bash
git sparse-checkout set backend database deployment docs/backend
```

비공개 저장소라면 GitHub 접근 인증이 필요합니다. EC2의 `.pem`을 GitHub 비밀번호로 사용하지 않습니다.

## 5. 도메인과 비밀번호 설정

### EC2 터미널

보유 도메인이 있으면 A 레코드를 EC2 공인 IPv4로 설정합니다. 도메인이 없다면
테스트용으로 `last-spark.[EC2 공인 IP].sslip.io`를 사용할 수 있습니다.
예를 들어 공인 IP가 `203.0.113.10`이면 주소 형식은 `last-spark.203.0.113.10.sslip.io`입니다.
이 IP는 **설명용 예시**이므로 실제 서버 주소로 사용하지 마세요.
sslip.io는 외부 서비스이므로 DNS 장애와 인증서 발급 제한의 영향을 받을 수 있습니다.

**새 DB를 만드는 경우에만** 다음 명령으로 비밀번호를 생성합니다. 기존 `deployment/.env`는 덮어쓰지 않습니다.

```bash
cd ~/project-escape
```

```bash
if [ -e deployment/.env ]; then
  echo '기존 설정을 유지합니다.'
else
  (umask 077; printf 'DB_USERNAME=last_spark_app\nDB_PASSWORD=%s\nMYSQL_ROOT_PASSWORD=%s\nSITE_ADDRESS=replace_with_your_hostname\n' "$(openssl rand -hex 24)" "$(openssl rand -hex 24)" > deployment/.env)
fi
```

파일을 열어 주소를 설정합니다.

```bash
nano deployment/.env
```

`SITE_ADDRESS` 줄의 값을 **실제 도메인**으로 바꿉니다. `https://`나 `/api/v1`은 넣지 않습니다.
DB가 이미 있다면 기존 사용자 이름과 비밀번호는 바꾸지 마세요. 설정 파일만 바꾸면 MySQL의 기존 계정 비밀번호가 변경되지 않습니다.

- 저장: `Ctrl+O` → `Enter`
- 종료: `Ctrl+X`

설정된 주소만 확인합니다. 출력에 설명용 IP나 `replace_with_your_hostname`이 남아 있으면 다시 수정합니다.

```bash
grep '^SITE_ADDRESS=' deployment/.env
```

기존 DB 볼륨이 있는데 `.env`가 없거나 DB 비밀번호를 모른다면 새 비밀번호를 임의로 만들지 말고
기존 설정을 복구하거나 DB 계정을 별도로 복구해야 합니다. 비밀번호를 채팅에 보내지 마세요.

## 6. Docker로 서버 시작

### EC2 터미널

AWS용 Compose 파일은 독립 구성입니다. 로컬용 `docker-compose.yml`과 합치지 않습니다.
먼저 설정을 검사합니다.

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml config --quiet
```

오류가 없다면 시작합니다. 첫 빌드는 다운로드와 DB 초기화로 시간이 걸립니다.

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml up -d --build
```

상태를 확인합니다.

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml ps -a
```

`db`는 `healthy`, `backend`와 `web`은 `Up`인지 확인합니다. `Up`은 API 응답까지 보장하지 않으므로
다음 단계도 진행합니다. 실패했다면 오류를 확인하고 다음 단계로 넘어가지 않습니다.

## 7. HTTPS와 API 확인

### 내 PC 브라우저

설정한 도메인으로 아래 주소를 엽니다.

```text
https://[도메인]/api/v1/ranks
```

`[]` 또는 랭킹 JSON이 나오면 HTTPS를 통한 백엔드 조회가 성공한 것입니다.
`[]`는 기록이 없다는 뜻이며 오류가 아닙니다. 게임은 이 주소가 아니라 `https://[도메인]/`에서 열립니다.
아직 WebGL 파일을 올리지 않았다면 게임 주소의 404는 정상일 수 있습니다.

인증서 발급에는 잠시 시간이 걸릴 수 있습니다. 오류가 계속되면 **EC2 터미널**에서 확인합니다.

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml logs --tail 40 web
```

`certificate obtained successfully`는 인증서 발급 성공입니다.
실패했다면 `SITE_ADDRESS`, 실제 공인 IP, DNS, 보안 그룹의 80/443 및 로그를 확인합니다.
인증서 경고를 무시하고 접속하지 않습니다.

## 8. Unity WebGL 게임 올리기

### Unity에서 빌드

1. Unity Editor에 Web Build Support를 설치하고 게임 씬을 포함해 Web/WebGL 빌드를 만듭니다.
2. `LastSparkApiClient`의 Inspector **Base Url**을 `https://[도메인]/api/v1`로 설정합니다.
   주소는 실제 도메인으로 바꿉니다. 코드를 수정해도 Inspector에 저장된 값이 우선될 수 있으므로 확인합니다.
3. 처음에는 Publishing Settings의 **Compression Format = Disabled**로 빌드합니다.
   현재 Caddy 설정은 사전 압축된 `.gz`/`.br` 빌드를 대상으로 하지 않습니다.
   멀티스레딩도 처음에는 끕니다. 이 구성에 필요한 브라우저 격리 헤더가 없습니다.
4. 빌드 결과의 `index.html`, `Build/`, `TemplateData/`, 생성된 경우 `StreamingAssets/`를 ZIP으로 압축합니다.

ZIP 안의 구조는 다음과 같아야 합니다. `WebBuild/index.html`처럼 상위 폴더가 한 겹 있으면 안 됩니다.

```text
index.html
Build/
TemplateData/
StreamingAssets/  (생성된 경우)
```

### 내 PC PowerShell — ZIP 업로드

ZIP 경로, 키 경로, 공인 IP를 실제 값으로 바꿉니다. 경로에 한글이나 공백이 있어도 따옴표로 감싸면 됩니다.

```powershell
scp -i "$env:USERPROFILE\Downloads\last-spark-test.pem" "C:\내 폴더\게임빌드.zip" ubuntu@[EC2 공인 IP]:~/LastSparkWeb.zip
```

`100%`로 완료되면 EC2의 `~/LastSparkWeb.zip`에 저장됩니다.
`stat local ... No such file or directory`가 나오면 **내 PC의 ZIP 경로**를 확인합니다.
EC2 주소와 SSH 키가 맞는지도 확인합니다.

### EC2 터미널 — ZIP 확인과 압축 해제

PowerShell이 아니라 `ubuntu@...`가 보이는 EC2에서 실행합니다.
EC2에 연결이 끊겼다면 2번의 SSH 명령으로 다시 접속합니다.

```bash
cd ~/project-escape
```

```bash
unzip -l ~/LastSparkWeb.zip
```

목록에서 `index.html`이 최상위에 있는지 확인합니다. 상위 폴더가 있다면 올바른 구조로 다시 압축합니다.
출처가 신뢰되는 빌드만 압축 해제합니다.

```bash
unzip -o ~/LastSparkWeb.zip -d deployment/web
```

```bash
test -f deployment/web/index.html && echo '게임 파일 확인 완료'
```

확인 메시지가 나오면 브라우저에서 `https://[도메인]/`을 엽니다.
해당 폴더는 Caddy에 연결되어 있으므로 Docker 재빌드는 필요하지 않습니다.
업로드 중에는 게임 테스트를 잠시 멈춥니다. 기존 파일과 이름이 같은 파일은 덮어쓰며,
새 빌드에 없는 이전 파일은 남을 수 있으므로 재배포 시 확인합니다.
파일 이름이 깨지거나 로드 오류가 생기면 ZIP 목록과 브라우저 오류를 확인합니다.

## 9. DB와 게임 연동 확인

### EC2 터미널 — DB 확인

프로젝트 폴더에서 다음 명령을 실행합니다. 컨테이너에 설정된 계정을 사용하며 비밀번호를 출력하지 않습니다.

```bash
cd ~/project-escape
```

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml exec db sh -c 'MYSQL_PWD="$MYSQL_PASSWORD" mysql -u "$MYSQL_USER" last_spark -e "SHOW TABLES;"'
```

`users`와 `game_results`가 보여야 합니다. 저장된 행 수는 다음으로 확인합니다.

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml exec db sh -c 'MYSQL_PWD="$MYSQL_PASSWORD" mysql -u "$MYSQL_USER" last_spark -e "SELECT COUNT(*) AS user_count FROM users; SELECT COUNT(*) AS result_count FROM game_results;"'
```

새 DB는 두 수가 0일 수 있습니다. 기존 PC나 다른 서버의 기록은 자동으로 이전되지 않습니다.
프로젝트 API는 보스 **클리어 결과**를 다루므로 클리어 전 사망만으로 결과 저장이 된다고 보장되지 않습니다.

### 게임과 API 연결 확인

1. 게임 시작, 사용자 생성, 보스 클리어를 실제로 진행합니다.
2. PC 브라우저에서 `F12` → **Network(네트워크)** 탭을 열고 게임을 다시 실행해
   `/api/v1/users`와 `/api/v1/scores` 요청의 주소와 상태 코드를 확인합니다.
3. 요청 주소가 `localhost`나 `http://`라면 Unity의 Base Url과 Inspector 설정을 확인합니다.
   변경 후에는 WebGL을 다시 빌드하고 업로드해야 합니다.
4. 요청이 없다면 실제 플레이 씬에 사용자 생성과 보스 결과 저장이 연결되어 있는지 확인합니다.
   자세한 구성은 [Unity API 연동](unity-api-integration.md)을 참고합니다.
5. 저장 성공 뒤 DB 행 수와 랭킹 API를 다시 확인합니다.

서버와 DB의 쓰기를 게임과 별도로 검사하려면 아래 명령을 사용할 수 있습니다.
**실제 DB에 테스트 사용자 1명이 남습니다.** 원할 때만 EC2 터미널에서 실행하세요.

```bash
curl -i -X POST 'https://[도메인]/api/v1/users' -H 'Content-Type: application/json' -d "{\"username\":\"test-$(date +%s)\"}"
```

`[도메인]`을 실제 도메인으로 바꿉니다. `200 OK`와 `userId`가 나오고 `user_count`가 증가하면
사용자 생성 API를 통한 DB 쓰기는 성공입니다. 이 검사는 Unity 연동이나 점수 저장의 성공을 의미하지는 않습니다.

## 10. 다시 배포하기

### 백엔드 변경

PC에서 변경 사항을 테스트하고 GitHub의 배포 브랜치에 push한 뒤 **EC2 터미널**에서:

```bash
cd ~/project-escape
```

```bash
git pull --ff-only
```

`git pull`이 성공한 경우에만:

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml up -d --build
```

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml ps -a
```

랭킹 API와 게임을 다시 확인합니다. 백엔드 교체 중에는 잠깐 연결이 끊길 수 있습니다.
`schema.sql`은 **새 DB 볼륨 초기화 때만** 자동 적용되므로 기존 DB의 스키마 변경은 별도 마이그레이션이 필요합니다.

### Unity 변경

Unity에서 다시 WebGL을 빌드하고 [8번](#8-unity-webgl-게임-올리기)의 업로드와 압축 해제를 반복합니다.
브라우저에 이전 버전이 보이면 강력 새로고침하거나 캐시를 확인합니다.

### 중지와 데이터 보존

컨테이너만 중지하려면:

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml down
```

위 명령은 볼륨을 유지합니다. 다시 시작하려면 6번의 `up -d --build`를 사용합니다.
**`down -v`와 `docker system prune --volumes`는 DB 볼륨 삭제로 이어질 수 있으므로 평소 사용하지 않습니다.**
Docker 볼륨은 백업이 아닙니다. 중요한 데이터는 별도로 백업하고 복원도 확인해야 합니다.

EC2를 중지 후 시작하면 공인 IP가 바뀔 수 있습니다. 바뀌면 DNS와 `SITE_ADDRESS`를 수정하고
다음 명령으로 웹 설정을 반영합니다.

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml up -d --no-deps web
```

Unity 빌드가 이전 절대 주소를 사용한다면 새 주소로 설정하고 다시 빌드해야 합니다.
고정 IP와 보유 도메인을 사용하기 전에는 비용을 확인합니다.

## 11. 문제 해결

### `no space left on device` / `WORKDIR` 단계 오류

Dockerfile의 `WORKDIR` 자체가 문제가 아니라 이미지 레이어를 저장할 공간이 부족할 수 있습니다.
EC2에서 확인합니다.

```bash
df -h /
df -i /
sudo docker system df
```

루트 디스크의 여유가 부족하면 **기존 EC2의 EBS 루트 볼륨을 확장**합니다.
새 EC2나 RDS를 만들 필요는 없습니다. 데이터가 중요하면 먼저 백업을 준비합니다.

1. AWS 콘솔 → EC2 인스턴스 → **스토리지** → 루트 볼륨 ID → **작업 → 볼륨 수정**에서 크기를 늘립니다.
   예를 들어 30 GiB로 설정합니다. 비용을 확인하고, 볼륨을 삭제하거나 새로 만드는 것이 아니라 기존 볼륨을 수정합니다.
2. 수정 상태가 `optimizing` 또는 `completed`가 된 뒤 EC2에서 장치와 파일시스템을 확인합니다.

   ```bash
   lsblk -o NAME,SIZE,FSTYPE,MOUNTPOINTS
   findmnt -no SOURCE,FSTYPE /
   ```

3. **`/`가 `/dev/nvme0n1p1`이고 `ext4`이며, 파티션이 아직 디스크보다 작을 때만** 다음 예시를 순서대로 실행합니다.

   ```bash
   sudo growpart /dev/nvme0n1 1
   ```

   ```bash
   sudo resize2fs /dev/nvme0n1p1
   ```

4. `df -h /`로 확장 결과를 확인하고 6번의 빌드를 다시 실행합니다.

장치 이름이나 파일시스템이 다르면 예시를 그대로 실행하지 마세요. XFS는 파일시스템 확장 명령이 다릅니다.
`growpart`가 없다면 `sudo apt-get install -y cloud-guest-utils`로 설치할 수 있습니다.
설치할 공간도 부족하다면 `sudo docker builder prune -af`로 **사용하지 않는 빌드 캐시**를 정리할 수 있습니다.
캐시는 재빌드 시 다시 다운로드될 수 있습니다.
상세 절차는 [AWS EBS 파일시스템 확장 문서](https://docs.aws.amazon.com/ebs/latest/userguide/recognize-expanded-volume-linux.html)를 참고합니다.

### 그 밖의 문제

| 증상 | 확인할 것 |
|---|---|
| `cd: too many arguments` | 여러 명령을 한 줄로 합치지 말고 각각 실행 |
| PowerShell에서 `unzip` 오류 | ZIP 업로드 후 SSH로 EC2에 접속해 실행 |
| 게임 주소 404 | `deployment/web/index.html` 존재 여부와 ZIP 최상위 구조 |
| 게임은 뜨지만 저장 안 됨 | Unity Base Url, 실제 씬의 저장 연결, Network 요청과 응답 |
| 502 | 백엔드 기동과 DB 준비 상태, backend 로그 |
| 인증서 오류 | 도메인 오타, DNS, 공인 IP, 80/443 보안 그룹, web 로그 |
| MySQL `Access denied` | 기존 DB 비밀번호와 `.env` 일치 여부; 볼륨을 삭제해 해결하지 않기 |
| 빌드 중 `exit 137` | `free -h`와 swap으로 메모리 부족 여부 확인 |

로그는 프로젝트 폴더의 **EC2 터미널**에서 확인합니다.

```bash
sudo docker compose --env-file deployment/.env -p last-spark -f deployment/docker-compose.aws.yml logs --tail 100 web backend db
```

로그를 공유할 때는 비밀번호나 개인정보가 없는지 확인합니다.
