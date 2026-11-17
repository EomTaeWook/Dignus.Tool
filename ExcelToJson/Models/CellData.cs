using Shared;
using System.Collections.Generic;

namespace ExcelToJson.Models
{
    internal class CellData
    {
        public DataType DataType { get; set; }
        public string DefineName { get; set; }
        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                //if (value == DBNull.Value)
                //{
                //    _value = DataHelper.GetDefaultValueBasedOnDataType(DataType);
                //}
                //else
                //{
                //    _value = value;
                //}
            }
        }
        private object _value;
        private readonly Dictionary<string, List<CellData>> _members = new Dictionary<string, List<CellData>>();
        public void AddToMemberList(string member, CellData cellData)
        {
            if (_members.ContainsKey(member) == false)
            {
                _members.Add(member, new List<CellData>());
            }
            _members[member].Add(cellData);
        }

        public Dictionary<string, List<CellData>> GetMembers()
        {
            return _members;
        }

    }
}
