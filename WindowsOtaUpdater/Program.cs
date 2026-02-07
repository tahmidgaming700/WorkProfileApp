using System.Diagnostics;
using System.Security.Principal;

namespace WindowsOtaUpdater;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        if (!IsAdministrator())
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath ?? "WindowsOtaUpdater.exe",
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(startInfo);
            }
            catch
            {
                MessageBox.Show(
                    "Administrator privileges are required to install Windows updates.",
                    "Elevation Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
