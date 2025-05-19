# ph-server

## 개요

ph-server는 .NET 8 기반의 RESTful API 서버로, JWT 인증, MySQL, Redis, Serilog, Swagger 등을 활용하여 게임 서버 백엔드 기능을 제공합니다.

## 주요 기술 스택

- **.NET 8 (ASP.NET Core Web API)**
- **Entity Framework Core** (MySQL 연동)
- **Redis** (캐시 및 세션)
- **Serilog** (로깅)
- **Swagger** (API 문서화)
- **Docker, docker-compose** (컨테이너 및 개발환경)

## 폴더 구조

```
ph-server/
├── Controllers/   # API 엔드포인트 (Item, Stage, Admin 등)
├── Services/      # 비즈니스 로직 (ItemService, StageService, AccountService 등)
├── DataContext/   # DB 컨텍스트 및 리포지토리
├── Entity/        # 엔티티 및 DTO
├── Managers/      # 게임 데이터 및 기타 매니저
├── Modules/       # DI 모듈
├── Protocols/     # 프로토콜 및 DTO
├── Migrations/    # DB 마이그레이션
├── logs/          # 로그 파일
├── appsettings.*.json # 환경설정 파일
├── Dockerfile     # 도커 빌드 파일
├── docker-compose.yml # 도커 컴포즈
└── ...
```

## 환경설정

- `appsettings.Development.json`, `appsettings.Production.json`, `appsettings.Local.json`에서 DB, Redis, JWT 등 환경별 설정을 관리합니다.
- 기본 포트: **8080**
- MySQL, Redis 정보는 docker-compose에 정의되어 있습니다.

## 빌드 및 실행 방법

### 1. 패키지 복원 및 빌드

```powershell
cd ph-server
# 패키지 복원
 dotnet restore
# 빌드
 dotnet build
```

### 2. 데이터베이스 마이그레이션

```powershell
# 마이그레이션 추가
Add-Migration [name]
# DB 적용
Update-Database
```

자세한 명령어는 `Migrations/readme.md` 참고

### 3. 실행

```powershell
# 개발 환경 실행
 dotnet run
```

### 4. Docker로 실행

```powershell
# 도커 이미지 빌드 및 실행
 docker-compose up --build
```

- 서버: http://localhost:8080
- MySQL: 3306, Redis: 6379

## 주요 API 예시

- JWT 인증 필요 (로그인 후 토큰 필요)

### 아이템 관련

- POST `/api/item/acquire` : 아이템 획득
- POST `/api/item/list` : 아이템 목록 조회
- POST `/api/item/unequip` : 아이템 해제

### 스테이지 관련

- POST `/api/stage/enter` : 스테이지 입장
- POST `/api/stage/clear` : 스테이지 클리어

### 관리자

- PUT `/api/admin/table/upload` : 테이블 데이터 업로드

## 로그 및 환경 변수

- 로그: `logs/` 폴더에 일별로 저장
- 환경 변수 예시 (PowerShell):

```powershell
$Env:ASPNETCORE_ENVIRONMENT = "Local"
```
