using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CastIt.Domain.Utils;

namespace CastIt.Cli.Models;

public class AppSettings
{
    public string ServerUrl { get; set; }

    public static async Task<AppSettings> Get()
    {
        string filePath = GetPath();
        if (!File.Exists(filePath))
        {
            var settings = new AppSettings();
            await settings.Save();
        }
        string json = await File.ReadAllTextAsync(filePath!);
        return JsonSerializer.Deserialize<AppSettings>(json);
    }

    private static string GetPath()
    {
        string path = AppFileUtils.GetBaseAppFolder("CastItCli");
        return Path.Combine(path, "settings.json");
    }

    public Task Save()
    {
        string filePath = GetPath();
        string json = JsonSerializer.Serialize(this);
        return File.WriteAllTextAsync(filePath, json);
    }
}