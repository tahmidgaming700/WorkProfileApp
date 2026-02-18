using System.IO;
using System.Text.Json;
using VmManagerWpf.Models;

namespace VmManagerWpf.Services;

public class TemplateManager
{
    private readonly string _catalogPath;

    public TemplateManager(string catalogPath)
    {
        _catalogPath = catalogPath;
    }

    public TemplateCatalog LoadCatalog()
    {
        if (!File.Exists(_catalogPath))
        {
            throw new FileNotFoundException("Template catalog not found.", _catalogPath);
        }

        var json = File.ReadAllText(_catalogPath);
        return JsonSerializer.Deserialize<TemplateCatalog>(json, JsonOptions()) ?? new TemplateCatalog();
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}
