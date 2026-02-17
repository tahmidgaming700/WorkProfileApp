# VM Manager WPF (.NET 8)

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
