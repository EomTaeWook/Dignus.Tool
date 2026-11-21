# DllDeploy

[English overview](../README.md) | [한국어 개요](../README.ko.md) | [전체 사용 가이드](../docs/Usage.ko.md)

버전별 원본 폴더에서 지정한 DLL과 외부 파일을 대상 폴더로 복사하는 .NET 10 도구입니다. 빌드는 수행하지 않습니다.

## 설정

실행 파일 폴더의 `config.json`에 복사할 항목을 지정합니다. 저장소의 설정은 빈 목록입니다. 아래 경로는 예시입니다.

```json
{
  "VersionPath": {
    "net10": "..\\..\\Share\\DataContainer\\bin\\Release\\net10.0"
  },
  "DeployInfos": [
    {
      "Version": "net10",
      "DeployPath": "..\\..\\Client\\Assets\\Plugins",
      "IncludeDll": ["DataContainer.dll"],
      "ExternalFile": []
    }
  ]
}
```

| 설정 | 의미 |
| --- | --- |
| VersionPath | 버전 키와 원본 폴더의 대응 |
| DeployInfos | 복사 작업 목록 |
| Version | 사용할 VersionPath의 키 |
| DeployPath | 대상 폴더 |
| IncludeDll | 원본 폴더에서 가져올 파일명 목록 |
| ExternalFile | 추가로 복사할 파일 경로 목록 |

버전 키는 원본 폴더를 선택하는 이름입니다. 도구 자체의 대상 프레임워크를 바꾸는 설정은 아닙니다.

## 실행과 결과

원본 파일과 대상 폴더를 준비하고 실행 파일 폴더에서 실행합니다.

```powershell
.\DllDeploy.exe
```

Release는 실행 파일 폴더의 `config.json`, Debug는 실행 파일 폴더 기준 `../../../config.json`을 읽습니다. 설정 내부의 상대 경로는 작업 디렉터리가 기준입니다.

복사된 파일마다 원본과 대상 경로를 출력합니다. 마지막에 키 입력을 기다리며 `--no-pause`는 지원하지 않습니다.

## 주의 사항

- 원본 폴더 바로 아래만 검색합니다. 대상 폴더는 자동 생성하지 않습니다.
- 같은 이름의 대상 파일을 삭제한 뒤 복사합니다. 원본과 대상이 겹치지 않게 지정합니다.
- IncludeDll에 있어도 원본 폴더에 없는 파일은 복사되지 않습니다. 존재하지 않는 ExternalFile도 건너뜁니다.
- 실패 전에 일부 파일만 복사되었을 수 있습니다. 실제 복사 로그를 확인합니다.

[전체 사용법](../docs/Usage.ko.md)
