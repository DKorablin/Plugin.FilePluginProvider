using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SAL.Flatbed;

namespace Plugin.FilePluginProvider
{
	/// <summary>Loading plugins from file system</summary>
	/// <remarks>11.07.2011 - Added file system monitoring support</remarks>
	public class Plugin : IPluginProvider
	{
		private readonly IHost _host;

		private TraceSource _trace;

		/// <summary>Arguments transferred from source application</summary>
		private FilePluginArgs _args;

		/// <summary>New plugins apperance monitor</summary>
		private List<FileSystemWatcher> _monitors;

		private TraceSource Trace { get => this._trace ?? (this._trace = Plugin.CreateTraceSource<Plugin>()); }

		/// <summary>Parent plugin provider</summary>
		IPluginProvider IPluginProvider.ParentProvider { get; set; }

		/// <summary>Create instance of <see cref="Plugin"/> with reference to the host instance.</summary>
		/// <param name="host">The host instance.</param>
		/// <exception cref="ArgumentNullException">The host instanse should be specified.</exception>
		public Plugin(IHost host)
			=> this._host = host ?? throw new ArgumentNullException(nameof(host));

		Boolean IPlugin.OnConnection(ConnectMode mode)
		{
			this._args = new FilePluginArgs();
			this._monitors = new List<FileSystemWatcher>();
			return true;
		}

		Boolean IPlugin.OnDisconnection(DisconnectMode mode)
		{
			if(mode == DisconnectMode.UserClosed)
				throw new NotSupportedException("Plugin Provider can't be unloaded");
			else
			{
				if(this._monitors != null)
				{
					foreach(FileSystemWatcher monitor in this._monitors)
						monitor.Dispose();
					this._monitors = null;
				}
				return true;
			}
		}

		void IPluginProvider.LoadPlugins()
		{
			//AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			/*if(String.IsNullOrEmpty(pluginPath))
				pluginPath = AppDomain.CurrentDomain.BaseDirectory;*/

			//System.Diagnostics.Debugger.Launch();
			foreach(String pluginPath in this._args.PluginPath)
				if(Directory.Exists(pluginPath))
				{
					foreach(String file in Directory.GetFiles(pluginPath))
						this.LoadPlugin(file, ConnectMode.Startup);

					foreach(String extension in FilePluginArgs.LibraryExtensions)
					{
						FileSystemWatcher watcher = new FileSystemWatcher(pluginPath, extension);
						watcher.Changed += new FileSystemEventHandler(Monitor_Changed);
						watcher.EnableRaisingEvents = true;
						this._monitors.Add(watcher);
					}
				}
		}

		Assembly IPluginProvider.ResolveAssembly(String assemblyName)
		{
			if(String.IsNullOrEmpty(assemblyName))
				throw new ArgumentNullException(nameof(assemblyName));

			AssemblyName targetName = new AssemblyName(assemblyName);
			foreach(String pluginPath in this._args.PluginPath)
				if(Directory.Exists(pluginPath))
					foreach(String file in Directory.GetFiles(pluginPath, "*.*", SearchOption.AllDirectories))//Поиск только файлов с расширением .dll
						if(this._args.CheckFileExtension(file))
						try
						{
							AssemblyName name = AssemblyName.GetAssemblyName(file);
							if(name.FullName == targetName.FullName)
								return Assembly.LoadFile(file);
							//return assembly;//TODO: Reference DLL из оперативной памяти не цепляются!
						} catch(BadImageFormatException)
						{
							continue;
						} catch(FileLoadException)
						{
							continue;
						} catch(Exception exc)
						{
							exc.Data.Add("Library", file);
							this.Trace.TraceData(TraceEventType.Error, 1, exc);
						}

			String errorMessage = $"The provider {this.GetType()} is unable to locate the assembly {assemblyName} in the path {String.Join(",", this._args.PluginPath)}";
			this.Trace.TraceEvent(TraceEventType.Warning, 5, errorMessage);
			IPluginProvider parentProvider = ((IPluginProvider)this).ParentProvider;
			return parentProvider?.ResolveAssembly(assemblyName);
		}

		/// <summary>Доступен новый файл для проверки</summary>
		/// <param name="sender">Отправитель события</param>
		/// <param name="e">Аргументы события</param>
		private void Monitor_Changed(Object sender, FileSystemEventArgs e)
		{
			if(e.ChangeType == WatcherChangeTypes.Changed)
				this.LoadPlugin(e.FullPath, ConnectMode.AfterStartup);
		}

		/// <summary>Загрузить плагин</summary>
		/// <param name="filePath">Пуь к файлу для загрузки</param>
		/// <param name="mode">The mode how plugin is connected.</param>
		private void LoadPlugin(String filePath, ConnectMode mode)
		{
			if(!this._args.CheckFileExtension(filePath))
				this.Trace.TraceInformation("Try to load file with unsupported extension. FilePath: {0}", filePath);
			else
				try
				{
					// Проверяем что плагин с таким источником ещё не загружен, если его уже загрузил родительский провайдер.
					// Загрузка из ФС так что источник должен быть по любому уникальный.
					foreach(IPluginDescription plugin in this._host.Plugins)
						if(filePath.Equals(plugin.Source, StringComparison.OrdinalIgnoreCase))
							return;

					Assembly asm = Assembly.LoadFile(filePath);
					this._host.Plugins.LoadPlugin(asm, filePath, mode);
				} catch(BadImageFormatException exc)//Ошибка загрузки плагина. Можно почитать заголовок загружаемого файла, но мне влом
				{
					exc.Data.Add("Library", filePath);
					this.Trace.TraceData(TraceEventType.Error, 1, exc);
				} catch(Exception exc)
				{
					exc.Data.Add("Library", filePath);
					this.Trace.TraceData(TraceEventType.Error, 1, exc);
				}
		}

		private static TraceSource CreateTraceSource<T>(String name = null) where T : IPlugin
		{
			TraceSource result = new TraceSource(typeof(T).Assembly.GetName().Name + name);
			result.Switch.Level = SourceLevels.All;
			result.Listeners.Remove("Default");
			result.Listeners.AddRange(System.Diagnostics.Trace.Listeners);
			return result;
		}
	}
}