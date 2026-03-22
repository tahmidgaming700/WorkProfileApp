namespace WindowsOtaUpdater;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;

    private Panel pnlHeader;
    private Label lblTitle;
    private Label lblSubtitle;
    private Label lblManifest;
    private TextBox txtManifestUrl;
    private Button btnCheckUpdate;
    private Label lblCurrentVersion;
    private Label lblLatestVersion;
    private Label lblBuildRange;
    private Label lblDownloadPath;
    private TextBox txtDownloadPath;
    private Button btnDownload;
    private Button btnInstall;
    private ProgressBar progressBar;
    private Label lblStatus;
    private GroupBox grpChangelog;
    private RichTextBox txtChangelog;
    private GroupBox grpAiSummary;
    private RichTextBox txtAiSummary;

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
        pnlHeader = new Panel();
        lblTitle = new Label();
        lblSubtitle = new Label();
        lblManifest = new Label();
        txtManifestUrl = new TextBox();
        btnCheckUpdate = new Button();
        lblCurrentVersion = new Label();
        lblLatestVersion = new Label();
        lblBuildRange = new Label();
        lblDownloadPath = new Label();
        txtDownloadPath = new TextBox();
        btnDownload = new Button();
        btnInstall = new Button();
        progressBar = new ProgressBar();
        lblStatus = new Label();
        grpChangelog = new GroupBox();
        txtChangelog = new RichTextBox();
        grpAiSummary = new GroupBox();
        txtAiSummary = new RichTextBox();
        pnlHeader.SuspendLayout();
        grpChangelog.SuspendLayout();
        grpAiSummary.SuspendLayout();
        SuspendLayout();
        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = Color.FromArgb(0, 120, 215);
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(1100, 80);
        pnlHeader.TabIndex = 0;
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(16, 9);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(281, 32);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Windows OTA Updater";
        // 
        // lblSubtitle
        // 
        lblSubtitle.AutoSize = true;
        lblSubtitle.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        lblSubtitle.ForeColor = Color.WhiteSmoke;
        lblSubtitle.Location = new Point(18, 45);
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Size = new Size(530, 19);
        lblSubtitle.TabIndex = 1;
        lblSubtitle.Text = "Friendly updater with build tracking, Microsoft changelog preview, and AI summary.";
        // 
        // lblManifest
        // 
        lblManifest.AutoSize = true;
        lblManifest.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblManifest.Location = new Point(12, 95);
        lblManifest.Name = "lblManifest";
        lblManifest.Size = new Size(126, 15);
        lblManifest.TabIndex = 1;
        lblManifest.Text = "Manifest JSON URL";
        // 
        // txtManifestUrl
        // 
        txtManifestUrl.Location = new Point(144, 92);
        txtManifestUrl.Name = "txtManifestUrl";
        txtManifestUrl.Size = new Size(792, 23);
        txtManifestUrl.TabIndex = 2;
        // 
        // btnCheckUpdate
        // 
        btnCheckUpdate.BackColor = Color.FromArgb(0, 153, 188);
        btnCheckUpdate.FlatAppearance.BorderSize = 0;
        btnCheckUpdate.FlatStyle = FlatStyle.Flat;
        btnCheckUpdate.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
        btnCheckUpdate.ForeColor = Color.White;
        btnCheckUpdate.Location = new Point(942, 89);
        btnCheckUpdate.Name = "btnCheckUpdate";
        btnCheckUpdate.Size = new Size(145, 30);
        btnCheckUpdate.TabIndex = 3;
        btnCheckUpdate.Text = "Check Update";
        btnCheckUpdate.UseVisualStyleBackColor = false;
        btnCheckUpdate.Click += btnCheckUpdate_Click;
        // 
        // lblCurrentVersion
        // 
        lblCurrentVersion.AutoSize = true;
        lblCurrentVersion.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblCurrentVersion.Location = new Point(12, 130);
        lblCurrentVersion.Name = "lblCurrentVersion";
        lblCurrentVersion.Size = new Size(148, 15);
        lblCurrentVersion.TabIndex = 4;
        lblCurrentVersion.Text = "Current app version: -";
        // 
        // lblLatestVersion
        // 
        lblLatestVersion.AutoSize = true;
        lblLatestVersion.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblLatestVersion.Location = new Point(290, 130);
        lblLatestVersion.Name = "lblLatestVersion";
        lblLatestVersion.Size = new Size(109, 15);
        lblLatestVersion.TabIndex = 5;
        lblLatestVersion.Text = "Latest version: -";
        // 
        // lblBuildRange
        // 
        lblBuildRange.AutoSize = true;
        lblBuildRange.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblBuildRange.ForeColor = Color.FromArgb(78, 92, 105);
        lblBuildRange.Location = new Point(560, 130);
        lblBuildRange.Name = "lblBuildRange";
        lblBuildRange.Size = new Size(217, 15);
        lblBuildRange.TabIndex = 6;
        lblBuildRange.Text = "Build transition: 26200.7080 → 26200.8080";
        // 
        // lblDownloadPath
        // 
        lblDownloadPath.AutoSize = true;
        lblDownloadPath.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblDownloadPath.Location = new Point(12, 163);
        lblDownloadPath.Name = "lblDownloadPath";
        lblDownloadPath.Size = new Size(91, 15);
        lblDownloadPath.TabIndex = 7;
        lblDownloadPath.Text = "Download path";
        // 
        // txtDownloadPath
        // 
        txtDownloadPath.Location = new Point(144, 160);
        txtDownloadPath.Name = "txtDownloadPath";
        txtDownloadPath.Size = new Size(792, 23);
        txtDownloadPath.TabIndex = 8;
        // 
        // btnDownload
        // 
        btnDownload.BackColor = Color.FromArgb(16, 124, 16);
        btnDownload.Enabled = false;
        btnDownload.FlatAppearance.BorderSize = 0;
        btnDownload.FlatStyle = FlatStyle.Flat;
        btnDownload.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
        btnDownload.ForeColor = Color.White;
        btnDownload.Location = new Point(144, 196);
        btnDownload.Name = "btnDownload";
        btnDownload.Size = new Size(130, 32);
        btnDownload.TabIndex = 9;
        btnDownload.Text = "Download";
        btnDownload.UseVisualStyleBackColor = false;
        btnDownload.Click += btnDownload_Click;
        // 
        // btnInstall
        // 
        btnInstall.BackColor = Color.FromArgb(255, 140, 0);
        btnInstall.Enabled = false;
        btnInstall.FlatAppearance.BorderSize = 0;
        btnInstall.FlatStyle = FlatStyle.Flat;
        btnInstall.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
        btnInstall.ForeColor = Color.White;
        btnInstall.Location = new Point(280, 196);
        btnInstall.Name = "btnInstall";
        btnInstall.Size = new Size(130, 32);
        btnInstall.TabIndex = 10;
        btnInstall.Text = "Install";
        btnInstall.UseVisualStyleBackColor = false;
        btnInstall.Click += btnInstall_Click;
        // 
        // progressBar
        // 
        progressBar.Location = new Point(416, 201);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(671, 23);
        progressBar.Style = ProgressBarStyle.Continuous;
        progressBar.TabIndex = 11;
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
        lblStatus.ForeColor = Color.FromArgb(51, 51, 51);
        lblStatus.Location = new Point(12, 236);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(70, 17);
        lblStatus.TabIndex = 12;
        lblStatus.Text = "Status: Idle";
        // 
        // grpChangelog
        // 
        grpChangelog.Controls.Add(txtChangelog);
        grpChangelog.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        grpChangelog.Location = new Point(12, 265);
        grpChangelog.Name = "grpChangelog";
        grpChangelog.Size = new Size(536, 252);
        grpChangelog.TabIndex = 13;
        grpChangelog.TabStop = false;
        grpChangelog.Text = "Microsoft changelog";
        // 
        // txtChangelog
        // 
        txtChangelog.BackColor = Color.FromArgb(245, 250, 255);
        txtChangelog.BorderStyle = BorderStyle.None;
        txtChangelog.Dock = DockStyle.Fill;
        txtChangelog.Location = new Point(3, 19);
        txtChangelog.Name = "txtChangelog";
        txtChangelog.ReadOnly = true;
        txtChangelog.Size = new Size(530, 230);
        txtChangelog.TabIndex = 0;
        txtChangelog.Text = "Changelog will appear here after Check Update.";
        // 
        // grpAiSummary
        // 
        grpAiSummary.Controls.Add(txtAiSummary);
        grpAiSummary.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        grpAiSummary.Location = new Point(554, 265);
        grpAiSummary.Name = "grpAiSummary";
        grpAiSummary.Size = new Size(533, 252);
        grpAiSummary.TabIndex = 14;
        grpAiSummary.TabStop = false;
        grpAiSummary.Text = "AI feature: smart summary";
        // 
        // txtAiSummary
        // 
        txtAiSummary.BackColor = Color.FromArgb(250, 245, 255);
        txtAiSummary.BorderStyle = BorderStyle.None;
        txtAiSummary.Dock = DockStyle.Fill;
        txtAiSummary.Location = new Point(3, 19);
        txtAiSummary.Name = "txtAiSummary";
        txtAiSummary.ReadOnly = true;
        txtAiSummary.Size = new Size(527, 230);
        txtAiSummary.TabIndex = 0;
        txtAiSummary.Text = "AI summary will appear after changelog is fetched.";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(1099, 529);
        Controls.Add(grpAiSummary);
        Controls.Add(grpChangelog);
        Controls.Add(lblStatus);
        Controls.Add(progressBar);
        Controls.Add(btnInstall);
        Controls.Add(btnDownload);
        Controls.Add(txtDownloadPath);
        Controls.Add(lblDownloadPath);
        Controls.Add(lblBuildRange);
        Controls.Add(lblLatestVersion);
        Controls.Add(lblCurrentVersion);
        Controls.Add(btnCheckUpdate);
        Controls.Add(txtManifestUrl);
        Controls.Add(lblManifest);
        Controls.Add(pnlHeader);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Windows OTA Updater";
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        grpChangelog.ResumeLayout(false);
        grpAiSummary.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}
