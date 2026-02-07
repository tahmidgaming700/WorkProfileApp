namespace WindowsOtaUpdater;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;

    private Label lblManifest;
    private TextBox txtManifestUrl;
    private Button btnCheckUpdate;
    private Label lblLatestVersion;
    private Label lblCurrentVersion;
    private Label lblDownloadPath;
    private TextBox txtDownloadPath;
    private Button btnDownload;
    private Button btnInstall;
    private ProgressBar progressBar;
    private Label lblStatus;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblManifest = new Label();
        txtManifestUrl = new TextBox();
        btnCheckUpdate = new Button();
        lblLatestVersion = new Label();
        lblCurrentVersion = new Label();
        lblDownloadPath = new Label();
        txtDownloadPath = new TextBox();
        btnDownload = new Button();
        btnInstall = new Button();
        progressBar = new ProgressBar();
        lblStatus = new Label();
        SuspendLayout();
        // 
        // lblManifest
        // 
        lblManifest.AutoSize = true;
        lblManifest.Location = new Point(12, 15);
        lblManifest.Name = "lblManifest";
        lblManifest.Size = new Size(130, 15);
        lblManifest.TabIndex = 0;
        lblManifest.Text = "Manifest JSON URL:";
        // 
        // txtManifestUrl
        // 
        txtManifestUrl.Location = new Point(148, 12);
        txtManifestUrl.Name = "txtManifestUrl";
        txtManifestUrl.Size = new Size(490, 23);
        txtManifestUrl.TabIndex = 1;
        // 
        // btnCheckUpdate
        // 
        btnCheckUpdate.Location = new Point(644, 11);
        btnCheckUpdate.Name = "btnCheckUpdate";
        btnCheckUpdate.Size = new Size(116, 25);
        btnCheckUpdate.TabIndex = 2;
        btnCheckUpdate.Text = "Check Update";
        btnCheckUpdate.UseVisualStyleBackColor = true;
        btnCheckUpdate.Click += btnCheckUpdate_Click;
        // 
        // lblLatestVersion
        // 
        lblLatestVersion.AutoSize = true;
        lblLatestVersion.Location = new Point(12, 54);
        lblLatestVersion.Name = "lblLatestVersion";
        lblLatestVersion.Size = new Size(128, 15);
        lblLatestVersion.TabIndex = 3;
        lblLatestVersion.Text = "Latest version: (none)";
        // 
        // lblCurrentVersion
        // 
        lblCurrentVersion.AutoSize = true;
        lblCurrentVersion.Location = new Point(12, 78);
        lblCurrentVersion.Name = "lblCurrentVersion";
        lblCurrentVersion.Size = new Size(173, 15);
        lblCurrentVersion.TabIndex = 4;
        lblCurrentVersion.Text = "Current app version: 1.0.0.0";
        // 
        // lblDownloadPath
        // 
        lblDownloadPath.AutoSize = true;
        lblDownloadPath.Location = new Point(12, 114);
        lblDownloadPath.Name = "lblDownloadPath";
        lblDownloadPath.Size = new Size(94, 15);
        lblDownloadPath.TabIndex = 5;
        lblDownloadPath.Text = "Download path:";
        // 
        // txtDownloadPath
        // 
        txtDownloadPath.Location = new Point(148, 111);
        txtDownloadPath.Name = "txtDownloadPath";
        txtDownloadPath.Size = new Size(612, 23);
        txtDownloadPath.TabIndex = 6;
        // 
        // btnDownload
        // 
        btnDownload.Enabled = false;
        btnDownload.Location = new Point(12, 152);
        btnDownload.Name = "btnDownload";
        btnDownload.Size = new Size(116, 30);
        btnDownload.TabIndex = 7;
        btnDownload.Text = "Download";
        btnDownload.UseVisualStyleBackColor = true;
        btnDownload.Click += btnDownload_Click;
        // 
        // btnInstall
        // 
        btnInstall.Enabled = false;
        btnInstall.Location = new Point(134, 152);
        btnInstall.Name = "btnInstall";
        btnInstall.Size = new Size(116, 30);
        btnInstall.TabIndex = 8;
        btnInstall.Text = "Install";
        btnInstall.UseVisualStyleBackColor = true;
        btnInstall.Click += btnInstall_Click;
        // 
        // progressBar
        // 
        progressBar.Location = new Point(12, 198);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(748, 23);
        progressBar.TabIndex = 9;
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(12, 233);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(71, 15);
        lblStatus.TabIndex = 10;
        lblStatus.Text = "Status: Idle";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(772, 266);
        Controls.Add(lblStatus);
        Controls.Add(progressBar);
        Controls.Add(btnInstall);
        Controls.Add(btnDownload);
        Controls.Add(txtDownloadPath);
        Controls.Add(lblDownloadPath);
        Controls.Add(lblCurrentVersion);
        Controls.Add(lblLatestVersion);
        Controls.Add(btnCheckUpdate);
        Controls.Add(txtManifestUrl);
        Controls.Add(lblManifest);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Windows OTA Updater";
        ResumeLayout(false);
        PerformLayout();
    }
}
