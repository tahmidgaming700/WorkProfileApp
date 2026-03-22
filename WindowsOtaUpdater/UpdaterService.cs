using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WindowsOtaUpdater;

public sealed class UpdaterService
{
    private static readonly HttpClient HttpClient = new();

    public async Task<UpdateManifest> GetManifestAsync(string manifestUrl, CancellationToken cancellationToken)
    {
        using var response = await HttpClient.GetAsync(manifestUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var manifest = await JsonSerializer.DeserializeAsync<UpdateManifest>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }, cancellationToken);

        if (manifest is null || string.IsNullOrWhiteSpace(manifest.Version) ||
            string.IsNullOrWhiteSpace(manifest.MsuUrl) || string.IsNullOrWhiteSpace(manifest.Sha256))
        {
            throw new InvalidOperationException("Manifest is missing required fields: version, msuUrl, sha256.");
        }

        return manifest;
    }

    public async Task DownloadFileAsync(string fileUrl, string destinationPath, IProgress<int> progress, CancellationToken cancellationToken)
    {
        using var response = await HttpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;
        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var output = File.Create(destinationPath);

        var buffer = new byte[81920];
        long readTotal = 0;
        int read;

        while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            readTotal += read;

            if (totalBytes.HasValue && totalBytes.Value > 0)
            {
                var percent = (int)(readTotal * 100L / totalBytes.Value);
                progress.Report(percent);
            }
        }

        progress.Report(100);
    }

    public string ComputeSha256(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }

    public bool VerifySha256(string filePath, string expectedSha256)
    {
        var actual = ComputeSha256(filePath);
        return string.Equals(actual, expectedSha256.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase);
    }

    public async Task<int> InstallMsuAsync(string filePath, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "wusa.exe",
            Arguments = $"\"{filePath}\" /quiet /norestart",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start wusa.exe.");

        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }

    public async Task<string> FetchChangelogTextAsync(string changelogUrl, CancellationToken cancellationToken)
    {
        using var response = await HttpClient.GetAsync(changelogUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);

        // Simple HTML-to-text cleanup suitable for release note pages.
        var withoutScript = Regex.Replace(html, "<script[\\s\\S]*?</script>", " ", RegexOptions.IgnoreCase);
        var withoutStyle = Regex.Replace(withoutScript, "<style[\\s\\S]*?</style>", " ", RegexOptions.IgnoreCase);
        var withoutTags = Regex.Replace(withoutStyle, "<[^>]+>", " ");
        var decoded = System.Net.WebUtility.HtmlDecode(withoutTags);
        var normalized = Regex.Replace(decoded, "\\s+", " ").Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Could not parse changelog text from the provided Microsoft URL.");
        }

        return normalized;
    }

    public string GenerateAiSummary(string changelogText, string fromBuild, string toBuild)
    {
        var chunks = changelogText
            .Split(new[] { '.', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Length > 30)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var keywords = new[] { "security", "fix", "improve", "resolved", "known issue", "quality", "performance", "update" };

        var ranked = chunks
            .Select(line => new
            {
                Line = line,
                Score = keywords.Count(k => line.Contains(k, StringComparison.OrdinalIgnoreCase))
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Line.Length)
            .Take(5)
            .Select(x => $"• {x.Line}.")
            .ToList();

        if (ranked.Count == 0)
        {
            ranked.Add("• Release notes were fetched, but no clear highlights were extracted automatically.");
        }

        return $"AI release summary ({fromBuild} → {toBuild})\n\n" + string.Join(Environment.NewLine, ranked);
    }
}
