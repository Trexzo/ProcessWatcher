# ProcessWatcher

A lightweight Windows process and performance monitor built with **C#**, **.NET 8**, and **WinForms**.

ProcessWatcher uses a custom dark/cyan control set instead of default WinForms combo boxes, buttons, borders, and scrollbars, giving the application a cleaner modern Windows look without third-party UI packages.

## Highlights

- Real-time CPU, memory, disk, network, and uptime metrics
- Live sparklines and usage bars
- Search by process name, PID, or window title
- Sort by CPU, memory, process name, or PID
- Per-process CPU, memory, PID, and thread count
- Selected-process detail panel
- Open executable location and copy PID
- Configurable refresh interval
- Custom rounded panels, buttons, text box, dropdowns, and scrollbar
- Disk capacity shown in decimal GB/TB, matching drive manufacturer capacity
- RAM/process memory shown in binary MiB/GiB
- GitHub Actions CI and tagged releases
- No third-party NuGet dependencies

## Storage units

Disk capacity is displayed using decimal units (`GB`, `TB`, base 1000), which is how storage manufacturers advertise drive size. RAM and process memory use binary units (`MiB`, `GiB`, base 1024).

For example, a nominal 1 TB drive is roughly 931 GiB. A reading such as 894 GiB used is about 960 GB used, which is approximately 96% of a 1 TB drive.

## Run from source

```powershell
dotnet restore
dotnet build
dotnet run
```

## Publish

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
