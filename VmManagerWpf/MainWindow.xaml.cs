using System.IO;
using System.Windows;
using Microsoft.Win32;
using VmManagerWpf.Models;
using VmManagerWpf.Services;

namespace VmManagerWpf;

public partial class MainWindow : Window
{
    private readonly string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
    private readonly string _isoDir;
    private readonly string _vmDir;
    private readonly string _diskDir;
    private readonly TemplateManager _templateManager;
    private readonly VmConfigService _vmConfigService;
    private readonly IsoDownloader _isoDownloader;
    private readonly QemuLauncher _qemuLauncher;

    private TemplateCatalog _catalog = new();

    public MainWindow()
    {
        InitializeComponent();

        _isoDir = Path.Combine(_baseDir, "isos");
        _vmDir = Path.Combine(_baseDir, "vms");
        _diskDir = Path.Combine(_baseDir, "disks");
        Directory.CreateDirectory(_isoDir);
        Directory.CreateDirectory(_vmDir);
        Directory.CreateDirectory(_diskDir);

        _templateManager = new TemplateManager(Path.Combine(_baseDir, "Catalog", "template-catalog.json"));
        _vmConfigService = new VmConfigService(_vmDir);
        _isoDownloader = new IsoDownloader();
        _qemuLauncher = new QemuLauncher(Path.Combine(_baseDir, "tools", "qemu-system-x86_64.exe"));

        LoadData();
    }

    private void LoadData()
    {
        _catalog = _templateManager.LoadCatalog();
        TemplatesListBox.ItemsSource = _catalog.Templates;
        VmsDataGrid.ItemsSource = _vmConfigService.LoadAll();
    }

    private async void CreateVmFromTemplate_Click(object sender, RoutedEventArgs e)
    {
        if (TemplatesListBox.SelectedItem is not VmTemplate template)
        {
            MessageBox.Show("Select a template first.");
            return;
        }

        var vmName = $"{template.Id}-{DateTime.Now:yyyyMMdd-HHmmss}";
        var settingsWindow = new VmSettingsWindow(vmName, template.DefaultRamMb, template.DefaultCpuCores, template.DefaultDiskGb);

        if (settingsWindow.ShowDialog() != true)
        {
            return;
        }

        try
        {
            // Download ISO if required and verify checksum before VM creation.
            var isoPath = await _isoDownloader.EnsureIsoAsync(template, _isoDir);
            var diskPath = Path.Combine(_diskDir, $"{settingsWindow.VmName}.qcow2");
            var qemuImg = Path.Combine(_baseDir, "tools", "qemu-img.exe");

            // One-click disk creation for template VM.
            _qemuLauncher.CreateDisk(qemuImg, diskPath, settingsWindow.DiskGb);

            var vm = new VmConfig
            {
                Name = settingsWindow.VmName,
                TemplateId = template.Id,
                IsoPath = isoPath,
                DiskPath = diskPath,
                RamMb = settingsWindow.RamMb,
                CpuCores = settingsWindow.CpuCores,
                Machine = "q35",
                EnableUefi = false,
                KernelTesting = new KernelTestingConfig()
            };

            _vmConfigService.Save(vm);
            LoadData();

            if (MessageBox.Show("VM created. Launch now?", "Success", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _qemuLauncher.Launch(vm);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create VM: {ex.Message}");
        }
    }

    private void LaunchVm_Click(object sender, RoutedEventArgs e)
    {
        if (VmsDataGrid.SelectedItem is not VmConfig vm)
        {
            MessageBox.Show("Select a VM first.");
            return;
        }

        try
        {
            _qemuLauncher.Launch(vm);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to launch VM: {ex.Message}");
        }
    }

    private void EditVm_Click(object sender, RoutedEventArgs e)
    {
        if (VmsDataGrid.SelectedItem is not VmConfig vm)
        {
            MessageBox.Show("Select a VM first.");
            return;
        }

        var dialog = new VmSettingsWindow(vm.Name, vm.RamMb, vm.CpuCores, ParseDiskGb(vm.DiskPath));
        if (dialog.ShowDialog() == true)
        {
            vm.Name = dialog.VmName;
            vm.RamMb = dialog.RamMb;
            vm.CpuCores = dialog.CpuCores;
            _vmConfigService.Save(vm);
            LoadData();
        }
    }

    private void SelectIso_Click(object sender, RoutedEventArgs e)
    {
        if (VmsDataGrid.SelectedItem is not VmConfig vm)
        {
            MessageBox.Show("Select a VM first.");
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = "Select ISO",
            Filter = "ISO files (*.iso)|*.iso|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            vm.IsoPath = dialog.FileName;
            _vmConfigService.Save(vm);
            LoadData();
        }
    }

    private void KernelTesting_Click(object sender, RoutedEventArgs e)
    {
        if (VmsDataGrid.SelectedItem is not VmConfig vm)
        {
            MessageBox.Show("Select a VM first.");
            return;
        }

        var dialog = new KernelTestingWindow(vm.KernelTesting);
        if (dialog.ShowDialog() == true)
        {
            vm.KernelTesting = dialog.Config;
            _vmConfigService.Save(vm);
            var generated = _qemuLauncher.BuildArguments(vm);
            MessageBox.Show($"Generated command args:\n{generated}", "Kernel Testing Mode");
            LoadData();
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadData();
    }

    private static int ParseDiskGb(string diskPath)
    {
        if (!File.Exists(diskPath))
        {
            return 20;
        }

        var gb = new FileInfo(diskPath).Length / 1024d / 1024d / 1024d;
        return Math.Max((int)Math.Ceiling(gb), 1);
    }
}
