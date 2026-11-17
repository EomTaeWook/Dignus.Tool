using Dignus.Collections;
using ExcelToJson.Exceptions;
using Shared;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ExcelToJson.Models
{
    internal class ExcelRowData
    {
        private readonly Dictionary<string, List<CellData>> _cellDatas = new Dictionary<string, List<CellData>>();
        public void Add(string key, CellData data)
        {
            if (_cellDatas.TryGetValue(key, out List<CellData> list) == false)
            {
                list = new List<CellData>();
                _cellDatas.Add(key, list);
            }
            list.Add(data);
        }
        public void AddRange(string key, List<CellData> datas)
        {
            if (_cellDatas.TryGetValue(key, out List<CellData> list) == false)
            {
                list = new List<CellData>();
                _cellDatas.Add(key, list);
            }
            list.AddRange(datas);
        }
        public Dictionary<string, List<CellData>> GetDatas()
        {
            return _cellDatas;
        }
        public List<CellData> GetFieldData(List<CellData> cellDatas, string findName)
        {
            var list = new List<CellData>();
            foreach (var cellData in cellDatas)
            {
                if (cellData.DefineName.ToLower().Equals(findName))
                {
                    list.Add(cellData);
                }
                if (cellData.GetMembers().Count > 0)
                {
                    foreach (var memberData in cellData.GetMembers().Values)
                    {
                        list.AddRange(GetFieldData(memberData, findName));
                    }
                }
            }
            return list;
        }

        public bool Contains(string fieldName, object findValue)
        {
            var fieldDatas = GetFieldData(fieldName);

            foreach (var item in fieldDatas)
            {
                if (item.Value.Equals(findValue))
                {
                    return true;
                }
            }
            return false;
        }
        public List<CellData> GetFieldData(string fieldName)
        {
            var list = new List<CellData>();

            foreach (var kv in _cellDatas)
            {
                list.AddRange(GetFieldData(kv.Value, fieldName));
            }

            return list;
        }
    }

    internal class ExcelDataContainer
    {
        internal string FileName { get; private set; }
        internal string FullFileName { get; private set; }

        private ArrayQueue<ExcelRowData> _rowContainer = new ArrayQueue<ExcelRowData>();

        private UniqueSet<object> _uniqueIdSet = new UniqueSet<object>();

        private UniqueSet<object> _uniqueNameSet = new UniqueSet<object>();
        public ExcelDataContainer(string fileName, string fullFileName)
        {
            FileName = fileName;
            FullFileName = fullFileName;
        }
        public void Add(ExcelRowData excelRow)
        {
            var idValue = excelRow.GetFieldData("id")[0].Value;
            if (_uniqueIdSet.Contains(idValue))
            {
                throw new DuplicateDataException("Id", idValue, FullFileName);
            }
            _uniqueIdSet.Add(excelRow.GetFieldData("id")[0].Value);

            var nameValue = excelRow.GetFieldData("name")[0].Value;
            if (_uniqueNameSet.Contains(idValue))
            {
                throw new DuplicateDataException("Name", nameValue, FullFileName);
            }
            _uniqueNameSet.Add(nameValue);
            _rowContainer.Add(excelRow);
        }
        public void AddRange(ArrayQueue<ExcelRowData> excelRows)
        {
            foreach (var item in excelRows)
            {
                Add(item);
            }
        }
        public List<List<CellData>> GetFieldData(string fieldName)
        {
            List<List<CellData>> list = new List<List<CellData>>();

            foreach (var item in _rowContainer)
            {
                var datas = item.GetFieldData(fieldName);
                list.Add(datas);
            }
            return list;
        }
        public bool Contains(string fieldName, object findValue)
        {
            foreach (var row in _rowContainer)
            {
                if (row.Contains(fieldName, findValue) == true)
                {
                    return true;
                }
            }
            return false;
        }
        public ArrayQueue<ExcelRowData> GetRows()
        {
            return _rowContainer;
        }
    }

    internal class DataContainer
    {
        private readonly Dictionary<string, List<Define>> _defineContainer = new Dictionary<string, List<Define>>();
        private readonly List<string> _defineKeys = new List<string>();
        private readonly Dictionary<string, ExcelDataContainer> _excelDataContainer = new Dictionary<string, ExcelDataContainer>();

        public void AddDefine(string key, List<Define> defineDatas)
        {
            _defineContainer.Add(key, defineDatas);
            _defineKeys.Add(key);
        }
        public List<Define> GetDefine(string name)
        {
            if (_defineContainer.TryGetValue(name, out List<Define> list) == false)
            {
                return null;
            }
            return list;
        }
        public void AddExcelData(string key, ExcelDataContainer container)
        {
            _excelDataContainer.Add(key, container);
        }
        public ExcelDataContainer GetExcelDataContainer(string name)
        {
            if (_excelDataContainer.TryGetValue(name, out ExcelDataContainer container) == false)
            {
                return null;
            }
            return container;
        }
        public ReadOnlyCollection<string> DefineKeys()
        {
            return new ReadOnlyCollection<string>(_defineKeys);
        }
    }
}
