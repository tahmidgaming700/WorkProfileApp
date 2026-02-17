using System.Windows;
using Microsoft.Win32;
using VmManagerWpf.Models;

namespace VmManagerWpf;

public partial class KernelTestingWindow : Window
{
    public KernelTestingConfig Config { get; private set; }

    public KernelTestingWindow(KernelTestingConfig config)
    {
        InitializeComponent();
        Config = new KernelTestingConfig
        {
            Enabled = config.Enabled,
            KernelPath = config.KernelPath,
            InitramfsPath = config.InitramfsPath,
            KernelCmdline = config.KernelCmdline
        };

        EnabledCheckBox.IsChecked = Config.Enabled;
        KernelTextBox.Text = Config.KernelPath;
        InitramfsTextBox.Text = Config.InitramfsPath;
        CmdlineTextBox.Text = Config.KernelCmdline;
    }

    private void BrowseKernel_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Title = "Select Kernel", Filter = "Kernel image|*.*" };
        if (dlg.ShowDialog() == true)
        {
            KernelTextBox.Text = dlg.FileName;
        }
    }

    private void BrowseInitramfs_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Title = "Select Initramfs", Filter = "Initramfs|*.*" };
        if (dlg.ShowDialog() == true)
        {
            InitramfsTextBox.Text = dlg.FileName;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Config = new KernelTestingConfig
        {
            Enabled = EnabledCheckBox.IsChecked == true,
            KernelPath = KernelTextBox.Text.Trim(),
            InitramfsPath = InitramfsTextBox.Text.Trim(),
            KernelCmdline = CmdlineTextBox.Text.Trim()
        };

        if (Config.Enabled && string.IsNullOrWhiteSpace(Config.KernelPath))
        {
            MessageBox.Show("Kernel path is required when Kernel Testing Mode is enabled.");
            return;
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
