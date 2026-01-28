using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Host.SDK;
using Microsoft.Extensions.Logging;

namespace Host.PluginRuntime
{
    public class ConnectionInstance
    {
        public string ConnectionId { get; init; } = Guid.NewGuid().ToString();
        public string PluginName { get; init; } = default!;
        public IDeviceCommunication? DeviceCommunication { get; set; }
        public IDictionary<string,string> Settings { get; init; } = new Dictionary<string,string>();
    }

    public class ConnectionManager : IDisposable
    {
        private readonly PluginManager _pluginManager;
        private readonly ConcurrentDictionary<string, ConnectionInstance> _instances = new();
        private readonly ILogger<ConnectionManager> _logger;

        public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        public ConnectionManager(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
            
            // 创建日志记录器
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<ConnectionManager>();
        }

        public async Task<ConnectionInstance> CreateConnectionAsync(string pluginOrProtocolName, IDictionary<string,string> settings, CancellationToken token = default)
        {
            _logger.LogInformation("Trying to create connection with pluginOrProtocolName: '{Name}'", pluginOrProtocolName);
            
            // 记录所有已加载的插件信息
            var allPlugins = _pluginManager.GetPlugins();
            _logger.LogInformation("Currently loaded plugins: {Count}", allPlugins.Count);
            foreach (var p in allPlugins)
            {
                _logger.LogInformation("  Plugin: Id={Id}, ProtocolName={ProtocolName}, Version={Version}", 
                    p.Id, p.ProtocolName, p.Version);
            }
            
            // 先尝试通过协议名称查找插件
            var plugin = _pluginManager.GetPluginByProtocolName(pluginOrProtocolName);
            _logger.LogInformation("First attempt (exact match) result: {Result}", plugin != null ? "Found" : "Not found");
            
            // 如果找不到，再尝试通过插件名称查找插件（保持向后兼容性）
            if (plugin == null)
            {
                plugin = _pluginManager.GetPlugins().FirstOrDefault(p => 
                    p.ProtocolName.Equals(pluginOrProtocolName, StringComparison.OrdinalIgnoreCase) ||
                    p.ProtocolName.Equals(pluginOrProtocolName + "TCP", StringComparison.OrdinalIgnoreCase));
                _logger.LogInformation("Second attempt (case-insensitive or TCP suffix) result: {Result}", plugin != null ? "Found" : "Not found");
            }
            
            if (plugin == null)
            {
                _logger.LogError("Failed to find plugin for pluginOrProtocolName: '{Name}'", pluginOrProtocolName);
                throw new Exception("plugin not loaded");
            }
            
            // 使用IDeviceCommunication接口
            IDeviceCommunication? deviceCommunication = plugin.DeviceCommunication;
            if (deviceCommunication == null)
            {
                _logger.LogError("IDeviceCommunication not found in plugin: '{Name}'", pluginOrProtocolName);
                throw new Exception("IDeviceCommunication not found in plugin");
            }
            
            var inst = new ConnectionInstance 
            {
                PluginName = pluginOrProtocolName, 
                DeviceCommunication = deviceCommunication,
                Settings = new Dictionary<string,string>(settings) 
            };
            
            _instances[inst.ConnectionId] = inst;

            return inst;
        }

        public async Task<bool> StartConnectionAsync(string connectionId, CancellationToken token = default)
        {
            if (!_instances.TryGetValue(connectionId, out var inst)) throw new Exception("no such connection");
            
            // 使用IDeviceCommunication连接
            if (inst.DeviceCommunication != null)
            {
                if (inst.Settings.TryGetValue("Host", out var host) && inst.Settings.TryGetValue("Port", out var portStr))
                {
                    int port = int.TryParse(portStr, out port) ? port : 502;
                    byte unitId = 1; // 默认单元ID
                    
                    // 检查连接是否成功建立
                    return await inst.DeviceCommunication.ConnectAsync(host, port, unitId, token);
                }
            }
            return false;
        }

        public async Task StopConnectionAsync(string connectionId, CancellationToken token = default)
        {
            if (!_instances.TryGetValue(connectionId, out var inst)) return;
            
            // 使用IDeviceCommunication断开连接
            if (inst.DeviceCommunication != null)
            {
                await inst.DeviceCommunication.DisconnectAsync();
            }
        }

        public async Task<ProtocolResponse> ExecuteAsync(string connectionId, ProtocolRequest req, CancellationToken token = default)
        {
            if (!_instances.TryGetValue(connectionId, out var inst)) throw new Exception("no such connection");
            
            // 使用IDeviceCommunication接口
            if (inst.DeviceCommunication != null)
            {
                try
                {
                    // 使用反射创建请求类型并调用相应方法
                    var deviceCommunicationType = inst.DeviceCommunication.GetType();
                    
                    if (req.Action == "Read")
                    {
                        // 获取ReadAsync方法
                        var readMethod = deviceCommunicationType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                            .FirstOrDefault(m => m.Name == "ReadAsync" && m.IsGenericMethod && m.GetParameters().Length == 2);
                        
                        if (readMethod != null)
                        {
                            // 获取请求类型（假设是第一个泛型参数的类型）
                            var readRequestType = readMethod.GetGenericArguments()[1];
                            
                            // 创建请求实例
                            var readRequest = Activator.CreateInstance(readRequestType);
                            
                            // 设置请求属性
                            SetRequestProperties(readRequest, req);
                            
                            // 调用ReadAsync方法
                            var task = (Task)readMethod.MakeGenericMethod(typeof(byte[]), readRequestType)
                                .Invoke(inst.DeviceCommunication, new object[] { readRequest, token });
                            
                            await task;
                            
                            // 获取结果
                            var result = task.GetType().GetProperty("Result")?.GetValue(task) as byte[];
                            return new ProtocolResponse(true, result ?? Array.Empty<byte>());
                        }
                    }
                    else if (req.Action == "Write")
                    {
                        // 获取WriteAsync方法
                        var writeMethod = deviceCommunicationType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                            .FirstOrDefault(m => m.Name == "WriteAsync" && m.IsGenericMethod && m.GetParameters().Length == 2);
                        
                        if (writeMethod != null)
                        {
                            // 获取请求类型（假设是第一个泛型参数的类型）
                            var writeRequestType = writeMethod.GetGenericArguments()[0];
                            
                            // 创建请求实例
                            var writeRequest = Activator.CreateInstance(writeRequestType);
                            
                            // 设置请求属性
                            if (writeRequest != null)
                            {
                                SetRequestProperties(writeRequest, req);
                            }
                            
                            // 调用WriteAsync方法
                            var task = (Task)writeMethod.MakeGenericMethod(writeRequestType)
                                .Invoke(inst.DeviceCommunication, new object[] { writeRequest, token });
                            
                            await task;
                            
                            // 获取结果
                            bool success = (bool)(task.GetType().GetProperty("Result")?.GetValue(task) ?? false);
                            return new ProtocolResponse(success);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing request with IDriverClient");
                    throw;
                }
            }
            
            throw new Exception("no valid connection for execution");
        }
        
        /// <summary>
        /// 设置请求对象的属性
        /// </summary>
        /// <param name="request">请求对象</param>
        /// <param name="protocolReq">协议请求</param>
        private void SetRequestProperties(object request, ProtocolRequest protocolReq)
        {
            var requestType = request.GetType();
            
            // 设置基本属性
            SetProperty(request, requestType, "UnitId", 1);
            
            // 从Props字典获取属性值
            if (protocolReq.Props.TryGetValue("Address", out var addressObj) && ushort.TryParse(addressObj.ToString(), out var address))
            {
                SetProperty(request, requestType, "StartingAddress", address);
            }
            
            if (protocolReq.Props.TryGetValue("Length", out var lengthObj) && int.TryParse(lengthObj.ToString(), out var length))
            {
                SetProperty(request, requestType, "Quantity", length);
            }
            else
            {
                SetProperty(request, requestType, "Quantity", 1);
            }
            
            // 根据数据类型设置功能码
            string dataType = protocolReq.Props.TryGetValue("DataType", out var dataTypeObj) ? dataTypeObj.ToString() : "holding";
            int functionCode = GetFunctionCode(dataType);
            SetProperty(request, requestType, "FunctionCode", functionCode);
        }
        
        /// <summary>
        /// 设置对象的属性
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="type">对象类型</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        private void SetProperty(object obj, Type type, string propertyName, object value)
        {
            var property = type.GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                // 转换值类型
                var targetType = property.PropertyType;
                if (targetType.IsValueType && value.GetType() != targetType)
                {
                    value = Convert.ChangeType(value, targetType);
                }
                property.SetValue(obj, value);
            }
        }
        
        /// <summary>
        /// 根据数据类型获取Modbus功能码
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <returns>Modbus功能码</returns>
        private int GetFunctionCode(string dataType)
        {
            switch (dataType?.ToLower())
            {
                case "coil": // 线圈
                    return 1;
                case "discrete": // 离散输入
                    return 2;
                case "holding": // 保持寄存器
                    return 3;
                case "input": // 输入寄存器
                    return 4;
                default:
                    return 3; // 默认使用保持寄存器
            }
        }

        public void Dispose()
        {
            // 清理操作
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
                bool isValid = false;
                
                // 检查连接是否有效
                if (instance.DeviceCommunication != null)
                {
                    // 使用IsConnectionValid方法检查连接有效性
                    isValid = IsConnectionValid(instance.ConnectionId);
                }
                
                if (isValid)
                {
                    activeConnections.Add(instance);
                }
                else
                {
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

        private bool IsConnectionValid(string connectionId)
        {
            if (_instances.TryGetValue(connectionId, out var instance))
            {
                // 使用IDeviceCommunication的连接状态检查
                if (instance.DeviceCommunication != null)
                {
                    return instance.DeviceCommunication.IsConnected;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取连接的当前状态
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>连接状态</returns>
        public Host.SDK.ConnectionState GetConnectionState(string connectionId)
        {
            if (_instances.TryGetValue(connectionId, out var instance))
            {
                // 使用IDeviceCommunication的连接状态
                if (instance.DeviceCommunication != null)
                {
                    return instance.DeviceCommunication.IsConnected ? Host.SDK.ConnectionState.Connected : Host.SDK.ConnectionState.Disconnected;
                }
            }
            return Host.SDK.ConnectionState.Disconnected;
        }
    }
}