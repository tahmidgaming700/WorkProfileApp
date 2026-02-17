using System.IO;
using System.Text.Json;
using VmManagerWpf.Models;

namespace VmManagerWpf.Services;

public class VmConfigService
{
    private readonly string _vmFolder;

    public VmConfigService(string vmFolder)
    {
        _vmFolder = vmFolder;
        Directory.CreateDirectory(_vmFolder);
    }

    public List<VmConfig> LoadAll()
    {
        var list = new List<VmConfig>();
        foreach (var file in Directory.GetFiles(_vmFolder, "*.json"))
        {
            var json = File.ReadAllText(file);
            var vm = JsonSerializer.Deserialize<VmConfig>(json, JsonOptions());
            if (vm != null)
            {
                list.Add(vm);
            }
        }

        return list.OrderBy(v => v.Name).ToList();
    }

    public void Save(VmConfig config)
    {
        var path = Path.Combine(_vmFolder, $"{SanitizeName(config.Name)}.json");
        var json = JsonSerializer.Serialize(config, JsonOptions());
        File.WriteAllText(path, json);
    }

    private static string SanitizeName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return name;
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
}
