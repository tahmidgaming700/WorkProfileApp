# Windows OTA Updater (WinForms)

A complete Windows OTA updater desktop app in C# that:

- Checks a remote JSON manifest for latest update metadata
- Downloads a `.msu` package
- Verifies SHA-256 checksum
- Installs with `wusa.exe`
- Requires administrator privileges
- Provides GUI with **Check Update**, **Download**, **Install**, and a progress bar
- Shows build transition (example: **26200.7080 → 26200.8080**)
- Fetches changelog text from a Microsoft URL and generates an AI-style summary in-app

## Important: browser vs desktop app

This project is a **Windows desktop app (WinForms)**, not a website.
You **cannot run it directly inside a web browser**.

What you can do with a browser:
- Host/open the manifest JSON URL in a browser to verify it is reachable.
- Host the `.msu` file on your web server/CDN.
- Open the Microsoft changelog page URL used by `changelogUrl`.

How to launch the updater itself:
- Build or publish the app to get `WindowsOtaUpdater.exe`.
- Run the `.exe` on Windows (it will request admin/UAC as needed).

## Manifest format

```json
{
  "version": "2026.08.0800",
  "currentBuild": "26200.7080",
  "targetBuild": "26200.8080",
  "msuUrl": "https://updates.example.com/windows11-kb9999999-x64.msu",
  "sha256": "D2A4B7D86FB2B07AAE6D4E2A3F2E1E8EF5F3D7DE2A1D13C93D15A7BF90A0C123",
  "description": "Security and quality update",
  "changelogUrl": "https://support.microsoft.com/windows"
}
```

## Build and run (Windows)

```powershell
cd WindowsOtaUpdater
dotnet restore
dotnet build -c Release
dotnet run -c Release
```

## Publish a single EXE (recommended)

```powershell
cd WindowsOtaUpdater
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

Output EXE path:

```text
WindowsOtaUpdater\bin\Release\net8.0-windows\win-x64\publish\WindowsOtaUpdater.exe
```

## Notes

- The app manifest is configured with `requireAdministrator`.
- On startup, the app also self-elevates with UAC prompt if needed.
- `wusa.exe` is launched with `/quiet /norestart`.
- The AI summary is an on-device heuristic summary of fetched changelog text.
