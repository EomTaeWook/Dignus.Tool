using ExcelToJson.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ExcelToJson
{
    internal class AssemblyReader
    {
        private readonly EnumContainer _enumContainer = new EnumContainer();
        private readonly GeneratorConfiguration _generatorConfiguration;
        public AssemblyReader(GeneratorConfiguration generatorConfiguration)
        {
            this._generatorConfiguration = generatorConfiguration;
        }
        public void Load(string path)
        {
            if (string.IsNullOrEmpty(path) == true)
            {
                return;
            }
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists == false)
            {
                throw new FileNotFoundException($"{path}");
            }
            var assembly = Assembly.LoadFrom(fileInfo.FullName);

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsEnum == true)
                {
                    foreach (var enumValue in Enum.GetValues(type))
                    {
                        _enumContainer.Add(type.Name, enumValue.ToString());
                    }
                    _enumContainer.AddNamespace(type.Name, type.Namespace);
                }
            }
        }
        public EnumContainer GetEnumContainer()
        {
            return _enumContainer;
        }

        public void Generate()
        {
            var path = Path.Combine(_generatorConfiguration.RefPath, Consts.RefName);

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach (var file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            AssemblyGenerate assemblyGenerate = new AssemblyGenerate
            {
                EnumNamespace = _enumContainer.GetEnumNamespaceDatas()
            };

            foreach (var key in _enumContainer.GetKeys())
            {
                var values = _enumContainer.Get(key);
                assemblyGenerate.EnumValueDatas.Add(key, new List<string>());
                foreach (var value in values)
                {
                    assemblyGenerate.EnumValueDatas[key].Add(value);
                }
            }
            var json = JsonHelper.Serialize(assemblyGenerate);
            var filePath = Path.Combine(path, Consts.EnumRefFileName);
            File.WriteAllText(filePath, json);
        }
    }
}
