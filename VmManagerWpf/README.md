# VM Manager WPF (.NET 8)

## How to download this project
```powershell
git clone https://github.com/<your-account>/<your-repo>.git
cd <your-repo>\VmManagerWpf
```

If you downloaded a ZIP archive instead:
1. Download the repository ZIP from GitHub.
2. Extract it.
3. Open the extracted `VmManagerWpf` folder.

## Build
```powershell
dotnet restore
dotnet build .\VmManagerWpf.csproj -c Release
```

## Run
```powershell
dotnet run --project .\VmManagerWpf.csproj
```

## Runtime prerequisites
- Place `qemu-system-x86_64.exe` and `qemu-img.exe` into `tools/` next to the app binary.
- Template catalog is stored at `Catalog/template-catalog.json`.
- VM configs are stored in `vms/`.
- Downloaded ISOs are stored in `isos/`.

## How ISO downloads work in the app
- Select a template (Ubuntu, Arch, Windows, Android-x86).
- Click **Create VM from Template**.
- The app checks `isos/` for the required ISO.
- If missing (or checksum mismatch), it downloads the ISO automatically and verifies SHA-256 when configured.
