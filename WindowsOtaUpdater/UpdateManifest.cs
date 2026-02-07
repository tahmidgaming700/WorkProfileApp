namespace WindowsOtaUpdater;

public sealed class UpdateManifest
{
    public string Version { get; init; } = string.Empty;
    public string MsuUrl { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string? Description { get; init; }
}
