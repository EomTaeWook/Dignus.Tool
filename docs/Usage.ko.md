# Dignus.Tool

[English overview](../README.md) | [한국어 개요](../README.ko.md)

Excel 데이터 변환, C# 코드 생성, 프로토콜 핸들러 생성, DLL 복사를 위한 .NET 도구 모음입니다.

| 프로젝트 | 기능 | 대상 프레임워크 |
| --- | --- | --- |
| [ExcelToJson](../ExcelToJson/README.md) | Excel에서 데이터 JSON과 필드·enum 정의 생성 | .NET 10 |
| [JsonToCSharp](../JsonToCSharp/README.md) | 정의 파일에서 C# 템플릿과 로더 생성 | .NET 10 |
| [ProtocolGenerator](../ProtocolGenerator/README.md) | DLL의 enum과 모델에서 프로토콜 핸들러 생성 | .NET 10 |
| [DllDeploy](../DllDeploy/README.md) | 지정한 DLL과 외부 파일을 대상 폴더로 복사 | .NET 10 |
| [Shared](../Shared/README.md) | ExcelToJson·JsonToCSharp 공통 타입과 생성 형식 | .NET 10 |

## 데이터 변환 흐름

```text
ExcelTable/*.xlsx → ExcelToJson ─┬─ 각 출력 경로/*.json
                               └─ Exported/
                                  ├─ ClassDefine/*.json
                                  └─ Ref/enumRef.json
                                        ↓
                                   JsonToCSharp
                                        ↓
                               DataContainer/Generated/*.cs
```

Excel을 원본으로 관리합니다. 필드나 값을 변경할 때는 Excel과 설정을 수정하고 두 도구를 순서대로 실행합니다. 생성 JSON과 C#을 직접 편집하면 다음 생성 시 덮어써집니다.

이 문서의 경로와 데이터는 사용법을 설명하기 위한 예시입니다. 실제 입력·출력 위치에 맞게 설정합니다. 현재 소스에서 확인한 제약은 [현재 구현의 주의 사항](#현재-구현의-주의-사항)에 기록했습니다.

## 실행 준비와 폴더 배치

- 해당 도구를 실행할 수 있는 .NET 환경을 준비합니다. 자체 포함 배포 여부에 따라 런타임 설치 필요 여부가 달라집니다.
- ExcelToJson은 Windows의 `Microsoft.ACE.OLEDB.12.0` 공급자를 사용합니다. 실행 프로세스와 공급자의 비트 수를 맞춥니다.
- enum을 사용하면 정의가 포함된 DLL과 로딩에 필요한 종속 DLL을 준비합니다.
- 변환 전에 Excel을 저장합니다. 변환기는 저장된 파일을 임시 파일로 복사해서 읽습니다.

폴더 배치 예시입니다.

```text
프로젝트/
├─ ExcelTable/                    # 원본 .xlsx
├─ Datas/                         # 데이터 JSON
├─ ExportTool/
│  ├─ ExcelToJson/
│  │  ├─ ExcelToJson.exe
│  │  └─ config.json
│  ├─ JsonToCSharp/
│  │  ├─ JsonToCSharp.exe
│  │  └─ config.json
│  └─ Exported/                   # 중간 정의·enum 참조 정보
└─ Share/DataContainer/Generated/ # 생성 C#
```

각 실행 파일에는 배포에 필요한 DLL 등도 함께 둡니다. 실제 입력·출력 위치는 `config.json`에서 지정합니다.

ExcelToJson·JsonToCSharp의 Release 실행은 **작업 디렉터리**의 `config.json`을 읽습니다. 상대 경로 역시 작업 디렉터리가 기준입니다. 실행 파일의 전체 경로만 지정해도 작업 디렉터리가 바뀌지는 않습니다. 각 실행 파일 폴더로 이동해서 실행합니다.

Debug 실행은 작업 디렉터리 기준 `../../../config.json`을 읽습니다. 두 도구 모두 설정 파일이 없으면 빈 설정을 생성하므로 경로를 채운 뒤 다시 실행합니다.

## Excel 작성 규칙

각 `.xlsx`에 `Define`, `Data` 시트를 둡니다. 파일 이름이 테이블 이름과 출력 JSON 이름이 됩니다. 입력 폴더 바로 아래의 `.xlsx`만 읽고, 하위 폴더와 `~$`로 시작하는 임시 파일은 제외합니다.

### 기본 테이블

`Constant.xlsx`를 작성하는 예시입니다.

`Define` 시트:

| Name | Count | Required | Type |
| --- | ---: | --- | --- |
| Id | 1 | TRUE | Int32 |
| Name | 1 | TRUE | String |
| Value | 1 | TRUE | Int32 |

`Data` 시트:

| Id | Name | Value |
| ---: | --- | ---: |
| 1000 | MaximumItemCount | 4 |

출력 JSON 구조:

```json
[
  { "Id": 1000, "Name": "MaximumItemCount", "Value": 4 }
]
```

- `Id`, `Name`이 필요합니다. 생성되는 기반 타입에 맞춰 `Id`는 `Int32`, `Name`은 `String`으로 정의합니다.
- `Id`를 첫 필드로 두고, 각 행의 `Id`와 `Name`을 고유하게 지정합니다. `Name`은 비울 수 없습니다.
- `Define`의 정의 순서와 `Data`의 열 순서를 맞춥니다. 불필요한 데이터 행은 셀 값만 비우지 말고 행을 삭제합니다.
- `Define`의 항목 이름은 아래 표의 대소문자를 사용합니다. `Type` 값은 `Int32`, `int32`처럼 대소문자를 달리 쓸 수 있습니다.
- `Required`와 `bool` 셀은 Excel 논리값 `TRUE` / `FALSE`로 작성합니다. 문자열로 입력한 `1` / `0`이 논리값으로 처리된다고 가정하지 않습니다.

### Define 항목

| 항목 | 의미 |
| --- | --- |
| Name | 필드명. enum이면 DLL에 있는 enum 타입명 |
| Count | 반복 개수. 1 이상이며 2 이상이면 JSON 배열·C# List 생성 |
| Required | 빈 값 검사 여부. enum은 FALSE여도 값 필요 |
| Type | Int32, Int64, String, bool, float, double, enum, class |
| Ref 또는 RefTemplate | 참조 대상 Excel 이름. 확장자 제외 |
| RefField 또는 RefTemplateField | 참조 대상 필드명 |
| Member | class 멤버명. 여러 멤버는 Member 열 반복 |
| ConditionField | 조건 판단에 사용할 현재 테이블 필드 |
| Condition | `is: 값, Ref: 테이블, RefField: 필드` 형식의 조건부 참조 |

선택 항목은 필요한 열만 추가합니다. `Member`는 `Type` 뒤에, `Condition`은 `ConditionField` 뒤에 둡니다. class의 멤버 정의는 class 정의 바로 다음에 Member에 지정한 순서대로 작성합니다.

### class 필드와 멤버: UnitStat

`Unit.xlsx`에서 `UnitStat`과 `Mp`를 각각 class로 정의하는 예시입니다. UnitStat은 HP·Def·MovementSpeed·Sight를, Mp는 InitialMp·MaxMp를 멤버로 가집니다. **Define에서는 클래스 선언 행의 Member 열에 멤버 이름을 가로로 나열하고, 바로 아래 행들에서 각 멤버의 타입을 정의합니다. Data에서는 위쪽 행에 클래스 이름, 아래쪽 행에 멤버 이름을 배치합니다.**

`Define` 시트:

| Name | Count | Required | Type | Member | Member | Member | Member |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| Id | 1 | TRUE | Int32 | | | | |
| Name | 1 | TRUE | String | | | | |
| UnitStat | 1 | TRUE | class | HP | Def | MovementSpeed | Sight |
| HP | 1 | TRUE | Int32 | | | | |
| Def | 1 | TRUE | Int32 | | | | |
| MovementSpeed | 1 | TRUE | Int32 | | | | |
| Sight | 1 | TRUE | Int32 | | | | |
| Mp | 1 | TRUE | class | InitialMp | MaxMp | | |
| InitialMp | 1 | TRUE | Int32 | | | | |
| MaxMp | 1 | TRUE | Int32 | | | | |

- 열 이름은 `Member`를 4번 반복합니다. `Member1` 등의 다른 이름을 사용하지 않습니다.
- UnitStat의 Count는 클래스의 반복 개수입니다. 멤버가 4개여도 단일 객체이면 1로 지정합니다.
- UnitStat 바로 다음에 Member에 지정한 순서대로 멤버 정의 행을 둡니다. 각 멤버의 타입과 Count는 해당 행에서 지정합니다.
- 멤버 정의 행에는 Member 값을 다시 쓰지 않습니다. UnitStat의 마지막 멤버인 Sight 다음에 Mp를 선언합니다. Mp는 UnitStat의 멤버가 아닌 별도의 class 필드입니다. Mp 행은 Member 열 2개만 채우고 나머지는 비웁니다.

`Data` 시트는 다음처럼 헤더를 두 행으로 작성합니다. 표의 왼쪽은 Excel 행 번호이고 A~H는 열 위치입니다. Mp의 데이터 값은 배치 설명용 예시입니다.

<table>
  <thead>
    <tr><th>행</th><th>A</th><th>B</th><th>C</th><th>D</th><th>E</th><th>F</th><th>G</th><th>H</th></tr>
  </thead>
  <tbody>
    <tr><th>1</th><td></td><td></td><td colspan="4" align="center">UnitStat</td><td colspan="2" align="center">Mp</td></tr>
    <tr><th>2</th><td>Id</td><td>Name</td><td>HP</td><td>Def</td><td>MovementSpeed</td><td>Sight</td><td>InitialMp</td><td>MaxMp</td></tr>
    <tr><th>3</th><td>1000</td><td>ExampleUnitA</td><td>100</td><td>0</td><td>400</td><td>4</td><td>0</td><td>100</td></tr>
    <tr><th>4</th><td>1001</td><td>ExampleUnitB</td><td>100</td><td>0</td><td>400</td><td>8</td><td>0</td><td>100</td></tr>
  </tbody>
</table>

- `C1:F1`을 병합해 `UnitStat`을 입력하고, `C2:F2`에 각 멤버 이름을 입력합니다. 클래스 이름은 범위의 왼쪽 첫 셀인 C1에서 읽습니다.
- `G1:H1`을 병합해 `Mp`를 입력하고, G2에 InitialMp, H2에 MaxMp를 입력합니다.
- A1·B1은 비워 두고 A2에 Id, B2에 Name을 둡니다. Id·Name을 1행부터 세로로 병합하지 않습니다.
- 데이터는 3행부터 입력합니다. UnitStat 값을 위한 별도 열을 추가하지 않고 C~F열의 값을 하나의 객체로 묶습니다.

출력되는 `Unit.json` 구조:

```json
[
  {
    "Id": 1000,
    "Name": "ExampleUnitA",
    "UnitStat": { "HP": 100, "Def": 0, "MovementSpeed": 400, "Sight": 4 },
    "Mp": { "InitialMp": 0, "MaxMp": 100 }
  },
  {
    "Id": 1001,
    "Name": "ExampleUnitB",
    "UnitStat": { "HP": 100, "Def": 0, "MovementSpeed": 400, "Sight": 8 },
    "Mp": { "InitialMp": 0, "MaxMp": 100 }
  }
]
```

JsonToCSharp의 기본 형식에서는 `UnitTemplate` 안에 `InnerUnitStat`·`InnerMp` 클래스와 각 타입의 `UnitStat`·`Mp` 속성이 생성됩니다. 각 멤버는 `int` 속성이 됩니다. 이 예시는 소스의 읽기 규칙을 기준으로 작성했으며, 이번 문서 변경에서 변환을 실행하지는 않았습니다.

### 반복 필드

`Position.xlsx`에서 `Y`를 7개 반복하는 예시입니다.

| Name | Count | Required | Type |
| --- | ---: | --- | --- |
| Id | 1 | TRUE | Int32 |
| Name | 1 | TRUE | String |
| X | 1 | TRUE | float |
| Y | 7 | TRUE | float |

| Id | Name | X | Y#1 | Y#2 | Y#3 | Y#4 | Y#5 | Y#6 | Y#7 |
| ---: | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 1000 | X0 | 0 | 0 | 1.2 | 2.4 | 3.6 | 4.8 | 6 | 7.2 |

열 이름의 `#` 이후는 필드명 비교에서 제외합니다. 연속된 7개 열이 JSON의 `Y` 배열과 C#의 `List<float>`가 됩니다. 문자열도 같은 방식으로 반복할 수 있습니다.

한 셀에 쉼표로 값을 나열하는 방식과 다릅니다. `Type=String`, `Count=1`인 필드에 `0,80,180,80`을 입력하면 문자열 그대로 출력합니다.

### 주석 열

Data의 열 이름이 `#`으로 시작하면 해당 열을 출력에서 제외합니다. Define에는 주석 필드를 추가하지 않습니다.

| Id | Name | # | MaxHealth |
| ---: | --- | --- | ---: |
| 1000 | ExampleItem | 편집용 설명 | 200 |

`#Status`는 주석 열이고 `Y#1`은 반복 필드 열입니다. 주석 열을 반복 필드의 요소 사이에 끼우지 않습니다.

### enum

DLL에 정의된 `DirectionType` enum을 사용하는 예시입니다.

| Name | Count | Required | Type |
| --- | ---: | --- | --- |
| DirectionType | 1 | TRUE | enum |

Data의 DirectionType 열에는 해당 enum에 정의된 `Left`, `Right` 같은 이름을 넣습니다. ExcelToJson의 `ReadAssemblyPath`에 enum이 들어 있는 DLL을 지정합니다.

JSON에는 enum 이름이 문자열로 출력됩니다. JsonToCSharp는 `enumRef.json`에서 타입의 네임스페이스를 읽습니다. enum을 추가·변경했다면 정의가 반영된 DLL을 준비한 뒤 변환합니다. 변환 도구가 DLL을 빌드하지는 않습니다.

### 다른 테이블 참조

`DisplayStringKey`가 `String.xlsx`의 `Name`을 참조하도록 정의하는 예시입니다.

| Name | Count | Required | Type | Ref | RefField |
| --- | ---: | --- | --- | --- | --- |
| DisplayStringKey | 1 | TRUE | String | String | Name |

대상 Excel도 같은 ReadPath에서 읽혀야 합니다. 도구는 대상 정의와 필드를 확인하고, Required가 TRUE이면 입력값의 존재도 검사합니다. FALSE이면 값의 존재 검사를 건너뜁니다.

JsonToCSharp는 필수 참조 필드에 원래 값과 `<필드명>Ref` 속성을 생성합니다. 기본 연결 코드는 정수 Id 또는 문자열 Name으로 조회하므로 임의의 RefField에 대한 C# 연결까지 지원된다고 가정하지 않습니다. 조건부 참조는 ExcelToJson에서 검사하지만 기본 C# 생성기에서 조건별 참조 속성을 생성하는 경로는 확인되지 않습니다.

현재 소스의 대소문자 처리와 병합 전 참조 검사 제약은 [주의 사항](#현재-구현의-주의-사항)을 확인합니다.

## ExcelToJson 설정

`ExportTool/ExcelToJson/config.json` 예시입니다.

```json
{
  "ReadAssemblyPath": ".\\DataContainer.dll",
  "ReadPath": "..\\..\\ExcelTable",
  "OutputRefPath": "..\\Exported",
  "OutputTemplatePath": [
    "..\\..\\Datas",
    "..\\..\\Client\\Assets\\StreamingAssets\\Datas"
  ],
  "ExcelMergeDefinitions": [
    {
      "Name": "String",
      "BaseDefine": "String",
      "Datas": ["String", "StringItems", "StringMessages"]
    }
  ]
}
```

| 설정 | 설명 |
| --- | --- |
| ReadAssemblyPath | enum을 읽을 DLL. enum을 쓰지 않으면 빈 문자열 |
| ReadPath | 원본 .xlsx 폴더 |
| OutputRefPath | ClassDefine과 Ref를 생성할 폴더 |
| OutputTemplatePath | 같은 데이터 JSON을 출력할 폴더 목록 |
| ExcelMergeDefinitions | 여러 테이블의 행을 합치는 설정. 미사용 시 빈 배열 또는 생략 |

### 문자열 테이블 병합

병합 파일들은 Id, Name, 언어 열 등 동일한 구조와 호환되는 타입을 사용합니다. 언어 열 이름도 `Korean`·`English`처럼 통일합니다. 도구가 언어 열 이름을 고정하지는 않습니다.

| 구성 예시 | BaseDefine | Datas |
| --- | --- | --- |
| 단일 문자열 테이블 | 병합 설정 없음 | String 단일 파일 |
| 여러 문자열 테이블 병합 | String | String, StringItems, StringMessages |

- Name은 병합 데이터 컨테이너 이름이며 출력 파일명을 정하지 않습니다.
- BaseDefine이 출력 정의와 파일 이름의 기준입니다. String이면 `String.json`, `ClassDefine/String.json`을 출력합니다.
- Datas는 합칠 원본 파일 목록입니다. 기준 파일의 행도 포함하려면 String을 목록에 넣습니다. 파일명은 확장자 없이 적습니다.
- Datas 순서대로 행을 합칩니다. 병합 입력의 개별 데이터 JSON·정의 파일은 새로 출력하지 않고 병합 결과를 출력합니다.
- 병합 전체에서 Id와 Name이 겹치지 않게 관리합니다. 파일 내부 중복 검사와 달리 현재 소스의 병합 Name 중복 검사에는 제약이 있습니다.

이전 개별 JSON이나 정의 파일은 출력 폴더에 남을 수 있습니다. 설정 변경 후 오래된 산출물이 로더에 섞이지 않았는지 확인합니다. Ref=String의 값 검사는 병합 결과가 아닌 원본 String.xlsx를 대상으로 합니다.

## JsonToCSharp 설정과 생성물

`ExportTool/JsonToCSharp/config.json` 예시입니다.

```json
{
  "ReadRefPath": "..\\Exported",
  "GeneratedPath": "..\\..\\Share\\DataContainer\\Generated",
  "ProjectPath": "..\\..\\Share\\DataContainer\\DataContainer.csproj",
  "Namespace": "DataContainer",
  "CSharpTemplateFormatPath": ""
}
```

| 설정 | 설명 |
| --- | --- |
| ReadRefPath | ExcelToJson의 OutputRefPath와 같은 폴더 |
| GeneratedPath | 생성 C# 전용 폴더 |
| Namespace | 생성 네임스페이스 접두사. DataContainer이면 DataContainer.Generated |
| ProjectPath | 기존 설정에 포함되지만 현재 생성 경로에서는 사용하지 않음 |
| CSharpTemplateFormatPath | 기본 형식 사용 시 빈 문자열. 현재 사용자 지정 파일 로딩 문제 있음 |

데이터 JSON 자체가 아닌 `ClassDefine/*.json`과 `Ref/enumRef.json`을 읽습니다.

| 생성 파일 | 역할 |
| --- | --- |
| `<테이블명>Template.cs` | 필드, 반복 목록, 필수 참조 속성 |
| BaseTemplate.cs | Id·Name, Invalid(), MakeRefTemplate(), Combine() |
| TemplateContainer.cs | Id·Name 조회와 데이터 보관 |
| TemplateLoader.cs | 전체 JSON 로드·참조 연결 진입점, ITemplateDeserializer |

소비 프로젝트는 `ITemplateDeserializer` 구현을 제공하고 `TemplateLoader.Load(...)` 후 `TemplateLoader.MakeRefTemplate()`을 호출해 참조를 연결합니다. enum을 사용하면 역직렬화 구현도 문자열 enum을 처리해야 합니다.

기본 컨테이너는 로드할 때 사전에 데이터를 추가하므로 같은 데이터를 반복 로드하면 중복 키가 발생할 수 있습니다. Find가 대상을 찾지 못하면 Id가 -1인 새 템플릿을 반환하며 Invalid()로 확인합니다.

## 실행 순서와 결과 확인

프로젝트 루트의 PowerShell에서 첫 번째 도구를 실행합니다.

```powershell
Push-Location .\ExportTool\ExcelToJson
try {
    .\ExcelToJson.exe --no-pause
}
finally {
    Pop-Location
}
```

오류 로그가 없고 데이터 JSON·정의·enum 정보가 생성되었는지 확인한 뒤 두 번째 도구를 실행합니다.

```powershell
Push-Location .\ExportTool\JsonToCSharp
try {
    .\JsonToCSharp.exe --no-pause
}
finally {
    Pop-Location
}
```

`--no-pause`는 마지막 키 입력 대기만 생략합니다. 두 도구 모두 예외를 내부에서 처리하므로 종료 코드만으로 성공을 판단하지 않습니다. 특히 JsonToCSharp는 오류 처리 후에도 완료 문구를 출력합니다.

산출물은 다음과 같습니다.

- 각 `OutputTemplatePath/<테이블명>.json`: 같은 데이터 JSON.
- `OutputRefPath/ClassDefine/<테이블명>.json`: C# 생성용 정의.
- `OutputRefPath/Ref/enumRef.json`: enum 값과 네임스페이스.
- `GeneratedPath/*.cs`: 템플릿·컨테이너·로더.

새 테이블은 JSON, 정의, 템플릿, 로더에 모두 반영되어야 합니다. 앞 단계가 실패하면 다음 도구를 실행하지 않습니다. 출력은 일괄 교체가 아니므로 실패 전에 일부 파일만 바뀌었을 수 있습니다.

### 출력 폴더 관리

- Ref 폴더의 기존 파일은 enum 정보 생성 전에 삭제합니다.
- ClassDefine은 입력에 없는 테이블 파일을 삭제합니다. 병합 입력처럼 Excel은 남아 있는 테이블의 오래된 정의는 남을 수 있습니다.
- GeneratedPath에서는 생성 대상 템플릿에 해당하지 않는 파일을 삭제하고 공통 생성 파일을 다시 씁니다. 직접 작성한 코드를 넣지 않습니다.
- 데이터 JSON 폴더의 오래된 테이블 파일을 자동으로 모두 정리하지는 않습니다.

## 자주 발생하는 오류

| 증상 | 확인할 내용 |
| --- | --- |
| ACE 공급자를 찾을 수 없음 | 공급자 설치 여부와 프로세스 비트 수 |
| DLL·타입 로딩 실패 | ReadAssemblyPath와 종속 DLL, 실행 환경 호환성 |
| Define·Data 시트를 찾을 수 없음 | 시트 이름과 파일 형식 |
| 빈 정의·헤더 오류 | Define 첫 행의 빈 열, Data 헤더의 빈 열, 불필요한 사용 영역 |
| 정의와 데이터 구조 불일치 | 필드 순서, Count에 해당하는 열 수, 주석 열 위치 |
| 필수 값·enum 오류 | 필수 필드의 빈 셀, enum 이름과 DLL 정의 |
| 중복 Id·Name | 파일 내부 및 병합 전체의 키 중복 |
| 참조 대상·값을 찾을 수 없음 | Ref·RefField, 입력 파일 포함 여부, 아래 구현 제약 |
| 예상과 다른 폴더에 출력 | 작업 디렉터리와 config.json의 상대 경로 |

## 현재 구현의 주의 사항

다음은 문서 정리 중 소스에서 확인한 사항입니다. 이 문서 작업에서 코드는 수정하지 않았으며 변환을 실행하지 않았습니다.

1. **참조 필드 대소문자:** ExcelRowData.GetFieldData는 정의 이름만 소문자로 바꾸고 조회 인자는 그대로 비교합니다. RefField=Name이나 대문자가 포함된 ConditionField는 값 조회가 일치하지 않을 수 있습니다. 반대로 RefField=name만으로 바꾸면 대상 정의 검사에서 Name과 일치하지 않습니다. 설정 예시만 바꿔 해결된다고 볼 수 없습니다.
2. **병합 전 참조 검사:** Ref=String은 병합 결과가 아닌 원본 String.xlsx에서 값을 찾습니다. 다른 문자열 파일에만 존재하는 값은 병합 목록에 포함했어도 검사에서 통과하지 못할 수 있습니다.
3. **병합 Name 중복 검사:** 병합 컨테이너에서 Name 중복 조회에 Name 대신 Id를 사용합니다. 병합 전체의 Name 유일성을 이 검사에만 맡기지 않습니다. 원본 파일 내부의 Name 중복 검사는 별도로 존재합니다.
4. **사용자 지정 C# 형식:** CSharpTemplateFormatPath가 비어 있지 않으면 지정 파일 대신 현재 config.json 경로를 다시 읽습니다. 기본 형식 사용 예시는 이 설정을 비워 둡니다.
5. **선택 실수형의 빈 값:** float·double의 빈 셀은 false로 치환한 뒤 숫자 파싱을 시도합니다. Required=FALSE여도 빈 실수형이 0으로 출력된다고 가정하지 않습니다.

## ProtocolGenerator

Excel 변환과 별도로 사용하는 도구입니다. DLL에서 ProtocolName과 이름이 같은 enum을 찾고, 각 멤버와 이름이 같은 공개 클래스를 연결해 `<ProtocolName>Handler.cs`를 생성합니다. Max 멤버는 대소문자를 구분하지 않고 제외합니다.

작업 디렉터리에 다음 형태의 config.json을 준비합니다. 경로·네임스페이스·타입은 소비 프로젝트에 맞게 바꿉니다.

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

출력 폴더를 미리 준비하고 해당 작업 디렉터리에서 `ProtocolGenerator.exe`를 실행합니다. Debug 설정 경로는 ../../../config.json입니다. 실행 파일 폴더에는 기본 생성 형식인 format.json도 기록됩니다.

HandlerFormat의 HandleClassFormat, RegistCabllackFormat, DeserializePacketFormat, DeserializeBodyFunctionFormat으로 기본 형식을 덮어쓸 수 있습니다. RegistCabllackFormat은 현재 코드의 표기입니다. 개별 Protocols 항목에도 UsingNamespace 속성이 있지만 현재 생성에는 최상위 UsingNamespace를 사용합니다. 기본 형식이 요구하는 타입과 함수는 소비 프로젝트에서 제공해야 합니다.

마지막에 키 입력을 기다리며 `--no-pause`는 지원하지 않습니다.

## DllDeploy

버전별 원본 폴더에서 IncludeDll에 지정한 파일과 ExternalFile의 존재하는 파일을 대상 폴더로 복사합니다. 빌드는 수행하지 않습니다.

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

Release는 **실행 파일 폴더**의 config.json, Debug는 실행 파일 폴더 기준 ../../../config.json을 읽습니다. 설정 내부의 상대 경로는 작업 디렉터리가 기준이므로 실행 파일 폴더에서 `DllDeploy.exe`를 실행합니다.

Version은 VersionPath의 키와 같아야 합니다. 원본 폴더 바로 아래만 검색하고 대상 폴더는 자동 생성하지 않습니다. 같은 이름의 대상 파일은 삭제 후 복사하므로 원본과 대상이 겹치지 않게 지정합니다.

IncludeDll 목록에 있어도 원본 폴더에 없는 파일과 존재하지 않는 ExternalFile은 복사되지 않습니다. 실제 복사 로그를 확인합니다. 마지막에 키 입력을 기다리며 `--no-pause`는 지원하지 않습니다.
