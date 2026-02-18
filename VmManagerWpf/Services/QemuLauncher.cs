using System.Diagnostics;
using System.Text;
using VmManagerWpf.Models;

namespace VmManagerWpf.Services;

public class QemuLauncher
{
    private readonly string _qemuPath;

    public QemuLauncher(string qemuPath)
    {
        _qemuPath = qemuPath;
    }

    public string BuildArguments(VmConfig vm)
    {
        // Build an optimized default command line with KVM-equivalent acceleration on Windows.
        var args = new StringBuilder();
        args.Append($"-machine {vm.Machine},accel=whpx ");
        args.Append($"-m {vm.RamMb} -smp {vm.CpuCores} ");
        args.Append("-cpu host -display sdl ");
        args.Append($"-drive file=\"{vm.DiskPath}\",if=virtio,format=qcow2 ");

        if (!vm.KernelTesting.Enabled)
        {
            args.Append($"-cdrom \"{vm.IsoPath}\" -boot order=d ");
        }
        else
        {
            // Kernel Testing Mode: boot directly from custom kernel + initramfs.
            args.Append($"-kernel \"{vm.KernelTesting.KernelPath}\" ");

            if (!string.IsNullOrWhiteSpace(vm.KernelTesting.InitramfsPath))
            {
                args.Append($"-initrd \"{vm.KernelTesting.InitramfsPath}\" ");
            }

            args.Append($"-append \"{vm.KernelTesting.KernelCmdline}\" ");
        }

        args.Append("-netdev user,id=n1 -device virtio-net-pci,netdev=n1 ");
        args.Append("-usb -device usb-tablet");

        return args.ToString().Trim();
    }

    public void Launch(VmConfig vm)
    {
        var psi = new ProcessStartInfo
        {
            FileName = _qemuPath,
            Arguments = BuildArguments(vm),
            UseShellExecute = false
        };

        Process.Start(psi);
    }

    public void CreateDisk(string qemuImgPath, string diskPath, int sizeGb)
    {
        var psi = new ProcessStartInfo
        {
            FileName = qemuImgPath,
            Arguments = $"create -f qcow2 \"{diskPath}\" {sizeGb}G",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Unable to run qemu-img.");
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var stderr = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"qemu-img failed: {stderr}");
        }
    }
}
