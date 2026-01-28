using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Host.SDK;

namespace Host.PluginRuntime
{
    public class PluginHandle
    {
        public string Name { get; init; } = default!;
        public string Path { get; init; } = default!;
        public AssemblyLoadContext? LoadContext { get; set; }
        public IDeviceCommunication? DeviceCommunicationInstance { get; set; }
        public WeakReference? WeakContextRef { get; set; }
    }

    public class PluginLoader : IDisposable
    {
        private readonly string _pluginDir;
        private readonly ConcurrentDictionary<string, PluginHandle> _handles = new();

        public PluginLoader(string pluginDir)
        {
            _pluginDir = pluginDir;
            if (!Directory.Exists(_pluginDir)) Directory.CreateDirectory(_pluginDir);
        }

        public IEnumerable<PluginHandle> ListPlugins() => _handles.Values;

        public PluginHandle? LoadPlugin(string dllPath)
        {
            var full = System.IO.Path.GetFullPath(dllPath);
            var name = System.IO.Path.GetFileNameWithoutExtension(full);

            // prevent duplicate
            if (_handles.ContainsKey(name)) return _handles[name];

            var alc = new AssemblyLoadContext(Guid.NewGuid().ToString(), isCollectible: true);
            var asm = alc.LoadFromAssemblyPath(full);
            var clientType = asm.GetTypes().FirstOrDefault(t => typeof(IDeviceCommunication).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            if (clientType == null)
            {
                alc.Unload();
                return null;
            }

            var inst = (IDeviceCommunication)Activator.CreateInstance(clientType)!;
            var handle = new PluginHandle { Name = name, Path = full, LoadContext = alc, DeviceCommunicationInstance = inst, WeakContextRef = new WeakReference(alc) };
            _handles[name] = handle;
            return handle;
        }

        public void UnloadPlugin(string name)
        {
            if (!_handles.TryRemove(name, out var handle)) return;
            try
            {
                handle.DeviceCommunicationInstance = null;
                handle.LoadContext?.Unload();
            }
            catch { /* log */ }
        }

        public void Dispose()
        {
            foreach (var kv in _handles.Keys.ToList()) UnloadPlugin(kv);
        }
    }
}