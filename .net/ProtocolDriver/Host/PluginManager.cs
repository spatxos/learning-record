using Host.SDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Host
{
    /// <summary>
    /// 插件信息
    /// </summary>
    public class PluginInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ProtocolName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string AssemblyPath { get; set; } = string.Empty;
        public AssemblyLoadContext LoadContext { get; set; } = null!;
        public IDeviceCommunication? DeviceCommunication { get; set; } = null!;
        public PluginStatus Status { get; set; } = PluginStatus.Loading;
    }

    /// <summary>
    /// 插件状态
    /// </summary>
    public enum PluginStatus
    {
        Loading,
        Running,
        Draining,
        Unloading,
        Failed
    }

    /// <summary>
    /// 插件管理器，负责插件的生命周期管理
    /// </summary>
    public class PluginManager : IDisposable
    {
        private readonly ILogger<PluginManager> _logger;
        private readonly IConfiguration _config;
        private readonly ITransportFactory _transportFactory;
        private readonly IHostApi _hostApi;
        private readonly Dictionary<string, PluginInfo> _plugins = new();
        private readonly Dictionary<string, WeakReference> _unloadedContexts = new();
        private readonly string _pluginsDirectory;
        private FileSystemWatcher? _watcher;

        public PluginManager(
            ILogger<PluginManager> logger,
            IConfiguration config,
            ITransportFactory transportFactory,
            IHostApi hostApi)
        {
            _logger = logger;
            _config = config;
            _transportFactory = transportFactory;
            _hostApi = hostApi;
            _pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
        }

        /// <summary>
        /// 初始化插件管理器
        /// </summary>
        /// <returns>任务</returns>
        public async Task InitializeAsync()
        {
            _logger.LogInformation("PluginManager.InitializeAsync started");
            
            // 记录当前工作目录和插件目录
            _logger.LogInformation("Current working directory: {Directory}", Directory.GetCurrentDirectory());
            _logger.LogInformation("Plugins directory: {Directory}", _pluginsDirectory);

            // 创建插件目录
            Directory.CreateDirectory(_pluginsDirectory);
            _logger.LogInformation("Plugin directory created (if not existed)");

            // 扫描并加载插件
            await ScanAndLoadPluginsAsync();

            // 启动文件系统监听器
            StartFileSystemWatcher();
            
            _logger.LogInformation("PluginManager.InitializeAsync completed");
        }

        /// <summary>
        /// 扫描并加载插件
        /// </summary>
        /// <returns>任务</returns>
        private async Task ScanAndLoadPluginsAsync()
        {
            _logger.LogInformation("Scanning plugins directory: {Directory}", _pluginsDirectory);
            _logger.LogInformation("Current working directory: {Directory}", Directory.GetCurrentDirectory());

            if (!Directory.Exists(_pluginsDirectory))
            {
                _logger.LogError("Plugins directory does not exist: {Directory}", _pluginsDirectory);
                return;
            }

            // 列出插件目录中的所有文件和子目录
            var dirContents = Directory.GetFileSystemEntries(_pluginsDirectory);
            _logger.LogInformation("Plugins directory contents: {Count} items", dirContents.Length);
            foreach (var item in dirContents)
            {
                var attr = File.GetAttributes(item);
                _logger.LogInformation("{ItemType}: {Item}", attr.HasFlag(FileAttributes.Directory) ? "Directory" : "File", item);
            }

            // 扫描所有协议目录
            var protocolDirectories = Directory.GetDirectories(_pluginsDirectory);
            _logger.LogInformation("Found {Count} protocol directories", protocolDirectories.Length);
            foreach (var protocolDir in protocolDirectories)
            {
                var protocolName = Path.GetFileName(protocolDir);
                _logger.LogInformation("Processing protocol: {ProtocolName} from directory: {Directory}", protocolName, protocolDir);

                // 扫描版本目录，选择最新版本
                var versionDirectories = Directory.GetDirectories(protocolDir);
                _logger.LogInformation("Found {Count} version directories for {ProtocolName}", versionDirectories.Length, protocolName);
                if (!versionDirectories.Any())
                {
                    _logger.LogWarning("No version directories found for {ProtocolName}", protocolName);
                    continue;
                }

                // 简单版本比较，实际应用中可以使用更复杂的版本解析
                var latestVersionDir = versionDirectories.OrderByDescending(Path.GetFileName).First();
                _logger.LogInformation("Selected latest version directory: {Directory}", latestVersionDir);

                // 查找DLL文件
                var dllFiles = Directory.GetFiles(latestVersionDir, "*.dll");
                _logger.LogInformation("Found {Count} DLL files in {Directory}", dllFiles.Length, latestVersionDir);
                if (!dllFiles.Any())
                {
                    _logger.LogWarning("No DLL files found in {Directory}", latestVersionDir);
                    continue;
                }

                var dllPath = dllFiles.First();
                _logger.LogInformation("Loading plugin DLL: {Path}", dllPath);
                try
                {
                    var pluginInfo = await LoadPluginAsync(dllPath, protocolName);
                    if (pluginInfo != null)
                    {
                        _logger.LogInformation("Successfully loaded plugin: {ProtocolName} v{Version}", pluginInfo.ProtocolName, pluginInfo.Version);
                    }
                    else
                    {
                        _logger.LogError("Failed to load plugin: {Path}", dllPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception when loading plugin: {Path}", dllPath);
                }
            }
        }

        /// <summary>
        /// 加载插件
        /// </summary>
        /// <param name="assemblyPath">程序集路径</param>
        /// <returns>任务</returns>
        public async Task<PluginInfo?> LoadPluginAsync(string assemblyPath, string protocolName)
        {
            _logger.LogInformation("Loading plugin: {Path}", assemblyPath);

            try
            {
                // 创建AssemblyLoadContext
                var loadContext = new AssemblyLoadContext(Guid.NewGuid().ToString(), true);
                loadContext.Resolving += OnAssemblyResolving;

                // 加载程序集
                var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);

                // 查找并创建IDeviceCommunication实例
                var clientType = assembly.ExportedTypes.FirstOrDefault(t => 
                    typeof(IDeviceCommunication).IsAssignableFrom(t) && !t.IsAbstract);
                if (clientType == null)
                {
                    _logger.LogError("No IDeviceCommunication implementation found in: {Path}", assemblyPath);
                    loadContext.Unload();
                    return null;
                }

                var deviceCommunication = (IDeviceCommunication)Activator.CreateInstance(clientType)!;

                // 获取版本信息
                string version = "1.0.0"; // 默认版本
                try
                {
                    // 从程序集版本信息获取版本号
                    var assemblyVersion = assembly.GetName().Version;
                    if (assemblyVersion != null)
                    {
                        version = assemblyVersion.ToString();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get assembly version for: {Path}", assemblyPath);
                }

                // 创建插件信息
                var pluginInfo = new PluginInfo
                {
                    ProtocolName = protocolName, // 使用目录名称作为协议名称
                    Version = version, // 使用程序集版本
                    AssemblyPath = assemblyPath,
                    LoadContext = loadContext,
                    DeviceCommunication = deviceCommunication,
                    Status = PluginStatus.Running
                };

                // 添加到插件字典
                _plugins[pluginInfo.Id] = pluginInfo;

                _logger.LogInformation("Plugin loaded successfully: {ProtocolName} v{Version}, PluginId: {PluginId}", 
                    pluginInfo.ProtocolName, pluginInfo.Version, pluginInfo.Id);
                
                // 满足async方法要求
                await Task.CompletedTask;
                
                // 记录所有已加载插件的详细信息
                _logger.LogInformation("Current loaded plugins count: {Count}", _plugins.Count);
                foreach (var p in _plugins.Values)
                {
                    _logger.LogInformation("  - Plugin: {ProtocolName} v{Version}, Id: {PluginId}", p.ProtocolName, p.Version, p.Id);
                }

                return pluginInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugin: {Path}", assemblyPath);
                return null;
            }
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <returns>任务</returns>
        public async Task<bool> UnloadPluginAsync(string pluginId)
        {
            if (!_plugins.TryGetValue(pluginId, out var pluginInfo))
            {
                _logger.LogWarning("Plugin not found: {PluginId}", pluginId);
                return false;
            }

            _logger.LogInformation("Unloading plugin: {ProtocolName} v{Version}", 
                pluginInfo.ProtocolName, pluginInfo.Version);

            try
            {
                // 设置状态为Draining
                pluginInfo.Status = PluginStatus.Draining;

                // 等待当前任务完成（实际应用中可以实现更复杂的会话管理）
                await Task.Delay(1000);

                // 设置状态为Unloading
                pluginInfo.Status = PluginStatus.Unloading;

                // 释放驱动资源，如果实现了IDisposable接口
                if (pluginInfo.DeviceCommunication is IDisposable disposableDriver)
                {
                    disposableDriver.Dispose();
                }

                // 卸载AssemblyLoadContext
                var loadContext = pluginInfo.LoadContext;
                loadContext.Unload();

                // 跟踪卸载的上下文，用于验证回收
                _unloadedContexts[pluginId] = new WeakReference(loadContext);

                // 从字典中移除
                _plugins.Remove(pluginId);

                // 触发GC（可选，实际应用中可以根据需要调整）
                GC.Collect();
                GC.WaitForPendingFinalizers();

                _logger.LogInformation("Plugin unloaded successfully: {ProtocolName} v{Version}", 
                    pluginInfo.ProtocolName, pluginInfo.Version);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unload plugin: {PluginId}", pluginId);
                pluginInfo.Status = PluginStatus.Failed;
                return false;
            }
        }

        /// <summary>
        /// 热更新插件
        /// </summary>
        /// <param name="newAssemblyPath">新插件路径</param>
        /// <returns>任务</returns>
        public async Task<PluginInfo?> ReloadPluginAsync(string newAssemblyPath)
        {
            // 从路径中提取协议名称（假设路径格式为 .../Plugins/ProtocolName/Version/...）
            string protocolName = Path.GetDirectoryName(Path.GetDirectoryName(newAssemblyPath))!;
            protocolName = Path.GetFileName(protocolName);

            // 加载新插件
            var newPlugin = await LoadPluginAsync(newAssemblyPath, protocolName);
            if (newPlugin == null)
                return null;

            // 查找并卸载旧版本插件
            var oldPlugins = _plugins.Values
                .Where(p => p.ProtocolName == newPlugin.ProtocolName && p.Id != newPlugin.Id)
                .ToList();

            foreach (var oldPlugin in oldPlugins)
            {
                await UnloadPluginAsync(oldPlugin.Id);
            }

            return newPlugin;
        }

        /// <summary>
        /// 获取所有插件
        /// </summary>
        /// <returns>插件列表</returns>
        public List<PluginInfo> GetPlugins()
        {
            return _plugins.Values.ToList();
        }

        /// <summary>
        /// 根据协议名称获取插件
        /// </summary>
        /// <param name="protocolName">协议名称</param>
        /// <returns>插件信息</returns>
        public PluginInfo? GetPluginByProtocolName(string protocolName)
        {
            return _plugins.Values.FirstOrDefault(p => p.ProtocolName == protocolName);
        }

        /// <summary>
        /// 启动文件系统监听器
        /// </summary>
        private void StartFileSystemWatcher()
        {
            _watcher = new FileSystemWatcher(_pluginsDirectory)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true
            };

            _watcher.Changed += OnFileSystemChanged;
            _watcher.Created += OnFileSystemChanged;
            _watcher.Deleted += OnFileSystemChanged;
            _watcher.Renamed += OnFileSystemChanged;

            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation("Started file system watcher for plugins directory");
        }

        /// <summary>
        /// 文件系统变化事件处理
        /// </summary>
        private async void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("FileSystem changed: {ChangeType} - {Path}", e.ChangeType, e.FullPath);

            // 防抖处理
            await Task.Delay(1000);

            // 重新扫描插件
            await ScanAndLoadPluginsAsync();
        }

        /// <summary>
        /// 程序集解析事件处理
        /// </summary>
        private Assembly? OnAssemblyResolving(AssemblyLoadContext context, AssemblyName name)
        {
            _logger.LogInformation("Resolving assembly: {Name}", name.Name);
            return null;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _watcher?.Dispose();

            // 卸载所有插件
            foreach (var plugin in _plugins.Values.ToList())
            {
                UnloadPluginAsync(plugin.Id).Wait();
            }
        }
    }
}