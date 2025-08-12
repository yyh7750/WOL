# WOL 원격 관리 시스템 (WPF)

Wake on LAN 및 프로그램 원격 실행을 관리하는 Windows 데스크톱 애플리케이션입니다.

## 기능

- **프로젝트 관리**: 여러 프로젝트를 생성하고 관리
- **디바이스 관리**: 각 프로젝트에 디바이스 추가/삭제
- **프로그램 관리**: 디바이스별 프로그램 추가/삭제 및 실행/중지
- **원격 제어**: 
  - 전체 디바이스 Wake on LAN
  - 전체 디바이스 종료
  - 모든 프로그램 실행/중지
  - 개별 프로그램 실행/중지

## 기술 스택

- **.NET 8.0**: 최신 .NET 프레임워크
- **WPF**: Windows Presentation Foundation
- **MVVM 패턴**: Model-View-ViewModel 아키텍처
- **XAML**: 사용자 인터페이스 정의
- **C#**: 백엔드 로직

## 프로젝트 구조

```
ProcessManager/
├── Models/                 # 데이터 모델
│   ├── Project.cs         # 프로젝트 모델
│   ├── Device.cs          # 디바이스 모델
│   └── Program.cs         # 프로그램 모델
├── ViewModels/            # 뷰모델
│   └── MainViewModel.cs   # 메인 뷰모델
├── Converters/            # 데이터 변환기
│   └── StatusColorConverter.cs
├── App.xaml              # 애플리케이션 리소스
├── App.xaml.cs           # 애플리케이션 진입점
├── MainWindow.xaml       # 메인 윈도우 UI
├── MainWindow.xaml.cs    # 메인 윈도우 코드 비하인드
└── ProcessManager.csproj # 프로젝트 파일
```

## 빌드 및 실행

### 요구사항
- .NET 8.0 SDK
- Visual Studio 2022 또는 Visual Studio Code

### 빌드 방법

1. 프로젝트 디렉토리로 이동:
```bash
cd ProcessManager
```

2. 프로젝트 빌드:
```bash
dotnet build
```

3. 애플리케이션 실행:
```bash
dotnet run
```

### Visual Studio에서 실행

1. `ProcessManager.csproj` 파일을 Visual Studio에서 열기
2. F5 키를 눌러 디버그 모드로 실행
3. Ctrl+F5 키를 눌러 릴리즈 모드로 실행

## 사용법

### 프로젝트 생성
1. 우측 상단의 "새 프로젝트 생성" 버튼 클릭
2. 프로젝트 정보 입력 후 생성

### 디바이스 추가
1. 프로젝트 선택
2. "디바이스 추가" 버튼 클릭
3. 디바이스 정보 입력 (이름, IP, MAC 주소)

### 프로그램 추가
1. 디바이스 선택
2. "프로그램 추가" 버튼 클릭
3. 프로그램 정보 입력 (이름, 실행 경로)

### 원격 제어
- **Wake on LAN**: 오프라인 디바이스를 온라인으로 전환
- **전체 종료**: 모든 디바이스를 오프라인으로 전환
- **프로그램 실행**: 온라인 디바이스의 모든 프로그램 실행
- **프로그램 중지**: 모든 프로그램 중지

## 샘플 데이터

애플리케이션은 다음과 같은 샘플 데이터로 시작됩니다:

### 개발 서버 그룹
- DEV-SERVER-01 (오프라인)
  - Visual Studio Code
  - Docker Desktop
  - IntelliJ IDEA
- DEV-SERVER-02 (온라인)
  - MySQL Workbench (실행 중)
  - Postman

### 게임 서버 클러스터
- GAME-SERVER-01 (온라인)
  - GameServer.exe (실행 중)
  - LogAnalyzer (실행 중)

### 미디어 작업 스튜디오
- MEDIA-WORKSTATION (오프라인)
  - Adobe Premiere Pro
  - After Effects
  - Blender

## 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다.

## 기여

버그 리포트나 기능 요청은 GitHub Issues를 통해 제출해주세요.
