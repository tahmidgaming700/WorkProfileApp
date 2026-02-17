using System.Text.Json.Serialization;

namespace VmManagerWpf.Models;

public class TemplateCatalog
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("templates")]
    public List<VmTemplate> Templates { get; set; } = new();
}
