using Dignus.Log;
using Dignus.Log.LogTarget;
using Dignus.Log.Formatting;
using Dignus.Log.Model;
using ExcelToJson;
using ExcelToJson.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {
        InitLog();
        try
        {
            var path = $"config.json";
#if DEBUG
            path = $"..\\..\\..\\config.json";
#endif

            if (File.Exists(path) == false)
            {
                var json = JsonHelper.Serialize<Config>(new Config());
                File.WriteAllText(path, json);
            }

            var config = JsonHelper.Deserialize<Config>(File.ReadAllText(path));

            var generatorConfiguration = new GeneratorConfiguration()
            {
                GeneratedPath = config.OutputTemplatePath,
                RefPath = config.OutputRefPath,
                ExcelMergeDefinitions = config.ExcelMergeDefinitions,
            };
            AssemblyReader assemblyReader = new AssemblyReader(generatorConfiguration);
            assemblyReader.Load(config.ReadAssemblyPath);

            DirectoryInfo directoryInfo = new DirectoryInfo(config.ReadPath);

            if (directoryInfo.Exists == false)
            {
                directoryInfo.Create();
            }

            var files = new List<FileInfo>();
            foreach (var file in directoryInfo.GetFiles())
            {
                var fileName = file.Name;

                if (fileName.StartsWith("~$"))
                {
                    continue;
                }

                if (file.Extension.Equals(".xlsx") == true)
                {
                    files.Add(file);
                }
            }

            ExcelReader excelReader = new ExcelReader(generatorConfiguration, assemblyReader.GetEnumContainer());

            foreach (var file in files)
            {
                excelReader.Read(file.FullName);
            }
            assemblyReader.Generate();
            excelReader.Generate();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($@"작업이 완료되었습니다. 파일을 확인해주세요.");
        }
        catch (Exception ex)
        {
            LogHelper.Error(ex.Message);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($@"데이터 오류가 있습니다. 에러를 확인해주세요.");
        }
        if (Flags.Contains(args, Flags.NoPause) == false)
        {
            Console.ReadKey();
        }
    }
    private static void InitLog()
    {
        var logConfiguration = new LogConfiguration();
        ConsoleLogTarget consoleLogTarget = new ConsoleLogTarget
        {
            LogFormatRenderer = new LogFormatRenderer("${message} | ${callerFileName} : ${callerLineNumber}"),
            ErrorColor = ConsoleColor.Red,
            FatalColor = ConsoleColor.Red,
        };
        logConfiguration.AddLogger("console", LoggerType.Sync).AddLogTarget(LogLevel.Debug, consoleLogTarget);
        LogBuilder.Configuration(logConfiguration);
        LogBuilder.Build();
        LogHelper.SetLogger(LogManager.GetLogger("console"));
    }
}
