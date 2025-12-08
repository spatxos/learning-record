using Host.SDK;
using Host.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Host.PluginRuntime
{
    /// <summary>
    /// 单例连接管理器，用于管理从ConnectionConfigs表读取的连接
    /// </summary>
    public class SingletonConnectionManager : IDisposable
    {
        private readonly ConnectionManager _connectionManager;
        private readonly SqliteStore _sqliteStore;
        private readonly ILogger<SingletonConnectionManager> _logger;
        private readonly ConcurrentDictionary<int, ConnectionConfig> _connectionConfigs;
        private readonly ConcurrentDictionary<int, ConnectionInstanceInfo> _connectionInstances;
        private readonly ConcurrentDictionary<int, System.Threading.Timer> _retryTimers;
        private readonly object _syncLock;
        
        // 状态检测定时器
        private readonly System.Timers.Timer _statusCheckTimer;
        
        // 状态检测间隔（毫秒）
        private const int STATUS_CHECK_INTERVAL = 5000; // 5秒检查一次

        /// <summary>
        /// 连接实例信息
        /// </summary>
        public class ConnectionInstanceInfo
        {
            public string ConnectionId { get; set; }
            public ConnectionState State { get; set; }
            public string PluginName { get; set; }
            public string ProtocolName { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public Dictionary<string, string> Parameters { get; set; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionManager">连接管理器</param>
        /// <param name="sqliteStore">SQLite存储</param>
        /// <param name="logger">日志记录器</param>
        public SingletonConnectionManager(
            ConnectionManager connectionManager,
            SqliteStore sqliteStore,
            ILogger<SingletonConnectionManager> logger)
        {
            _connectionManager = connectionManager;
            _sqliteStore = sqliteStore;
            _logger = logger;
            _connectionConfigs = new ConcurrentDictionary<int, ConnectionConfig>();
            _connectionInstances = new ConcurrentDictionary<int, ConnectionInstanceInfo>();
            _retryTimers = new ConcurrentDictionary<int, System.Threading.Timer>();
            _syncLock = new object();
            
            // 初始化状态检测定时器
            _statusCheckTimer = new System.Timers.Timer(STATUS_CHECK_INTERVAL);
            _statusCheckTimer.Elapsed += StatusCheckTimer_Elapsed;
            _statusCheckTimer.AutoReset = true;
            _statusCheckTimer.Start();
        }

        /// <summary>
        /// 初始化所有单例连接
        /// </summary>
        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing singleton connections...");
            var configs = _sqliteStore.GetAllEnabledConnectionConfigs();
            foreach (var config in configs)
            {
                await InitializeConnectionAsync(config);
            }
        }

        /// <summary>
        /// 初始化单个连接
        /// </summary>
        /// <param name="config">连接配置</param>
        private async Task InitializeConnectionAsync(ConnectionConfig config)
        {
            if (!_connectionConfigs.TryAdd(config.Id, config))
            {
                _logger.LogWarning("Connection config with id {Id} already exists", config.Id);
                return;
            }

            _logger.LogInformation("Initializing connection for config id {Id}: {PluginName} - {Host}:{Port}",
                config.Id, config.PluginName, config.Host, config.Port);

            // 立即尝试连接
            await TryConnectAsync(config);
        }

        /// <summary>
        /// 尝试连接
        /// </summary>
        /// <param name="config">连接配置</param>
        private async Task TryConnectAsync(ConnectionConfig config)
        {
            try
            {
                _logger.LogInformation("Trying to connect for config id {Id}", config.Id);

                // 解析参数
                var parameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(config.Parameters) ?? new Dictionary<string, string>();

                // 创建连接设置
                var settings = new Dictionary<string, string>
                {
                    { "Host", config.Host },
                    { "Port", config.Port.ToString() }
                };

                // 合并额外参数
                foreach (var param in parameters)
                {
                    settings[param.Key] = param.Value;
                }

                // 创建并启动连接
                var connectionInstance = await _connectionManager.CreateConnectionAsync(config.PluginName, settings);
                await _connectionManager.StartConnectionAsync(connectionInstance.ConnectionId);

                // 更新连接实例信息
                var instanceInfo = new ConnectionInstanceInfo
                {
                    ConnectionId = connectionInstance.ConnectionId,
                    State = ConnectionState.Connected,
                    PluginName = config.PluginName,
                    ProtocolName = config.ProtocolName,
                    Host = config.Host,
                    Port = config.Port,
                    Parameters = parameters
                };

                _connectionInstances[config.Id] = instanceInfo;
                _sqliteStore.InsertConnection(
                    connectionInstance.ConnectionId,
                    config.PluginName,
                    config.ProtocolName,
                    config.Host,
                    config.Port,
                    config.Parameters
                );

                _logger.LogInformation("Connection successful for config id {Id}, connection id: {ConnectionId}",
                    config.Id, connectionInstance.ConnectionId);

                // 取消重试定时器
                CancelRetryTimer(config.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection failed for config id {Id}: {ErrorMessage}", config.Id, ex.Message);

                // 更新连接状态
                UpdateConnectionState(config.Id, string.Empty, ConnectionState.Error, config.PluginName, config.ProtocolName, config.Host, config.Port, config.Parameters);

                // 设置重试定时器
                SetupRetryTimer(config);
            }
        }
        
        /// <summary>
        /// 更新连接状态
        /// </summary>
        /// <param name="configId">配置ID</param>
        /// <param name="connectionId">连接ID</param>
        /// <param name="state">连接状态</param>
        /// <param name="pluginName">插件名称</param>
        /// <param name="protocolName">协议名称</param>
        /// <param name="host">主机</param>
        /// <param name="port">端口</param>
        /// <param name="parametersJson">参数JSON</param>
        private void UpdateConnectionState(int configId, string connectionId, ConnectionState state, string pluginName, string protocolName, string host, int port, string parametersJson)
        {
            // 解析参数
            var parameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(parametersJson) ?? new Dictionary<string, string>();
            
            var instanceInfo = new ConnectionInstanceInfo
            {
                ConnectionId = connectionId,
                State = state,
                PluginName = pluginName,
                ProtocolName = protocolName,
                Host = host,
                Port = port,
                Parameters = parameters
            };
            _connectionInstances[configId] = instanceInfo;
            
            _logger.LogInformation("Updated connection state for config id {Id}: {State}, ConnectionId: {ConnectionId}", 
                configId, state, connectionId);
        }
        
        /// <summary>
        /// 状态检测定时器事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private async void StatusCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _logger.LogInformation("Status check timer elapsed, checking {Count} connections...", _connectionConfigs.Count);
                
                // 遍历所有连接配置，检查状态
                foreach (var config in _connectionConfigs.Values)
                {
                    // 检查是否有连接实例信息
                    if (_connectionInstances.TryGetValue(config.Id, out var instanceInfo))
                    {

                        _logger.LogInformation($"{instanceInfo.ConnectionId} connection is {instanceInfo.State.ToString()}", _connectionConfigs.Count);
                        // 检查连接是否有效
                        if (instanceInfo.State == ConnectionState.Connected && !string.IsNullOrEmpty(instanceInfo.ConnectionId))
                        {
                            // 检查连接是否有效
                            if (!IsConnectionValid(instanceInfo.ConnectionId))
                            {
                                _logger.LogWarning("Connection for config id {Id} is disconnected, trying to reconnect", config.Id);
                                
                                // 更新连接状态为Disconnected
                                UpdateConnectionState(config.Id, instanceInfo.ConnectionId, ConnectionState.Disconnected, config.PluginName, config.ProtocolName, config.Host, config.Port, config.Parameters);
                                
                                // 尝试重新连接
                                await TryConnectAsync(config);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Connection config id {Id} needs to reconnect. Current state: {State}, ConnectionId: {ConnectionId}", 
                                config.Id, instanceInfo.State, instanceInfo.ConnectionId);

                            UpdateConnectionState(config.Id, instanceInfo.ConnectionId, ConnectionState.Disconnected,
                                    config.PluginName, config.ProtocolName, config.Host, config.Port, config.Parameters);

                            // 如果当前状态不是Disconnected或Error，更新为Disconnected
                            if (instanceInfo.State != ConnectionState.Disconnected && instanceInfo.State != ConnectionState.Error)
                            {
                                _logger.LogWarning("Connection config id {Id} has inconsistent state, updating to Disconnected", config.Id);
                                UpdateConnectionState(config.Id, instanceInfo.ConnectionId, ConnectionState.Disconnected, 
                                    config.PluginName, config.ProtocolName, config.Host, config.Port, config.Parameters);
                            }
                            
                            // 尝试重新连接
                            await TryConnectAsync(config);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No instance info found for connection config id {Id}, initializing connection...", config.Id);
                        await TryConnectAsync(config);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during status check timer elapsed event");
            }
        }
        
        /// <summary>
        /// 检查连接是否有效
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>连接是否有效</returns>
        private bool IsConnectionValid(string connectionId)
        {
            try
            {
                // 获取连接实例
                var connection = _connectionManager.GetConnection(connectionId);
                if (connection == null)
                {
                    _logger.LogWarning("Connection {Id} not found", connectionId);
                    return false;
                }
                
                // 使用心跳机制检查连接是否有效
                connection.SetHeartbeatPoint("0", "Coil"); // 设置默认心跳点位
                var task = connection.CheckHeartbeatAsync(CancellationToken.None);
                return task.Wait(2000); // 心跳检查超时时间为2秒
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking connection validity for connection id {Id}", connectionId);
                return false;
            }
        }

        /// <summary>
        /// 设置重试定时器
        /// </summary>
        /// <param name="config">连接配置</param>
        private void SetupRetryTimer(ConnectionConfig config)
        {
            CancelRetryTimer(config.Id);

            var retryInterval = config.RetryInterval > 0 ? config.RetryInterval : 30000; // 默认30秒
            _logger.LogInformation("Setting up retry timer for config id {Id}, interval: {Interval}ms", config.Id, retryInterval);

            var timer = new System.Threading.Timer(async _ =>
            {
                await TryConnectAsync(config);
            }, null, retryInterval, Timeout.Infinite);

            _retryTimers[config.Id] = timer;
        }

        /// <summary>
        /// 取消重试定时器
        /// </summary>
        /// <param name="configId">配置ID</param>
        private void CancelRetryTimer(int configId)
        {
            if (_retryTimers.TryRemove(configId, out var timer))
            {
                timer.Dispose();
                _logger.LogInformation("Cancelled retry timer for config id {Id}", configId);
            }
        }

        /// <summary>
        /// 添加新的连接配置
        /// </summary>
        /// <param name="config">连接配置</param>
        public async Task AddConnectionConfigAsync(ConnectionConfig config)
        {
            await InitializeConnectionAsync(config);
        }

        /// <summary>
        /// 获取所有连接实例信息
        /// </summary>
        /// <returns>连接实例信息列表</returns>
        public List<(ConnectionConfig Config, ConnectionInstanceInfo InstanceInfo)> GetAllConnectionInstances()
        {
            var result = new List<(ConnectionConfig, ConnectionInstanceInfo)>();

            foreach (var config in _connectionConfigs.Values)
            {
                if (_connectionInstances.TryGetValue(config.Id, out var instanceInfo))
                {
                    result.Add((config, instanceInfo));
                }
            }

            return result;
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            foreach (var timer in _retryTimers.Values)
            {
                timer.Dispose();
            }
            
            // 停止并释放状态检测定时器
            _statusCheckTimer.Stop();
            _statusCheckTimer.Dispose();
        }
    }
}
