# WOL (Wake On LAN) 애플리케이션

WOL 애플리케이션은 Wake-on-LAN(WOL) 기능을 통해 네트워크 장치를 관리하고 제어하기 위한 Windows Presentation Foundation (WPF) 기반의 데스크톱 애플리케이션입니다. 이 애플리케이션을 통해 사용자는 장치를 추가, 수정, 삭제하고, 프로젝트별로 장치를 그룹화하며, 장치의 온라인/오프라인 상태를 모니터링하고, 원격으로 장치를 켜거나 끌 수 있습니다.

## 주요 기능

*   **장치 관리:** IP 주소, MAC 주소 등 장치 정보를 추가, 수정, 삭제할 수 있습니다.
*   **프로젝트 관리:** 여러 장치를 프로젝트 단위로 그룹화하여 효율적으로 관리할 수 있습니다.
*   **Wake-on-LAN (WOL):** 등록된 장치에 WOL 패킷을 전송하여 원격으로 장치를 켤 수 있습니다.
*   **장치 상태 모니터링:** 하트비트(Heartbeat) 신호를 통해 장치의 온라인/오프라인 상태를 실시간으로 모니터링합니다. (하트비트 송신 간격 + 5초 동안 신호가 없으면 오프라인으로 간주)
*   **원격 종료:** 등록된 장치에 종료 명령을 전송하여 원격으로 장치를 끌 수 있습니다.
*   **설정 관리:** INI 파일을 통해 포트 설정 등 애플리케이션 관련 설정을 관리합니다.

## 기술 스택

*   **프론트엔드:** C#, WPF (.NET)
*   **아키텍처:** MVVM (Model-View-ViewModel) 패턴
*   **데이터베이스:** MySQL (Entity Framework Core를 통한 데이터 접근)
*   **네트워킹:** UDP (Wake-on-LAN 및 하트비트 통신)

## 프로젝트 구조

```
WOL/
├───App.xaml
├───App.xaml.cs             # 애플리케이션 시작/종료 로직 및 의존성 주입 설정
├───MainWindow.xaml
├───MainWindow.xaml.cs
├───README.md               # 이 파일
├───WOL.csproj
├───Commands/               # UI 명령 구현 (RelayCommand)
├───Converters/             # 데이터 바인딩을 위한 값 변환기
├───Data/                   # 데이터베이스 관련 로직 (DbContext, Repository 패턴)
│   ├───AppDbContext.cs     # Entity Framework Core DbContext
│   └───Repositories/       # 데이터 접근 리포지토리 (DeviceRepository, ProjectRepository 등)
├───Models/                 # 애플리케이션 데이터 모델 (Device, Program, Project)
├───Services/               # 비즈니스 로직 및 외부 서비스 연동 (WakeOnLanService, DeviceService 등)
│   ├───DeviceService.cs    # 장치 상태 관리 및 오프라인 감지 로직
│   └───WakeOnLanService.cs # WOL 패킷 전송 및 하트비트 수신 처리
├───Styles/                 # WPF UI 스타일 및 템플릿
├───View/                   # WPF 사용자 인터페이스 (XAML 파일)
└───ViewModels/             # View와 Data를 연결하는 ViewModel (MainViewModel, DeviceViewModel 등)
WOLClient/              # (별도 프로젝트) 장치에서 실행되는 하트비트 송신 클라이언트
```

## 시작하기

### 1. 데이터베이스 설정

*   MySQL 서버를 설치하고 `wol` 데이터베이스를 생성합니다.
*   `App.xaml.cs` 파일에서 `connectionString`을 사용자 환경에 맞게 수정합니다.
    ```csharp
    var connectionString = "Server=localhost;Database=wol;Uid=root;Pwd=str123;";
    ```

### 2. WOLClient 설정

*   `WOLClient` 프로젝트는 모니터링 대상 장치에서 실행되어야 합니다.
*   `WOLClient/Services/IniService.cs` 또는 관련 설정 파일을 통해 서버 IP 주소와 포트 설정을 확인합니다.
    *   `HEARTBEAT_INTERVAL_MS` (기본 1000ms = 1초)는 하트비트 송신 간격을 나타냅니다.

### 3. 애플리케이션 실행

*   Visual Studio에서 `WOL.sln` 솔루션을 엽니다.
*   솔루션을 빌드합니다.
*   `WOL` 프로젝트를 시작 프로젝트로 설정하고 실행합니다.
