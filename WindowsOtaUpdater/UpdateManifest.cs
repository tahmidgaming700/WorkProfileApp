namespace WindowsOtaUpdater;

public sealed class UpdateManifest
{
    public string Version { get; init; } = string.Empty;
    public string MsuUrl { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? CurrentBuild { get; init; }
    public string? TargetBuild { get; init; }
    public string? ChangelogUrl { get; init; }
}
