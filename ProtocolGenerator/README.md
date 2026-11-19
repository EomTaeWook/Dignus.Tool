# ProtocolGenerator

[English overview](../README.md) | [한국어 개요](../README.ko.md) | [전체 사용 가이드](../docs/Usage.ko.md)

DLL의 프로토콜 enum과 공개 모델 클래스를 읽어 C# 핸들러를 생성하는 .NET 10 도구입니다. Excel 변환과 별도로 사용합니다.

## 입력 규칙

- ProtocolName과 이름이 같은 enum이 필요합니다.
- 각 enum 멤버와 이름이 같은 공개 클래스가 필요합니다.
- Max 멤버는 대소문자를 구분하지 않고 제외합니다.
- DLL과 종속 DLL, 출력 폴더를 미리 준비합니다.

## 설정

작업 디렉터리의 `config.json`을 소비 프로젝트에 맞게 작성합니다.

```json
{
  "UsingNamespace": ["using Example.Protocol;"],
  "HandlerFormat": {},
  "Protocols": [
    {
      "Namespace": "Example.Generated",
      "BodyType": "string",
      "ProtocolName": "ClientProtocol",
      "ReadAssemblyPath": ".\\Example.Protocol.dll",
      "GeneratedPaths": [".\\Generated"]
    }
  ]
}
```

| 설정 | 의미 |
| --- | --- |
| UsingNamespace | 생성 코드의 using 문 목록. 최상위에 배열로 지정 |
| HandlerFormat | 기본 생성 형식을 덮어쓸 항목 |
| Protocols | 생성할 프로토콜 설정 목록 |
| Namespace | 생성 코드 네임스페이스 |
| BodyType | 생성 형식에 전달할 본문 타입 표현 |
| ProtocolName | 대상 enum 이름 |
| ReadAssemblyPath | enum과 모델이 포함된 DLL |
| GeneratedPaths | 같은 핸들러를 출력할 폴더 목록 |

HandlerFormat은 HandleClassFormat, RegistCabllackFormat, DeserializePacketFormat, DeserializeBodyFunctionFormat을 지원합니다. RegistCabllackFormat은 현재 코드의 표기입니다. 기본 형식이 요구하는 타입과 함수는 소비 프로젝트에서 제공해야 합니다.

## 실행과 결과

설정 파일이 있는 작업 디렉터리에서 실행합니다.

```powershell
.\ProtocolGenerator.exe
```

Release는 작업 디렉터리의 `config.json`, Debug는 작업 디렉터리 기준 `../../../config.json`을 읽습니다. 상대 경로는 작업 디렉터리가 기준입니다.

각 출력 폴더에 `<ProtocolName>Handler.cs`를 씁니다. 실행 파일 폴더에는 기본 생성 형식인 `format.json`도 기록합니다. 오류 처리 후에도 완료 문구가 출력될 수 있으므로 오류 로그와 산출물을 확인합니다.

마지막에 키 입력을 기다립니다. `--no-pause`는 지원하지 않습니다. 개별 Protocols 항목의 UsingNamespace는 현재 생성 경로에서 사용하지 않으므로 최상위 값을 지정합니다.

[전체 사용법](../README.md)
