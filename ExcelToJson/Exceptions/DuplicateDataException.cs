using System;

namespace ExcelToJson.Exceptions
{
    internal class DuplicateDataException : Exception
    {
        public string FieldName { get; }
        public object DuplicateValue { get; }
        public string FileName { get; }

        public DuplicateDataException(string fieldName,
            object duplicateValue,
            string fileName) : base($"Duplicate value detected. Field: {fieldName}, Value: {duplicateValue}, File: {fileName}")
        {
            FieldName = fieldName;
            DuplicateValue = duplicateValue;
            FileName = fileName;
        }
    }
}
