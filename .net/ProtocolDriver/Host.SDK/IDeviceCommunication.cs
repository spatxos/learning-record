using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Host.SDK.ByteTransform;

namespace Host.SDK
{
    /// <summary>
    /// 通用设备通信接口，定义所有设备协议类需要实现的读写操作及公共属性。
    /// </summary>
    public interface IDeviceCommunication : IReadWriteNet, IDisposable
    {
        /// <summary>
        /// 字节转换工具，用于数据解析和封装。
        /// </summary>
        IByteTransform ByteTransform { get; set; }

        /// <summary>
        /// 设备连接唯一标识。
        /// </summary>
        string ConnectionId { get; set; }

        /// <summary>
        /// 获取或是设置远程服务器的IP地址，如果是本机测试，那么需要设置为127.0.0.1
        /// </summary>
        string IPAddress { get; set; }

        /// <summary>
        /// 获取或设置服务器的端口号，具体的值需要取决于对方的配置
        /// </summary>
        string Port { get; set; }

        /// <summary>
        /// 开始位
        /// </summary>
        int StartAddress { get; set; }

        /// <summary>
        /// 连接状态
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 异步连接到设备
        /// </summary>
        /// <param name="host">设备主机地址</param>
        /// <param name="port">设备端口</param>
        /// <param name="unitId">设备单元ID</param>
        /// <param name="token">取消令牌</param>
        /// <returns>连接是否成功</returns>
        Task<bool> ConnectAsync(string host, int port, byte unitId, CancellationToken token = default);

        /// <summary>
        /// 异步断开与设备的连接
        /// </summary>
        /// <returns>断开连接是否成功</returns>
        Task<bool> DisconnectAsync();

    }
}
