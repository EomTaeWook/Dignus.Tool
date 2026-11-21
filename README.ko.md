# Dignus.Tool

[English](README.md) | 한국어

Excel 변환, C#·프로토콜 핸들러 생성과 DLL 복사를 위한 도구 모음.

## 요구 사항

.NET 10이 필요하다. ExcelToJson은 Windows와 실행 프로세스의 비트 수에 맞는 Microsoft.ACE.OLEDB.12.0 공급자도 필요하다.

## 설치

열기: `Tools.sln`

## 빠른 시작

config.json과 출력 폴더를 준비하고 ExcelToJson 다음 JsonToCSharp를 실행한다. 작업 디렉터리와 설정 경로는 가이드를 따른다.

```text
ExcelTable/*.xlsx
  -> ExcelToJson
  -> JSON + Exported definitions
  -> JsonToCSharp
  -> DataContainer/Generated/*.cs
```

## 사용법

Excel을 원본으로 관리하고 생성 JSON·C#은 직접 수정하지 않는다. ProtocolGenerator는 프로토콜 DLL을 읽고 DllDeploy는 빌드 없이 설정된 파일을 복사한다.

## 문서

- [전체 흐름과 설정](docs/Usage.ko.md)
- [ExcelToJson](ExcelToJson/README.md)
- [JsonToCSharp](JsonToCSharp/README.md)
- [ProtocolGenerator](ProtocolGenerator/README.md)
- [DllDeploy](DllDeploy/README.md)
- [공통 계약](Shared/README.md)

## 라이선스

[TBD] 이 저장소에는 루트 LICENSE가 없으며 확인한 도구 프로젝트에도 패키지 라이선스 선언이 없다. 재배포 전에 적용 라이선스를 확정해야 한다.
