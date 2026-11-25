using Dignus.Collections;
using Dignus.Log;
using ExcelToJson.Exceptions;
using ExcelToJson.Models;
using Shared;
using Shared.Buffer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ExcelToJson
{
    internal class ExcelReader
    {
        private const string DataSheetName = "Data";
        private const string DefineSheetName = "Define";

        private readonly DataContainer _dataContainer = new DataContainer();

        private readonly GeneratorConfiguration _generatorConfiguration;
        private readonly EnumContainer _enumContainer;

        private readonly UniqueSet<long> _remarkColumns = new UniqueSet<long>();
        private readonly UniqueSet<string> _mergeExcludedFiles = new UniqueSet<string>();

        public ExcelReader(GeneratorConfiguration generatorConfiguration, EnumContainer enumContainer)
        {
            _generatorConfiguration = generatorConfiguration;
            _enumContainer = enumContainer;

            foreach (var excelMergeDefinition in generatorConfiguration.ExcelMergeDefinitions)
            {
                foreach (var dataFilePath in excelMergeDefinition.Datas)
                {
                    var fileName = Path.GetFileNameWithoutExtension(dataFilePath);
                    _mergeExcludedFiles.Add(fileName);
                }
            }
        }

        public void Read(string excelPath)
        {
            var temporaryExcelPath = CopyToTemporaryExcelFile(excelPath);

            try
            {
                var fileName = Path.GetFileNameWithoutExtension(excelPath);
                var fullFileName = Path.GetFileName(excelPath);

                var defineTable = GetDataTableFromDataSheet(temporaryExcelPath, DefineSheetName);
                if (defineTable == null)
                {
                    throw new Exception($"not found define data sheet");
                }

                var dataTable = GetDataTableFromDataSheet(temporaryExcelPath, DataSheetName);
                if (dataTable == null)
                {
                    throw new Exception($"not found data data sheet");
                }

                _remarkColumns.Clear();

                var defines = GetDefineFromDataTable(defineTable, fullFileName);
                _dataContainer.AddDefine(fileName, defines);

                var excelData = MakeExcelDataFromDataTable(fileName, fullFileName, dataTable);
                _dataContainer.AddExcelData(fileName, excelData);
            }
            finally
            {
                if (File.Exists(temporaryExcelPath))
                {
                    File.Delete(temporaryExcelPath);
                }
            }
        }

        public void Generate()
        {
            foreach (var key in _dataContainer.DefineKeys())
            {
                var define = _dataContainer.GetDefine(key);
                var excelData = _dataContainer.GetExcelDataContainer(key);

                CheckRefDatas(define, key);

                if (_mergeExcludedFiles.Contains(key))
                {
                    continue;
                }

                ExportJsonFile(key, GenerateJsonObjects(excelData));
            }

            foreach (var excelMergeDefinition in _generatorConfiguration.ExcelMergeDefinitions)
            {
                var baseDefineKey = Path.GetFileNameWithoutExtension(excelMergeDefinition.BaseDefine);
                var baseDefine = _dataContainer.GetDefine(baseDefineKey);

                CheckRefDatas(baseDefine, baseDefineKey);

                var excelDataContainer = new ExcelDataContainer(excelMergeDefinition.Name, excelMergeDefinition.BaseDefine);

                foreach (var mergeDataKey in excelMergeDefinition.Datas)
                {
                    var defineKey = Path.GetFileNameWithoutExtension(mergeDataKey);
                    var mergeExcelDataContainer = _dataContainer.GetExcelDataContainer(defineKey);

                    if (mergeExcelDataContainer == null)
                    {
                        var error = $"not found merge excel data. mergeName:{excelMergeDefinition.Name}, mergeDataKey:{mergeDataKey}, defineKey:{defineKey}";
                        LogHelper.Error(error);
                        throw new Exception(error);
                    }
                    try
                    {
                        excelDataContainer.AddRange(mergeExcelDataContainer.GetRows());
                    }
                    catch (DuplicateDataException duplicateDataException)
                    {
                        LogHelper.Error($"duplicate value detected - field: {duplicateDataException.FieldName}, value: {duplicateDataException.DuplicateValue}, file: {duplicateDataException.FileName}, merge data key: {mergeDataKey}");
                        throw;
                    }
                }

                ExportJsonFile(baseDefineKey, GenerateJsonObjects(excelDataContainer));
            }

            GenerateDefineFile();
        }

        private List<JsonObject> GenerateJsonObjects(ExcelDataContainer excelDataContainer)
        {
            var jsonObjects = new List<JsonObject>();
            var rowIndex = 0;

            foreach (var row in excelDataContainer.GetRows())
            {
                var jsonObject = new JsonObject();

                rowIndex++;

                var colIndex = 0;
                foreach (var kv in row.GetDatas())
                {
                    colIndex++;

                    if (kv.Value.Count > 1)
                    {
                        var jsonArray = new JsonArray();

                        foreach (var item in kv.Value)
                        {
                            if (item.Value == DBNull.Value)
                            {
                                continue;
                            }

                            jsonArray.Add(ConvertCellDataToJsonNode(item, rowIndex, colIndex, excelDataContainer.FullFileName));
                            colIndex++;
                        }

                        jsonObject[kv.Key] = jsonArray;
                    }
                    else
                    {
                        var item = kv.Value[0];
                        var jsonNode = ConvertCellDataToJsonNode(item, rowIndex, colIndex, excelDataContainer.FullFileName);
                        jsonObject[item.DefineName] = jsonNode;
                    }
                }

                jsonObjects.Add(jsonObject);
            }

            return jsonObjects;
        }

        private JsonNode ConvertCellDataToJsonNode(CellData cellData, int rowIndex, int colIndex, string fileName)
        {
            object dataValue;

            if (cellData.DataType == DataType.Int32)
            {
                if (cellData.Value == DBNull.Value)
                {
                    cellData.Value = 0;
                }

                if (cellData.Value == null)
                {
                    throw new Exception($"{fileName} has invalid data at row {rowIndex}, column {colIndex}, field {cellData.DefineName}. expects a type : {cellData.DataType}, data is empty");
                }

                if (int.TryParse(cellData.Value.ToString(), out int intValue) == true)
                {
                    dataValue = intValue;
                }
                else
                {
                    throw new Exception($"{fileName} has invalid data at row {rowIndex}, column {colIndex}, field {cellData.DefineName}. expects a type : {cellData.DataType}, data {cellData.Value}");
                }
            }
            else if (cellData.DataType == DataType.Int64)
            {
                if (cellData.Value == DBNull.Value)
                {
                    cellData.Value = 0;
                }

                if (long.TryParse(cellData.Value.ToString(), out long longValue) == true)
                {
                    dataValue = longValue;
                }
                else
                {
                    throw new Exception($"{fileName} has invalid data at row {rowIndex}, column {colIndex}, field {cellData.DefineName}. expects a type : {cellData.DataType}, data {cellData.Value}");
                }
            }
            else if (cellData.DataType == DataType.String)
            {
                dataValue = cellData.Value ?? cellData.Value.ToString();
            }
            else if (cellData.DataType == DataType.Class)
            {
                var jsonObject = new JsonObject();

                if (cellData.GetMembers().Count == 0)
                {
                    throw new Exception($"{fileName} row {rowIndex}, column : {colIndex} - {cellData.DefineName} not found class member");
                }

                foreach (var kv in cellData.GetMembers())
                {
                    var memberCellDatas = kv.Value;

                    if (memberCellDatas.Count > 1)
                    {
                        var jsonArray = new JsonArray();

                        foreach (var memberCellData in memberCellDatas)
                        {
                            jsonArray.Add(ConvertCellDataToJsonNode(memberCellData, rowIndex, colIndex, fileName));
                        }

                        jsonObject.Add(kv.Key, jsonArray);
                    }
                    else
                    {
                        jsonObject.Add(kv.Key, ConvertCellDataToJsonNode(memberCellDatas[0], rowIndex, colIndex, fileName));
                    }
                }

                return jsonObject;
            }
            else if (cellData.DataType == DataType.Bool)
            {
                if (cellData.Value == DBNull.Value)
                {
                    cellData.Value = false;
                }
                if (bool.TryParse(cellData.Value.ToString(), out bool boolValue) == true)
                {
                    dataValue = boolValue;
                }
                else
                {
                    throw new Exception($"{fileName} has invalid data at row {rowIndex}, column {colIndex}, field {cellData.DefineName}. expects a type : {cellData.DataType}, data {cellData.Value}");
                }
            }
            else if (cellData.DataType == DataType.Float)
            {
                if (cellData.Value == DBNull.Value)
                {
                    cellData.Value = false;
                }
                if (float.TryParse(cellData.Value.ToString(), out float floatValue) == true)
                {
                    dataValue = floatValue;
                }
                else
                {
                    throw new Exception($"{fileName} has invalid data at row {rowIndex}, column {colIndex}, field {cellData.DefineName}. expects a type : {cellData.DataType}, data {cellData.Value}");
                }
            }
            else if (cellData.DataType == DataType.Double)
            {
                if (cellData.Value == DBNull.Value)
                {
                    cellData.Value = false;
                }
                if (double.TryParse(cellData.Value.ToString(), out double doubleValue) == true)
                {
                    dataValue = doubleValue;
                }
                else
                {
                    throw new Exception($"{fileName} has invalid data at row {rowIndex}, column {colIndex}, field {cellData.DefineName}. expects a type : {cellData.DataType}, data {cellData.Value}");
                }
            }
            else if (cellData.DataType == DataType.Enum)
            {
                if (cellData.Value == DBNull.Value || cellData.Value == null)
                {
                    throw new Exception($"{fileName} has invalid data at row {rowIndex}, column {colIndex}, field {cellData.DefineName}. expects a type : {cellData.DataType}, Value is null");
                }

                if (_enumContainer.Contains(cellData.DefineName, cellData.Value.ToString()) == false)
                {
                    throw new Exception($"{fileName} has invalid data at row {rowIndex}, column {colIndex}, field {cellData.DefineName}. expects a type : {cellData.DataType}, data {cellData.Value}");
                }

                dataValue = cellData.Value.ToString();
            }
            else
            {
                throw new Exception($"{fileName} has invalid data at row {rowIndex}, column {colIndex}, field {cellData.DefineName}. expects a type : {cellData.DataType}, data {cellData.Value}");
            }

            if (dataValue == DBNull.Value)
            {
                return null;
            }
            return JsonValue.Create(dataValue);
        }

        private void ExportJsonFile(string fileName, List<JsonObject> jsonDatas)
        {
            foreach (var generatedPath in _generatorConfiguration.GeneratedPath)
            {
                var path = Path.Combine(generatedPath);

                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                var json = JsonHelper.Serialize(jsonDatas);
                var filePath = Path.Combine(path, $"{fileName}.json");

                File.WriteAllText(filePath, json);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"{filePath} 파일 생성");
            }
        }

        private void ExportDefineFile(string outputPath, string fileName, List<Define> defines)
        {
            var json = JsonHelper.Serialize(defines);
            var filePath = Path.Combine(outputPath, $"{fileName}.json");

            File.WriteAllText(filePath, json);
        }

        private List<Define> GetDefineFromDataTable(DataTable dataTable, string fullFileName)
        {
            var defineDatas = new List<Define>();
            var defineColumns = new List<DefineColumnType>();

            for (int i = 0; i < dataTable.Rows[0].ItemArray.Length; ++i)
            {
                var value = dataTable.Rows[0].ItemArray[i].ToString();

                if (string.IsNullOrEmpty(value) == true)
                {

                    throw new Exception($"{fullFileName} data sheet has an empty definition for column index : {i}. please ensure all columns are defined.");
                }

                if (Enum.TryParse(value, out DefineColumnType defineColumnType) == false)
                {
                    throw new Exception($"{fullFileName} data sheet column index : {i} invalid! {value}");
                }

                defineColumns.Add(defineColumnType);
            }

            for (int i = 1; i < dataTable.Rows.Count; ++i)
            {
                var row = dataTable.Rows[i];
                var define = MakeDefineData(row, defineColumns, i, fullFileName);

                if (define != null)
                {
                    defineDatas.Add(define);
                }
            }

            var findId = false;
            var findName = false;

            foreach (var define in defineDatas)
            {
                if (define.DataType == DataType.Class)
                {
                    if (define.Members.Count == 0)
                    {
                        throw new Exception($"{fullFileName} data sheet [{define.Name}] class types need member");
                    }
                }
                else if (define.Name.ToLower().Equals("id") == true)
                {
                    findId = true;
                }
                else if (define.Name.ToLower().Equals("name") == true)
                {
                    findName = true;
                }
            }

            if (findId == false)
            {
                throw new Exception($"{fullFileName} data sheet need id");
            }

            if (findName == false)
            {
                throw new Exception($"{fullFileName} data sheet need name column");
            }

            return defineDatas;
        }

        private Define MakeDefineData(DataRow row, List<DefineColumnType> defineMembers, int rowIndex, string fullFileName)
        {
            var define = new Define();
            var index = 0;

            foreach (var defineMember in defineMembers)
            {
                var columnType = defineMember;
                var value = row.ItemArray[index++].ToString();

                if (columnType == DefineColumnType.Name)
                {
                    if (string.IsNullOrEmpty(value) == true)
                    {
                        return null;
                    }

                    define.Name = value;
                }
                else if (columnType == DefineColumnType.Count)
                {
                    define.Count = int.Parse(value);

                    if (define.Count <= 0)
                    {
                        throw new Exception($"{fullFileName} data sheet row : {rowIndex} [{define.Name}] must be greater than 0. {value}");
                    }
                }
                else if (columnType == DefineColumnType.Type)
                {
                    var dataType = DataHelper.GetDataTypeToString(value);

                    if (dataType == DataType.Max)
                    {
                        throw new Exception($"{fullFileName} data sheet row : {rowIndex} [{define.Name}] data type invalid! type : {value}");
                    }

                    define.DataType = dataType;
                }
                else if (columnType == DefineColumnType.Required)
                {
                    if (bool.TryParse(value, out bool result) == true)
                    {
                        define.Required = result;
                    }
                    else
                    {
                        throw new Exception($"{fullFileName} data sheet row : {rowIndex} [{define.Name}] data type invalid! type : {value}");
                    }
                }
                else if (columnType == DefineColumnType.Member)
                {
                    if (string.IsNullOrEmpty(value) == true)
                    {
                        continue;
                    }

                    if (define.DataType != DataType.Class)
                    {
                        throw new Exception($"{fullFileName} data sheet row : {rowIndex} [{define.Name}] type is not class! {define.DataType}");
                    }

                    define.Members.Add(value);
                }
                else if (columnType == DefineColumnType.RefTemplate)
                {
                    define.RefTemplate = value;
                }
                else if (columnType == DefineColumnType.RefTemplateField)
                {
                    define.RefTemplateField = value;
                }
                else if (columnType == DefineColumnType.Ref)
                {
                    define.RefTemplate = value;
                }
                else if (columnType == DefineColumnType.RefField)
                {
                    define.RefTemplateField = value;
                }
                else if (columnType == DefineColumnType.ConditionField)
                {
                    define.ConditionField = value;
                }
                else if (columnType == DefineColumnType.Condition)
                {
                    if (string.IsNullOrEmpty(value) == false)
                    {
                        if (string.IsNullOrEmpty(define.ConditionField) == true)
                        {
                            throw new Exception($"{fullFileName} data sheet row : {rowIndex} [ConditionField] is empty");
                        }

                        var conditionParts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        var condition = new Condition();

                        foreach (var part in conditionParts)
                        {
                            var trimmedConditionPart = part.Trim();
                            var keyValuePair = trimmedConditionPart.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                            if (keyValuePair.Length != 2)
                            {
                                throw new Exception($"{fullFileName} the entry {trimmedConditionPart} in {value} is not a valid key-value pair.");
                            }

                            var conditionKey = keyValuePair[0].Trim().ToLower();
                            var conditionValue = keyValuePair[1].Trim();

                            if (conditionKey.Equals("is"))
                            {
                                condition.Is = conditionValue;
                            }
                            else if (conditionKey.Equals("ref"))
                            {
                                condition.Ref = conditionValue;
                            }
                            else if (conditionKey.Equals("reffield"))
                            {
                                condition.RefField = conditionValue;
                            }
                            else
                            {
                                throw new Exception($"{fullFileName} file error : the field {conditionKey} in {value} is undefined or not recognized.");
                            }
                        }

                        define.Conditions.Add(condition);
                    }
                }
            }

            return define;
        }

        private DataTable GetDataTableFromDataSheet(string path, string sheetName)
        {
            try
            {
                using (var archive = ZipFile.OpenRead(path))
                {
                    XNamespace spreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                    XNamespace documentRelationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
                    XNamespace packageRelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
                    XDocument workbook;
                    XDocument relationships;
                    using (var stream = archive.GetEntry("xl/workbook.xml").Open())
                    {
                        workbook = XDocument.Load(stream);
                    }
                    using (var stream = archive.GetEntry("xl/_rels/workbook.xml.rels").Open())
                    {
                        relationships = XDocument.Load(stream);
                    }

                    var sheet = workbook.Root.Element(spreadsheetNamespace + "sheets").Elements(spreadsheetNamespace + "sheet").FirstOrDefault(element => (string)element.Attribute("name") == sheetName);
                    if (sheet == null)
                    {
                        return null;
                    }

                    var relationshipIdentifier = (string)sheet.Attribute(documentRelationshipNamespace + "id");
                    var relationship = relationships.Root.Elements(packageRelationshipNamespace + "Relationship").First(element => (string)element.Attribute("Id") == relationshipIdentifier);
                    var sheetUri = new Uri(new Uri("file:///xl/workbook.xml"), (string)relationship.Attribute("Target"));
                    var sheetPath = Uri.UnescapeDataString(sheetUri.AbsolutePath).TrimStart('/');
                    var sharedStrings = new List<string>();
                    var sharedStringsEntry = archive.GetEntry("xl/sharedStrings.xml");
                    if (sharedStringsEntry != null)
                    {
                        using (var stream = sharedStringsEntry.Open())
                        {
                            var sharedStringsDocument = XDocument.Load(stream, LoadOptions.PreserveWhitespace);
                            foreach (var sharedString in sharedStringsDocument.Root.Elements(spreadsheetNamespace + "si"))
                            {
                                sharedStrings.Add(ReadSpreadsheetText(sharedString, spreadsheetNamespace));
                            }
                        }
                    }

                    XDocument worksheet;
                    using (var stream = archive.GetEntry(sheetPath).Open())
                    {
                        worksheet = XDocument.Load(stream, LoadOptions.PreserveWhitespace);
                    }

                    var dataTable = new DataTable(sheetName);
                    foreach (var row in worksheet.Root.Element(spreadsheetNamespace + "sheetData").Elements(spreadsheetNamespace + "row"))
                    {
                        var rowIndex = int.Parse((string)row.Attribute("r"), CultureInfo.InvariantCulture) - 1;
                        foreach (var cell in row.Elements(spreadsheetNamespace + "c"))
                        {
                            var valueElement = cell.Element(spreadsheetNamespace + "v");
                            var inlineString = cell.Element(spreadsheetNamespace + "is");
                            if (valueElement == null)
                            {
                                if (inlineString == null)
                                {
                                    continue;
                                }
                            }

                            var cellType = (string)cell.Attribute("t");
                            var cellValue = string.Empty;
                            if (cellType == "s")
                            {
                                cellValue = sharedStrings[int.Parse(valueElement.Value, CultureInfo.InvariantCulture)];
                            }
                            else if (cellType == "inlineStr")
                            {
                                cellValue = ReadSpreadsheetText(inlineString, spreadsheetNamespace);
                            }
                            else if (valueElement != null)
                            {
                                cellValue = valueElement.Value;
                                if (cellType == "str")
                                {
                                    cellValue = DecodeSpreadsheetText(cellValue);
                                }
                            }

                            if (cellValue.Length == 0)
                            {
                                continue;
                            }
                            if (cellType == "e")
                            {
                                throw new FormatException($"{sheetName}!{(string)cell.Attribute("r")} contains an Excel error: {cellValue}");
                            }

                            var columnIndex = 0;
                            foreach (var character in (string)cell.Attribute("r"))
                            {
                                if (character < 'A')
                                {
                                    break;
                                }
                                if (character > 'Z')
                                {
                                    break;
                                }
                                columnIndex = columnIndex * 26 + character - 'A' + 1;
                            }
                            columnIndex--;

                            while (dataTable.Columns.Count <= columnIndex)
                            {
                                dataTable.Columns.Add($"F{dataTable.Columns.Count + 1}", typeof(object));
                            }
                            while (dataTable.Rows.Count <= rowIndex)
                            {
                                dataTable.Rows.Add(dataTable.NewRow());
                            }
                            if (cellType == "b")
                            {
                                dataTable.Rows[rowIndex][columnIndex] = cellValue == "1";
                            }
                            else
                            {
                                dataTable.Rows[rowIndex][columnIndex] = cellValue;
                            }
                        }
                    }

                    return dataTable;
                }
            }
            catch (Exception exception)
            {
                throw new FormatException($"{exception.Message} path : {path}", exception);
            }
        }

        private string ReadSpreadsheetText(XElement element, XNamespace spreadsheetNamespace)
        {
            var text = new StringBuilder();
            foreach (var child in element.Elements())
            {
                if (child.Name == spreadsheetNamespace + "t")
                {
                    text.Append(child.Value);
                }
                else if (child.Name == spreadsheetNamespace + "r")
                {
                    foreach (var textElement in child.Elements(spreadsheetNamespace + "t"))
                    {
                        text.Append(textElement.Value);
                    }
                }
            }
            return DecodeSpreadsheetText(text.ToString());
        }

        private string DecodeSpreadsheetText(string value)
        {
            return Regex.Replace(value, "_x([0-9A-Fa-f]{4})_", match => ((char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToString());
        }

        private List<CellData> GetDataCell(DataStream<Define> defineReader, DataStream<object> rowReader, string fileName)
        {
            var define = defineReader.Read();
            var cellDataList = new List<CellData>();
            var currentDefinePostion = defineReader.Position;

            for (int i = 0; i < define.Count; ++i)
            {
                var cellData = new CellData
                {
                    DefineName = define.Name,
                    DataType = define.DataType,
                };

                if (define.DataType == DataType.Class)
                {
                    defineReader.SetPosition(currentDefinePostion);

                    foreach (var member in define.Members)
                    {
                        var memberDatas = GetDataCell(defineReader, rowReader, fileName);

                        foreach (var item in memberDatas)
                        {
                            cellData.AddToMemberList(member, item);
                        }
                    }
                }
                else
                {
                    cellData.Value = rowReader.Read();
                }

                cellDataList.Add(cellData);
            }

            return cellDataList;
        }

        private void CheckColumnName(string fileName, DataTable dataTable, DataStream<Define> defineReader, int rowIndex, ref int colIndex, int depth)
        {
            var define = defineReader.Read();
            var position = defineReader.Position;

            for (var count = 0; count < define.Count; count++)
            {
                if (_remarkColumns.Contains(colIndex))
                {
                    count--;
                    colIndex++;
                    continue;
                }

                if (dataTable.Rows[rowIndex - (rowIndex - depth)].ItemArray.Length <= colIndex)
                {
                    throw new Exception($"{fileName} row : {rowIndex} col : {colIndex} not found data column! define : [{define.Name}]");
                }

                string columnName;

                if (define.DataType == DataType.Class)
                {
                    columnName = dataTable.Rows[rowIndex - (rowIndex - depth)][colIndex].ToString();

                    defineReader.SetPosition(position);

                    for (int member = 0; member < define.Members.Count; ++member)
                    {
                        var memberDefine = defineReader.Peek();

                        if (memberDefine.Name.Equals(define.Members[member]) == false)
                        {
                            throw new Exception($"file {fileName} - row: {rowIndex}, column: {colIndex}, column Name: {columnName} - member {define.Members[member]} is not defined.");
                        }

                        CheckColumnName(fileName, dataTable, defineReader, rowIndex, ref colIndex, depth + 1);
                    }

                    colIndex--;
                }
                else
                {
                    columnName = dataTable.Rows[rowIndex][colIndex].ToString();
                }

                var remarkIndex = columnName.IndexOf("#");

                if (remarkIndex != -1)
                {
                    columnName = columnName.Substring(0, remarkIndex);
                }

                columnName = columnName.ToLower();

                if (define.Name.ToLower().Equals(columnName) == false)
                {
                    throw new Exception($"file {fileName} row : {rowIndex} column : {colIndex} [{columnName}] : data does not match the defined structure for {define.Name}");
                }

                if (defineReader.CanRead == false)
                {
                    return;
                }

                colIndex++;
            }
        }

        private ExcelDataContainer MakeExcelDataFromDataTable(string name, string fullFileName, DataTable dataTable)
        {
            var defineDatas = _dataContainer.GetDefine(name);

            if (defineDatas.Count == 0)
            {
                throw new Exception($"not found first field");
            }

            var defineReader = new DataStream<Define>(defineDatas);
            var firstField = defineDatas[0].Name.ToLower();
            var dummyLine = 0;

            for (int i = 0; i < dataTable.Rows.Count; ++i)
            {
                var value = dataTable.Rows[i].ItemArray[0].ToString();
                if (string.IsNullOrEmpty(value) == true)
                {
                    dummyLine++;
                }
                if (value.ToLower().Equals(firstField) == true)
                {
                    var columnIndex = 0;
                    var itemReader = new DataStream<object>(dataTable.Rows[i].ItemArray);
                    while (itemReader.CanRead == true)
                    {
                        var columnName = itemReader.Read().ToString();
                        if (string.IsNullOrEmpty(columnName))
                        {
                            throw new Exception($"{name} file has an empty column. please populate the column");
                        }

                        if (columnName[0] == '#')
                        {
                            _remarkColumns.Add(itemReader.Position - 1);
                        }
                    }
                    while (defineReader.CanRead == true)
                    {
                        CheckColumnName(name, dataTable, defineReader, i, ref columnIndex, 0);
                    }

                    break;
                }
            }

            var excelDataContainer = new ExcelDataContainer(name, fullFileName);

            var checkIdContainer = new UniqueSet<object>();
            var checkNameContainer = new UniqueSet<object>();

            for (int i = dummyLine + 1; i < dataTable.Rows.Count; ++i)
            {
                var rowData = new ExcelRowData();
                var itemReader = new DataStream<object>(dataTable.Rows[i].ItemArray);

                defineReader.SetPosition(0);

                while (defineReader.CanRead)
                {
                    if (_remarkColumns.Contains(itemReader.Position))
                    {
                        itemReader.Read();
                        continue;
                    }

                    var define = defineReader.Peek();
                    var cellDatas = GetDataCell(defineReader, itemReader, fullFileName);

                    if (define.Required == true || define.DataType is DataType.Enum)
                    {
                        foreach (var cellData in cellDatas)
                        {
                            if (cellData.Value == DBNull.Value)
                            {
                                throw new Exception($"the file '{name}' has an empty define '{define.Name}' at row {i + 1}. please provide a value.");

                            }
                        }
                    }

                    rowData.AddRange(define.Name, cellDatas);
                }

                foreach (var item in rowData.GetDatas())
                {
                    if (item.Key.ToLower().Equals(firstField))
                    {
                        if (!checkIdContainer.Contains(item.Value[0].Value))
                        {
                            checkIdContainer.Add(item.Value[0].Value);
                        }
                        else
                        {
                            throw new Exception($"the file {name} has a duplicated id in the row {i + 1}: {item.Value[0].Value}. please ensure each id is unique.");
                        }
                    }
                    else if (item.Key.ToLower().Equals("name"))
                    {
                        var value = item.Value[0].Value;

                        if (string.IsNullOrEmpty(value as string))
                        {
                            throw new Exception($"the 'Name' field is empty in row {i + 1} of the file '{name}'. please ensure all 'Name' fields contain valid entries.");
                        }

                        if (!checkNameContainer.Contains(value))
                        {
                            checkNameContainer.Add(item.Value[0].Value);
                        }
                        else
                        {
                            throw new Exception($"the file {name} has a duplicated name in the row {i + 1}: {item.Value[0].Value}. please ensure each name is unique.");
                        }
                    }
                }

                excelDataContainer.Add(rowData);
            }

            return excelDataContainer;
        }

        private void CheckConditionRefDatas(Define define, string name)
        {
            var excelDataContainer = _dataContainer.GetExcelDataContainer(name);
            var rows = excelDataContainer.GetRows();
            var rowIndex = 0;

            foreach (var row in rows)
            {
                var conditionField = row.GetFieldData(define.ConditionField);
                var rowFieldDatas = row.GetFieldData(define.Name);

                foreach (var rowCellData in rowFieldDatas)
                {
                    foreach (var cell in conditionField)
                    {
                        var conditionFieldValue = cell.Value.ToString();
                        var find = false;
                        Condition conditionRef = null;

                        foreach (var condition in define.Conditions)
                        {
                            conditionRef = condition;

                            if (condition.Is.ToString() == conditionFieldValue)
                            {
                                var refExcelDataContainer = _dataContainer.GetExcelDataContainer(condition.Ref);

                                if (refExcelDataContainer == null)
                                {
                                    throw new Exception($"{excelDataContainer.FullFileName} has a definition [ {define.Name} ] that could not find the referenced data : {condition.Ref}");
                                }

                                find = refExcelDataContainer.Contains(condition.RefField, rowCellData.Value);
                                break;
                            }
                        }
                        if (find == false && define.Required == true)
                        {
                            throw new Exception($"{excelDataContainer.FullFileName} has no match for {define.Name} in {conditionRef.Ref}.{conditionRef.RefField} with data: {rowCellData.Value}");
                        }
                    }
                }

                rowIndex++;
            }
        }

        private void GenerateDefineFile()
        {
            var path = Path.Combine(_generatorConfiguration.RefPath, Consts.ClassDefineName);

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            var directoryInfo = new DirectoryInfo(path);
            var removed = new List<FileInfo>();

            foreach (var file in directoryInfo.GetFiles())
            {
                if (_dataContainer.GetDefine(Path.GetFileNameWithoutExtension(file.Name)) == null)
                {
                    removed.Add(file);
                }
            }

            foreach (var item in removed)
            {
                item.Delete();
            }

            removed.Clear();

            foreach (var key in _dataContainer.DefineKeys())
            {
                var define = _dataContainer.GetDefine(key);

                if (_mergeExcludedFiles.Contains(key))
                {
                    continue;
                }

                ExportDefineFile(path, key, define);
            }

            foreach (var excelMergeDefinition in _generatorConfiguration.ExcelMergeDefinitions)
            {
                var defineKey = Path.GetFileNameWithoutExtension(excelMergeDefinition.BaseDefine);
                var define = _dataContainer.GetDefine(defineKey);

                ExportDefineFile(path, defineKey, define);
            }
        }

        private void CheckRefTemplate(Define define, string name)
        {
            var refDefine = _dataContainer.GetDefine(define.RefTemplate);
            var excelDataContainer = _dataContainer.GetExcelDataContainer(name);

            if (refDefine == null)
            {
                throw new Exception($"{excelDataContainer.FullFileName} definition [ {define.Name} ] not found. missing reference template : [ {define.RefTemplate} ]");
            }

            var find = false;

            foreach (var item in refDefine)
            {
                if (item.Name.Equals(define.RefTemplateField))
                {
                    find = true;
                    break;
                }
            }

            if (find == false)
            {
                throw new Exception($"{excelDataContainer.FullFileName} definition [ {define.Name} ] not found. missing reference field : {define.RefTemplate}.{define.RefTemplateField}");
            }

            var rowFieldDatas = excelDataContainer.GetFieldData(define.Name);
            var refExcelDataContainer = _dataContainer.GetExcelDataContainer(define.RefTemplate);

            foreach (var rowData in rowFieldDatas)
            {
                if (define.Required == false)
                {
                    continue;
                }

                find = false;

                foreach (var cellData in rowData)
                {
                    if (cellData.Value == null)
                    {
                        continue;
                    }

                    if (cellData.Value == DBNull.Value)
                    {
                        continue;
                    }

                    find = refExcelDataContainer.Contains(define.RefTemplateField, cellData.Value);

                    if (find == false)
                    {
                        throw new Exception($"{excelDataContainer.FullFileName} has no match for {define.Name} in {define.RefTemplate}.{define.RefTemplateField} with data : {cellData.Value}");
                    }
                }
            }
        }

        private string CopyToTemporaryExcelFile(string excelPath)
        {
            var temporaryExcelPath = Path.Combine(
                Path.GetTempPath(),
                $"{Path.GetFileNameWithoutExtension(excelPath)}_{Guid.NewGuid()}{Path.GetExtension(excelPath)}");

            using (var sourceStream = new FileStream(
                       excelPath,
                       FileMode.Open,
                       FileAccess.Read,
                       FileShare.ReadWrite))
            using (var destinationStream = new FileStream(
                       temporaryExcelPath,
                       FileMode.Create,
                       FileAccess.Write,
                       FileShare.None))
            {
                sourceStream.CopyTo(destinationStream);
            }

            return temporaryExcelPath;
        }

        private bool CheckRefDatas(List<Define> defines, string name)
        {
            foreach (var define in defines)
            {
                if (string.IsNullOrEmpty(define.RefTemplate) == false)
                {
                    CheckRefTemplate(define, name);
                }

                if (string.IsNullOrEmpty(define.ConditionField) == false)
                {
                    CheckConditionRefDatas(define, name);
                }
            }

            return true;
        }
    }
}
