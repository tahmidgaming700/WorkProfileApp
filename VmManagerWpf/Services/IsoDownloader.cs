using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using VmManagerWpf.Models;

namespace VmManagerWpf.Services;

public class IsoDownloader
{
    private readonly HttpClient _httpClient = new();

    public async Task<string> EnsureIsoAsync(VmTemplate template, string isoFolder)
    {
        Directory.CreateDirectory(isoFolder);
        var targetPath = Path.Combine(isoFolder, template.IsoFileName);

        if (File.Exists(targetPath) && await VerifySha256Async(targetPath, template.Sha256))
        {
            return targetPath;
        }

        using (var response = await _httpClient.GetAsync(template.IsoUrl, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();
            await using var networkStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await networkStream.CopyToAsync(fileStream);
        }

        var verified = await VerifySha256Async(targetPath, template.Sha256);
        if (!verified)
        {
            File.Delete(targetPath);
            throw new InvalidDataException($"Checksum verification failed for {template.IsoFileName}.");
        }

        return targetPath;
    }

    private static async Task<bool> VerifySha256Async(string path, string expectedSha)
    {
        if (string.IsNullOrWhiteSpace(expectedSha))
        {
            return true;
        }

        await using var stream = File.OpenRead(path);
        var hashBytes = await SHA256.HashDataAsync(stream);
        var actual = Convert.ToHexString(hashBytes).ToLowerInvariant();
        return actual == expectedSha.Trim().ToLowerInvariant();
    }
}
