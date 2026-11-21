# JsonToCSharp

[English overview](../README.md) | [한국어 개요](../README.ko.md) | [전체 사용 가이드](../docs/Usage.ko.md)

[ExcelToJson](../ExcelToJson/README.md)이 만든 필드 정의와 enum 참조 정보를 읽어 C# 템플릿·컨테이너·로더를 생성하는 .NET 10 도구입니다. 데이터 JSON 자체를 입력으로 사용하지 않습니다.

## 설정

실행 파일 폴더의 `config.json`을 채웁니다. 저장소의 설정은 프로젝트별 값이 비워져 있습니다. 아래 상대 경로는 배치 예시입니다.

```json
{
  "ReadRefPath": "..\\Exported",
  "GeneratedPath": "..\\..\\Share\\DataContainer\\Generated",
  "ProjectPath": "",
  "Namespace": "DataContainer",
  "CSharpTemplateFormatPath": ""
}
```

| 설정 | 의미 |
| --- | --- |
| ReadRefPath | ExcelToJson의 OutputRefPath. ClassDefine과 Ref가 있는 폴더 |
| GeneratedPath | 생성 C# 전용 폴더 |
| Namespace | 생성 네임스페이스 접두사. 예시는 DataContainer.Generated |
| ProjectPath | 기존 설정 항목이며 현재 생성 경로에서는 사용하지 않음 |
| CSharpTemplateFormatPath | 기본 형식을 사용하려면 빈 문자열 |

## 실행과 결과

ExcelToJson이 오류 없이 완료된 뒤 실행 파일 폴더에서 실행합니다.

```powershell
.\JsonToCSharp.exe --no-pause
```

Release는 작업 디렉터리의 `config.json`, Debug는 작업 디렉터리 기준 `../../../config.json`을 읽습니다. 설정 내부의 상대 경로도 작업 디렉터리가 기준입니다. `--no-pause`를 생략하면 마지막에 키 입력을 기다립니다.

| 생성 파일 | 역할 |
| --- | --- |
| `<테이블명>Template.cs` | 필드, 반복 목록, 필수 참조 속성 |
| BaseTemplate.cs | Id·Name과 기본 동작 |
| TemplateContainer.cs | Id·Name 조회와 데이터 보관 |
| TemplateLoader.cs | 전체 JSON 로드·참조 연결, ITemplateDeserializer |

소비 프로젝트는 `ITemplateDeserializer`를 구현하고 `TemplateLoader.Load(...)` 후 `TemplateLoader.MakeRefTemplate()`을 호출합니다. enum을 사용하면 문자열 enum 역직렬화도 처리해야 합니다. [생성물 사용 안내](../docs/Usage.ko.md#jsontocsharp-설정과-생성물)를 참고합니다.

## 주의 사항

- GeneratedPath에서 생성 대상 템플릿에 해당하지 않는 파일을 삭제합니다. 직접 작성한 파일을 넣지 않습니다.
- 예외 처리 뒤에도 완료 문구를 출력합니다. 완료 문구나 종료 코드만으로 성공을 판단하지 않습니다.
- 현재 CSharpTemplateFormatPath를 지정하면 해당 파일 대신 config.json을 다시 읽는 문제가 있습니다. [현재 구현의 주의 사항](../docs/Usage.ko.md#현재-구현의-주의-사항)을 확인합니다.

[전체 사용법](../docs/Usage.ko.md)
