using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Host.SDK
{
    /// <summary>
    /// 驱动上下文
    /// </summary>
    public record DriverContext(
        ILogger Logger,
        ITransportFactory TransportFactory,
        IConfiguration Config,
        IHostApi HostApi
    );

    /// <summary>
    /// 执行结果
    /// </summary>
    public record DriverResult(
        bool Success,
        byte[] Response,
        string? ErrorMessage = null
    );

    /// <summary>
    /// 解析结果
    /// </summary>
    public record DriverParseResult(
        string[] Tags,
        IDictionary<string, object> Data
    );

    /// <summary>
    /// 健康检查结果
    /// </summary>
    public record DriverHealth(
        bool IsHealthy,
        string Message
    );

    /// <summary>
    /// 传输工厂接口
    /// </summary>
    public interface ITransportFactory
    {
        /// <summary>
        /// 创建TCP客户端
        /// </summary>
        /// <returns>TCP客户端</returns>
        TcpClient CreateTcpClient();

        /// <summary>
        /// 创建UDP客户端
        /// </summary>
        /// <returns>UDP客户端</returns>
        UdpClient CreateUdpClient();
    }

    /// <summary>
    /// Host API接口
    /// </summary>
    public interface IHostApi
    {
        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="data">事件数据</param>
        /// <param name="token">取消令牌</param>
        /// <returns>任务</returns>
        Task PublishEventAsync(string eventName, object data, CancellationToken token = default);

        /// <summary>
        /// 写入数据到数据库
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="data">数据</param>
        /// <param name="token">取消令牌</param>
        /// <returns>任务</returns>
        Task WriteToDatabaseAsync(string tableName, object data, CancellationToken token = default);

        /// <summary>
        /// 上报指标
        /// </summary>
        /// <param name="metricName">指标名称</param>
        /// <param name="value">指标值</param>
        /// <param name="labels">标签</param>
        /// <param name="token">取消令牌</param>
        /// <returns>任务</returns>
        Task ReportMetricAsync(string metricName, double value, IDictionary<string, string>? labels = null, CancellationToken token = default);
    }

    /// <summary>
    /// 读取请求基类
    /// </summary>
    public abstract class ReadRequestBase
    {
        /// <summary>
        /// 单元ID
        /// </summary>
        public byte UnitId { get; set; } = 0x01;

        /// <summary>
        /// 功能码
        /// </summary>
        public int FunctionCode { get; set; }

        /// <summary>
        /// 起始地址
        /// </summary>
        public ushort StartAddress { get; set; }

        /// <summary>
        /// 读取数量
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 写入请求基类
    /// </summary>
    public abstract class WriteRequestBase
    {
        /// <summary>
        /// 单元ID
        /// </summary>
        public byte UnitId { get; set; } = 0x01;

        /// <summary>
        /// 起始地址
        /// </summary>
        public ushort StartAddress { get; set; }

        /// <summary>
        /// 功能码
        /// </summary>
        public int FunctionCode { get; set; }

        /// <summary>
        /// 数据字节数组
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}