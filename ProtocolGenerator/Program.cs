// See https://aka.ms/new-console-template for more information


using ProtocolGenerator;
using ProtocolGenerator.Models;
using System.Text.Json;

try
{
    var path = $".\\config.json";

#if DEBUG
    path = $"..\\..\\..\\config.json";
#endif
    var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(path));

    var usingNamespace = config.UsingNamespace;
    var generators = new List<CSharpGenerator>();
    foreach (var item in config.Protocols)
    {
        CSharpGenerator csharpGenerator = new(usingNamespace,
             config.HandlerFormat,
             item);

        csharpGenerator.Load();
        generators.Add(csharpGenerator);
    }
    foreach (var item in generators)
    {
        item.Generate();
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(ex.Message);
}
var jsonFormat = JsonSerializer.Serialize(new DefaultCSharpHandlerFormat(), new JsonSerializerOptions()
{
    WriteIndented = true,
});
File.WriteAllText($"{AppContext.BaseDirectory}format.json", jsonFormat);

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"작업이 완료되었습니다. 파일을 확인해주세요.");
Console.ReadKey();
