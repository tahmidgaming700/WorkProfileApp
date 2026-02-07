using System.Reflection;

namespace WindowsOtaUpdater;

public partial class MainForm : Form
{
    private readonly UpdaterService _updater = new();
    private UpdateManifest? _manifest;

    public MainForm()
    {
        InitializeComponent();
        txtManifestUrl.Text = "https://example.com/updates/latest.json";
        txtDownloadPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "update.msu");
        lblCurrentVersion.Text = $"Current app version: {GetCurrentVersion()}";
    }

    private static string GetCurrentVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
    }

    private async void btnCheckUpdate_Click(object? sender, EventArgs e)
    {
        try
        {
            SetBusy(true, "Checking for updates...");
            _manifest = await _updater.GetManifestAsync(txtManifestUrl.Text.Trim(), CancellationToken.None);

            lblLatestVersion.Text = $"Latest version: {_manifest.Version}";
            btnDownload.Enabled = true;
            btnInstall.Enabled = File.Exists(txtDownloadPath.Text) && _updater.VerifySha256(txtDownloadPath.Text, _manifest.Sha256);
            SetBusy(false, "Update manifest loaded.");
        }
        catch (Exception ex)
        {
            SetBusy(false, "Failed to check updates.");
            MessageBox.Show($"Could not check updates.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnDownload_Click(object? sender, EventArgs e)
    {
        if (_manifest is null)
        {
            MessageBox.Show("Please click Check Update first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var path = txtDownloadPath.Text.Trim();
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");

            SetBusy(true, "Downloading update package...");
            progressBar.Value = 0;

            var progress = new Progress<int>(value =>
            {
                progressBar.Value = Math.Clamp(value, 0, 100);
                lblStatus.Text = $"Status: Downloading... {progressBar.Value}%";
            });

            await _updater.DownloadFileAsync(_manifest.MsuUrl, path, progress, CancellationToken.None);

            if (!_updater.VerifySha256(path, _manifest.Sha256))
            {
                File.Delete(path);
                throw new InvalidDataException("Checksum verification failed. The downloaded file was deleted.");
            }

            btnInstall.Enabled = true;
            SetBusy(false, "Download completed and SHA-256 verified.");
        }
        catch (Exception ex)
        {
            SetBusy(false, "Download failed.");
            MessageBox.Show($"Download failed.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnInstall_Click(object? sender, EventArgs e)
    {
        if (_manifest is null)
        {
            MessageBox.Show("Please click Check Update first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var path = txtDownloadPath.Text.Trim();
        if (!File.Exists(path))
        {
            MessageBox.Show("Downloaded .msu file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!_updater.VerifySha256(path, _manifest.Sha256))
        {
            MessageBox.Show("Checksum mismatch. Re-download the update package.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            SetBusy(true, "Installing update (wusa.exe)...");
            var exitCode = await _updater.InstallMsuAsync(path, CancellationToken.None);
            SetBusy(false, $"Install finished (exit code {exitCode}).");

            var message = exitCode switch
            {
                0 => "Update installed successfully.",
                3010 => "Update installed successfully. A reboot is required.",
                2359302 => "The update is already installed.",
                _ => $"Installer finished with exit code {exitCode}. Check CBS.log / Windows Update logs for details."
            };

            MessageBox.Show(message, "Install Result", MessageBoxButtons.OK,
                exitCode == 0 || exitCode == 3010 || exitCode == 2359302 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            SetBusy(false, "Install failed.");
            MessageBox.Show($"Install failed.\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SetBusy(bool busy, string status)
    {
        btnCheckUpdate.Enabled = !busy;
        btnDownload.Enabled = !busy && _manifest is not null;
        btnInstall.Enabled = !busy && _manifest is not null && File.Exists(txtDownloadPath.Text.Trim());
        lblStatus.Text = $"Status: {status}";
    }
}
