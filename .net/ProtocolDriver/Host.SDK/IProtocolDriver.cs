using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Host.SDK
{
    // 表示插件元信息
    public record PluginMetadata(string ProtocolName, string Version);

    // 请求与响应
    public record ProtocolRequest(string Action, IDictionary<string,string> Props, byte[]? Payload = null);
    public record ProtocolResponse(bool Success, byte[]? Payload = null, IDictionary<string, object>? Parsed = null, string? Error = null);

    public enum ConnectionState { Disconnected, Connecting, Connected, Error }

    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        public string ConnectionId { get; init; } = default!;
        public ConnectionState State { get; init; }
        public string? Message { get; init; }
    }

    // 连接实例接口
    public interface IProtocolConnection : IDisposable
    {
        string ConnectionId { get; }
        IDictionary<string, string> Settings { get; }
        ConnectionState State { get; }
        event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        Task OpenAsync(CancellationToken token = default);
        Task CloseAsync(CancellationToken token = default);
        Task<ProtocolResponse> ExecuteAsync(ProtocolRequest request, CancellationToken token = default);
        
        // 心跳机制相关方法和属性
        /// <summary>
        /// 是否支持原生心跳机制
        /// </summary>
        bool SupportsNativeHeartbeat { get; }
        
        /// <summary>
        /// 设置心跳检查点位（当协议不支持原生心跳时使用）
        /// </summary>
        /// <param name="heartbeatAddress">心跳检查的地址</param>
        /// <param name="dataType">数据类型（如Coil、Register等）</param>
        void SetHeartbeatPoint(string heartbeatAddress, string dataType);
        
        /// <summary>
        /// 执行心跳检查
        /// </summary>
        /// <param name="token">取消令牌</param>
        /// <returns>心跳是否成功</returns>
        Task<bool> CheckHeartbeatAsync(CancellationToken token = default);
    }

    // 插件接口（DLL 实现此接口）
    public interface IProtocolDriver
    {
        PluginMetadata Metadata { get; }
        // 创建连接实例（设置来自 Host 保存的连接配置）
        Task<IProtocolConnection> CreateConnectionAsync(IDictionary<string,string> settings, CancellationToken token = default);
    }
}