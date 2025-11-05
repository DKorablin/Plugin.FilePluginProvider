# File Plugin Provider

[![Auto build](https://github.com/DKorablin/Plugin.FilePluginProvider/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/Plugin.FilePluginProvider/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A basic plugin provider for the SAL (Software Abstraction Layer) framework that loads plugins from the file system with automatic monitoring for new plugins.

## Overview

This plugin provider enables dynamic plugin loading from one or more directories. It monitors the specified folders and automatically loads new plugins when they appear, eliminating the need to restart the host application.

Unlike built-in plugin providers that search in the application's root directory, this provider uses the `SAL_Path` configuration to specify custom plugin locations.

## Features

- ✅ **Dynamic Plugin Loading** - Load plugins from custom directories
- ✅ **Hot Reload Support** - Automatically detect and load new plugins without restart
- ✅ **Multi-Path Support** - Search for plugins in multiple directories
- ✅ **Flexible Configuration** - Configure via command-line arguments or app settings
- ✅ **Assembly Resolution** - Resolve dependencies from plugin directories
- ✅ **Diagnostic Tracing** - Built-in trace logging for troubleshooting
- ✅ **Multi-Framework Support** - Targets .NET Framework 3.5 and .NET Standard 2.0

## Installation
To install the File Plugin Provider Plugin, follow these steps:
1. Download the latest release from the [Releases](https://github.com/DKorablin/Plugin.Winlogon/releases)
2. Extract the downloaded ZIP file to a desired location.
3. Use the provided [Flatbed.Dialog (Lite)](https://dkorablin.github.io/Flatbed-Dialog-Lite) executable or download one of the supported host applications:
	- [Flatbed.Dialog](https://dkorablin.github.io/Flatbed-Dialog)
	- [Flatbed.MDI](https://dkorablin.github.io/Flatbed-MDI)
	- [Flatbed.MDI (WPF)](https://dkorablin.github.io/Flatbed-MDI-Avalon)
	- [Flatbed.WorkerService](https://dkorablin.github.io/Flatbed-WorkerService)

## Configuration

The plugin provider can be configured in two ways:

### 1. Command-Line Arguments

Specify plugin directories using the `/SAL_Path:` argument:

```bash
# Single directory
YourApp.exe /SAL_Path:C:\Plugins

# Multiple directories (use | as delimiter)
YourApp.exe /SAL_Path:C:\Plugins|D:\AdditionalPlugins|E:\ThirdPartyPlugins
```

### 2. Application Configuration File

Add the `SAL_Path` key to your `app.config` or `web.config`:

```xml
<configuration>
  <appSettings>
    <add key="SAL_Path" value="C:\Plugins|D:\AdditionalPlugins" />
  </appSettings>
</configuration>
```

### 3. Default Behavior

If no configuration is provided, the provider searches for plugins in:
1. The current directory (`Environment.CurrentDirectory`)
2. The assembly location directory (as a fallback)

## Usage Example

```csharp
using SAL.Flatbed;
using Plugin.FilePluginProvider;

// The plugin provider is loaded by the SAL host automatically
// when present in the plugin directory

public class MyHost : IHost
{
    public void Initialize()
    {
        // Plugins from configured paths will be automatically discovered
        // and loaded by the FilePluginProvider
    }
}
```

## How It Works

1. **Initialization**: The provider reads the `SAL_Path` configuration from command-line arguments or app settings
2. **Initial Scan**: All `.dll` files in the specified directories are loaded
3. **Monitoring**: FileSystemWatcher instances are created for each directory
4. **Hot Loading**: When new `.dll` files appear, they are automatically loaded
5. **Assembly Resolution**: When dependencies are needed, the provider searches plugin directories

## Supported File Types

Currently, only `.dll` files are loaded as plugins. The provider checks file extensions before attempting to load assemblies.

## ⚠️ Important Warnings

### AppDomain Loading

**This plugin loads ALL assemblies found in the specified directories into the current AppDomain.**

- ✅ **DO**: Use dedicated directories that contain only plugin assemblies
- ❌ **DON'T**: Point to system directories (e.g., `%windir%\system32`) or shared library folders

### DLL Hell Risk

If a plugin directory contains a different version of an already-loaded assembly (like `SAL.dll`), subsequent plugins may reference the wrong version, leading to **DLL Hell**.

**Example Problem Scenario:**
```
Host loads:         SAL.dll v1.0.0
Plugin dir has:     SAL.dll v1.1.0
Result:             New plugins reference v1.1.0, causing version conflicts
```

### Recommended Alternative

For safer plugin loading with assembly version checks and isolation, use the [File Domain Plugin Provider](https://github.com/DKorablin/Plugin.FileDomainPluginProvider) instead, which loads plugins in separate AppDomains.

## Technical Details

### Target Frameworks
- .NET Framework 3.5
- .NET Standard 2.0

### Dependencies
- **SAL.Flatbed** - Core SAL framework interfaces
- **System.Configuration.ConfigurationManager** (.NET Standard 2.0 only)

### Interfaces Implemented
- `IPluginProvider` - Core plugin provider interface
- `IPlugin` - Plugin lifecycle management

### Key Components

| Component | Description |
|-----------|-------------|
| `Plugin` | Main plugin provider implementation |
| `FilePluginArgs` | Configuration parser for `SAL_Path` |
| `FileSystemWatcher` | Monitors directories for new plugins |

## Diagnostics

The provider includes built-in diagnostic tracing. Configure trace listeners in your `app.config`:

```xml
<system.diagnostics>
  <sources>
    <source name="Plugin.FilePluginProvider" switchValue="All">
      <listeners>
        <add name="console" />
      </listeners>
    </source>
  </sources>
  <sharedListeners>
    <add name="console" type="System.Diagnostics.ConsoleTraceListener" />
  </sharedListeners>
</system.diagnostics>
```

## Events and Lifecycle

### OnConnection
Called when the plugin provider is loaded. Initializes configuration and file system watchers.

### OnDisconnection
Called when shutting down. **Note**: User-initiated unload is not supported and will throw `NotSupportedException`.

### LoadPlugins
Scans all configured directories and loads available plugins.

### ResolveAssembly
Resolves assembly dependencies from plugin directories when requested by the host.

## Troubleshooting

### Plugins Not Loading
- Verify the `SAL_Path` configuration is correct
- Check that plugin files are `.dll` extension
- Review trace logs for `BadImageFormatException` errors
- Ensure plugins implement the required SAL interfaces

### Assembly Not Found
- Verify dependencies are in the same directory as the plugin
- Check that the plugin directories are included in `SAL_Path`
- Review trace logs for assembly resolution failures

### Version Conflicts
- Ensure only one version of each assembly exists in plugin directories
- Consider using File Domain Plugin Provider for better isolation

## Building from Source

```bash
# Clone the repository
git clone https://github.com/DKorablin/Plugin.FilePluginProvider.git
cd Plugin.FilePluginProvider

# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Run tests (if available)
dotnet test
```

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## Related Projects

- **SAL.Flatbed** - Core SAL framework
- **File Domain Plugin Provider** - Enhanced plugin provider with AppDomain isolation