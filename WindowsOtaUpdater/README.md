# Windows OTA Updater (WinForms)

A complete Windows OTA updater desktop app in C# that:

- Checks a remote JSON manifest for latest update metadata
- Downloads a `.msu` package
- Verifies SHA-256 checksum
- Installs with `wusa.exe`
- Requires administrator privileges
- Provides GUI with **Check Update**, **Download**, **Install**, and a progress bar

## Manifest format

```json
{
  "version": "2026.02.01",
  "msuUrl": "https://updates.example.com/windows10-kb5034123-x64.msu",
  "sha256": "D2A4B7D86FB2B07AAE6D4E2A3F2E1E8EF5F3D7DE2A1D13C93D15A7BF90A0C123",
  "description": "Optional text"
}
```

## Build and run

```powershell
cd WindowsOtaUpdater
dotnet restore
dotnet build -c Release
dotnet run -c Release
```

## Notes

- The app manifest is configured with `requireAdministrator`.
- On startup, the app also self-elevates with UAC prompt if needed.
- `wusa.exe` is launched with `/quiet /norestart`.
