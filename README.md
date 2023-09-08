# patcher
windows application updater

## 사용방법

1. 배포될 application pacakge(patcher 를 제외)를 하나의 압축파일(`.zip`)로 압축해둔다. (예 - `SamplePacakge.zip`)

2. 웹서버를 준비하고 위 package 파일에 접근(다운로드)할 수 있도록 설정한다. (예 - `https://host.sample/downlaod/SamplePackage.zip` )

3. 마찬가지로 웹에서 다운로드 가능한
[버전 정보파일](https://github.com/alkee/patcher/blob/748d5b32bdedcc03d1cd32eeefd5541b6a19af7d/Patcher/AppConfig.cs#L36)
(아래 참고)을 준비한다. (예 - `https://host.sample/downlaod/version.json`)

```json
{
	"Version": "1.0.0.1",
	"PackageUrl": "https://host.sample/downlaod/SamplePackage.zip",
	"ExecuteFilePath": "sample.exe",
        "Arguments": "sample args"
}
```

4. 실행파일과 같은이름으로 [설정 파일(AppConfig)](https://github.com/alkee/patcher/blob/748d5b32bdedcc03d1cd32eeefd5541b6a19af7d/Patcher/AppConfig.cs#L7)
을 준비 (예 - `patcher.config`)

```json
{
	"VersionJsonUrl": "https://host.sample/download/version.json",
	"Title": "Title of the patcher"
}
```

5. 패치 실행파일(`patcher.exe`)과 설정파일(`patcher.config`)을 같은 경로에 두고 실행한다.


## 추가 정보

### 동작 flow

```mermaid
flowchart TD
    PREPARE["patcher.config 에서 서버정보 확인"]
    CHECK_VERSION["version.json 확인\n(서버와 로컬 정보 비교)\n파일이 없으면 항상 오래된 버전이라고 판단"]
    IF_NEW{"최신버전"}
    RUN["version.json 정보 기반 실행"]
    DOWNLOAD[".zip 다운로드 및 압축해제"]
    UPDATE_VERSION["version.json 정보 업데이트"]

    subgraph "버전확인"
        PREPARE-->CHECK_VERSION
    end

    CHECK_VERSION --> IF_NEW
    IF_NEW -- "Yes" --> RUN
    IF_NEW -- "No" --> DOWNLOAD
    DOWNLOAD --> UPDATE_VERSION

    UPDATE_VERSION --> RUN
```

### patch 과정

```mermaid
sequenceDiagram
    participant Patcher
    participant Server

    note over Patcher: patcher.config 로드
    note over Patcher: local VersionInfo(version.json) 로드
    Patcher -->> Server: VersionInfo(version.json)요청
    note over Patcher: VersionInfo.Version 정보확인
    Patcher -->> Server: VersionInfo.PackageUrl 파일 다운로드
    note over Patcher: 압축해제
    note over Patcher: VersionInfo.ExecuteFilePath + <br> Arguments 실행
```
