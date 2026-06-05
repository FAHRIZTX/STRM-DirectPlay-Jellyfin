# Build Instructions

## Prerequisites

- .NET 9.0 SDK
- Git

## Windows

```powershell
# Clone repository
git clone https://github.com/FAHRIZTX/STRM-DirectPlay-Jellyfin.git
cd STRM-DirectPlay-Jellyfin

# Restore dependencies
dotnet restore

# Build Release
dotnet build -c Release

# Output location
# Jellyfin.Plugin.StrmDirectPlay\bin\Release\net9.0\
```

## Linux/macOS

```bash
# Clone repository
git clone https://github.com/FAHRIZTX/STRM-DirectPlay-Jellyfin.git
cd STRM-DirectPlay-Jellyfin

# Restore dependencies
dotnet restore

# Build Release
dotnet build -c Release

# Output location
# Jellyfin.Plugin.StrmDirectPlay/bin/Release/net9.0/
```

## Create Release Package

```powershell
# Build
dotnet build -c Release

# Create plugins folder structure
$pluginDir = "release/plugins/Jellyfin.Plugin.StrmDirectPlay"
New-Item -ItemType Directory -Path $pluginDir -Force

# Copy DLL and dependencies
Copy-Item "Jellyfin.Plugin.StrmDirectPlay/bin/Release/net9.0/Jellyfin.Plugin.StrmDirectPlay.dll" $pluginDir

# Create ZIP
Compress-Archive -Path "release/plugins/*" -DestinationPath "STRM-DirectPlay-Jellyfin_1.0.0.0.zip"
```

## Installation

Copy plugin folder to Jellyfin plugins directory:

- **Windows**: `%AppData%\Jellyfin\Server\plugins\`
- **Linux**: `/var/lib/jellyfin/plugins/`
- **Docker**: `/config/plugins/`

Then restart Jellyfin.

## Development Build

For development with hot reload:

```bash
dotnet watch build
```

## Clean Build

```bash
dotnet clean
dotnet build -c Release
```
