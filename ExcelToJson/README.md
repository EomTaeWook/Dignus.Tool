# ExcelToJson

[English overview](../README.md) | [한국어 개요](../README.ko.md) | [전체 사용 가이드](../docs/Usage.ko.md)

Excel의 `Define`·`Data` 시트를 읽어 데이터 JSON, C# 생성용 필드 정의, enum 참조 정보를 출력하는 .NET 10 도구입니다.

## 준비

- Windows와 `Microsoft.ACE.OLEDB.12.0` 공급자가 필요합니다. 실행 프로세스와 공급자의 비트 수를 맞춥니다.
- 입력 폴더 바로 아래에 `.xlsx`를 둡니다. 하위 폴더와 `~$` 임시 파일은 읽지 않습니다.
- enum을 사용하면 정의가 포함된 DLL과 종속 DLL을 준비합니다.
- [Excel 작성 규칙](../README.md#excel-작성-규칙)에 따라 원본을 작성하고 저장합니다.

## 설정

실행 파일 폴더의 `config.json`을 채웁니다. 저장소의 설정은 프로젝트별 값이 비워져 있습니다. 아래 상대 경로는 배치 예시입니다.

```json
{
  "ReadAssemblyPath": "",
  "ReadPath": "..\\..\\ExcelTable",
  "OutputRefPath": "..\\Exported",
  "OutputTemplatePath": ["..\\..\\Datas"],
  "ExcelMergeDefinitions": []
}
```

| 설정 | 의미 |
| --- | --- |
| ReadAssemblyPath | enum 정의 DLL. enum 미사용 시 빈 문자열 |
| ReadPath | 원본 Excel 폴더 |
| OutputRefPath | 필드 정의와 enum 참조 출력 폴더 |
| OutputTemplatePath | 데이터 JSON 출력 폴더 목록 |
| ExcelMergeDefinitions | 병합할 테이블 목록. [병합 예시](../README.md#문자열-테이블-병합) 참고 |

## 실행과 결과

실행 파일 폴더에서 실행합니다.

```powershell
.\ExcelToJson.exe --no-pause
```

Release는 작업 디렉터리의 `config.json`, Debug는 작업 디렉터리 기준 `../../../config.json`을 읽습니다. 설정 내부의 상대 경로도 작업 디렉터리가 기준입니다. `--no-pause`를 생략하면 마지막에 키 입력을 기다립니다.

- 각 OutputTemplatePath에 `<테이블명>.json`을 출력합니다.
- OutputRefPath에 `ClassDefine/*.json`, `Ref/enumRef.json`을 출력합니다.
- 오류 로그와 산출물을 확인한 다음 [JsonToCSharp](../JsonToCSharp/README.md)를 실행합니다. 종료 코드만으로 성공을 판단하지 않습니다.

## 주의 사항

Ref 폴더의 기존 파일과 입력에서 사라진 ClassDefine 파일을 삭제합니다. 출력 폴더는 생성물 전용으로 사용합니다. 오래된 데이터 JSON이나 병합 전 개별 정의가 모두 자동 정리되지는 않습니다.

현재 소스에는 참조 필드 대소문자, 병합 전 참조 검사, 병합 Name 중복 검사, 빈 float·double 처리 제약이 있습니다. [현재 구현의 주의 사항](../README.md#현재-구현의-주의-사항)을 확인합니다.

[전체 사용법](../README.md)
