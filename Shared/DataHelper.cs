using System.Text;

namespace Shared
{
    public class DataHelper
    {
        private static string[] DataTypeToString { get; set; } = new string[(int)DataType.Max]
        {
            "class",
            "int32",
            "int64",
            "string",
            "bool",
            "float",
            "double",
            "enum"
        };

        private static string[] DataTypeToCSharpString { get; set; } = new string[(int)DataType.Max]
        {
            "Inner",
            "int",
            "long",
            "string",
            "bool",
            "float",
            "double",
            ""
        };
        public static object GetDefaultValueBasedOnDataType(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Class:
                    return null;
                case DataType.Int32:
                    return default(int);
                case DataType.Int64:
                    return default(long);
                case DataType.String:
                    return string.Empty;
                case DataType.Bool:
                    return default(bool);
                case DataType.Float:
                    return default(float);
                case DataType.Double:
                    return default(double);
                default:
                    return null;
            }
        }
        public static DataType GetDataTypeToString(string dataType)
        {
            var index = 0;
            foreach (var item in DataTypeToString)
            {
                if (item == dataType.ToLower())
                {
                    return (DataType)index;
                }
                index++;
            }
            return DataType.Max;
        }
        public static string GetDataTypeToCSharpString(Define define)
        {
            var sb = new StringBuilder();
            sb.Append(DataTypeToCSharpString[(int)define.DataType]);
            if (define.DataType == DataType.Class)
            {
                sb.Append(define.Name);
            }
            else if (define.DataType == DataType.Enum)
            {
                sb.Append(define.Name);
            }

            return sb.ToString();
        }
    }
}
