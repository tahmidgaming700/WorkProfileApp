using System.Text.Json.Serialization;

namespace VmManagerWpf.Models;

public class VmTemplate
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("isoUrl")]
    public string IsoUrl { get; set; } = string.Empty;

    [JsonPropertyName("isoFileName")]
    public string IsoFileName { get; set; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    [JsonPropertyName("defaultRamMb")]
    public int DefaultRamMb { get; set; }

    [JsonPropertyName("defaultCpuCores")]
    public int DefaultCpuCores { get; set; }

    [JsonPropertyName("defaultDiskGb")]
    public int DefaultDiskGb { get; set; }

    [JsonPropertyName("accelerator")]
    public string Accelerator { get; set; } = "whpx";

    public override string ToString() => Name;
}
