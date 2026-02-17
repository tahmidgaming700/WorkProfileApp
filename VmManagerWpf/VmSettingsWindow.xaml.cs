using System.Windows;

namespace VmManagerWpf;

public partial class VmSettingsWindow : Window
{
    public string VmName { get; private set; }
    public int RamMb { get; private set; }
    public int CpuCores { get; private set; }
    public int DiskGb { get; private set; }

    public VmSettingsWindow(string vmName, int ramMb, int cpuCores, int diskGb)
    {
        InitializeComponent();
        VmNameTextBox.Text = vmName;
        RamTextBox.Text = ramMb.ToString();
        CpuTextBox.Text = cpuCores.ToString();
        DiskTextBox.Text = diskGb.ToString();
        VmName = vmName;
        RamMb = ramMb;
        CpuCores = cpuCores;
        DiskGb = diskGb;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(VmNameTextBox.Text)
            || !int.TryParse(RamTextBox.Text, out var ram)
            || !int.TryParse(CpuTextBox.Text, out var cpu)
            || !int.TryParse(DiskTextBox.Text, out var disk))
        {
            MessageBox.Show("Please enter valid values.");
            return;
        }

        VmName = VmNameTextBox.Text.Trim();
        RamMb = ram;
        CpuCores = cpu;
        DiskGb = disk;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
