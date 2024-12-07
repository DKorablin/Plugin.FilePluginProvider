using System.Reflection;
using System.Runtime.InteropServices;

[assembly: Guid("31541a27-5c01-4458-9a7d-90d1935e6e09")]
[assembly: ComVisible(false)]
[assembly: System.CLSCompliant(true)]

#if NETSTANDARD || NETCOREAPP
[assembly: AssemblyMetadata("ProjectUrl", "https://dkorablin.ru/project/Default.aspx?File=104")]
#else
[assembly: AssemblyTitle("Plugin.FilePluginProvider")]
[assembly: AssemblyProduct("Plugin loader from file system")]
[assembly: AssemblyCompany("Danila Korablin")]
[assembly: AssemblyCopyright("Copyright © Danila Korablin 2010-2024")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

#endif