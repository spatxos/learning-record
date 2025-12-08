using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Host;
using Host.SDK;

namespace Host.PluginRuntime
{
    public class ConnectionInstance
    {
        public string ConnectionId { get; init; } = Guid.NewGuid().ToString();
        public string PluginName { get; init; } = default!;
        public IProtocolConnection? Connection { get; set; }
        public IDictionary<string,string> Settings { get; init; } = new Dictionary<string,string>();
    }

    public class ConnectionManager : IDisposable
    {
        private readonly PluginManager _pluginManager;
        private readonly ConcurrentDictionary<string, ConnectionInstance> _instances = new();

        public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        public ConnectionManager(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        public async Task<ConnectionInstance> CreateConnectionAsync(string pluginName, IDictionary<string,string> settings, CancellationToken token = default)
        {
            var plugin = _pluginManager.GetPluginByProtocolName(pluginName);
            if (plugin == null) throw new Exception("plugin not loaded");
            var conn = await plugin.Driver.CreateConnectionAsync(settings, token);
            var inst = new ConnectionInstance { PluginName = pluginName, Connection = conn, Settings = new Dictionary<string,string>(settings) };
            _instances[inst.ConnectionId] = inst;

            conn.ConnectionStatusChanged += (s, e) =>
            {
                ConnectionStatusChanged?.Invoke(this, e);
            };

            return inst;
        }

        public async Task StartConnectionAsync(string connectionId, CancellationToken token = default)
        {
            if (!_instances.TryGetValue(connectionId, out var inst)) throw new Exception("no such connection");
            await inst.Connection!.OpenAsync(token);
        }

        public async Task StopConnectionAsync(string connectionId, CancellationToken token = default)
        {
            if (!_instances.TryGetValue(connectionId, out var inst)) return;
            await inst.Connection?.CloseAsync(token)!;
        }

        public async Task<ProtocolResponse> ExecuteAsync(string connectionId, ProtocolRequest req, CancellationToken token = default)
        {
            if (!_instances.TryGetValue(connectionId, out var inst)) throw new Exception("no such connection");
            return await inst.Connection!.ExecuteAsync(req, token);
        }

        public void Dispose()
        {
            foreach (var kv in _instances.Values)
            {
                try { kv.Connection?.Dispose(); } catch { }
            }
        }

        /// <summary>
        /// 获取所有当前连接状态的连接
        /// </summary>
        /// <returns>连接实例列表</returns>
        public IEnumerable<ConnectionInstance> GetActiveConnections()
        {
            var inactiveConnections = new List<string>();
            var activeConnections = new List<ConnectionInstance>();

            foreach (var kv in _instances)
            {
                var instance = kv.Value;
                if (instance.Connection != null)
                {
                    // 检查连接状态
                    if (instance.Connection.State == Host.SDK.ConnectionState.Connected)
                    {
                        // 主动检测连接是否仍然有效
                        if (IsConnectionValid(instance.Connection))
                        {
                            activeConnections.Add(instance);
                        }
                        else
                        {
                            inactiveConnections.Add(instance.ConnectionId);
                        }
                    }
                    else
                    {
                        // 记录已断开的连接ID
                        inactiveConnections.Add(instance.ConnectionId);
                    }
                }
                else
                {
                    // 没有实际连接对象的实例也标记为无效
                    inactiveConnections.Add(instance.ConnectionId);
                }
            }

            // 清理已断开的连接
            foreach (var id in inactiveConnections)
            {
                _instances.TryRemove(id, out _);
            }

            return activeConnections;
        }

        private bool IsConnectionValid(IProtocolConnection connection)
        {
            try
            {
                // 设置心跳检查点位（这里使用默认值，实际应用中可以从配置读取）
                connection.SetHeartbeatPoint("0", "Coil");
                
                // 使用心跳机制检查连接是否有效
                var task = connection.CheckHeartbeatAsync(CancellationToken.None);
                return task.Wait(2000); // 心跳检查超时时间为2秒
            }
            catch
            {
                // 如果发生异常，连接无效
                return false;
            }
        }

        /// <summary>
        /// 获取连接的当前状态
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>连接状态</returns>
        public Host.SDK.ConnectionState GetConnectionState(string connectionId)
        {
            if (_instances.TryGetValue(connectionId, out var instance) && instance.Connection != null)
            {
                return instance.Connection.State;
            }
            return Host.SDK.ConnectionState.Disconnected;
        }
        
        /// <summary>
        /// 获取IProtocolConnection实例
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>IProtocolConnection实例，如果不存在则返回null</returns>
        public IProtocolConnection? GetConnection(string connectionId)
        {
            if (_instances.TryGetValue(connectionId, out var instance))
            {
                return instance.Connection;
            }
            return null;
        }
    }
}