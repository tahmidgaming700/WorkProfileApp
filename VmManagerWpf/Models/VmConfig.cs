using System.Text.Json.Serialization;

namespace VmManagerWpf.Models;

public class VmConfig
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("templateId")]
    public string TemplateId { get; set; } = string.Empty;

    [JsonPropertyName("isoPath")]
    public string IsoPath { get; set; } = string.Empty;

    [JsonPropertyName("diskPath")]
    public string DiskPath { get; set; } = string.Empty;

    [JsonPropertyName("ramMb")]
    public int RamMb { get; set; }

    [JsonPropertyName("cpuCores")]
    public int CpuCores { get; set; }

    [JsonPropertyName("machine")]
    public string Machine { get; set; } = "q35";

    [JsonPropertyName("enableUefi")]
    public bool EnableUefi { get; set; }

    [JsonPropertyName("kernelTesting")]
    public KernelTestingConfig KernelTesting { get; set; } = new();
}

public class KernelTestingConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("kernelPath")]
    public string KernelPath { get; set; } = string.Empty;

    [JsonPropertyName("initramfsPath")]
    public string InitramfsPath { get; set; } = string.Empty;

    [JsonPropertyName("kernelCmdline")]
    public string KernelCmdline { get; set; } = "console=ttyS0";
}
