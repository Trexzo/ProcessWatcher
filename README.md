# ProcessWatcher

[![Build](https://github.com/Trexzo/ProcessWatcher/actions/workflows/build.yml/badge.svg)](https://github.com/Trexzo/ProcessWatcher/actions/workflows/build.yml)
[![Release](https://img.shields.io/github/v/release/Trexzo/ProcessWatcher?include_prereleases)](https://github.com/Trexzo/ProcessWatcher/releases)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6?logo=windows&logoColor=white)

A lightweight Windows process and performance monitor built with **C#**, **.NET 8**, and **WinForms**.

ProcessWatcher provides real-time system metrics, process inspection, search and sorting, and a custom dark/cyan interface without third-party UI packages.

## Features

- Real-time **CPU**, **memory**, **disk**, **network**, and **uptime** metrics
- Live sparklines and usage bars
- Search processes by **name**, **PID**, or **window title**
- Sort by CPU usage, memory usage, name, or PID
- Per-process CPU usage, memory usage, PID, and thread count
- Selected-process details panel
- Open a process executable location from the UI
- Copy process PID
- Configurable refresh interval: 500 ms, 1 s, 2 s, or 5 s
- Custom rounded panels, buttons, text box, dropdowns, and scrollbar
- Dark Windows title bar on supported versions of Windows
- No third-party NuGet dependencies
- GitHub Actions CI and automated tagged releases

## Download

Download the latest Windows build from the [Releases](https://github.com/Trexzo/ProcessWatcher/releases) page.

Release builds are self-contained, so a separate .NET runtime installation is not required.

Current release target:

- Windows x64
- Single-file self-contained executable

## Run from source

Requirements:

- Windows 10 or Windows 11
- .NET 8 SDK
- Git

```powershell
git clone https://github.com/Trexzo/ProcessWatcher.git
cd ProcessWatcher
dotnet restore
dotnet build
dotnet run
```

## Publish locally

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The published executable is written under:

```text
bin\Release\net8.0-windows\win-x64\publish\
```

## Storage units

Disk capacity is displayed using decimal units (`GB`, `TB`, base 1000), matching how drive manufacturers advertise storage capacity.

RAM and process memory use binary units (`MiB`, `GiB`, base 1024).

For example, a nominal 1 TB drive is roughly 931 GiB. A value around 960 GB used therefore corresponds to roughly 96% usage on a 1 TB drive.

## Project structure

```text
ProcessWatcher/
├── .github/workflows/       # CI and automated releases
├── MainForm.cs              # Main dashboard and process UI
├── SystemSampler.cs         # CPU, memory, disk, network, uptime metrics
├── ProcessSampler.cs        # Per-process metrics
├── NativeMethods.cs         # Windows API interop
├── MetricCard.cs            # Dashboard metric cards
├── SparklineControl.cs      # Live metric graph
├── UsageBar.cs              # Metric progress indicator
├── RoundedPanel.cs          # Custom rounded container
├── ModernButton.cs          # Custom button
├── ModernDropDown.cs        # Custom dropdown
├── ModernScrollBar.cs       # Custom scrollbar
├── ModernTextBox.cs         # Custom search box
├── Theme.cs                 # Dark/cyan color system
├── Program.cs
├── app.manifest
└── ProcessWatcher.csproj
```

## Releases

Version tags matching `v*` trigger the release workflow automatically.

Example:

```powershell
git tag -a v0.3.2 -m "ProcessWatcher v0.3.2"
git push origin v0.3.2
```

The workflow publishes:

```text
ProcessWatcher.exe
ProcessWatcher-win-x64.zip
```

## Notes

Some protected or elevated processes may expose limited metadata unless ProcessWatcher is running with matching permissions.

## License

ProcessWatcher is released under the [MIT License](LICENSE).
