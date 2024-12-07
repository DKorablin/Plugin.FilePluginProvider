# Basic File Plugin Provider
This is a basic plugin loader, all hosts use this approach to find and load plugins in a local folder. But unlike built-in plugins, the current plugin uses the SAL_Path command line argument to look for plugins in a folder other than the root.

The character «;» is used as a delimiter for the array of paths used in the SAL_Path command line argument. If the SAL_Path command line argument is missing, the current directory where the host executable is launched is used to search for plugins.

After all plugins are loaded, the folder will be monitoring for new plugins, so that when new plugins appear in the folder, they will be loaded without having to restart the host.

## Warning

This plugin can only be used if the folders specified in the SAL_Path argument contain only plugins for the application. Because all other libraries will be loaded inside current AppDomain. (default folder for Windows Service is %windir%\system32).

For example, if copy of loaded version of SAL.dll will be found in folders, then all subsequent plugins will be referenced to last loaded SAL.dll, but not to the SAL.dll that is loaded by the host. As a result there will be DLL Hell. To avoid this problem, I recommend using a provider with additional assembly checks — File Domain Plugin Provider