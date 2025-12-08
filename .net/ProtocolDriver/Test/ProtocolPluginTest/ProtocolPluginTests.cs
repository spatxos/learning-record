using Host;
using Host.SDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ProtocolPluginTest
{
    /// <summary>
    /// 协议插件化功能测试类
    /// </summary>
    public class ProtocolPluginTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly ServiceProvider _serviceProvider;

        public ProtocolPluginTests()
        {
            _loggerMock = new Mock<ILogger>();
            
            // 创建依赖项的模拟对象
            var loggerPluginManagerMock = new Mock<ILogger<PluginManager>>();
            var loggerHostApiMock = new Mock<ILogger<HostApi>>();
            var configurationMock = new Mock<IConfiguration>();
            var transportFactoryMock = new Mock<ITransportFactory>();
            
            // 创建HostApi实例
            var hostApi = new HostApi(loggerHostApiMock.Object);
            
            // 创建PluginManager实例
            var pluginManager = new PluginManager(
                loggerPluginManagerMock.Object,
                configurationMock.Object,
                transportFactoryMock.Object,
                hostApi);
            
            // 模拟Modbus驱动
            var modbusDriverMock = new Mock<IProtocolDriver>();
            modbusDriverMock.SetupGet(d => d.Metadata).Returns(new PluginMetadata("Modbus", "1.0.0"));
            
            // 模拟Modbus连接
            var modbusConnectionMock = new Mock<IProtocolConnection>();
            modbusConnectionMock.SetupGet(c => c.ConnectionId).Returns("test-modbus-connection");
            modbusConnectionMock.SetupGet(c => c.State).Returns(ConnectionState.Disconnected);
            modbusConnectionMock.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => modbusConnectionMock.SetupGet(c => c.State).Returns(ConnectionState.Connected));
            modbusConnectionMock.Setup(c => c.CloseAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => modbusConnectionMock.SetupGet(c => c.State).Returns(ConnectionState.Disconnected));
            
            // 设置Modbus驱动的CreateConnectionAsync方法
            modbusDriverMock.Setup(d => d.CreateConnectionAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(modbusConnectionMock.Object);
            
            // 模拟MitsubishiMC驱动
            var mitsubishiDriverMock = new Mock<IProtocolDriver>();
            mitsubishiDriverMock.SetupGet(d => d.Metadata).Returns(new PluginMetadata("MitsubishiMC", "1.0.0"));
            
            // 模拟MitsubishiMC连接
            var mitsubishiConnectionMock = new Mock<IProtocolConnection>();
            mitsubishiConnectionMock.SetupGet(c => c.ConnectionId).Returns("test-mitsubishi-connection");
            mitsubishiConnectionMock.SetupGet(c => c.State).Returns(ConnectionState.Disconnected);
            
            // 设置MitsubishiMC驱动的CreateConnectionAsync方法
            mitsubishiDriverMock.Setup(d => d.CreateConnectionAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mitsubishiConnectionMock.Object);
            
            // 使用反射获取PluginManager的私有字段_plugins
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (pluginsField != null)
            {
                var plugins = new Dictionary<string, PluginInfo>();
                
                // 添加Modbus插件
                plugins.Add("modbus-1", new PluginInfo
                {
                    Id = "modbus-1",
                    ProtocolName = "Modbus",
                    Version = "1.0.0",
                    Driver = modbusDriverMock.Object,
                    Status = PluginStatus.Running,
                    AssemblyPath = "plugins/Modbus/Modbus.dll",
                    LoadContext = new AssemblyLoadContext("modbus-test-context")
                });
                
                // 添加MitsubishiMC插件
                plugins.Add("mitsubishi-1", new PluginInfo
                {
                    Id = "mitsubishi-1",
                    ProtocolName = "MitsubishiMC",
                    Version = "1.0.0",
                    Driver = mitsubishiDriverMock.Object,
                    Status = PluginStatus.Running,
                    AssemblyPath = "plugins/MitsubishiMC/MitsubishiMC.dll",
                    LoadContext = new AssemblyLoadContext("mitsubishi-test-context")
                });
                
                pluginsField.SetValue(pluginManager, plugins);
            }
            
            // 创建服务提供者
            var services = new ServiceCollection();
            services.AddSingleton(_loggerMock.Object);
            services.AddSingleton<PluginManager>(pluginManager);
            services.AddSingleton<HostApi>(hostApi);
            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// 测试插件服务注册
        /// </summary>
        [Fact]
        public async Task TestPluginServiceRegistration()
        {
            // 初始化插件管理器
            var pluginManager = _serviceProvider.GetRequiredService<PluginManager>();
            await pluginManager.InitializeAsync();

            // 获取插件列表
            var plugins = pluginManager.GetPlugins();
            
            // 验证至少有一个插件被加载
            Assert.True(plugins.Count > 0, "No plugins were loaded");
            
            // 验证插件信息
            foreach (var plugin in plugins)
            {
                Assert.NotNull(plugin);
                Assert.NotNull(plugin.ProtocolName);
                Assert.NotNull(plugin.Version);
            }
        }

        /// <summary>
        /// 测试获取特定协议的插件
        /// </summary>
        [Fact]
        public async Task TestGetPluginByProtocolName()
        {
            // 初始化插件管理器
            var pluginManager = _serviceProvider.GetRequiredService<PluginManager>();
            await pluginManager.InitializeAsync();

            // 获取Modbus协议插件
            var modbusPlugin = pluginManager.GetPluginByProtocolName("Modbus");
            
            // 验证插件存在
            Assert.NotNull(modbusPlugin);
            Assert.Equal("Modbus", modbusPlugin.ProtocolName);
        }

        /// <summary>
        /// 测试连接管理功能
        /// </summary>
        [Fact]
        public async Task TestConnectionManagement()
        {
            // 初始化插件管理器
            var pluginManager = _serviceProvider.GetRequiredService<PluginManager>();
            await pluginManager.InitializeAsync();

            // 获取Modbus协议插件
            var modbusPlugin = pluginManager.GetPluginByProtocolName("Modbus");
            Assert.NotNull(modbusPlugin);

            // 创建连接设置
            var settings = new Dictionary<string, string>
            {
                { "IpAddress", "127.0.0.1" },
                { "Port", "502" },
                { "UnitId", "1" }
            };

            // 创建连接
            var connection = await modbusPlugin.Driver.CreateConnectionAsync(settings, CancellationToken.None);
            
            // 验证连接创建成功
            Assert.NotNull(connection);
            
            // 验证连接初始状态
            Assert.Equal(ConnectionState.Disconnected, connection.State);
        }

        /// <summary>
        /// 测试指令发送功能（模拟）
        /// </summary>
        [Fact]
        public async Task TestCommandExecution()
        {
            // 创建模拟的协议连接
            var mockConnection = new Mock<IProtocolConnection>();
            mockConnection.SetupGet(c => c.ConnectionId).Returns("test-connection-1");
            mockConnection.SetupGet(c => c.State).Returns(ConnectionState.Connected);

            // 设置ExecuteAsync方法的模拟返回值
            var expectedResponse = new ProtocolResponse(
                Success: true,
                Payload: new byte[] { 0x01, 0x02, 0x03 },
                Parsed: new Dictionary<string, object> { { "TestKey", "TestValue" } }
            );

            mockConnection.Setup(c => c.ExecuteAsync(It.IsAny<ProtocolRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // 创建模拟的协议请求
            var request = new ProtocolRequest(
                Action: "ReadRegisters",
                Props: new Dictionary<string, string>
                {
                    { "FunctionCode", "3" },
                    { "StartAddress", "0" },
                    { "Count", "2" }
                }
            );

            // 执行请求
            var response = await mockConnection.Object.ExecuteAsync(request);

            // 验证响应结果
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Parsed);
            Assert.True(response.Parsed.ContainsKey("TestKey"));
        }

        /// <summary>
        /// 测试连接状态变更事件
        /// </summary>
        [Fact]
        public async Task TestConnectionStatusChangedEvent()
        {
            // 创建模拟的协议连接
            var mockConnection = new Mock<IProtocolConnection>();
            mockConnection.SetupGet(c => c.ConnectionId).Returns("test-connection-1");
            mockConnection.SetupGet(c => c.State).Returns(ConnectionState.Disconnected);

            ConnectionState newState = ConnectionState.Disconnected;
            var eventTriggered = new ManualResetEvent(false);

            // 订阅连接状态变更事件
            mockConnection.Object.ConnectionStatusChanged += (sender, e) =>
            {
                newState = e.State;
                eventTriggered.Set();
            };

            // 模拟连接状态变更
            var connectionStateArgs = new ConnectionStatusChangedEventArgs
            {
                ConnectionId = "test-connection-1",
                State = ConnectionState.Connected
            };

            // 触发事件
            mockConnection.Raise(m => m.ConnectionStatusChanged += null, connectionStateArgs);

            // 等待事件触发或超时
            var eventOccurred = eventTriggered.WaitOne(TimeSpan.FromSeconds(5));

            // 验证事件是否被触发
            Assert.True(eventOccurred, "Connection status changed event was not triggered");
            Assert.Equal(ConnectionState.Connected, newState);
            Assert.Equal(ConnectionState.Connected, connectionStateArgs.State);
        }

        /// <summary>
        /// 测试插件元数据获取
        /// </summary>
        [Fact]
        public async Task TestPluginMetadataRetrieval()
        {
            // 初始化插件管理器
            var pluginManager = _serviceProvider.GetRequiredService<PluginManager>();
            await pluginManager.InitializeAsync();

            // 获取所有插件
            var plugins = pluginManager.GetPlugins();
            
            foreach (var plugin in plugins)
            {
                // 验证插件信息完整性
                Assert.False(string.IsNullOrWhiteSpace(plugin.ProtocolName));
                Assert.False(string.IsNullOrWhiteSpace(plugin.Version));
                Assert.NotNull(plugin.Driver);
                Assert.NotNull(plugin.Driver.Metadata);
                Assert.False(string.IsNullOrWhiteSpace(plugin.Driver.Metadata.ProtocolName));
                Assert.False(string.IsNullOrWhiteSpace(plugin.Driver.Metadata.Version));
            }
        }

        [Fact]
        public async Task TestPluginInfoMetadata()
        {
            // 创建模拟的协议驱动
            var mockDriver = new Mock<IProtocolDriver>();
            mockDriver.SetupGet(d => d.Metadata).Returns(new PluginMetadata("Modbus", "1.0.0"));

            // 创建插件信息
            var pluginInfo = new PluginInfo
            {
                Id = "test-plugin-1",
                ProtocolName = "Modbus",
                Version = "1.0.0",
                Driver = mockDriver.Object,
                Status = PluginStatus.Running,
                AssemblyPath = "plugins/Modbus/Modbus.dll",
                LoadContext = new AssemblyLoadContext("test-context")
            };

            // 验证插件信息
            Assert.Equal("Modbus", pluginInfo.ProtocolName);
            Assert.Equal("1.0.0", pluginInfo.Version);
            Assert.Equal("Modbus", pluginInfo.Driver.Metadata.ProtocolName);
            Assert.Equal("1.0.0", pluginInfo.Driver.Metadata.Version);
        }
    }

    /// <summary>
    /// 模拟服务提供者类
    /// </summary>
    public class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public ServiceProvider AddSingleton<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
            return this;
        }

        public ServiceProvider AddSingleton<TService, TImplementation>() where TImplementation : class, TService
        {
            var service = Activator.CreateInstance<TImplementation>();
            _services[typeof(TService)] = service;
            return this;
        }

        public ServiceProvider AddSingleton(Type serviceType, object service)
        {
            _services[serviceType] = service;
            return this;
        }

        public T GetRequiredService<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"Service of type {typeof(T)} not found");
        }

        public object GetService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var service);
            return service;
        }
    }

    /// <summary>
    /// 服务集合类
    /// </summary>
    public class ServiceCollection
    {
        private readonly List<ServiceDescriptor> _services = new List<ServiceDescriptor>();

        public ServiceCollection AddSingleton<T>(T instance)
        {
            _services.Add(new ServiceDescriptor(typeof(T), instance));
            return this;
        }

        public ServiceCollection AddSingleton<T>() where T : class, new()
        {
            _services.Add(new ServiceDescriptor(typeof(T), typeof(T)));
            return this;
        }

        public ServiceProvider BuildServiceProvider()
        {
            var provider = new ServiceProvider();
            
            foreach (var descriptor in _services)
            {
                if (descriptor.ImplementationInstance != null)
                {
                    provider.AddSingleton(descriptor.ServiceType, descriptor.ImplementationInstance);
                }
                else if (descriptor.ImplementationType != null)
                {
                    var instance = Activator.CreateInstance(descriptor.ImplementationType);
                    provider.AddSingleton(descriptor.ServiceType, instance);
                }
            }
            
            return provider;
        }
    }

    /// <summary>
    /// 服务描述符类
    /// </summary>
    public class ServiceDescriptor
    {
        public ServiceDescriptor(Type serviceType, object implementationInstance)
        {
            ServiceType = serviceType;
            ImplementationInstance = implementationInstance;
        }

        public ServiceDescriptor(Type serviceType, Type implementationType)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
        }

        public Type ServiceType { get; }
        public object ImplementationInstance { get; }
        public Type ImplementationType { get; }
    }
}