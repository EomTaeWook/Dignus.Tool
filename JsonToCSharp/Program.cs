using Dignus.Log;
using Dignus.Log.LogTarget;
using Dignus.Log.Formatting;
using Dignus.Log.Model;
using JsonToCSharp;
using JsonToCSharp.Models;
using Shared;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {
        InitLog();

        DefaultCSharpTemplateFormat DefaultCSharpTemplateFormat = new DefaultCSharpTemplateFormat();

        try
        {
            var path = $".\\config.json";

#if DEBUG
            path = $"..\\..\\..\\config.json";
#endif

            if(File.Exists(path) == false)
            {
                var json = JsonHelper.Serialize<Config>(new Config());
                File.WriteAllText(path, json);
            }

            var config = JsonHelper.Deserialize<Config>(File.ReadAllText(path));
            List<string> usingNamespace = new List<string>
            {
                "using System;",
                "using System.Collections.Generic;",
                "using System.Diagnostics;",
                "using System.Text;",
            };

            if (string.IsNullOrEmpty(config.Namespace) == false)
            {
                usingNamespace.Add($"using {config.Namespace};");
            }

            ICSharpTemplateFormat csharpTemplateFormat;
            if (string.IsNullOrEmpty(config.CSharpTemplateFormatPath) == false)
            {
                csharpTemplateFormat = JsonHelper.Deserialize<CSharpTemplateFormat>(File.ReadAllText(path));
            }
            else
            {
                csharpTemplateFormat = DefaultCSharpTemplateFormat;
            }

            var definePath = Path.Combine(config.ReadRefPath, Consts.ClassDefineName);
            DirectoryInfo directoryInfo = new DirectoryInfo(definePath);

            if (directoryInfo.Exists == false)
            {
                directoryInfo.Create();
            }

            var files = new List<FileInfo>();
            foreach (var file in directoryInfo.GetFiles())
            {
                if (file.Extension.Equals(".json") == true)
                {
                    files.Add(file);
                }
            }
            var generatorConfiguration = new GeneratorConfiguration()
            {
                Namespace = $"{config.Namespace}.Generated",
                UsingNamespace = usingNamespace,
                GeneratedPath = config.GeneratedPath,
                LoaderClassName = "TemplateLoader",
                CSharpTemplateFormat = csharpTemplateFormat
            };

            var enumRefReader = new EnumRefReader();

            CSharpGenerator generator = new CSharpGenerator(generatorConfiguration, enumRefReader.Load(config.ReadRefPath));

            foreach (var file in files)
            {
                generator.Read(file.FullName);
            }
            generator.Generate();
        }
        catch (Exception ex)
        {
            LogHelper.Error(ex);
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($@"작업이 완료되었습니다. 파일을 확인해주세요.");

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
        };
        logConfiguration.AddLogger("console", LoggerType.Sync).AddLogTarget(LogLevel.Debug, consoleLogTarget);
        LogBuilder.Configuration(logConfiguration);
        LogBuilder.Build();
        LogHelper.SetLogger(LogManager.GetLogger("console"));
    }
}
