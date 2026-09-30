# Building and troubleshooting

ProcessWatcher is a Windows desktop application targeting **.NET 8** and **Windows x64**. The project file uses `net8.0-windows`, Windows Forms, a `win-x64` runtime identifier, and self-contained single-file publishing.

## Prerequisites

Use Windows 10 or Windows 11 with the .NET 8 SDK installed.

Verify the active SDK before building:

```powershell
dotnet --version
dotnet --info
```

The first command should report an 8.x SDK. If multiple SDKs are installed, `dotnet --info` shows which SDK and base path are currently selected.

## Clean local build

From the repository root:

```powershell
dotnet clean
dotnet restore
dotnet build -c Release
```

A successful build confirms that the project can restore and compile against the Windows desktop target.

## Reproduce the release publish

The project is configured for a self-contained Windows x64 single-file build. Reproduce it with:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The published output is written under:

```text
bin\Release\net8.0-windows\win-x64\publish\
```

Because the build is self-contained, the published application should not require a separate .NET runtime installation on the target machine.

## Common build problems

### The active SDK is not .NET 8

Check:

```powershell
dotnet --list-sdks
dotnet --version
```

Install or select a .NET 8 SDK, then run restore again.

### Windows desktop targeting fails

ProcessWatcher uses Windows Forms and targets `net8.0-windows`. Build it on Windows with a .NET 8 SDK that includes the Windows desktop targeting packs.

A clean restore often helps after changing SDKs:

```powershell
dotnet clean
dotnet nuget locals all --clear
dotnet restore
```

### A stale publish directory behaves differently from the current source

Delete the old Release output and republish:

```powershell
Remove-Item -Recurse -Force .\bin\Release -ErrorAction SilentlyContinue
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

### The executable is locked during rebuild or publish

Close any running ProcessWatcher instance before rebuilding. Windows cannot replace a binary that is still locked by a running process.

## Pre-PR verification

Before opening a pull request that changes application code:

1. Run `dotnet restore`.
2. Run `dotnet build -c Release`.
3. Reproduce the self-contained `win-x64` publish when packaging or startup behavior is affected.
4. Launch the resulting build and verify the changed behavior on Windows.
5. Keep generated `bin/` and `obj/` output out of commits.
