# Dignus.Tool

English | [한국어](README.ko.md)

Tools for Excel conversion, C# generation, protocol handler generation, and DLL deployment.

## Requirements

.NET 10. ExcelToJson also needs Windows and Microsoft.ACE.OLEDB.12.0 with process-compatible bitness.

## Installation

Open: `Tools.sln`

## Quick start

Prepare config.json and output directories, then run ExcelToJson before JsonToCSharp. Follow the guide's working-directory and configuration-path rules.

```text
ExcelTable/*.xlsx
  -> ExcelToJson
  -> JSON + Exported definitions
  -> JsonToCSharp
  -> DataContainer/Generated/*.cs
```

## Usage

Excel is authoritative; do not hand-edit generated JSON or C#. ProtocolGenerator reads protocol DLLs. DllDeploy copies configured files without building them.

## Documentation

- [Workflow and configuration / Korean](docs/Usage.ko.md)
- [ExcelToJson / Korean](ExcelToJson/README.md)
- [JsonToCSharp / Korean](JsonToCSharp/README.md)
- [ProtocolGenerator / Korean](ProtocolGenerator/README.md)
- [DllDeploy / Korean](DllDeploy/README.md)
- [Shared contracts / Korean](Shared/README.md)

## License

[TBD] This repository has no root LICENSE or package license declaration in the inspected tool projects. Confirm the applicable license before redistribution.
