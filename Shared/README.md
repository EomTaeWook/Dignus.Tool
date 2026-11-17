# Shared

[ExcelToJson](../ExcelToJson/README.md)과 [JsonToCSharp](../JsonToCSharp/README.md)이 프로젝트 참조로 사용하는 .NET 10 라이브러리입니다. 독립 실행 프로그램이나 config.json은 없습니다.

## 구성

| 파일·폴더 | 역할 |
| --- | --- |
| Define.cs, Condition.cs, Enums.cs | 필드 정의, 조건부 참조, 정의 항목·데이터 타입 |
| DataHelper.cs | 데이터 타입 문자열과 C# 타입 표현 변환 |
| EnumContainer.cs, AssemblyGenerate.cs | enum 값·네임스페이스와 참조 정보 모델 |
| JsonHelper.cs | JSON 직렬화·역직렬화 |
| CsharpTemplateFormat.cs | 기본 C# 생성 형식 |
| Interfaces/ICSharpTemplateFormat.cs | C# 생성 형식 계약 |
| Buffer/DataStream.cs | 정의와 셀 값을 순서대로 읽는 스트림 |
| Const.cs | 생성 파일·폴더 이름 등 공통 상수 |

ExcelToJson이 내보내는 정의·enum 참조 정보와 JsonToCSharp가 읽는 정보가 이 타입을 공유합니다. 생성 결과를 소비하는 프로젝트의 사용법은 [전체 README](../README.md#jsontocsharp-설정과-생성물)를 참고합니다.
