using DllDeploy.Models;
using System.Text.Json;

namespace DllDeploy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}config.json";
#if DEBUG
            path = $"{AppDomain.CurrentDomain.BaseDirectory}../../../config.json";
#endif

            var configJson = File.ReadAllText(path);

            var config = JsonSerializer.Deserialize<Config>(configJson);

            foreach (var deploy in config.DeployInfos)
            {
                var filePath = config.VersionPath[deploy.Version];

                var di = new DirectoryInfo(filePath);
                var deployFiles = new List<FileInfo>();
                foreach (var file in di.GetFiles())
                {
                    if (deploy.IncludeDll.Contains(file.Name))
                    {
                        deployFiles.Add(file);
                    }
                }

                foreach (var item in deploy.ExternalFile)
                {
                    if (File.Exists(item) == true)
                    {
                        deployFiles.Add(new FileInfo(item));
                    }
                }

                foreach (var file in deployFiles)
                {
                    File.Delete(Path.Combine(deploy.DeployPath, file.Name));
                    File.Copy(file.FullName, Path.Combine(deploy.DeployPath, file.Name), true);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"{file.FullName} ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($">>>");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($" {Path.Combine(deploy.DeployPath, file.Name)}");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($" deployed");
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"작업이 완료되었습니다.");
            Console.ReadKey();
        }
    }
}