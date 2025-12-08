# 协议插件化框架

## 项目概述

协议插件化框架是一个基于.NET 8.0的模块化协议通信框架，支持多种工业通信协议（如Modbus、MitsubishiMC等）的插件化扩展。框架采用了灵活的插件架构，允许用户动态加载、卸载和管理不同的协议驱动，实现了协议通信的标准化和统一化管理。

## 功能介绍

### 核心功能

1. **插件化架构**：支持动态加载和卸载协议插件，无需重启应用即可更新协议驱动
2. **统一接口**：所有协议驱动实现统一的IProtocolDriver接口，提供一致的使用体验
3. **连接管理**：统一的连接状态管理，支持连接状态监听和事件通知
4. **指令发送**：标准化的指令发送和响应处理机制
5. **配置管理**：灵活的配置管理，支持从配置文件或数据库加载连接设置
6. **服务注册**：支持插件服务的自动注册和发现
7. **热更新**：支持插件的热更新，无需停止服务即可更新插件版本

### 技术特性

- **.NET 8.0**：基于最新的.NET平台，支持跨平台部署
- **依赖注入**：使用Microsoft.Extensions.DependencyInjection实现服务的依赖注入
- **日志记录**：集成Microsoft.Extensions.Logging，支持多种日志输出
- **异步编程**：全面支持异步编程模型，提高系统性能和响应性
- **REST API**：提供RESTful API接口，方便外部系统集成
- **WebSocket通知**：支持WebSocket实时通知连接状态变化

## 项目结构

```
ProtocolDriver/
├── Host.SDK/               # 框架核心SDK
│   ├── Host.SDK.csproj
│   ├── IProtocolDriver.cs  # 协议驱动接口定义
│   ├── Models.cs           # 核心数据模型
│   └── IDriverClient.cs    # 驱动客户端接口
├── Host/                   # 框架宿主应用
│   ├── Controllers/        # API控制器
│   ├── HostApi.cs          # 宿主API实现
│   ├── PluginManager.cs    # 插件管理器
│   ├── Program.cs          # 应用入口
│   └── Storage/            # 存储管理
├── Plugins/                # 协议插件目录
│   ├── Modbus/             # Modbus协议插件
│   ├── MitsubishiMC/       # MitsubishiMC协议插件
│   ├── OmronFINS/          # OmronFINS协议插件
│   └── S7/                 # S7协议插件
└── Test/                   # 测试项目
    ├── ModbusTcpClientTest/  # Modbus客户端测试
    └── ProtocolPluginTest/   # 插件框架测试
```

## 快速开始

### 环境要求

- .NET 8.0 SDK 或更高版本
- Visual Studio 2022 或 Visual Studio Code

### 构建项目

1. 克隆代码库
```bash
git clone <repository-url>
cd ProtocolDriver
```

2. 构建整个解决方案
```bash
dotnet build Host.SDK
dotnet build Host
dotnet build Plugins/Modbus
dotnet build Plugins/MitsubishiMC
```

3. 运行宿主应用
```bash
cd Host
dotnet run
```

### 测试项目

运行测试以验证框架功能
```bash
cd Test/ProtocolPluginTest
dotnet test
```

## 插件开发指南

### 1. 创建插件项目

创建一个新的.NET类库项目，目标框架设置为net8.0

```bash
dotnet new classlib -n MyProtocolPlugin -f net8.0
```

### 2. 引用SDK

添加对Host.SDK项目的引用

```bash
dotnet add reference ../Host.SDK/Host.SDK.csproj
```

### 3. 实现IProtocolDriver接口

创建一个类实现IProtocolDriver接口

```csharp
using Host.SDK;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyProtocolPlugin
{
    public class MyProtocolDriver : IProtocolDriver
    {
        public PluginMetadata Metadata => new PluginMetadata("MyProtocol", "1.0.0");

        public Task<IProtocolConnection> CreateConnectionAsync(IDictionary<string, string> settings, CancellationToken token = default)
        {
            return Task.FromResult<IProtocolConnection>(new MyProtocolConnection(settings));
        }

        // 内部类：实现IProtocolConnection接口
        private class MyProtocolConnection : IProtocolConnection
        {
            public string ConnectionId { get; }
            public IDictionary<string, string> Settings { get; }
            public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
            public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

            public MyProtocolConnection(IDictionary<string, string> settings)
            {
                ConnectionId = System.Guid.NewGuid().ToString();
                Settings = settings;
            }

            public async Task OpenAsync(CancellationToken token = default)
            {
                // 实现连接逻辑
                State = ConnectionState.Connecting;
                OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs { ConnectionId = ConnectionId, State = State });

                // 模拟连接过程
                await Task.Delay(1000, token);

                State = ConnectionState.Connected;
                OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs { ConnectionId = ConnectionId, State = State });
            }

            public async Task CloseAsync(CancellationToken token = default)
            {
                // 实现断开连接逻辑
                State = ConnectionState.Disconnected;
                OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs { ConnectionId = ConnectionId, State = State });
                await Task.CompletedTask;
            }

            public async Task<ProtocolResponse> ExecuteAsync(ProtocolRequest request, CancellationToken token = default)
            {
                // 实现指令执行逻辑
                return await Task.FromResult(new ProtocolResponse(
                    Success: true,
                    Payload: new byte[] { 0x01, 0x02, 0x03 },
                    Parsed: new Dictionary<string, object> { { "Result", "Success" } }
                ));
            }

            private void OnConnectionStatusChanged(ConnectionStatusChangedEventArgs e)
            {
                ConnectionStatusChanged?.Invoke(this, e);
            }

            public void Dispose()
            {
                // 实现资源释放逻辑
            }
        }
    }
}
```

### 4. 构建插件

```bash
dotnet build
```

## 插件打包与导入

### 插件打包

1. **构建插件**：确保插件项目已经成功构建

2. **创建插件目录结构**：按照以下结构创建插件目录

```
plugins/
└── MyProtocol/
    └── 1.0.0/
        └── MyProtocolPlugin.dll
```

3. **复制插件文件**：将构建好的插件DLL文件复制到对应的版本目录

### 插件导入

框架支持两种插件导入方式：

#### 1. 自动扫描导入

框架启动时会自动扫描plugins目录下的所有插件，并加载最新版本的插件。

#### 2. API导入

通过REST API上传插件文件：

```bash
POST /api/Plugin/upload
Content-Type: multipart/form-data

file=@MyProtocolPlugin.dll
```

#### 3. 手动导入

将插件文件手动复制到plugins目录下的对应位置，框架会自动检测并加载新的插件。

## Modbus服务注册、调用与验证

### 1. Modbus服务注册

Modbus插件会在框架启动时自动注册到插件管理器中。插件管理器会：

- 扫描plugins/Modbus目录下的最新版本插件
- 加载ModbusDriver.dll文件
- 创建ModbusDriver实例
- 注册到插件字典中，可通过"Modbus"协议名称访问

### 2. 连接创建与管理

#### 创建Modbus连接

```csharp
// 获取插件管理器实例
var pluginManager = serviceProvider.GetRequiredService<PluginManager>();

// 获取Modbus插件
var modbusPlugin = pluginManager.GetPluginByProtocolName("Modbus");

// 创建连接设置
var settings = new Dictionary<string, string>
{
    { "IpAddress", "127.0.0.1" },
    { "Port", "502" },
    { "UnitId", "1" }
};

// 创建连接
var connection = await modbusPlugin.Driver.CreateConnectionAsync(settings, CancellationToken.None);
```

#### 打开连接

```csharp
await connection.OpenAsync();
```

#### 监听连接状态变化

```csharp
connection.ConnectionStatusChanged += (sender, e) =>
{
    Console.WriteLine($"Connection {e.ConnectionId} state changed to {e.State}");
};
```

### 3. 指令发送与响应

#### 读取保持寄存器

```csharp
// 创建读取保持寄存器的请求
var request = new ProtocolRequest(
    Action: "ReadRegisters",
    Props: new Dictionary<string, string>
    {
        { "FunctionCode", "3" },
        { "StartAddress", "0" },
        { "Count", "2" }
    }
);

// 发送请求
var response = await connection.ExecuteAsync(request);

// 处理响应
if (response.Success)
{
    Console.WriteLine("读取成功");
    foreach (var item in response.Parsed)
    {
        Console.WriteLine($"{item.Key}: {item.Value}");
    }
}
else
{
    Console.WriteLine($"读取失败: {response.Error}");
}
```

#### 写入单个保持寄存器

```csharp
// 创建写入保持寄存器的请求
var request = new ProtocolRequest(
    Action: "WriteRegister",
    Props: new Dictionary<string, string>
    {
        { "FunctionCode", "6" },
        { "Address", "0" },
        { "Value", "1234" }
    }
);

// 发送请求
var response = await connection.ExecuteAsync(request);
```

### 4. 连接关闭

```csharp
await connection.CloseAsync();
```

### 5. 验证过程

#### 使用API验证

```bash
# 获取所有插件
GET /api/Plugin

# 获取Modbus插件
GET /api/Plugin/Modbus

# 创建连接
POST /api/Connection
Content-Type: application/json

{
    "protocolName": "Modbus",
    "settings": {
        "IpAddress": "127.0.0.1",
        "Port": "502",
        "UnitId": "1"
    }
}

# 打开连接
POST /api/Connection/{connectionId}/open

# 发送指令
POST /api/Connection/{connectionId}/execute
Content-Type: application/json

{
    "action": "ReadRegisters",
    "props": {
        "FunctionCode": "3",
        "StartAddress": "0",
        "Count": "2"
    }
}

# 关闭连接
POST /api/Connection/{connectionId}/close
```

#### 使用测试项目验证

```bash
cd Test/ProtocolPluginTest
dotnet test
```

测试项目包含了对Modbus服务的完整测试，包括服务注册、连接创建、指令发送等功能。

## API文档

### 插件管理API

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | /api/Plugin | 获取所有插件 |
| GET | /api/Plugin/{protocolName} | 根据协议名称获取插件 |
| POST | /api/Plugin/upload | 上传插件文件 |
| DELETE | /api/Plugin/{pluginId} | 卸载插件 |
| GET | /api/Plugin/{pluginId}/health | 检查插件健康状态 |

### 连接管理API

| 方法 | 路径 | 描述 |
|------|------|------|
| POST | /api/Connection | 创建连接 |
| GET | /api/Connection | 获取所有连接 |
| GET | /api/Connection/{connectionId} | 根据连接ID获取连接 |
| POST | /api/Connection/{connectionId}/open | 打开连接 |
| POST | /api/Connection/{connectionId}/close | 关闭连接 |
| DELETE | /api/Connection/{connectionId} | 删除连接 |

### 指令执行API

| 方法 | 路径 | 描述 |
|------|------|------|
| POST | /api/Connection/{connectionId}/execute | 执行指令 |

## 部署

### Docker部署

1. 构建Docker镜像

```bash
docker build -t protocol-driver .
```

2. 运行Docker容器

```bash
docker run -p 8080:80 protocol-driver
```

### Kubernetes部署

使用提供的Kubernetes配置文件部署

```bash
kubectl apply -f kubernetes/deployment.yml
```

## 贡献

欢迎提交Issue和Pull Request来帮助改进这个项目。

### 开发流程

1. Fork项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开Pull Request

## 许可证

本项目采用MIT许可证 - 查看[LICENSE](LICENSE)文件了解详情。

## 联系方式

如有问题或建议，请通过以下方式联系：

- GitHub Issues: [项目Issues页面](https://github.com/yourusername/protocol-driver/issues)
- 电子邮件: your.email@example.com

---

**协议插件化框架** - 让工业协议通信更简单、更灵活！